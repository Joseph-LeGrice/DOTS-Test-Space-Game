using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

partial struct ProjectileSpawningSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (spawner, localToWorld) in SystemAPI.Query<RefRW<ProjectileSource>, RefRO<LocalToWorld>>())
        {
            if (spawner.ValueRO.IsFiring && spawner.ValueRO.ProjectileWeaponEntity != Entity.Null && spawner.ValueRO.NextSpawnTime < SystemAPI.Time.ElapsedTime)
            {
                ProjectileSourceConfiguration pw = SystemAPI.GetComponent<ProjectileSourceConfiguration>(spawner.ValueRO.ProjectileWeaponEntity);
                Entity newEntity = state.EntityManager.Instantiate(pw.ProjectilePrefab);
                state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(localToWorld.ValueRO.Position));

                Projectile b = state.EntityManager.GetComponentData<Projectile>(pw.ProjectilePrefab);
                b.Velocity = pw.ProjectileSpeed * localToWorld.ValueRO.Forward;

                if (spawner.ValueRO.RelatedRigidbodyEntity != Entity.Null)
                {
                    PhysicsVelocity pv = SystemAPI.GetComponent<PhysicsVelocity>(spawner.ValueRO.RelatedRigidbodyEntity);
                    b.Velocity += pv.Linear;
                }
                
                state.EntityManager.SetComponentData(newEntity, b);
                
                spawner.ValueRW.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + pw.ProjectileSpawnRate;
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
