using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct ShipAspect : IAspect
{
    public readonly Entity Self;
    public readonly RefRO<ShipInput> ShipInput;
    public readonly RefRW<ShipMovementData> ShipMovementData;
    public readonly RefRW<ShipBoosterState> PlayerBoostState;
    public readonly RefRW<PhysicsVelocity> Velocity;
    public readonly RefRW<PhysicsMass> PhysicsMass;
    public readonly RefRO<Damageable> Damageable;
    public readonly RefRO<LocalToWorld> LocalToWorld;
    public readonly DynamicBuffer<ShipHardpointReference> ShipHardpoints;
    public readonly DynamicBuffer<DetectedTarget> DetectedTargets;
}

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

            float maxAcceleration = thrusterSetup.ForwardThrustersAcceleration;
            if (vDelta < 0.0f)
            {
                maxAcceleration = -thrusterSetup.ReverseThrustersAcceleration;
            }

            float lowerBound = 0.5f;
            float upperBound = 5.0f;
            acceleration.z = maxAcceleration * math.unlerp(lowerBound, upperBound, math.min(math.abs(vDelta), upperBound));
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
        
        float3 angularAcceleration = float3.zero;
        
        if (!shipAspect.ShipInput.ValueRO.IsADS)
        {
            float yawAngleAcceleration = shipAspect.ShipInput.ValueRO.LookDelta.x;
            yawAngleAcceleration = math.sign(yawAngleAcceleration) *
                                   math.min(math.abs(yawAngleAcceleration), thrusterSetup.MaxTurnAcceleration);
            angularAcceleration.y = math.radians(yawAngleAcceleration);

            float pitchAngleAcceleration = shipAspect.ShipInput.ValueRO.LookDelta.y;
            pitchAngleAcceleration = math.sign(pitchAngleAcceleration) *
                                     math.min(math.abs(pitchAngleAcceleration), thrusterSetup.MaxTurnAcceleration);
            angularAcceleration.x = math.radians(pitchAngleAcceleration);
        }
        
        float3 angularVelocity = shipAspect.Velocity.ValueRO.Angular;

        float targetRollSpeed = thrusterSetup.MaxRollSpeed * shipAspect.ShipInput.ValueRO.RollDirection;
        float targetRollSpeedDelta = targetRollSpeed - angularVelocity.z;

        float maxVelocityDelta = thrusterSetup.MaxRollAcceleration * SystemAPI.Time.DeltaTime;
        float accelerationT = math.sign(targetRollSpeedDelta) * math.min(math.abs(targetRollSpeedDelta) / maxVelocityDelta, 1.0f);
        float rollAngleAcceleration = accelerationT * thrusterSetup.MaxRollAcceleration;
        angularAcceleration.z = math.radians(rollAngleAcceleration);

        if (shipAspect.ShipInput.ValueRO.IsADS)
        {
            float yawTargetVelocity = shipAspect.ShipInput.ValueRO.LookDelta.x;
            yawTargetVelocity = math.sign(yawTargetVelocity) * math.min(math.abs(yawTargetVelocity), thrusterSetup.MaxTurnSpeed);
            angularVelocity.y = math.radians(yawTargetVelocity);
            
            float pitchTargetVelocity = shipAspect.ShipInput.ValueRO.LookDelta.y;
            pitchTargetVelocity = math.sign(pitchTargetVelocity) * math.min(math.abs(pitchTargetVelocity), thrusterSetup.MaxTurnSpeed);
            angularVelocity.x = math.radians(pitchTargetVelocity);
        }
        
        angularVelocity += angularAcceleration * SystemAPI.Time.DeltaTime;
        
        angularVelocity.x = math.sign(angularVelocity.x) * math.min(math.abs(angularVelocity.x), math.radians(thrusterSetup.MaxTurnSpeed));
        angularVelocity.y = math.sign(angularVelocity.y) * math.min(math.abs(angularVelocity.y), math.radians(thrusterSetup.MaxTurnSpeed));
        
        angularVelocity.z = math.sign(angularVelocity.z) * math.min(math.abs(angularVelocity.z), math.radians(thrusterSetup.MaxRollSpeed));

        if (shipAspect.ShipInput.ValueRO.AngularDampersActive)
        {
            angularVelocity = DampAngularVelocity(angularVelocity, angularAcceleration, thrusterSetup.AngularDamperDeceleration);
        }

        return angularVelocity;
    }

    private float3 DampAngularVelocity(float3 currentVelocity, float3 currentAcceleration, float velocityDampingDeceleration)
    {
        float3 currentVelocityNormalised = math.normalizesafe(currentVelocity);
        float3 currentAccelerationNormalised = math.normalizesafe(currentAcceleration);
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
