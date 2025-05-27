using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
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

public struct CreateAsteroidsJob : IJobParallelFor
{
    public Entity m_prefab; // ??
    public int m_chunkIndex; // ??
    public AsteroidField m_asteroidField;
    public EntityCommandBuffer.ParallelWriter m_entityCommandBuffer;
    [ReadOnly] public DynamicBuffer<AsteroidBufferData> m_bufferData;
    
    public void Execute(int i)
    {
        if (m_bufferData[i].State)
        {
            Entity instance = m_entityCommandBuffer.Instantiate(m_chunkIndex, m_prefab);
            
            // Todo: Set up mesh and transform parenting correctly
            // m_bufferData[i].Type[i]; // ??
            // m_bufferData[i].MeshIndex[i]; // ??
            // m_entityCommandBuffer.SetComponent(m_chunkIndex, instance, ); // ??
            
            m_entityCommandBuffer.SetComponent(m_chunkIndex, instance, LocalTransform.FromPosition(m_bufferData[i].LocalPosition));
            
            m_entityCommandBuffer.SetComponent(m_chunkIndex, instance, new Asteroid()
            {
                TumbleAxis = new float3(0.0f, 1.0f, 0.0f),
                TumbleSpeed = 5.0f,
            });
        }
    }
}

public partial struct EnvironmentStreamingSystem : ISystem
{
    private BufferLookup<AsteroidBufferData> m_asteroidBufferDataLookup;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_asteroidBufferDataLookup = state.GetBufferLookup<AsteroidBufferData>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        m_asteroidBufferDataLookup.Update(ref state);
        // foreach (asteroidFieldEntity in asteroidFieldQuery)
        // {
        //      DynamicBuffer<AsteroidBufferData> bufferData = m_asteroidBufferDataLookup[asteroidFieldEntity];
        //      CreateAsteroidsJob caj = new CreateAsteroidsJob() { ... }
        //      caj.Schedule();
        // }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}
