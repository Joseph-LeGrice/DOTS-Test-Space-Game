using Unity.Entities;
using Unity.Transforms;

public partial class CameraSyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var (_, localToWorld, entity) in SystemAPI.Query<RefRO<PlayerData>, RefRO<LocalToWorld>>().WithEntityAccess())
        { 
            PlayerManagedAccess pma = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(entity);
            pma.CameraGameObject.transform.position = localToWorld.ValueRO.Position;
            pma.CameraGameObject.transform.rotation = localToWorld.ValueRO.Rotation;
        }
    }
}
