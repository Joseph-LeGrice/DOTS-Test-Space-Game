using Unity.Entities;
using Unity.Transforms;

public partial class CameraSyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Dependency.Complete();
        ComponentLookup<LocalToWorld> localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        foreach (var (player, entity) in SystemAPI.Query<RefRO<PlayerTag>>().WithEntityAccess())
        {
            if (EntityManager.Exists(player.ValueRO.ControllingShip))
            {
                PlayerManagedAccess pma = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(entity);
                LocalToWorld localToWorld = localToWorldLookup[player.ValueRO.ControllingShip];
                pma.ManagedLocalPlayer.UpdatePlayerPositionAndRotation(
                    localToWorld.Position, localToWorld.Rotation
                );
            }
        }
    }
}
