using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[BurstCompile]
public partial struct ProjectileMoveJob : IJobEntity
{
    public float DeltaTime;
    
    private void Execute(ref Projectile projectile, ref LocalTransform transform)
    {
        transform = transform.Translate(projectile.Velocity * DeltaTime);
        projectile.Lifetime -= DeltaTime;
    }
}

[BurstCompile]
public partial struct ProjectileDestroyJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    private void Execute(Entity e,  [ChunkIndexInQuery] int sortKey, ref Projectile projectile)
    {
        if (projectile.Lifetime <= 0.0f)
        {
            Ecb.DestroyEntity(sortKey, e);
        }
    }
}

partial struct ProjectileUpdateSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ProjectileMoveJob bmj = new ProjectileMoveJob()
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        bmj.ScheduleParallel();

        BeginSimulationEntityCommandBufferSystem.Singleton commandBufferSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        ProjectileDestroyJob bdj = new ProjectileDestroyJob()
        {
            Ecb = commandBufferSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        };
        bdj.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
