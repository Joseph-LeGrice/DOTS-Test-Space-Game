using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public struct Bullet : IComponentData
{
    public float3 CurrentVelocity;
    public float Speed;
    public float Lifetime;

    public Bullet(float Speed, float Lifetime)
    {
        this.Speed = Speed;
        this.Lifetime = Lifetime;
        this.CurrentVelocity = 0.0f;
    }
}

[BurstCompile]
public partial struct BulletMoveJob : IJobEntity
{
    public float DeltaTime;
    
    private void Execute(ref Bullet bullet, ref LocalTransform transform)
    {
        transform = transform.Translate(bullet.CurrentVelocity * DeltaTime);
        bullet.Lifetime -= DeltaTime;
    }
}

[BurstCompile]
public partial struct BulletDestroyJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    private void Execute(Entity e, ref Bullet bullet)
    {
        if (bullet.Lifetime <= 0.0f)
        {
            Ecb.DestroyEntity(0, e);
        }
    }
}

partial struct BulletUpdateSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        BulletMoveJob bmj = new BulletMoveJob()
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        bmj.ScheduleParallel();

        BeginSimulationEntityCommandBufferSystem.Singleton commandBufferSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        BulletDestroyJob bdj = new BulletDestroyJob()
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
