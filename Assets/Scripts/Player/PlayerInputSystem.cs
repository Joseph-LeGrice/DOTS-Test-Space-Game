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
    public readonly RefRW<PhysicsVelocity> Velocity;
    public readonly RefRW<PhysicsMass> PhysicsMass;
    public readonly RefRO<LocalToWorld> LocalToWorld;
    public readonly DynamicBuffer<ShipHardpointBufferElement> ShipHardpoints;
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

            float angularVelocityModifier = 1.0f;
            float linearVelocityModifier = 1.0f;
            if (GetAttachedMass(player.Self, out PhysicsMass attachedPhysicsMass))
            {
                angularVelocityModifier = math.pow(attachedPhysicsMass.InverseMass, 0.1f);
                linearVelocityModifier = math.pow(attachedPhysicsMass.InverseMass, 0.1f);
            }

            float3 linearVelocity = GetLinearVelocity(player, managedAccess);
            player.Velocity.ValueRW.Linear = linearVelocityModifier * linearVelocity;

            float3 angularVelocity = GetAngularVelocity(player, managedAccess);
            player.Velocity.ValueRW.Angular = angularVelocityModifier * angularVelocity;
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

        ThrusterSetup thrusterSetup = playerData.DefaultMovement;
        if (playerInput.IsADS)
        {
            thrusterSetup = playerData.ADSMovement;
        }
        
        float3 acceleration = playerInput.TargetDirection;
        if (acceleration.z > 0.0f)
        {
            acceleration.z *= thrusterSetup.ForwardThrustersAcceleration;
        }
        else
        {
            acceleration.z *= thrusterSetup.ReverseThrustersAcceleration;
        }
        
        acceleration.x *= thrusterSetup.LateralThrustersAcceleration;
        acceleration.y *= thrusterSetup.LateralThrustersAcceleration;
        
        LocalToWorld localToWorld = player.LocalToWorld.ValueRO;
        float3 currentVelocityLocal = localToWorld.Value.InverseTransformDirection(player.Velocity.ValueRO.Linear);
        currentVelocityLocal += acceleration * SystemAPI.Time.DeltaTime;
        currentVelocityLocal = math.normalizesafe(currentVelocityLocal) * math.min(playerData.MaximumVelocity, math.length(currentVelocityLocal));
        
        if (!playerInput.IsADS)
        {
            currentVelocityLocal = DampVelocity(currentVelocityLocal, acceleration, playerData.VelocityDamperDecelerationDefault);
        }
        else
        {
            currentVelocityLocal = DampVelocity(currentVelocityLocal, acceleration, playerData.VelocityDamperDecelerationADS);
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
        LocalToWorld localToWorld = player.LocalToWorld.ValueRO;
        PlayerData playerData = player.PlayerData.ValueRO;
        
        float3 angularVelocity = float3.zero;
        
        float lookSensitivity = managedAccess.ManagedLocalPlayer.GetLookSensitivity();
        
        float yawAngleVelocity = lookSensitivity * playerInput.LookDelta.x;
        yawAngleVelocity = math.sign(yawAngleVelocity) * math.min(math.abs(yawAngleVelocity), playerData.MaxTurnSpeed);
        yawAngleVelocity = math.radians(yawAngleVelocity);  
        quaternion yawRotation = quaternion.AxisAngle(localToWorld.Up, yawAngleVelocity);
            
        float pitchAngleVelocity = lookSensitivity * playerInput.LookDelta.y;
        pitchAngleVelocity = math.sign(pitchAngleVelocity) * math.min(math.abs(pitchAngleVelocity), playerData.MaxTurnSpeed);
        pitchAngleVelocity = math.radians(pitchAngleVelocity);
        quaternion pitchRotation = quaternion.AxisAngle(localToWorld.Right, pitchAngleVelocity);
            
        quaternion pitchYawRotation = math.mul(pitchRotation, yawRotation);
        angularVelocity += 2.0f * math.acos(pitchYawRotation.value.w) * pitchYawRotation.value.xyz;

        float rollAngleVelocity = playerData.MaxRollSpeed * playerInput.RollDirection;
        rollAngleVelocity = math.radians(rollAngleVelocity);
            
        quaternion rollRotation = quaternion.AxisAngle(localToWorld.Forward, rollAngleVelocity);
        angularVelocity += 2.0f * math.acos(rollRotation.value.w) * rollRotation.value.xyz;
            
        quaternion inertiaOrientationInWorldSpace = math.mul(localToWorld.Rotation, player.PhysicsMass.ValueRO.InertiaOrientation);
        return math.rotate(math.inverse(inertiaOrientationInWorldSpace), angularVelocity);
    }
}
