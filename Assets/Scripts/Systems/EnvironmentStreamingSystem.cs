using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Content;
using Unity.Entities.Serialization;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Hash128 = Unity.Entities.Hash128;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct UpdateAsteroids : IJobEntity
{
    public float m_elapsedTime;

    private void Execute(ref Asteroid asteroid, ref LocalTransform localToWorld)
    {
        localToWorld = localToWorld.Rotate(quaternion.AxisAngle(asteroid.TumbleAxis, m_elapsedTime * asteroid.TumbleSpeed));
    }
}

[BurstCompile]
public struct CreateAsteroidsJob : IJobParallelFor
{
    public int m_chunkIndex;
    public EntityCommandBuffer.ParallelWriter m_entityCommandBuffer;
    [NativeDisableParallelForRestriction]
    public DynamicBuffer<AsteroidBufferData> m_asteroidBufferData;
    [ReadOnly]
    public DynamicBuffer<AsteroidTypeBufferData> m_asteroidTypeBufferData;
    
    public void Execute(int i)
    {
        AsteroidBufferData asteroidData = m_asteroidBufferData[i];
        if (!asteroidData.Created)
        {
            Entity prefab = m_asteroidTypeBufferData[asteroidData.AsteroidType].Prefab;
            Entity instance = m_entityCommandBuffer.Instantiate(i * 3, prefab);
            
            m_entityCommandBuffer.SetComponent(i * 3 + 1, instance, LocalTransform.FromPosition(asteroidData.LocalPosition));
            m_entityCommandBuffer.SetComponent(i * 3 + 2, instance, new Asteroid()
            {
                TumbleAxis = asteroidData.RotationAxis,
                TumbleSpeed = asteroidData.RotationSpeed,
            });

            asteroidData.Created = true;
            
            m_asteroidBufferData[i] = asteroidData;
        }
    }
}

readonly partial struct AsteroidFieldAspect : IAspect
{
    public readonly Entity Self;
    public readonly RefRW<AsteroidField> AsteroidFieldData;
    public readonly DynamicBuffer<AsteroidBufferData> AsteroidBufferData;
}

public partial struct EnvironmentStreamingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        DynamicBuffer<AsteroidTypeBufferData> asteroidTypeBuffer = SystemAPI.GetSingletonBuffer<AsteroidTypeBufferData>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        foreach (AsteroidFieldAspect asteroidField in SystemAPI.Query<AsteroidFieldAspect>())
        {
            int chunkIndex = (int)SystemAPI.GetEntityStorageInfoLookup()[asteroidField.Self].Chunk.SequenceNumber;
            CreateAsteroidsJob caj = new CreateAsteroidsJob()
            {
                m_chunkIndex = chunkIndex,
                m_entityCommandBuffer = ecb.AsParallelWriter(),
                m_asteroidBufferData = asteroidField.AsteroidBufferData,
                m_asteroidTypeBufferData = asteroidTypeBuffer,
            };
            state.Dependency = JobHandle.CombineDependencies(state.Dependency,
                caj.Schedule(asteroidField.AsteroidBufferData.Length, 64));
        }

        state.Dependency.Complete();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        UpdateAsteroids updateAsteroidsJob = new UpdateAsteroids() { m_elapsedTime = (float)SystemAPI.Time.ElapsedTime };
        updateAsteroidsJob.Schedule();
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}
