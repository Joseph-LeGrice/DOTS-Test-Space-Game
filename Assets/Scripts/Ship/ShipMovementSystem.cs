using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial class ShipMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (ShipAspect shipAspect in SystemAPI.Query<ShipAspect>())
        {
            foreach (ShipHardpointReference shipHardpoint in shipAspect.ShipHardpoints)
            {
                ShipHardpointInstance hardpoint = SystemAPI.GetComponent<ShipHardpointInstance>(shipHardpoint.Self);
                hardpoint.IsFiring = shipAspect.ShipInput.ValueRO.IsAttacking;
                SystemAPI.SetComponent(shipHardpoint.Self, hardpoint);
            }

            shipAspect.PlayerBoostState.ValueRW.UpdateBoost(SystemAPI.Time.DeltaTime, shipAspect.ShipMovementData.ValueRO.BoostRechargeTime);
            if (shipAspect.ShipInput.ValueRO.IsBoosting)
            {
                shipAspect.PlayerBoostState.ValueRW.TryBoostPerformed(shipAspect.ShipMovementData.ValueRO.BoostTime);
            }

            // float angularVelocityModifier = 1.0f;
            // float linearVelocityModifier = 1.0f;
            // if (GetAttachedMass(player.Self, out PhysicsMass attachedPhysicsMass))
            // {
            //     angularVelocityModifier = m_slowdown * (1.0f - attachedPhysicsMass.InverseMass);
            //     linearVelocityModifier = m_slowdown * (1.0f - attachedPhysicsMass.InverseMass) * SystemAPI.Time.DeltaTime;
            // }

            float3 linearVelocity = GetLinearVelocity(shipAspect);
            shipAspect.Velocity.ValueRW.Linear = linearVelocity;// - linearVelocityModifier;

            float3 angularVelocity = GetAngularVelocity(shipAspect); // GetAngularVelocity(player, managedAccess);
            shipAspect.Velocity.ValueRW.Angular = angularVelocity; // * angularVelocityModifier;
        }
    }

    private bool GetAttachedMass(Entity ownerEntity, out PhysicsMass attachedPhysicsMass)
    {
        var physicsMassLookup = SystemAPI.GetComponentLookup<PhysicsMass>();
        foreach (var activeTether in SystemAPI.Query<RefRO<GravityTetherJoint>>())
        {
            if (activeTether.ValueRO.OwnerEntity == ownerEntity)
            {
                attachedPhysicsMass = physicsMassLookup[activeTether.ValueRO.AttachedEntity];
                return true;
            }
        }

        attachedPhysicsMass = new PhysicsMass();
        return false;
    }

    private float3 GetLinearVelocity(ShipAspect shipAspect)
    {
        ShipMovementData shipMovementData = shipAspect.ShipMovementData.ValueRO;
        ShipBoosterState shipBoosters = shipAspect.PlayerBoostState.ValueRO;

        ThrusterSetup thrusterSetup = shipMovementData.DefaultMovement;
        if (shipAspect.ShipInput.ValueRO.IsADS)
        {
            thrusterSetup = shipMovementData.ADSMovement;
        }
        
        float3 acceleration = new float3();
        float maximumVelocity = shipMovementData.MaximumVelocity;
        
        LocalToWorld localToWorld = shipAspect.LocalToWorld.ValueRO;
        float3 currentVelocityLocal = localToWorld.Value.InverseTransformDirection(shipAspect.Velocity.ValueRO.Linear);
        
        if (shipBoosters.IsBoosting())
        {
            acceleration.z = shipBoosters.GetBoostAcceleration(shipMovementData.BoostAcceleration, shipMovementData.BoostTime);
            maximumVelocity = shipMovementData.BoostMaximumVelocity;
        }
        else
        {
            float targetVelocity = shipAspect.ShipInput.ValueRO.Throttle * shipAspect.ShipMovementData.ValueRO.MaximumVelocity;
            float vDelta = targetVelocity - currentVelocityLocal.z;

            if (shipAspect.ShipInput.ValueRO.LinearDampersActive)
            {
                float maxThrottleAcceleration = thrusterSetup.ForwardThrustersAcceleration;
                if (vDelta < 0.0f)
                {
                    maxThrottleAcceleration = -thrusterSetup.ReverseThrustersAcceleration;
                }

                float maxVelocityDelta = math.abs(maxThrottleAcceleration) * SystemAPI.Time.DeltaTime;
                float accelerationT = math.min(math.abs(vDelta) / maxVelocityDelta, 1.0f);
                acceleration.z = accelerationT * maxThrottleAcceleration;
            }
            else
            {
                if (vDelta > 0.0f && targetVelocity > 0.0f)
                {
                    float maxThrottleAcceleration = thrusterSetup.ForwardThrustersAcceleration;
                    float maxVelocityDelta = math.abs(maxThrottleAcceleration) * SystemAPI.Time.DeltaTime;
                    float accelerationT = math.min(math.abs(vDelta) / maxVelocityDelta, 1.0f);
                    acceleration.z = accelerationT * maxThrottleAcceleration;
                }
                else if (vDelta < 0.0f && targetVelocity < 0.0f)
                {
                    float maxThrottleAcceleration = -thrusterSetup.ReverseThrustersAcceleration;
                    float maxVelocityDelta = math.abs(maxThrottleAcceleration) * SystemAPI.Time.DeltaTime;
                    float accelerationT = math.min(math.abs(vDelta) / maxVelocityDelta, 1.0f);
                    acceleration.z = accelerationT * maxThrottleAcceleration;
                }
            }
        }

        acceleration.x = shipAspect.ShipInput.ValueRO.StrafeThrusters.x * thrusterSetup.LateralThrustersAcceleration;
        acceleration.y = shipAspect.ShipInput.ValueRO.StrafeThrusters.y * thrusterSetup.LateralThrustersAcceleration;

        float3 nextVelocityLocal = currentVelocityLocal;
        nextVelocityLocal += acceleration * SystemAPI.Time.DeltaTime;
        nextVelocityLocal = math.normalizesafe(nextVelocityLocal) * math.min(maximumVelocity, math.length(nextVelocityLocal));

        if (shipAspect.ShipInput.ValueRO.LinearDampersActive)
        {
            nextVelocityLocal = DampVelocity(nextVelocityLocal, acceleration, thrusterSetup.VelocityDamperDeceleration);
        }

        return localToWorld.Value.TransformDirection(nextVelocityLocal);
    }
    
    private float3 DampVelocity(float3 currentVelocity, float3 targetDirection, float velocityDampingDeceleration)
    {
        targetDirection = math.normalizesafe(targetDirection);
        float3 targetVelocity = math.length(currentVelocity) * targetDirection;
        float likeness = math.max(math.dot(targetVelocity, currentVelocity), 0.0f);
        float3 toDamp = currentVelocity - likeness * targetDirection;
        float3 dampedComponent = new float3(
            math.sign(toDamp.x) * math.max(math.abs(toDamp.x) - velocityDampingDeceleration * SystemAPI.Time.DeltaTime, 0.0f),
            math.sign(toDamp.y) * math.max(math.abs(toDamp.y) - velocityDampingDeceleration * SystemAPI.Time.DeltaTime, 0.0f),
            math.sign(toDamp.z) * math.max(math.abs(toDamp.z) - velocityDampingDeceleration * SystemAPI.Time.DeltaTime, 0.0f)
        );
        return dampedComponent + likeness * targetDirection;
    }
    
    private float3 GetAngularVelocity(ShipAspect shipAspect)
    {
        ShipMovementData shipMovementData = shipAspect.ShipMovementData.ValueRO;
        
        ThrusterSetup thrusterSetup = shipMovementData.DefaultMovement;
        if (shipAspect.ShipInput.ValueRO.IsADS)
        {
            thrusterSetup = shipMovementData.ADSMovement;
        }
        
        float3 targetVelocity = float3.zero;
        targetVelocity.xy = shipAspect.ShipInput.ValueRO.AngularThrottle.xy * thrusterSetup.MaxTurnSpeed;
        targetVelocity.z = shipAspect.ShipInput.ValueRO.RollDirection * thrusterSetup.MaxRollSpeed;
        targetVelocity = math.radians(targetVelocity);
        
        float3 currentVelocity = shipAspect.Velocity.ValueRO.Angular;
        float3 velocityDelta = targetVelocity - currentVelocity;
        
        float3 angularAcceleration = float3.zero;

        float maxPitchYawVelocityDelta = math.radians(thrusterSetup.MaxTurnAcceleration) * SystemAPI.Time.DeltaTime;
        float2 pitchYawAccelerationT = math.sign(velocityDelta.xy) * math.min(math.abs(velocityDelta.xy) / maxPitchYawVelocityDelta, 1.0f);
        angularAcceleration.xy = pitchYawAccelerationT * math.radians(thrusterSetup.MaxTurnAcceleration);
        
        float maxRollVelocityDelta = math.radians(thrusterSetup.MaxRollAcceleration) * SystemAPI.Time.DeltaTime;
        float accelerationT = math.sign(velocityDelta.z) * math.min(math.abs(velocityDelta.z) / maxRollVelocityDelta, 1.0f);
        angularAcceleration.z = accelerationT * math.radians(thrusterSetup.MaxRollAcceleration);

        float3 nextAngularVelocity = currentVelocity + angularAcceleration * SystemAPI.Time.DeltaTime;
        nextAngularVelocity.xy = math.sign(nextAngularVelocity.xy) * math.min(math.abs(nextAngularVelocity.xy), math.radians(thrusterSetup.MaxTurnSpeed));
        nextAngularVelocity.z = math.sign(nextAngularVelocity.z) * math.min(math.abs(nextAngularVelocity.z), math.radians(thrusterSetup.MaxRollSpeed));

        if (shipAspect.ShipInput.ValueRO.AngularDampersActive)
        {
            nextAngularVelocity = DampAngularVelocity(nextAngularVelocity, targetVelocity, thrusterSetup.AngularDamperDeceleration);
        }

        return nextAngularVelocity;
    }

    private float3 DampAngularVelocity(float3 currentVelocity, float3 targetDirection, float velocityDampingDeceleration)
    {
        float3 currentVelocityNormalised = math.normalizesafe(currentVelocity);
        float3 currentAccelerationNormalised = math.normalizesafe(targetDirection);
        float likeness = math.max(math.dot(currentVelocityNormalised, currentAccelerationNormalised), 0.0f);
        float3 toDamp = math.length(currentVelocity) * (currentVelocityNormalised - likeness * currentAccelerationNormalised);
        float3 dampedComponent = new float3(
            math.sign(toDamp.x) * math.max(math.abs(toDamp.x) - math.radians(velocityDampingDeceleration) * SystemAPI.Time.DeltaTime, 0.0f),
            math.sign(toDamp.y) * math.max(math.abs(toDamp.y) - math.radians(velocityDampingDeceleration) * SystemAPI.Time.DeltaTime, 0.0f),
            math.sign(toDamp.z) * math.max(math.abs(toDamp.z) - math.radians(velocityDampingDeceleration) * SystemAPI.Time.DeltaTime, 0.0f)
        );
        return dampedComponent + math.length(currentVelocity) * likeness * currentAccelerationNormalised;
    }
}
