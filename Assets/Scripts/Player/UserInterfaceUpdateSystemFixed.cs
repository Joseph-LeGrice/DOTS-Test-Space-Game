using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct PlayerUIAspectFixed : IAspect
{
    public readonly Entity Self;
    public readonly DynamicBuffer<ShipHardpointBufferElement> ShipHardpoints;
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial class UserInterfaceUpdateSystemFixed : SystemBase
{
    protected override void OnUpdate()
    {
        Dependency.Complete();

        ComponentLookup<LocalToWorld> localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        foreach (PlayerUIAspectFixed player in SystemAPI.Query<PlayerUIAspectFixed>())
        {
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(player.Self);
            ManagedLocalPlayer localPlayer = managedAccess.ManagedLocalPlayer;
            LocalPlayerUserInterface playerUi = localPlayer.GetUserInterface();
            
            if (!playerUi.HasCreated())
            {
                playerUi.Initialize(player.ShipHardpoints.Length);
            }

            float aimDistance = 500.0f;
            for (int i = 0; i < player.ShipHardpoints.Length; i++)
            {
                LocalToWorld l2w = localToWorldLookup[player.ShipHardpoints[i].Self];
                Vector3 aimPositionWorld = l2w.Position + aimDistance * l2w.Forward;
                playerUi.SetHardpointAim(i, aimPositionWorld);
            }

            LocalToWorld shipLocalToWorld = localToWorldLookup[player.Self];
            Vector3 shipForwardWorld = shipLocalToWorld.Position + aimDistance * shipLocalToWorld.Forward;
            playerUi.SetShipAim(shipForwardWorld);
        }
    }
}
