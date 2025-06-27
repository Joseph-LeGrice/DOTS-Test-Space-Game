using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial class UserInterfaceUpdateSystemFixed : SystemBase
{
    protected override void OnUpdate()
    {
        ComponentLookup<LocalToWorld> localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        ComponentLookup<ShipHardpointInstance> hardpointInstanceLookup = SystemAPI.GetComponentLookup<ShipHardpointInstance>();
        ComponentLookup<Gimbal> gimbalLookup = SystemAPI.GetComponentLookup<Gimbal>();
        foreach (var (localPlayer, self) in SystemAPI.Query<RefRO<PlayerTag>>().WithEntityAccess())
        {
            ShipAspect playerShip = SystemAPI.GetAspect<ShipAspect>(localPlayer.ValueRO.ControllingShip);
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(self);
            ManagedLocalPlayer managedLocalPlayer = managedAccess.ManagedLocalPlayer;
            LocalPlayerUserInterface playerUi = managedLocalPlayer.GetUserInterface();
            
            if (!playerUi.HasCreated())
            {
                playerUi.Initialize(playerShip.ShipHardpoints.Length, playerShip.TargetDetector.ValueRO.RangeSquared);
            }

            float avgAimDistance = 0.0f;
            for (int i = 0; i < playerShip.ShipHardpoints.Length; i++)
            {
                ShipHardpointReference hardpointReference = playerShip.ShipHardpoints[i];
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

            avgAimDistance /= playerShip.ShipHardpoints.Length;

            LocalToWorld shipLocalToWorld = localToWorldLookup[localPlayer.ValueRO.ControllingShip];
            Vector3 shipForwardWorld = shipLocalToWorld.Position + avgAimDistance * shipLocalToWorld.Forward;
            playerUi.SetShipAim(shipForwardWorld);
        }
    }
}
