
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Targetable : IComponentData
{
}

public struct TargetDetector : IComponentData
{
    public float RangeSquared;
}

public struct DetectedTarget : IBufferElementData
{
    public Entity TargetableEntity;
    public bool IsSelected;
    public bool CanTargetAhead;
}

public static class TargetHelpers
{
    public static bool GetSelectedTarget(this DynamicBuffer<DetectedTarget> targetBuffer, ref SystemState state, out DetectedTarget selectedTarget)
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

[BurstCompile]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct TargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ComponentLookup<LocalToWorld> localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        foreach (var (td, targetBuffer, localToWorldSelf, entitySelf) in SystemAPI.Query<RefRO<TargetDetector>, DynamicBuffer<DetectedTarget>, RefRO<LocalToWorld>>().WithEntityAccess())
        {
            int numElements = targetBuffer.Length;
            for (int i = 0; i < numElements; i++)
            {
                bool shouldRemove = false;
                if (!state.EntityManager.Exists(targetBuffer[i].TargetableEntity))
                {
                    shouldRemove = true;
                }
                else
                {
                    var l2w = localToWorldLookup[targetBuffer[i].TargetableEntity];
                    float distSq = math.lengthsq(l2w.Position - localToWorldSelf.ValueRO.Position);
                    if (distSq > td.ValueRO.RangeSquared)
                    {
                        shouldRemove = true;
                    }
                }

                if (shouldRemove)
                {
                    targetBuffer.RemoveAtSwapBack(i);
                    i--;
                    numElements--;
                }
            }
            
            foreach (var (targetable, localToWorldTarget, entityTarget) in SystemAPI.Query<RefRO<Targetable>, RefRO<LocalToWorld>>().WithEntityAccess())
            {
                if (entitySelf != entityTarget)
                {
                    float sqDist = math.lengthsq(localToWorldSelf.ValueRO.Position - localToWorldTarget.ValueRO.Position);
                    if (sqDist <= td.ValueRO.RangeSquared)
                    {
                        bool canAdd = true;
                        
                        foreach (DetectedTarget currentTargets in targetBuffer)
                        {
                            if (currentTargets.TargetableEntity == entityTarget)
                            {
                                canAdd = false;
                            }
                        }

                        if (canAdd)
                        {
                            targetBuffer.Add(new DetectedTarget() { TargetableEntity = entityTarget });
                        }
                    }
                }
            }
        }
    }
}
