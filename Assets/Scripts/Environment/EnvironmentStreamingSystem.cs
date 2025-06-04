using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;

// [BurstCompile]
public struct CreateAsteroidsJob : IJobParallelFor
{
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
            Entity instance = m_entityCommandBuffer.Instantiate(i, prefab);
            
            m_entityCommandBuffer.SetComponent(i, instance, LocalTransform.FromPosition(asteroidData.LocalPosition));
            m_entityCommandBuffer.SetComponent(i, instance, new Asteroid());
            m_entityCommandBuffer.SetComponent(i, instance, new PhysicsVelocity() { Angular = asteroidData.RotationSpeed * asteroidData.RotationAxis });

            asteroidData.Created = true;
            
            m_asteroidBufferData[i] = asteroidData;
        }
    }
}

public partial struct EnvironmentStreamingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (DynamicBuffer<AsteroidBufferData> asteroidBuffer in SystemAPI.Query<DynamicBuffer<AsteroidBufferData>>())
        {
            CreateAsteroidsJob caj = new CreateAsteroidsJob()
            {
                m_entityCommandBuffer = ecb.AsParallelWriter(),
                m_asteroidBufferData = asteroidBuffer,
                m_asteroidTypeBufferData = SystemAPI.GetSingletonBuffer<AsteroidTypeBufferData>(),
            };
            state.Dependency = JobHandle.CombineDependencies(state.Dependency,
                caj.Schedule(asteroidBuffer.Length, 64));
        }
    }
}
