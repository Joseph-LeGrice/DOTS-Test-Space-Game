using Unity.Entities;
using Unity.Transforms;

public partial struct EnemyMovementAISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        ComponentLookup<LocalToWorld> localToWorldLookup = state.GetComponentLookup<LocalToWorld>();
        ComponentLookup<ShipInput> shipInputLookup = state.GetComponentLookup<ShipInput>();
        BufferLookup<DetectedTarget> targetLookup = state.GetBufferLookup<DetectedTarget>();
        
        foreach (var enemyAi in SystemAPI.Query<RefRO<EnemyAI>>())
        {
            DynamicBuffer<DetectedTarget> targetBuffer = targetLookup[enemyAi.ValueRO.ControllingShip];
            if (targetBuffer.GetSelectedTarget(ref state, out DetectedTarget selectedTarget))
            {
                LocalToWorld localToWorldSelf = localToWorldLookup[enemyAi.ValueRO.ControllingShip];
                LocalToWorld localToWorldTarget = localToWorldLookup[selectedTarget.TargetableEntity];
                RefRW<ShipInput> shipInput = shipInputLookup.GetRefRW(enemyAi.ValueRO.ControllingShip);
                
                // TODO: Put logic in AI Graphs and update in parallel via jobs
                // Think about whether to / how to roll movement and targeting logic into graph
                // Make graphs serializable and assign them in the EnemyAI component
            }
        }
    }
}
