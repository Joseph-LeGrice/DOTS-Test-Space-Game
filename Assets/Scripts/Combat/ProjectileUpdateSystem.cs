using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;

[BurstCompile]
public partial struct ProjectileUpdate : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter m_ecbWriter;
    [ReadOnly]
    public CollisionWorld m_physicsWorld;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<LocalToWorld> m_localToWorldLookup;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Damageable> m_damageableLookup;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<PhysicsVelocity> m_physicsVelocityLookup;
    [ReadOnly]
    public ComponentLookup<PhysicsMass> m_physicsMassLookup;
    
    private void Execute(Entity self, in Projectile projectile)
    {
        LocalToWorld localToWorld = m_localToWorldLookup[self];
        float3 moveDelta = projectile.Velocity * DeltaTime;
        localToWorld.Value = float4x4.TRS(
            localToWorld.Position + (projectile.Velocity * DeltaTime),
            quaternion.LookRotation(projectile.Velocity, new float3(0, 1, 0)),
            new float3(1.0f)
        );
        m_localToWorldLookup[self] = localToWorld;
        
        RaycastInput ri = new RaycastInput()
        {
            Start = localToWorld.Position - localToWorld.Forward * math.length(moveDelta),
            End = localToWorld.Position,
            Filter = PhysicsConfiguration.GetDamageDealerFilter()
        };
        
        if (m_physicsWorld.CastRay(ri, out RaycastHit hit) && m_damageableLookup.HasComponent(hit.Entity))
        {
            Damageable d = m_damageableLookup[hit.Entity];
            d.CurrentHealth -= projectile.FlatDamage;
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
            
            Entity impactFx = m_ecbWriter.Instantiate(0, projectile.ImpactEffectEntity);
            m_ecbWriter.SetComponent(0, impactFx, LocalTransform.FromPosition(hit.Position));
        }
    }
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct ProjectileUpdateSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        BeginSimulationEntityCommandBufferSystem.Singleton ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecbForDestroy = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        RefRW<PhysicsWorldSingleton> physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>();
        
        state.Dependency = new ProjectileUpdate()
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            m_ecbWriter = ecbForDestroy.AsParallelWriter(),
            m_physicsWorld = physicsWorld.ValueRO.CollisionWorld,
            m_localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(),
            m_damageableLookup = SystemAPI.GetComponentLookup<Damageable>(),
            m_physicsVelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
            m_physicsMassLookup = SystemAPI.GetComponentLookup<PhysicsMass>(),
        }.ScheduleParallel(state.Dependency);
    }
}
