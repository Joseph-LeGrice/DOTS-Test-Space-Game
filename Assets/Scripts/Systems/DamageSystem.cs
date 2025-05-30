using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

// https://docs.unity3d.com/Packages/com.unity.physics@1.4/manual/ecs-packages.html

[BurstCompile]
public partial struct ImpactDamageUpdate : IJobEntity
{
    [ReadOnly] public CollisionWorld m_physicsWorld;
    public ComponentLookup<Damageable> m_damageableLookup;
    
    private void Execute(ref ImpactDamage impactDamage, ref LocalToWorld localToWorld)
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
        }
    }
}

public partial struct DamageSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        ComponentLookup<Damageable> damageableLookup = SystemAPI.GetComponentLookup<Damageable>();
        
        new ImpactDamageUpdate()
        {
            m_physicsWorld = physicsWorld.CollisionWorld,
            m_damageableLookup = damageableLookup
        }.Schedule();
        
        state.Dependency.Complete();
        
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (d, e) in SystemAPI.Query<RefRO<Damageable>>().WithEntityAccess())
        {
            if (d.ValueRO.CurrentHealth < 0.0f)
            {
                ecb.DestroyEntity(e);
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
