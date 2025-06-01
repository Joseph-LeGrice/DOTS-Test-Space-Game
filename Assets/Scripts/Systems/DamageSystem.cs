using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using RaycastHit = Unity.Physics.RaycastHit;

[BurstCompile]
public partial struct ImpactDamageUpdate : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter m_entityCommandBuffer;
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
            m_entityCommandBuffer.DestroyEntity(0, self);
            
            Entity impactFx = m_entityCommandBuffer.Instantiate(0, impactDamage.ImpactEffectEntity);
            m_entityCommandBuffer.SetComponent(0, impactFx, LocalTransform.FromPosition(hit.Position));
        }
    }
}

[BurstCompile]
public partial struct DamageableUpdate : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter m_ecbWriterForEntityDestruction;
    public EntityCommandBuffer.ParallelWriter m_ecbWriterForDetach;
    [ReadOnly]
    public BufferLookup<DetachablePart> m_detachablePartsLookup;
    [ReadOnly]
    public ComponentLookup<LocalTransform> m_localTransformLookup;
    // public ComponentLookup<PhysicsVelocity> m_physicsVelocityLookup;
    public Random m_random;
    
    private void Execute(Entity self, in Damageable d)
    {
        if (d.CurrentHealth < 0.0f)
        {
            m_ecbWriterForEntityDestruction.DestroyEntity(2, self); // also removes children as parent system not updated
            
            if (m_detachablePartsLookup.HasBuffer(self))
            {
                foreach (DetachablePart detachablePart in m_detachablePartsLookup[self])
                {
                    m_ecbWriterForDetach.RemoveComponent<Parent>(0, detachablePart.DetachableEntity);
                    // https://docs.unity3d.com/Packages/com.unity.entities@1.3/manual/transforms-comparison.html
                    
                    LocalTransform localTransform = m_localTransformLookup[detachablePart.DetachableEntity];

                    if (detachablePart.EffectPrefab != Entity.Null)
                    {
                        Entity effect = m_ecbWriterForEntityDestruction.Instantiate(0, detachablePart.EffectPrefab);
                        m_ecbWriterForDetach.SetComponent(0, effect, LocalTransform.FromPosition(localTransform.Position));
                    }
                    
                    // m_ecbWriterForDetach.AddComponent<PhysicsVelocity>(1, detachablePart.DetachableEntity);
                    // m_ecbWriterForDetach.SetComponent(0, detachablePart.DetachableEntity, new PhysicsVelocity()
                    // {
                    //     Linear = localTransform.Forward() * m_random.NextFloat(detachablePart.ImpulseForceMinimum, detachablePart.ImpulseForceMaximum),
                    //     Angular = m_random.NextFloat(detachablePart.AngularForceMinimum, detachablePart.AngularForceMaximum),
                    // });
                    
                    // PhysicsVelocity pv = m_physicsVelocityLookup[detachablePart.DetachableEntity];
                    // pv.Linear = localTransform.Forward() * m_random.NextFloat(detachablePart.ImpulseForceMinimum, detachablePart.ImpulseForceMaximum);
                    // pv.Angular = m_random.NextFloat(detachablePart.AngularForceMinimum, detachablePart.AngularForceMaximum);
                    // m_physicsVelocityLookup[detachablePart.DetachableEntity] = pv;
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
            m_entityCommandBuffer= ecbForDestroy.AsParallelWriter(),
            m_physicsWorld = physicsWorld.CollisionWorld,
            m_damageableLookup = SystemAPI.GetComponentLookup<Damageable>(),
        }.Schedule();

        state.Dependency.Complete();
        
        foreach (var (d, self) in SystemAPI.Query<RefRO<Damageable>>().WithEntityAccess())
        {
            if (d.ValueRO.CurrentHealth < 0.0f)
            {
                ecbForDestroy.DestroyEntity(self);
            }
        }
        
        // new DamageableUpdate()
        // {
        //     m_ecbWriterForEntityDestruction = ecbForDestroy.AsParallelWriter(),
        //     m_ecbWriterForDetach = ecbForDetach.AsParallelWriter(),
        //     m_detachablePartsLookup = SystemAPI.GetBufferLookup<DetachablePart>(),
        //     m_localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
        //     // m_physicsVelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
        //     m_random = new Random((uint)SystemAPI.Time.ElapsedTime + 100),
        // }.Schedule();
        //
        // state.Dependency.Complete();
        //
        // ecbForDetach.Playback(state.EntityManager);
        // ecbForDetach.Dispose();
    }
}
