using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct PlayerUIAspectFixed : IAspect
{
    public readonly DynamicBuffer<ShipHardpointReference> ShipHardpoints;
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial class UserInterfaceUpdateSystemFixed : SystemBase
{
    protected override void OnUpdate()
    {
        ComponentLookup<LocalToWorld> localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        ComponentLookup<ShipHardpointInstance> hardpointInstanceLookup = SystemAPI.GetComponentLookup<ShipHardpointInstance>();
        ComponentLookup<Gimbal> gimbalLookup = SystemAPI.GetComponentLookup<Gimbal>();
        foreach (var (localPlayer, self) in SystemAPI.Query<RefRO<PlayerTag>>().WithEntityAccess())
        {
            PlayerUIAspectFixed player = SystemAPI.GetAspect<PlayerUIAspectFixed>(localPlayer.ValueRO.ControllingShip);
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(self);
            ManagedLocalPlayer managedLocalPlayer = managedAccess.ManagedLocalPlayer;
            LocalPlayerUserInterface playerUi = managedLocalPlayer.GetUserInterface();
            
            if (!playerUi.HasCreated())
            {
                playerUi.Initialize(player.ShipHardpoints.Length);
            }

            float avgAimDistance = 0.0f;
            for (int i = 0; i < player.ShipHardpoints.Length; i++)
            {
                ShipHardpointReference hardpointReference = player.ShipHardpoints[i];
                ShipHardpointInstance hardpointInstance = hardpointInstanceLookup[hardpointReference.Self];
                
                LocalToWorld l2w = localToWorldLookup[hardpointInstance.WeaponInstanceEntity];
                if (gimbalLookup.HasComponent(hardpointInstance.WeaponInstanceEntity))
                {
                    l2w = localToWorldLookup[gimbalLookup[hardpointInstance.WeaponInstanceEntity].GimbalEntity];
                }
                Vector3 aimPositionWorld = l2w.Position + hardpointInstance.AimDistance * l2w.Forward;
                playerUi.SetHardpointAim(i, aimPositionWorld);
                
                avgAimDistance += hardpointInstance.AimDistance;
            }

            avgAimDistance /= player.ShipHardpoints.Length;

            LocalToWorld shipLocalToWorld = localToWorldLookup[localPlayer.ValueRO.ControllingShip];
            Vector3 shipForwardWorld = shipLocalToWorld.Position + avgAimDistance * shipLocalToWorld.Forward;
            playerUi.SetShipAim(shipForwardWorld);
        }
    }
}
