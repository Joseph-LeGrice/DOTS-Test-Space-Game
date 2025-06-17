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
        ComponentLookup<Gimbal> gimbalLookup = SystemAPI.GetComponentLookup<Gimbal>();
        foreach (PlayerUIAspectFixed player in SystemAPI.Query<PlayerUIAspectFixed>())
        {
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(player.Self);
            ManagedLocalPlayer localPlayer = managedAccess.ManagedLocalPlayer;
            LocalPlayerUserInterface playerUi = localPlayer.GetUserInterface();
            
            if (!playerUi.HasCreated())
            {
                playerUi.Initialize(player.ShipHardpoints.Length);
            }

            float avgAimDistance = 0.0f;
            for (int i = 0; i < player.ShipHardpoints.Length; i++)
            {
                ShipHardpointBufferElement hardpoint = player.ShipHardpoints[i];
                LocalToWorld l2w = localToWorldLookup[hardpoint.Self];
                if (gimbalLookup.HasComponent(hardpoint.Self))
                {
                    l2w = localToWorldLookup[gimbalLookup[hardpoint.Self].GimbalEntity];
                }
                Vector3 aimPositionWorld = l2w.Position + hardpoint.AimDistance * l2w.Forward;
                playerUi.SetHardpointAim(i, aimPositionWorld);
                
                avgAimDistance += hardpoint.AimDistance;
            }

            avgAimDistance /= player.ShipHardpoints.Length;

            LocalToWorld shipLocalToWorld = localToWorldLookup[player.Self];
            Vector3 shipForwardWorld = shipLocalToWorld.Position + avgAimDistance * shipLocalToWorld.Forward;
            playerUi.SetShipAim(shipForwardWorld);
        }
    }
}
