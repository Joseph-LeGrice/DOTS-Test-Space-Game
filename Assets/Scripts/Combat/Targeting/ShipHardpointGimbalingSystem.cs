using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
        foreach (var (shipHardpointBuffer, targetBuffer) in SystemAPI.Query<DynamicBuffer<ShipHardpointBufferElement>, DynamicBuffer<DetectedTarget>>())
        {
            if (GetSelectedTarget(ref state, targetBuffer, out DetectedTarget selectedTarget))
            {
                LocalToWorld l2wTarget = localToWorldLookup[selectedTarget.TargetableEntity];
                
                for (int i = 0; i < shipHardpointBuffer.Length; i++)
                {
                    ref ShipHardpointBufferElement hardpoint = ref shipHardpointBuffer.ElementAt(i);
                    LocalToWorld l2wHardpoint = localToWorldLookup[hardpoint.Self];
                    float3 worldDir = l2wTarget.Position - l2wHardpoint.Position;
                    float3 worldDirNormalised = math.normalize(worldDir);
                    float angle = math.degrees(math.acos(math.dot(l2wHardpoint.Forward, worldDirNormalised)));

                    float3 targetLocalForward = new float3(0.0f, 0.0f, 1.0f);
                    if (angle < 45.0f)
                    {
                        targetLocalForward = l2wHardpoint.Value.InverseTransformDirection(worldDirNormalised);
                    }
                    hardpoint.TargetLocalForward = targetLocalForward;
                    hardpoint.AimDistance = math.length(worldDir);
                }
            }
            else
            {
                for (int i = 0; i < shipHardpointBuffer.Length; i++)
                {
                    ref ShipHardpointBufferElement hardpoint = ref shipHardpointBuffer.ElementAt(i);
                    hardpoint.AimDistance = 500.0f;
                    hardpoint.TargetLocalForward = new float3(0.0f, 0.0f, 1.0f);
                }
            }
        }

        float adjustSpeed = math.radians(45.0f);
        foreach (var shipHardpointBuffer in SystemAPI.Query<DynamicBuffer<ShipHardpointBufferElement>>())
        {
            foreach (var hardpoint in shipHardpointBuffer)
            {
                if (gimbalLookup.HasComponent(hardpoint.Self))
                {
                    Gimbal hardpointGimbal = gimbalLookup[hardpoint.Self];
                    LocalTransform ltGimbal = localTransformLookup[hardpointGimbal.GimbalEntity];
                    
                    quaternion fromToRotation = PhysicsHelpers.GetFromToRotation(ltGimbal.Forward(), hardpoint.TargetLocalForward);
                    // fromToRotation.value.w = math.min(fromToRotation.value.w, adjustSpeed * SystemAPI.Time.DeltaTime);
                    float3 newForward = math.rotate(fromToRotation, ltGimbal.Forward());
                    ltGimbal.Rotation = quaternion.LookRotation(newForward, new float3(0, 1, 0));
                    
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
