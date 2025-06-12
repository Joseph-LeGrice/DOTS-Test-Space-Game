using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct PlayerUIAspect : IAspect
{
    public readonly Entity Self;
    public readonly RefRW<PlayerData> PlayerData;
    public readonly RefRW<PlayerBoosterState> PlayerBoostState;
    public readonly DynamicBuffer<ShipHardpointBufferElement> ShipHardpoints;
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial class UserInterfaceUpdateSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Dependency.Complete();

        ComponentLookup<LocalToWorld> localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        foreach (PlayerUIAspect player in SystemAPI.Query<PlayerUIAspect>())
        {
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(player.Self);
            ManagedLocalPlayer localPlayer = managedAccess.ManagedLocalPlayer;
            CrosshairLayout playerUi = localPlayer.GetCrosshair();
            
            if (!localPlayer.GetCrosshair().HasCreated())
            {
                playerUi.RefreshCrosshairs(player.ShipHardpoints.Length);
            }

            float aimDistance = 500.0f;
            for (int i = 0; i < player.ShipHardpoints.Length; i++)
            {
                LocalToWorld l2w = localToWorldLookup[player.ShipHardpoints[i].Self];
                Vector3 aimPositionWorld = l2w.Position + aimDistance * l2w.Forward;
                Vector2 aimPositionScreen = localPlayer.GetMainCamera().WorldToScreenPoint(aimPositionWorld);
                playerUi.SetDotPosition(i, aimPositionScreen);
            }

            LocalToWorld shipLocalToWorld = localToWorldLookup[player.Self];
            Vector3 shipForwardWorld = shipLocalToWorld.Position + aimDistance * shipLocalToWorld.Forward;
            Vector2 shipForwardScreen = localPlayer.GetMainCamera().WorldToScreenPoint(shipForwardWorld);
            playerUi.SetShipAim(shipForwardScreen);
        }
    }
}
