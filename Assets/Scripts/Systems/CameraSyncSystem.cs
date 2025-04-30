using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public partial class CameraSyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(ComponentType.ReadWrite<PlayerManagedAccess>());
        NativeArray<Entity> allEntities = entityQuery.ToEntityArray(Allocator.Temp);
        foreach (Entity e in allEntities)
        {
            PlayerManagedAccess pma = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(e);
            LocalToWorld entityTransform = SystemAPI.GetComponent<LocalToWorld>(e);
            pma.CameraGameObject.transform.position = entityTransform.Position;
            pma.CameraGameObject.transform.rotation = entityTransform.Rotation;
        }
    }
}
