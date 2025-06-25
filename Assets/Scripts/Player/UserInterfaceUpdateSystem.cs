using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(TargetSystem))]
public partial class UserInterfaceUpdateSystem : SystemBase
{
    protected override void OnUpdate()
    {
        ComponentLookup<LocalToWorld> localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        foreach (var (localPlayer, self) in SystemAPI.Query<RefRO<PlayerTag>>().WithEntityAccess())
        {
            ShipAspect playerShip = SystemAPI.GetAspect<ShipAspect>(localPlayer.ValueRO.ControllingShip);
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(self);
            ManagedLocalPlayer managedLocalPlayer = managedAccess.ManagedLocalPlayer;
            LocalPlayerUserInterface playerUi = managedLocalPlayer.GetUserInterface();
            
            if (!playerShip.ShipInput.ValueRO.IsADS)
            {
                playerUi.SetAngularThrottle(playerShip.ShipInput.ValueRO.AngularThrottle);
            }
            else
            {
                playerUi.SetAngularThrottle(Vector2.zero);
            }
            
            List<TargetData> targetData = new List<TargetData>();
            foreach (DetectedTarget dt in playerShip.DetectedTargets)
            {
                var l2wTarget = localToWorldLookup[dt.TargetableEntity];
                targetData.Add(new TargetData()
                {
                    IsTargeting = dt.IsSelected,
                    Position = l2wTarget.Position,
                    CanTargetAhead = dt.CanTargetAhead,
                });
            }
            playerUi.SetTargets(targetData);

            playerUi.GetHUD().UpdateHUD(ref playerShip);
        }
    }
}
