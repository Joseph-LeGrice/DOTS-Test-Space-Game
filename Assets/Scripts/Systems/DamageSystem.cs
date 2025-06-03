using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[BurstCompile]
public partial struct ImpactDamageUpdate : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter m_ecbWriter;
    [ReadOnly] public CollisionWorld m_physicsWorld;
    [ReadOnly] public ComponentLookup<LocalToWorld> m_localToWorldLookup;
    public ComponentLookup<Damageable> m_damageableLookup;
    public ComponentLookup<PhysicsVelocity> m_physicsVelocityLookup;
    [ReadOnly] public ComponentLookup<PhysicsMass> m_physicsMassLookup;
    
    private void Execute(Entity self, in ImpactDamage impactDamage, in LocalToWorld localToWorld)
    {
        float distance = 5.0f;
        RaycastInput ri = new RaycastInput()
        {
            Start = localToWorld.Position - localToWorld.Forward * distance,
            End = localToWorld.Position,
            Filter = PhysicsConfiguration.GetDamageDealerFilter()
        };
        
        if (m_physicsWorld.CastRay(ri, out RaycastHit hit) && m_damageableLookup.HasComponent(hit.Entity))
        {
            Damageable d = m_damageableLookup[hit.Entity];
            d.CurrentHealth -= impactDamage.FlatDamage;
            m_damageableLookup[hit.Entity] = d;

            if (m_physicsVelocityLookup.HasComponent(hit.Entity) && m_physicsMassLookup.HasComponent(hit.Entity))
            {
                LocalToWorld hitLocalToWorld = m_localToWorldLookup[hit.Entity];
                float impulseForce = 4.0f;
                PhysicsVelocity velocity = m_physicsVelocityLookup[hit.Entity];
                velocity.ApplyImpulse(
                    m_physicsMassLookup[hit.Entity],
                    hitLocalToWorld.Position,
                    hitLocalToWorld.Rotation,
                    impulseForce * localToWorld.Forward,
                    hit.Position
                );
                m_physicsVelocityLookup[hit.Entity] = velocity;
            }
            
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
                    LocalTransform detachTransform = LocalTransform.FromMatrix(dp.LocalTransform);
                    detachTransform = detachTransform.TransformTransform(parentTransform);
                    m_ecbWriter.SetComponent(0, detachInstance, detachTransform);
                }
            }

            if (d.SpawnOnDestroy != Entity.Null)
            {
                Entity spawnedonDestroy = m_ecbWriter.Instantiate(0, d.SpawnOnDestroy);
                m_ecbWriter.SetComponent(0, spawnedonDestroy, parentTransform);
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
        RefRW<PhysicsWorldSingleton> physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>();
        
        new ImpactDamageUpdate()
        {
            m_ecbWriter = ecbForDestroy.AsParallelWriter(),
            m_physicsWorld = physicsWorld.ValueRO.CollisionWorld,
            m_localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(),
            m_damageableLookup = SystemAPI.GetComponentLookup<Damageable>(),
            m_physicsVelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
            m_physicsMassLookup = SystemAPI.GetComponentLookup<PhysicsMass>(),
        }.Schedule();
        
        state.Dependency.Complete();
        
        new DamageableUpdate()
        {
            m_ecbWriter = ecbForDestroy.AsParallelWriter(),
            m_detachablePartLookup = SystemAPI.GetBufferLookup<DetachablePart>(),
        }.Schedule();
    }
}
