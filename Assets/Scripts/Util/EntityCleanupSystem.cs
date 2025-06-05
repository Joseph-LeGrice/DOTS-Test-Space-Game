using Unity.Burst;
using Unity.Entities;

public struct MarkForCleanup : IComponentData
{
    public float RemovalTime;
    
    public MarkForCleanup(float removeTime)
    {
        RemovalTime = removeTime;
    }
}

[BurstCompile]
public partial struct QueueEntitiesForCleanup : IJobEntity
{
    public float m_elapsedTime;
    public EntityCommandBuffer.ParallelWriter m_ecbWriter;
    
    private void Execute(Entity e, [ChunkIndexInQuery] int sortKey, ref QueueForCleanup projectile)
    {
        m_ecbWriter.RemoveComponent<QueueForCleanup>(sortKey, e);
        m_ecbWriter.AddComponent<MarkForCleanup>(sortKey, e);
        m_ecbWriter.SetComponent(sortKey, e, new MarkForCleanup(m_elapsedTime + projectile.RemoveAfterDelay));
    }
}

[BurstCompile]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct CleanupEntities : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter m_ecbWriter;
    public float m_elapsedTime;
    
    private void Execute(Entity e, [ChunkIndexInQuery] int sortKey, ref MarkForCleanup markForCleanup)
    {
        if (markForCleanup.RemovalTime < m_elapsedTime)
        {
            m_ecbWriter.DestroyEntity(sortKey, e);
        }
    }
}

partial struct EntityCleanupSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        BeginSimulationEntityCommandBufferSystem.Singleton commandBufferSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        new QueueEntitiesForCleanup()
        {
            m_ecbWriter = commandBufferSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            m_elapsedTime = (float)SystemAPI.Time.ElapsedTime,
        }.ScheduleParallel();
        new CleanupEntities()
        {
            m_ecbWriter = commandBufferSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            m_elapsedTime = (float)SystemAPI.Time.ElapsedTime,
        }.ScheduleParallel();
    }
}
