using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;

[BurstCompile]
public partial struct ImpactDamageUpdate : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter m_ecbWriter;
    [ReadOnly] public CollisionWorld m_physicsWorld;
    public ComponentLookup<Damageable> m_damageableLookup;
    
    private void Execute(Entity self, in ImpactDamage impactDamage, in LocalToWorld localToWorld)
    {
        float distance = 5.0f;
        RaycastInput ri = new RaycastInput()
        {
            Start = localToWorld.Position - localToWorld.Forward * distance,
            End = localToWorld.Position,
            Filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0
            }
        };
        
        if (m_physicsWorld.CastRay(ri, out RaycastHit hit) && m_damageableLookup.HasComponent(hit.Entity))
        {
            Damageable d = m_damageableLookup[hit.Entity];
            d.CurrentHealth -= impactDamage.FlatDamage;
            
            m_damageableLookup[hit.Entity] = d;
            m_ecbWriter.DestroyEntity(0, self);
            
            Entity impactFx = m_ecbWriter.Instantiate(0, impactDamage.ImpactEffectEntity);
            m_ecbWriter.SetComponent(0, impactFx, LocalTransform.FromPosition(hit.Position));
        }
    }
}

[BurstCompile]
public partial struct DamageableUpdate : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter m_ecbWriter;
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
                    LocalTransform detachTransform = parentTransform.TransformTransform(LocalTransform.FromMatrix(dp.LocalTransform));
                    m_ecbWriter.SetComponent(0, detachInstance, detachTransform);
                }
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
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        
        new ImpactDamageUpdate()
        {
            m_ecbWriter = ecbForDestroy.AsParallelWriter(),
            m_physicsWorld = physicsWorld.CollisionWorld,
            m_damageableLookup = SystemAPI.GetComponentLookup<Damageable>(),
        }.Schedule();
        
        state.Dependency.Complete();
        
        new DamageableUpdate()
        {
            m_ecbWriter = ecbForDestroy.AsParallelWriter(),
            m_detachablePartLookup = SystemAPI.GetBufferLookup<DetachablePart>(),
        }.Schedule();
    }
}
