using Unity.Entities;
using UnityEngine;

public class MonoMarkForCleanup : MonoBehaviour
{
    public float CleanupAfterDelay;
    public TransformUsageFlags TransformFlags;
}

public class MonoMarkForCleanupBaker : Baker<MonoMarkForCleanup>
{
    public override void Bake(MonoMarkForCleanup authoring)
    {
        Entity e = GetEntity(authoring.TransformFlags);
        AddComponent(e, new QueueForCleanup(authoring.CleanupAfterDelay));
    }
}
