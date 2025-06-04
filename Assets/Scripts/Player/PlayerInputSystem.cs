using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct PlayerAspect : IAspect
{
    public readonly Entity Self;
    public readonly RefRO<PlayerData> PlayerData;
    public readonly RefRW<PhysicsVelocity> Velocity;
    public readonly RefRW<PhysicsMass> PhysicsMass;
    public readonly RefRO<LocalTransform> LocalToWorld;
    public readonly DynamicBuffer<ShipHardpointBufferElement> ShipHardpoints;
}

partial class PlayerInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (PlayerAspect player in SystemAPI.Query<PlayerAspect>())
        {
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(player.Self);

            foreach (ShipHardpointBufferElement shipHardpoint in player.ShipHardpoints)
            {
                if (SystemAPI.HasComponent<ProjectileSource>(shipHardpoint.Self))
                {
                    ProjectileSource ps = SystemAPI.GetComponent<ProjectileSource>(shipHardpoint.Self);
                    ps.IsFiring = managedAccess.ManagedLocalPlayer.GetPlayerInput().IsAttacking;
                    SystemAPI.SetComponent(shipHardpoint.Self, ps);
                }
                if (SystemAPI.HasComponent<BeamSource>(shipHardpoint.Self))
                {
                    BeamSource bs = SystemAPI.GetComponent<BeamSource>(shipHardpoint.Self);
                    bs.IsFiring = managedAccess.ManagedLocalPlayer.GetPlayerInput().IsAttacking;
                    SystemAPI.SetComponent(shipHardpoint.Self, bs);
                }
            }

            float3 targetVelocity = managedAccess.ManagedLocalPlayer.GetPlayerInput().TargetDirection;
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
            
            LocalTransform t = player.LocalToWorld.ValueRO;
            float3 currentVelocityLocal = t.InverseTransformDirection(player.Velocity.ValueRW.Linear);
            currentVelocityLocal += targetVelocity;
            currentVelocityLocal.z = math.clamp(currentVelocityLocal.z, -player.PlayerData.ValueRO.ReverseThrusters.MaximumVelocity, player.PlayerData.ValueRO.ForwardThrusters.MaximumVelocity);
            currentVelocityLocal.x = math.sign(currentVelocityLocal.x) * math.min(math.abs(currentVelocityLocal.x), player.PlayerData.ValueRO.LateralThrusters.MaximumVelocity);
            currentVelocityLocal.y = math.sign(currentVelocityLocal.y) * math.min(math.abs(currentVelocityLocal.y), player.PlayerData.ValueRO.LateralThrusters.MaximumVelocity);

            float currentSpeed = math.lengthsq(currentVelocityLocal); 
            if (currentSpeed > 0.0f && managedAccess.ManagedLocalPlayer.GetPlayerInput().VelocityDampersActive)
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
            
            player.Velocity.ValueRW.Linear = t.TransformDirection(currentVelocityLocal);
            
            managedAccess.ManagedLocalPlayer.GetCameraAimDirection(out Vector3 forward, out Vector3 up);
            quaternion forwardRotation = GetFromToRotation(t.Forward(), forward);
            quaternion upRotation = GetFromToRotation(t.Up(), up);
            quaternion finalRotation = math.mul(forwardRotation, upRotation);
            
            float3 angularVelocity = float3.zero;
            float rotationAngle = 2.0f * math.acos(finalRotation.value.w);
            if (rotationAngle != 0.0f)
            {
                float3 rotationAxis = math.normalize(finalRotation.value.xyz);
                float rotationSpeed = player.PlayerData.ValueRO.TurnSpeed * math.clamp(math.degrees(rotationAngle) / 90.0f, 0, 1);
                angularVelocity = rotationSpeed * SystemAPI.Time.DeltaTime * rotationAxis;
                
                quaternion inertiaOrientationInWorldSpace = math.mul(player.LocalToWorld.ValueRO.Rotation, player.PhysicsMass.ValueRO.InertiaOrientation);
                angularVelocity = math.rotate(math.inverse(inertiaOrientationInWorldSpace), angularVelocity);
            }
            player.Velocity.ValueRW.Angular = angularVelocity;
        }
    }

    private quaternion GetFromToRotation(float3 from , float3 to)
    {
        from = math.normalize(from);
        to = math.normalize(to);
        
        float angle = math.acos(math.clamp(math.dot(from, to), -1f, 1f));
        float3 axis = math.cross(from, to);
        return quaternion.AxisAngle(axis, angle);
    }
}
