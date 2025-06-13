
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Targetable : IComponentData
{
    // Add stealth / team stuff here eventually
}

public struct TargetDetector : IComponentData
{
    public float Range;
}

public struct DetectedTarget : IBufferElementData
{
    public Entity TargetableEntity;
}

[BurstCompile]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct TargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (td, targetBuffer, localToWorldSelf, entitySelf) in SystemAPI.Query<RefRO<TargetDetector>, DynamicBuffer<DetectedTarget>, RefRO<LocalToWorld>>().WithEntityAccess())
        {
            targetBuffer.Clear();
            foreach (var (targetable, localToWorldTarget, entityTarget) in SystemAPI.Query<RefRO<Targetable>, RefRO<LocalToWorld>>().WithEntityAccess())
            {
                if (entitySelf != entityTarget)
                {
                    float sqDist = math.lengthsq(localToWorldSelf.ValueRO.Position - localToWorldTarget.ValueRO.Position);
                    if (sqDist <= math.square(td.ValueRO.Range))
                    {
                        targetBuffer.Add(new DetectedTarget() { TargetableEntity = entityTarget });
                    }
                }
            }
        }
    }
}
