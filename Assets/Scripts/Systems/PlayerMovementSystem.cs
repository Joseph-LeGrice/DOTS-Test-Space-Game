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
        foreach (var (playerData, velocity, physicsMass, localToWorld, entity) in SystemAPI.Query<RefRO<PlayerData>, RefRW<PhysicsVelocity>, RefRW<PhysicsMass>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(entity);
            LocalTransform t = localToWorld.ValueRO;

            float3 targetVelocity = managedAccess.ManagedLocalPlayer.GetPlayerInput().TargetDirection;
            if (targetVelocity.z > 0.0f)
            {
                targetVelocity.z *= playerData.ValueRO.ForwardThrusters.Acceleration;
            }
            else
            {
                targetVelocity.z *= playerData.ValueRO.ReverseThrusters.Acceleration;
            }
            targetVelocity.x *= playerData.ValueRO.LateralThrusters.Acceleration;
            targetVelocity.y *= playerData.ValueRO.LateralThrusters.Acceleration;
            
            float3 currentVelocityLocal = t.InverseTransformDirection(velocity.ValueRW.Linear);
            currentVelocityLocal += targetVelocity;
            currentVelocityLocal.z = math.clamp(currentVelocityLocal.z, -playerData.ValueRO.ReverseThrusters.MaximumVelocity, playerData.ValueRO.ForwardThrusters.MaximumVelocity);
            currentVelocityLocal.x = math.sign(currentVelocityLocal.x) * math.min(math.abs(currentVelocityLocal.x), playerData.ValueRO.LateralThrusters.MaximumVelocity);
            currentVelocityLocal.y = math.sign(currentVelocityLocal.y) * math.min(math.abs(currentVelocityLocal.y), playerData.ValueRO.LateralThrusters.MaximumVelocity);

            if (math.lengthsq(targetVelocity) == 0.0f && math.lengthsq(currentVelocityLocal) > 0.0f && managedAccess.ManagedLocalPlayer.GetPlayerInput().VelocityDampersActive)
            {
                currentVelocityLocal -= math.min(playerData.ValueRO.VelocityDamperDeceleration, math.length(currentVelocityLocal)) * math.normalize(currentVelocityLocal);
            }
            
            velocity.ValueRW.Linear = t.TransformDirection(currentVelocityLocal);
            
            managedAccess.ManagedLocalPlayer.GetCameraAimDirection(out Vector3 forward, out Vector3 up);
            quaternion forwardRotation = GetFromToRotation(t.Forward(), forward);
            quaternion upRotation = GetFromToRotation(t.Up(), up);
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
