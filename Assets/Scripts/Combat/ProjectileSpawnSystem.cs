using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[WriteGroup(typeof(LocalToWorld))]
public struct Projectile : IComponentData
{
    public float3 Velocity;
    public float FlatDamage;
    public Entity ImpactEffectEntity;
}

[BurstCompile]
public partial struct CreateProjectileJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter m_ecbWriter;
    [ReadOnly]
    public ComponentLookup<PhysicsVelocity> m_velocityLookup;
    [ReadOnly]
    public ComponentLookup<LocalToWorld> m_localToWorldLookup;
    [ReadOnly]
    public ComponentLookup<ShipHardpointInstance> m_hardpointLookup;
    public float ElapsedTime;
    
    private void Execute(ref ProjectileSourceData spawner, ref ProjectileSourceConfiguration config)
    {
        var hardpoint = m_hardpointLookup[spawner.RelatedHardpoint];
        if (hardpoint.IsFiring && hardpoint.WeaponInstanceEntity != Entity.Null && spawner.NextSpawnTime < ElapsedTime)
        {
            LocalToWorld l2wFireNode = m_localToWorldLookup[config.FireNode];
            
            Projectile p = new Projectile();
            p.FlatDamage = config.ProjectileDamage;
            p.ImpactEffectEntity = config.ImpactEffectPrefab;
            p.Velocity = config.ProjectileSpeed * l2wFireNode.Forward;
            if (hardpoint.RelatedRigidbodyEntity != Entity.Null)
            {
                PhysicsVelocity pv = m_velocityLookup[hardpoint.RelatedRigidbodyEntity];
                p.Velocity += pv.Linear;
            }

            int sortKey = 0;
            Entity newEntity = m_ecbWriter.Instantiate(sortKey, config.ProjectilePrefab);
            m_ecbWriter.AddComponent(sortKey, newEntity, p);
            m_ecbWriter.AddComponent(sortKey, newEntity, new MarkForCleanup(ElapsedTime + config.ProjectileLifetime));
            m_ecbWriter.SetComponent(sortKey, newEntity, new LocalToWorld() { Value = float4x4.TRS(l2wFireNode.Position, l2wFireNode.Rotation, new float3(1.0f))});
            
            spawner.NextSpawnTime = ElapsedTime + config.ProjectileSpawnRate;
        }
    }
}

partial struct ProjectileSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        new CreateProjectileJob()
        {
            m_ecbWriter = ecb.AsParallelWriter(),
            m_hardpointLookup = SystemAPI.GetComponentLookup<ShipHardpointInstance>(),
            m_velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
            m_localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(),
            ElapsedTime = (float)SystemAPI.Time.ElapsedTime,
        }.ScheduleParallel();
    }
}
