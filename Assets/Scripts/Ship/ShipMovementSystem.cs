using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct ShipAspect : IAspect
{
    public readonly Entity Self;
    public readonly RefRO<PlayerTag> PlayerTag;
    public readonly RefRW<ShipMovementData> PlayerData;
    public readonly RefRW<ShipBoosterState> PlayerBoostState;
    public readonly RefRW<PhysicsVelocity> Velocity;
    public readonly RefRW<PhysicsMass> PhysicsMass;
    public readonly RefRO<LocalToWorld> LocalToWorld;
    public readonly DynamicBuffer<ShipHardpointReference> ShipHardpoints;
    public readonly DynamicBuffer<DetectedTarget> DetectedTargets;
}

partial class ShipMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // TODO: Turn into generic ship controller- handle input elsewhere 
        foreach (ShipAspect player in SystemAPI.Query<ShipAspect>())
        {
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(player.Self);
            InputHandler playerInput = managedAccess.ManagedLocalPlayer.GetPlayerInput();
            
            foreach (ShipHardpointReference shipHardpoint in player.ShipHardpoints)
            {
                ShipHardpointInstance hardpoint = SystemAPI.GetComponent<ShipHardpointInstance>(shipHardpoint.Self);
                hardpoint.IsFiring = playerInput.IsAttacking;
                SystemAPI.SetComponent(shipHardpoint.Self, hardpoint);
            }

            UpdateTargets(player, playerInput);

            player.PlayerBoostState.ValueRW.UpdateBoost(SystemAPI.Time.DeltaTime, player.PlayerData.ValueRO.BoostRechargeTime);
            if (playerInput.IsBoosting)
            {
                player.PlayerBoostState.ValueRW.TryBoostPerformed(player.PlayerData.ValueRO.BoostTime);
            }

            // float angularVelocityModifier = 1.0f;
            // float linearVelocityModifier = 1.0f;
            // if (GetAttachedMass(player.Self, out PhysicsMass attachedPhysicsMass))
            // {
            //     angularVelocityModifier = m_slowdown * (1.0f - attachedPhysicsMass.InverseMass);
            //     linearVelocityModifier = m_slowdown * (1.0f - attachedPhysicsMass.InverseMass) * SystemAPI.Time.DeltaTime;
            // }

            float3 linearVelocity = GetLinearVelocity(player, managedAccess);
            player.Velocity.ValueRW.Linear = linearVelocity;// - linearVelocityModifier;

            float3 angularVelocity = GetAngularVelocity(player, managedAccess); // GetAngularVelocity(player, managedAccess);
            player.Velocity.ValueRW.Angular = angularVelocity; // * angularVelocityModifier;
        }
    }

    private void UpdateTargets(ShipAspect player, InputHandler playerInput)
    {
        var localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        
        float threshold = 0.99f;
        int bestTargetAhead = -1;
        float bestDot = 0.0f;
        for (int i=0; i< player.DetectedTargets.Length; i++)
        {
            DetectedTarget target = player.DetectedTargets[i];
            if (EntityManager.Exists(target.TargetableEntity))
            {
                LocalToWorld targetTransform = localToWorldLookup[target.TargetableEntity];
                float3 deltaNormalised = math.normalize(targetTransform.Position - player.LocalToWorld.ValueRO.Position);
                float dot = math.dot(player.LocalToWorld.ValueRO.Forward, deltaNormalised);
                if (dot > bestDot && dot >= threshold)
                {
                    bestDot = dot;
                    bestTargetAhead = i;
                }
            }
        }
            
        var detectedTargets = player.DetectedTargets;
        for (int i = 0; i < player.DetectedTargets.Length; i++)
        {
            var t = player.DetectedTargets[i];
            t.CanTargetAhead = i == bestTargetAhead;
            if (playerInput.TargetSelectAhead)
            {
                if (i == bestTargetAhead)
                {
                    t.IsSelected = !t.IsSelected;
                }
                else
                {
                    t.IsSelected = false;
                }
            }
            detectedTargets[i] = t;
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

    private float3 GetLinearVelocity(ShipAspect player, PlayerManagedAccess managedAccess)
    {
        InputHandler playerInput = managedAccess.ManagedLocalPlayer.GetPlayerInput();
        ShipMovementData shipMovementData = player.PlayerData.ValueRO;
        ShipBoosterState shipBoosters = player.PlayerBoostState.ValueRO;

        ThrusterSetup thrusterSetup = shipMovementData.DefaultMovement;
        if (playerInput.IsADS)
        {
            thrusterSetup = shipMovementData.ADSMovement;
        }
        
        float3 acceleration = new float3();
        float maximumVelocity = shipMovementData.MaximumVelocity;
        
        if (shipBoosters.IsBoosting())
        {
            acceleration.z = shipBoosters.GetBoostAcceleration(shipMovementData.BoostAcceleration, shipMovementData.BoostTime);
            maximumVelocity = shipMovementData.BoostMaximumVelocity;
        }
        else
        {
            float dir = playerInput.TargetDirection.z;
            if (dir > 0.0f)
            {
                acceleration.z = thrusterSetup.ForwardThrustersAcceleration;
            }
            else if (dir < 0.0f)
            {
                acceleration.z = -thrusterSetup.ReverseThrustersAcceleration;
            }
        }

        acceleration.x = playerInput.TargetDirection.x * thrusterSetup.LateralThrustersAcceleration;
        acceleration.y = playerInput.TargetDirection.y * thrusterSetup.LateralThrustersAcceleration;
        
        LocalToWorld localToWorld = player.LocalToWorld.ValueRO;
        float3 currentVelocityLocal = localToWorld.Value.InverseTransformDirection(player.Velocity.ValueRO.Linear);
        currentVelocityLocal += acceleration * SystemAPI.Time.DeltaTime;
        currentVelocityLocal = math.normalizesafe(currentVelocityLocal) * math.min(maximumVelocity, math.length(currentVelocityLocal));

        if (playerInput.LinearDampersActive)
        {
            currentVelocityLocal = DampVelocity(currentVelocityLocal, acceleration, thrusterSetup.VelocityDamperDeceleration);
        }

        return localToWorld.Value.TransformDirection(currentVelocityLocal);
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
    
    private float3 GetAngularVelocity(ShipAspect player, PlayerManagedAccess managedAccess)
    {
        InputHandler playerInput = managedAccess.ManagedLocalPlayer.GetPlayerInput();
        ShipMovementData shipMovementData = player.PlayerData.ValueRO;
        
        ThrusterSetup thrusterSetup = shipMovementData.DefaultMovement;
        if (playerInput.IsADS)
        {
            thrusterSetup = shipMovementData.ADSMovement;
        }
        
        float3 angularAcceleration = float3.zero;
        
        if (!playerInput.IsADS)
        {
            float lookSensitivity = managedAccess.ManagedLocalPlayer.GetLookSensitivity();
            float yawAngleAcceleration = lookSensitivity * playerInput.LookDelta.x;
            yawAngleAcceleration = math.sign(yawAngleAcceleration) *
                                   math.min(math.abs(yawAngleAcceleration), thrusterSetup.MaxTurnAcceleration);
            angularAcceleration.y = math.radians(yawAngleAcceleration);

            float pitchAngleAcceleration = lookSensitivity * playerInput.LookDelta.y;
            pitchAngleAcceleration = math.sign(pitchAngleAcceleration) *
                                     math.min(math.abs(pitchAngleAcceleration), thrusterSetup.MaxTurnAcceleration);
            angularAcceleration.x = math.radians(pitchAngleAcceleration);
        }

        float rollSensitivity = managedAccess.ManagedLocalPlayer.GetRollSensitivity();
        float rollAngleAcceleration = rollSensitivity * thrusterSetup.MaxRollAcceleration * playerInput.RollDirection;
        angularAcceleration.z = math.radians(rollAngleAcceleration);
        
        float3 angularVelocity = player.Velocity.ValueRO.Angular;

        if (playerInput.IsADS)
        {
            float lookSensitivity = managedAccess.ManagedLocalPlayer.GetLookSensitivityADS();
            float yawTargetVelocity = lookSensitivity * playerInput.LookDelta.x;
            yawTargetVelocity = math.sign(yawTargetVelocity) * math.min(math.abs(yawTargetVelocity), thrusterSetup.MaxTurnSpeed);
            angularVelocity.y = math.radians(yawTargetVelocity);
            
            float pitchTargetVelocity = lookSensitivity * playerInput.LookDelta.y;
            pitchTargetVelocity = math.sign(pitchTargetVelocity) * math.min(math.abs(pitchTargetVelocity), thrusterSetup.MaxTurnSpeed);
            angularVelocity.x = math.radians(pitchTargetVelocity);
        }
        
        angularVelocity += angularAcceleration * SystemAPI.Time.DeltaTime;
        
        angularVelocity.x = math.sign(angularVelocity.x) * math.min(math.abs(angularVelocity.x), math.radians(thrusterSetup.MaxTurnSpeed));
        angularVelocity.y = math.sign(angularVelocity.y) * math.min(math.abs(angularVelocity.y), math.radians(thrusterSetup.MaxTurnSpeed));
        
        angularVelocity.z = math.sign(angularVelocity.z) * math.min(math.abs(angularVelocity.z), math.radians(thrusterSetup.MaxRollSpeed));

        if (playerInput.AngularDampersActive)
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
