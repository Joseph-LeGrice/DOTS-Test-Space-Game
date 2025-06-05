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
    public ComponentLookup<ProjectileSourceConfiguration> m_sourceConfigLookup;
    [ReadOnly]
    public ComponentLookup<PhysicsVelocity> m_velocityLookup;
    public float ElapsedTime;
    
    private void Execute(ref ProjectileSource spawner, ref LocalToWorld localToWorld)
    {
        if (spawner.IsFiring && spawner.ProjectileWeaponEntity != Entity.Null && spawner.NextSpawnTime < ElapsedTime)
        {
            ProjectileSourceConfiguration pw = m_sourceConfigLookup[spawner.ProjectileWeaponEntity];

            Projectile p = new Projectile();
            p.FlatDamage = pw.ProjectileDamage;
            p.ImpactEffectEntity = pw.ImpactEffectPrefab;
            p.Velocity = pw.ProjectileSpeed * localToWorld.Forward;
            if (spawner.RelatedRigidbodyEntity != Entity.Null)
            {
                PhysicsVelocity pv = m_velocityLookup[spawner.RelatedRigidbodyEntity];
                p.Velocity += pv.Linear;
            }

            int sortKey = 0;
            Entity newEntity = m_ecbWriter.Instantiate(sortKey, pw.ProjectilePrefab);
            m_ecbWriter.AddComponent(sortKey, newEntity, p);
            m_ecbWriter.AddComponent(sortKey, newEntity, new MarkForCleanup(ElapsedTime + pw.ProjectileLifetime));
            m_ecbWriter.SetComponent(sortKey, newEntity, localToWorld);
            
            spawner.NextSpawnTime = ElapsedTime + pw.ProjectileSpawnRate;
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
            m_sourceConfigLookup = SystemAPI.GetComponentLookup<ProjectileSourceConfiguration>(),
            m_velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
            ElapsedTime = (float)SystemAPI.Time.ElapsedTime,
        }.ScheduleParallel();
    }
}
