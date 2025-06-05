using Unity.Entities;
using UnityEngine;

public struct QueueForCleanup : IComponentData
{
    public float RemoveAfterDelay;
    
    public QueueForCleanup(float removalDelay)
    {
        RemoveAfterDelay = removalDelay;
    }
}

public class MarkForCleanupBaker : MonoBehaviour
{
    public float CleanupAfterDelay;
    public TransformUsageFlags TransformFlags;
    
    public class Baker : Baker<MarkForCleanupBaker>
    {
        public override void Bake(MarkForCleanupBaker authoring)
        {
            Entity e = GetEntity(authoring.TransformFlags);
            AddComponent(e, new QueueForCleanup(authoring.CleanupAfterDelay));
        }
    }
}
