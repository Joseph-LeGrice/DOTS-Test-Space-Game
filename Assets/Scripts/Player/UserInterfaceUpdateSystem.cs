using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct PlayerUIAspect : IAspect
{
    public readonly Entity Self;
    public readonly RefRO<ShipMovementData> PlayerData;
    public readonly RefRO<PhysicsVelocity> PhysicsVelocity;
    public readonly DynamicBuffer<DetectedTarget> DetectedTargets;
}

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(TargetSystem))]
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
            LocalPlayerUserInterface playerUi = localPlayer.GetUserInterface();
            
            if (!localPlayer.GetPlayerInput().IsADS)
            {
                var thrusterSetup = player.PlayerData.ValueRO.DefaultMovement;

                float2 accelerationXY = new float2(player.PhysicsVelocity.ValueRO.Angular.y,
                    -player.PhysicsVelocity.ValueRO.Angular.x);
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
            foreach (DetectedTarget dt in player.DetectedTargets)
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
        }
    }
}
