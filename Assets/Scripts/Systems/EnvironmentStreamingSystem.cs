using Unity.Burst;
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

[BurstCompile]
public partial struct CreateAsteroidField : IJobEntity
{
    private void Execute(ref AsteroidField asteroidField)
    {
        if (!asteroidField.IsCreated)
        {
            ref AsteroidSettingsBlob settingsBlob = ref asteroidField.Settings.Value;
            int numTypes = settingsBlob.AsteroidTypes.Length;
            
            Random r = new Random(0);
            for (int i = 0; i < asteroidField.AsteroidFieldDensity; i++)
            {
                int asteroidTypeIndex = r.NextInt(numTypes); 
                asteroidField.AsteroidType[i] = asteroidTypeIndex;
                
                int numMeshes = settingsBlob.AsteroidTypes[asteroidTypeIndex].PossibleMeshHashes.Length;
                int meshIndex = r.NextInt(numMeshes);
                asteroidField.AsteroidMeshIndex[i] = meshIndex;
                
                asteroidField.AsteroidStates[i] = true;
                asteroidField.AsteroidLocalPositions[i] = asteroidField.AsteroidFieldRadius * r.NextFloat3();
            }
        }
    }
}

public struct CreateAsteroids : IJobParallelFor
{
    public Entity m_prefab; // ??
    public int m_chunkIndex; // ??
    public AsteroidField m_asteroidField;
    public EntityCommandBuffer.ParallelWriter m_entityCommandBuffer;
    
    public void Execute(int i)
    {
        if (m_asteroidField.AsteroidStates[i])
        {
            Entity instance = m_entityCommandBuffer.Instantiate(m_chunkIndex, m_prefab);
            // m_asteroidField.AsteroidType[i]; // ??
            // m_asteroidField.AsteroidMeshIndex[i]; // ??
            // m_entityCommandBuffer.SetComponent(m_chunkIndex, instance, ); // ??
            
            m_entityCommandBuffer.SetComponent(m_chunkIndex, instance, LocalTransform.FromPosition(m_asteroidField.AsteroidLocalPositions[i]));
            
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
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // ??
        CreateAsteroidField caf = new CreateAsteroidField();
        caf.ScheduleParallel();
        
        
        // authoring.AsteroidFieldDensity
        // BulletMoveJob bmj = new BulletMoveJob()
        // {
        //     DeltaTime = SystemAPI.Time.DeltaTime
        // };
        // bmj.ScheduleParallel();
        //
        // BeginSimulationEntityCommandBufferSystem.Singleton commandBufferSystem =
        //     SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        // BulletDestroyJob bdj = new BulletDestroyJob()
        // {
        //     Ecb = commandBufferSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        // };
        // bdj.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}
