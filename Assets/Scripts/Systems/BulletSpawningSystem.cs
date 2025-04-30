using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct BulletSpawningSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<BulletSource> spawner in SystemAPI.Query<RefRW<BulletSource>>())
        {
            if (spawner.ValueRO.NextSpawnTime < SystemAPI.Time.ElapsedTime)
            {
                Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);
                state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition));

                Bullet b = state.EntityManager.GetComponentData<Bullet>(spawner.ValueRO.Prefab);
                b.CurrentVelocity = b.Speed * spawner.ValueRO.BulletFireDirection;
                state.EntityManager.SetComponentData(newEntity, b);
                
                spawner.ValueRW.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.ValueRO.SpawnRate;
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
