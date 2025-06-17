using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct PlayerAspect : IAspect
{
    public readonly Entity Self;
    public readonly RefRW<PlayerData> PlayerData;
    public readonly RefRW<PlayerBoosterState> PlayerBoostState;
    public readonly RefRW<PhysicsVelocity> Velocity;
    public readonly RefRW<PhysicsMass> PhysicsMass;
    public readonly RefRO<LocalToWorld> LocalToWorld;
    public readonly DynamicBuffer<ShipHardpointBufferElement> ShipHardpoints;
    public readonly DynamicBuffer<DetectedTarget> DetectedTargets;
}

partial class PlayerInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (PlayerAspect player in SystemAPI.Query<PlayerAspect>())
        {
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(player.Self);
            InputHandler playerInput = managedAccess.ManagedLocalPlayer.GetPlayerInput();
            
            foreach (ShipHardpointBufferElement shipHardpoint in player.ShipHardpoints)
            {
                if (SystemAPI.HasComponent<ProjectileSource>(shipHardpoint.Self))
                {
                    ProjectileSource ps = SystemAPI.GetComponent<ProjectileSource>(shipHardpoint.Self);
                    ps.IsFiring = playerInput.IsAttacking;
                    SystemAPI.SetComponent(shipHardpoint.Self, ps);
                }
                if (SystemAPI.HasComponent<BeamSource>(shipHardpoint.Self))
                {
                    BeamSource bs = SystemAPI.GetComponent<BeamSource>(shipHardpoint.Self);
                    bs.IsFiring = playerInput.IsAttacking;
                    SystemAPI.SetComponent(shipHardpoint.Self, bs);
                }
                if (SystemAPI.HasComponent<GravityTether>(shipHardpoint.Self))
                {
                    GravityTether gravityTether = SystemAPI.GetComponent<GravityTether>(shipHardpoint.Self);
                    gravityTether.IsFiring = playerInput.IsAttacking;
                    SystemAPI.SetComponent(shipHardpoint.Self, gravityTether);
                }
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

    private void UpdateTargets(PlayerAspect player, InputHandler playerInput)
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

    private float3 GetLinearVelocity(PlayerAspect player, PlayerManagedAccess managedAccess)
    {
        InputHandler playerInput = managedAccess.ManagedLocalPlayer.GetPlayerInput();
        PlayerData playerData = player.PlayerData.ValueRO;
        PlayerBoosterState playerBoosters = player.PlayerBoostState.ValueRO;

        ThrusterSetup thrusterSetup = playerData.DefaultMovement;
        if (playerInput.IsADS)
        {
            thrusterSetup = playerData.ADSMovement;
        }
        
        float3 acceleration = new float3();
        float maximumVelocity = playerData.MaximumVelocity;
        
        if (playerBoosters.IsBoosting())
        {
            acceleration.z = playerBoosters.GetBoostAcceleration(playerData.BoostAcceleration, playerData.BoostTime);
            maximumVelocity = playerData.BoostMaximumVelocity;
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
    
    private float3 GetAngularVelocity(PlayerAspect player, PlayerManagedAccess managedAccess)
    {
        InputHandler playerInput = managedAccess.ManagedLocalPlayer.GetPlayerInput();
        PlayerData playerData = player.PlayerData.ValueRO;
        
        ThrusterSetup thrusterSetup = playerData.DefaultMovement;
        if (playerInput.IsADS)
        {
            thrusterSetup = playerData.ADSMovement;
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
