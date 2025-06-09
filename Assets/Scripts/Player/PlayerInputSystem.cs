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

            float3 targetVelocity = playerInput.TargetDirection;
            if (targetVelocity.z > 0.0f)
            {
                targetVelocity.z *= player.PlayerData.ValueRO.ForwardThrusters.Acceleration;
            }
            else
            {
                targetVelocity.z *= player.PlayerData.ValueRO.ReverseThrusters.Acceleration;
            }
            targetVelocity.x *= player.PlayerData.ValueRO.LateralThrusters.Acceleration;
            targetVelocity.y *= player.PlayerData.ValueRO.LateralThrusters.Acceleration;

            targetVelocity = linearVelocityModifier * targetVelocity;
            
            LocalToWorld localToWorld = player.LocalToWorld.ValueRO;
            float3 currentVelocityLocal = localToWorld.Value.InverseTransformDirection(player.Velocity.ValueRW.Linear);
            currentVelocityLocal += targetVelocity;
            currentVelocityLocal.z = math.clamp(currentVelocityLocal.z, -player.PlayerData.ValueRO.ReverseThrusters.MaximumVelocity, player.PlayerData.ValueRO.ForwardThrusters.MaximumVelocity);
            currentVelocityLocal.x = math.sign(currentVelocityLocal.x) * math.min(math.abs(currentVelocityLocal.x), player.PlayerData.ValueRO.LateralThrusters.MaximumVelocity);
            currentVelocityLocal.y = math.sign(currentVelocityLocal.y) * math.min(math.abs(currentVelocityLocal.y), player.PlayerData.ValueRO.LateralThrusters.MaximumVelocity);

            float currentSpeed = math.lengthsq(currentVelocityLocal); 
            if (currentSpeed > 0.0f && playerInput.ADS)
            {
                float threshold = 0.001f;
                if (math.abs(targetVelocity.x) < threshold)
                {
                    float absX = math.abs(currentVelocityLocal.x);
                    currentVelocityLocal.x = math.sign(currentVelocityLocal.x) * math.max(absX - player.PlayerData.ValueRO.VelocityDamperDeceleration * SystemAPI.Time.DeltaTime, 0.0f);
                }
                if (math.abs(targetVelocity.y) < threshold)
                {
                    float absY = math.abs(currentVelocityLocal.y);
                    currentVelocityLocal.y = math.sign(currentVelocityLocal.y) * math.max(absY - player.PlayerData.ValueRO.VelocityDamperDeceleration * SystemAPI.Time.DeltaTime, 0.0f);
                }
                if (math.abs(targetVelocity.z) < threshold)
                {
                    float absZ = math.abs(currentVelocityLocal.z);
                    currentVelocityLocal.z = math.sign(currentVelocityLocal.z) * math.max(absZ - player.PlayerData.ValueRO.VelocityDamperDeceleration * SystemAPI.Time.DeltaTime, 0.0f);
                }
            }
            
            player.Velocity.ValueRW.Linear = localToWorld.Value.TransformDirection(currentVelocityLocal);

            float3 angularVelocity = float3.zero;
            
            float lookSensitivity = managedAccess.ManagedLocalPlayer.GetLookSensitivity();

            float yawAngleVelocity = lookSensitivity * playerInput.LookDelta.x;
            yawAngleVelocity = math.sign(yawAngleVelocity) * math.min(math.abs(yawAngleVelocity), player.PlayerData.ValueRO.MaxTurnSpeed);
            yawAngleVelocity = math.radians(yawAngleVelocity);  
            quaternion yawRotation = quaternion.AxisAngle(localToWorld.Up, yawAngleVelocity);
            
            float pitchAngleVelocity = lookSensitivity * playerInput.LookDelta.y;
            pitchAngleVelocity = math.sign(pitchAngleVelocity) * math.min(math.abs(pitchAngleVelocity), player.PlayerData.ValueRO.MaxTurnSpeed);
            pitchAngleVelocity = math.radians(pitchAngleVelocity);
            quaternion pitchRotation = quaternion.AxisAngle(localToWorld.Right, pitchAngleVelocity);
            
            quaternion pitchYawRotation = math.mul(pitchRotation, yawRotation);
            angularVelocity += 2.0f * math.acos(pitchYawRotation.value.w) * pitchYawRotation.value.xyz;

            float rollAngleVelocity = player.PlayerData.ValueRO.MaxRollSpeed * playerInput.RollDirection;
            rollAngleVelocity = math.radians(rollAngleVelocity);
            
            quaternion rollRotation = quaternion.AxisAngle(localToWorld.Forward, rollAngleVelocity);
            angularVelocity += 2.0f * math.acos(rollRotation.value.w) * rollRotation.value.xyz;
            
            quaternion inertiaOrientationInWorldSpace = math.mul(localToWorld.Rotation, player.PhysicsMass.ValueRO.InertiaOrientation);
            angularVelocity = math.rotate(math.inverse(inertiaOrientationInWorldSpace), angularVelocity);
            
            player.Velocity.ValueRW.Angular = angularVelocity;
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
}
