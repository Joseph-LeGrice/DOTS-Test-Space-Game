using Unity.Entities;

public partial struct EnemyTargetingAISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        BufferLookup<DetectedTarget> targetLookup = state.GetBufferLookup<DetectedTarget>();
        foreach (var enemyAi in SystemAPI.Query<RefRO<EnemyAI>>())
        {
            DynamicBuffer<DetectedTarget> targetBuffer = targetLookup[enemyAi.ValueRO.ControllingShip];
            
            int bestTarget = 0;
            // for (int i = 0; i < targetBuffer.Length; i++)
            // {
            //     
            // }
            
            for (int i=0; i<targetBuffer.Length; i++)
            {
                DetectedTarget detectedTarget = targetBuffer[i];
                detectedTarget.IsSelected = bestTarget == i;
                targetBuffer[i] = detectedTarget;
            }
        }
    }
}
