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
                var thrusterSetup = playerShip.ShipMovementData.ValueRO.DefaultMovement;
            
                float2 accelerationXY = new float2(playerShip.Velocity.ValueRO.Angular.y,
                    -playerShip.Velocity.ValueRO.Angular.x);
                float2 accelerationDirection = math.normalizesafe(accelerationXY);
                float accelerationNormalised = math.lengthsq(accelerationXY) /
                                               math.pow(math.radians(thrusterSetup.MaxTurnSpeed), 2.0f);
                playerUi.SetAcceleration(accelerationDirection, accelerationNormalised);
            }
            else
            {
                playerUi.SetAcceleration(Vector2.up, 0.0f);
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
