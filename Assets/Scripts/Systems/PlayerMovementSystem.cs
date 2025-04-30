using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((var playerData, var velocity) in SystemAPI.Query<RefRO<PlayerData>, RefRW<PhysicsVelocity>>())
        {
            
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
