using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

partial class PlayerMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var (playerData, velocity, physicsMass, localToWorld, entity) in SystemAPI.Query<RefRO<PlayerData>, RefRW<PhysicsVelocity>, RefRW<PhysicsMass>, RefRO<LocalToWorld>>().WithEntityAccess())
        {
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(entity);
            velocity.ValueRW.Linear = playerData.ValueRO.MovementSpeed * math.mul(localToWorld.ValueRO.Rotation, managedAccess.ManagedLocalPlayer.GetPlayerInput().TargetDirection);
            
            managedAccess.ManagedLocalPlayer.GetCameraAimDirection(out Vector3 forward, out Vector3 up);
            quaternion forwardRotation = GetFromToRotation(localToWorld.ValueRO.Forward, forward);
            quaternion upRotation = GetFromToRotation(localToWorld.ValueRO.Up, up);
            quaternion finalRotation = math.mul(forwardRotation, upRotation);
            
            float3 angularVelocity = float3.zero;
            float rotationAngle = 2.0f * math.acos(finalRotation.value.w);
            if (rotationAngle != 0.0f)
            {
                float3 rotationAxis = math.normalize(finalRotation.value.xyz);
                float rotationSpeed = playerData.ValueRO.TurnSpeed * math.clamp(math.degrees(rotationAngle) / 90.0f, 0, 1);
                angularVelocity = rotationSpeed * SystemAPI.Time.DeltaTime * rotationAxis;
                
                quaternion inertiaOrientationInWorldSpace = math.mul(localToWorld.ValueRO.Rotation, physicsMass.ValueRO.InertiaOrientation);
                angularVelocity = math.rotate(math.inverse(inertiaOrientationInWorldSpace), angularVelocity);
            }
            velocity.ValueRW.Angular = angularVelocity;
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
