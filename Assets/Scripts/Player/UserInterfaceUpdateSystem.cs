using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct PlayerUIAspect : IAspect
{
    public readonly Entity Self;
    public readonly RefRO<PlayerData> PlayerData;
    public readonly RefRO<PhysicsVelocity> PhysicsVelocity;
    public readonly RefRO<PlayerBoosterState> PlayerBoostState;
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
                Vector2 aimPositionScreen = localPlayer.GetMainCamera().WorldToScreenPoint(aimPositionWorld);
                playerUi.SetHardpointAim(i, aimPositionScreen);
            }

            LocalToWorld shipLocalToWorld = localToWorldLookup[player.Self];
            Vector3 shipForwardWorld = shipLocalToWorld.Position + aimDistance * shipLocalToWorld.Forward;
            Vector2 shipForwardScreen = localPlayer.GetMainCamera().WorldToScreenPoint(shipForwardWorld);
            playerUi.SetShipAim(shipForwardScreen);

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
        }
    }
}
