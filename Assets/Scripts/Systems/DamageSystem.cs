using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
public partial struct ImpactDamageUpdate : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter m_entityCommandBuffer;
    [ReadOnly] public CollisionWorld m_physicsWorld;
    public ComponentLookup<Damageable> m_damageableLookup;
    
    private void Execute(Entity self, ref ImpactDamage impactDamage, ref LocalToWorld localToWorld)
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
            m_entityCommandBuffer.DestroyEntity(0, self);
            
            Entity impactFx = m_entityCommandBuffer.Instantiate(0, impactDamage.ImpactEffectEntity);
            m_entityCommandBuffer.SetComponent(0, impactFx, LocalTransform.FromPosition(hit.Position));
        }
    }
}

public partial struct DamageSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        BeginSimulationEntityCommandBufferSystem.Singleton ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        ComponentLookup<Damageable> damageableLookup = SystemAPI.GetComponentLookup<Damageable>();
        
        new ImpactDamageUpdate()
        {
            m_entityCommandBuffer= ecb.AsParallelWriter(),
            m_physicsWorld = physicsWorld.CollisionWorld,
            m_damageableLookup = damageableLookup
        }.Schedule();
        
        state.Dependency.Complete();
        
        foreach (var (d, e) in SystemAPI.Query<RefRO<Damageable>>().WithEntityAccess())
        {
            if (d.ValueRO.CurrentHealth < 0.0f)
            {
                ecb.DestroyEntity(e);
            }
        }
    }
}
