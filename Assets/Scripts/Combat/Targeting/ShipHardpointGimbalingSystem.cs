using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
                    float3 worldDir = math.normalize(l2wTarget.Position - l2wHardpoint.Position);
                    float angle = math.radians(math.acos(math.dot(l2wHardpoint.Forward, worldDir)));

                    float3 targetLocalForward = new float3(0.0f, 0.0f, 1.0f);
                    if (angle < 30.0f)
                    {
                        LocalTransform ltTarget = localTransformLookup[hardpoint.Self];
                        targetLocalForward = ltTarget.TransformDirection(worldDir);
                    }
                    hardpoint.TargetLocalForward = targetLocalForward;
                }
            }
            else
            {
                for (int i = 0; i < shipHardpointBuffer.Length; i++)
                {
                    shipHardpointBuffer.ElementAt(i).TargetLocalForward = new float3(0.0f, 0.0f, 1.0f);
                }
            }
        }

        float adjustSpeed = math.radians(360.0f);
        foreach (var shipHardpointBuffer in SystemAPI.Query<DynamicBuffer<ShipHardpointBufferElement>>())
        {
            foreach (var hardpoint in shipHardpointBuffer)
            {
                if (gimbalLookup.HasComponent(hardpoint.Self))
                {
                    Gimbal hardpointGimbal = gimbalLookup[hardpoint.Self];
                    LocalTransform ltHardpoint = localTransformLookup[hardpointGimbal.GimbalEntity];
                    // quaternion fromToRotation = PhysicsHelpers.GetFromToRotation(ltHardpoint.Forward(), hardpoint.TargetLocalForward);
                    // float3 rotationAxis = math.normalize(fromToRotation.value.xyz);
                    // quaternion rotation = quaternion.AxisAngle(rotationAxis, math.min(adjustSpeed * SystemAPI.Time.DeltaTime, fromToRotation.value.w));
                    // float3 newForward = math.mul(rotation, ltHardpoint.Forward());
                    // ltHardpoint.Rotation = quaternion.LookRotation(newForward, new float3(0, 1, 0));
                    ltHardpoint.Rotation = quaternion.LookRotation(hardpoint.TargetLocalForward, new float3(0, 1, 0));
                    localTransformLookup[hardpointGimbal.GimbalEntity] = ltHardpoint;
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
