using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.VisualScripting;

public struct Gimbal : IComponentData
{
    public Entity GimbalEntity;
}

public partial struct ShipHardpointGimbalingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency.Complete();
        
        ComponentLookup<Gimbal> gimbalLookup = SystemAPI.GetComponentLookup<Gimbal>();
        ComponentLookup<LocalToWorld> localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        ComponentLookup<LocalTransform> localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>();
        ComponentLookup<ShipHardpointInstance> hardpointInstanceLookup = SystemAPI.GetComponentLookup<ShipHardpointInstance>();
        ComponentLookup<PhysicsVelocity> physicsVelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>();
        ComponentLookup<ProjectileSourceConfiguration> projectileSourceConfigLookup = SystemAPI.GetComponentLookup<ProjectileSourceConfiguration>();
        
        foreach (var (shipHardpointBuffer, targetBuffer, self) in SystemAPI.Query<DynamicBuffer<ShipHardpointReference>, DynamicBuffer<DetectedTarget>>().WithEntityAccess())
        {
            if (GetSelectedTarget(ref state, targetBuffer, out DetectedTarget selectedTarget))
            {
                LocalToWorld l2wTarget = localToWorldLookup[selectedTarget.TargetableEntity];
                
                for (int i = 0; i < shipHardpointBuffer.Length; i++)
                {
                    ref ShipHardpointReference hardpointReference = ref shipHardpointBuffer.ElementAt(i);
                    RefRW<ShipHardpointInstance> hardpoint = hardpointInstanceLookup.GetRefRW(hardpointReference.Self);
                    
                    LocalToWorld l2wHardpoint = localToWorldLookup[hardpoint.ValueRO.WeaponInstanceEntity];
                    float3 worldDir = l2wTarget.Position - l2wHardpoint.Position;

                    float3 aimAhead = new float3();
                    if (physicsVelocityLookup.HasComponent(self) && projectileSourceConfigLookup.HasComponent(hardpoint.ValueRO.WeaponInstanceEntity))
                    {
                        // todo: work on aim ahead
                        var config = projectileSourceConfigLookup[hardpoint.ValueRO.WeaponInstanceEntity];
                        
                        PhysicsVelocity velocitySelf = physicsVelocityLookup[self];
                        float likeness = math.dot(math.normalize(worldDir), velocitySelf.Linear);

                        float distanceToTarget = math.distance(l2wTarget.Position, localToWorldLookup[config.FireNode].Position);
                        float timeToTarget = distanceToTarget / (config.ProjectileSpeed + likeness);

                        float remove = math.sqrt(math.lengthsq(velocitySelf.Linear) - math.square(likeness));
                        float3 dir = math.cross(math.normalize(worldDir), math.cross(math.normalize(worldDir), math.normalizesafe(velocitySelf.Linear)));
                        aimAhead += remove * dir * timeToTarget;

                        // if (physicsVelocityLookup.HasComponent(selectedTarget.TargetableEntity))
                        // {
                        //     PhysicsVelocity velocityTarget = physicsVelocityLookup[selectedTarget.TargetableEntity];
                        //     aimAhead += math.normalizesafe(velocityTarget.Linear) * timeToTarget;
                        // }
                    }
                    
                    worldDir += aimAhead;
                    
                    float3 worldDirNormalised = math.normalize(worldDir);
                    float angle = math.degrees(math.acos(math.dot(l2wHardpoint.Forward, worldDirNormalised)));

                    float3 targetLocalForward = new float3(0.0f, 0.0f, 1.0f);
                    // if (angle < 45.0f)
                    {
                        targetLocalForward = l2wHardpoint.Value.InverseTransformDirection(worldDirNormalised);
                    }
                    hardpoint.ValueRW.TargetLocalForward = targetLocalForward;
                    hardpoint.ValueRW.AimDistance = math.length(worldDir);
                }
            }
            else
            {
                for (int i = 0; i < shipHardpointBuffer.Length; i++)
                {
                    ref ShipHardpointReference hardpointReference = ref shipHardpointBuffer.ElementAt(i);
                    RefRW<ShipHardpointInstance> hardpoint = hardpointInstanceLookup.GetRefRW(hardpointReference.Self);
                    
                    hardpoint.ValueRW.AimDistance = 500.0f;
                    hardpoint.ValueRW.TargetLocalForward = new float3(0.0f, 0.0f, 1.0f);
                }
            }
        }

        float adjustSpeed = math.radians(45.0f);
        foreach (var shipHardpointBuffer in SystemAPI.Query<DynamicBuffer<ShipHardpointReference>>())
        {
            foreach (ShipHardpointReference hardpointReference in shipHardpointBuffer)
            {
                ShipHardpointInstance hardpoint = hardpointInstanceLookup[hardpointReference.Self];
                if (gimbalLookup.HasComponent(hardpoint.WeaponInstanceEntity))
                {
                    Gimbal hardpointGimbal = gimbalLookup[hardpoint.WeaponInstanceEntity];
                    LocalTransform ltGimbal = localTransformLookup[hardpointGimbal.GimbalEntity];
                    
                    quaternion fromToRotation = PhysicsHelpers.GetFromToRotation(ltGimbal.Forward(), hardpoint.TargetLocalForward);
                    // fromToRotation.value.w = math.min(fromToRotation.value.w, adjustSpeed * SystemAPI.Time.DeltaTime);
                    float3 newForward = math.rotate(fromToRotation, ltGimbal.Forward());
                    // ltGimbal.Rotation = quaternion.LookRotation(newForward, new float3(0, 1, 0));
                    ltGimbal.Rotation = quaternion.LookRotation(hardpoint.TargetLocalForward, new float3(0, 1, 0));
                    
                    localTransformLookup[hardpointGimbal.GimbalEntity] = ltGimbal;
                }
            }
        }
    }

    private bool GetSelectedTarget(ref SystemState state, DynamicBuffer<DetectedTarget> targetBuffer, out DetectedTarget selectedTarget)
    {
        foreach (DetectedTarget target in targetBuffer)
        {
            if (target.IsSelected && state.EntityManager.Exists(target.TargetableEntity))
            {
                selectedTarget = target;
                return true;
            }
        }

        selectedTarget = default;
        return false;
    }
}
