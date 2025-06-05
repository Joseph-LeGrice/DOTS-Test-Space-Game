using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Damageable : IComponentData
{
    public float CurrentHealth;
    public float MaxHealth;
    public Entity SpawnOnDestroy;

    public static Damageable WithHealth(float health)
    {
        return new Damageable()
        {
            CurrentHealth = health,
            MaxHealth = health,
        };
    }
}

public struct DetachablePart : IBufferElementData
{
    public Entity EffectPrefab;
    public Entity DetachableEntityPrefab;
    public float4x4 LocalTransform;
}

[BurstCompile]
public partial struct DamageableUpdate : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter m_ecbWriter;
    [ReadOnly]
    public BufferLookup<DetachablePart> m_detachablePartLookup;
    
    private void Execute(Entity self, in Damageable d, in LocalTransform parentTransform)
    {
        if (d.CurrentHealth < 0.0f)
        {
            m_ecbWriter.DestroyEntity(2, self);
            
            if (m_detachablePartLookup.HasBuffer(self))
            {
                DynamicBuffer<DetachablePart> detachableParts = m_detachablePartLookup[self];
                foreach (DetachablePart dp in detachableParts)
                {
                    Entity detachInstance = m_ecbWriter.Instantiate(0, dp.DetachableEntityPrefab);
                    m_ecbWriter.AddComponent(0, detachInstance, new QueueForCleanup(5.0f));
                    LocalTransform detachTransform = LocalTransform.FromMatrix(dp.LocalTransform);
                    detachTransform = detachTransform.TransformTransform(parentTransform);
                    m_ecbWriter.SetComponent(0, detachInstance, detachTransform);
                }
            }

            if (d.SpawnOnDestroy != Entity.Null)
            {
                Entity spawnedOnDestroy = m_ecbWriter.Instantiate(0, d.SpawnOnDestroy);
                m_ecbWriter.SetComponent(0, spawnedOnDestroy, parentTransform);
            }
        }
    }
}

[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct DamageSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        BeginSimulationEntityCommandBufferSystem.Singleton ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecbForDestroy = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        
        new DamageableUpdate()
        {
            m_ecbWriter = ecbForDestroy.AsParallelWriter(),
            m_detachablePartLookup = SystemAPI.GetBufferLookup<DetachablePart>(),
        }.ScheduleParallel();
    }
}
