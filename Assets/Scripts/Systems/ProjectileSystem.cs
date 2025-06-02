using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
public partial struct CreateProjectileJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter m_ecbWriter;
    [ReadOnly]
    public ComponentLookup<ProjectileSourceConfiguration> m_sourceConfigLookup;
    [ReadOnly]
    public ComponentLookup<PhysicsVelocity> m_velocityLookup;
    public float ElapsedTime;
    
    private void Execute([ChunkIndexInQuery] int chunkIndex, ref ProjectileSource spawner, ref LocalToWorld localToWorld)
    {
        if (spawner.IsFiring && spawner.ProjectileWeaponEntity != Entity.Null && spawner.NextSpawnTime < ElapsedTime)
        {
            ProjectileSourceConfiguration pw = m_sourceConfigLookup[spawner.ProjectileWeaponEntity];

            float3 projectileVelocity = pw.ProjectileSpeed * localToWorld.Forward;
            if (spawner.RelatedRigidbodyEntity != Entity.Null)
            {
                PhysicsVelocity pv = m_velocityLookup[spawner.RelatedRigidbodyEntity];
                projectileVelocity += pv.Linear;
            }
            
            Entity newEntity = m_ecbWriter.Instantiate(chunkIndex, pw.ProjectilePrefab);
            m_ecbWriter.SetComponent(chunkIndex, newEntity, LocalTransform.FromPositionRotation(localToWorld.Position, localToWorld.Rotation));
            m_ecbWriter.SetComponent(chunkIndex, newEntity, new Projectile() { Velocity = projectileVelocity});
            m_ecbWriter.SetComponent(chunkIndex, newEntity, new MarkForCleanup(ElapsedTime + pw.ProjectileLifetime));
                
            spawner.NextSpawnTime = ElapsedTime + pw.ProjectileSpawnRate;
        }
    }
}

[BurstCompile]
public partial struct ProjectileMoveJob : IJobEntity
{
    public float DeltaTime;
    
    private void Execute(ref Projectile projectile, ref LocalTransform transform)
    {
        transform = transform.Translate(projectile.Velocity * DeltaTime);
    }
}

partial struct ProjectileSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        new CreateProjectileJob()
        {
            m_ecbWriter = ecb.AsParallelWriter(),
            m_sourceConfigLookup = SystemAPI.GetComponentLookup<ProjectileSourceConfiguration>(),
            m_velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
            ElapsedTime = (float)SystemAPI.Time.ElapsedTime,
        }.ScheduleParallel();
        
        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        
        ProjectileMoveJob bmj = new ProjectileMoveJob()
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        bmj.ScheduleParallel();
    }
}
