using Unity.Entities;
using Unity.Physics;

partial class PlayerMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var (playerData, velocity, entity) in SystemAPI.Query<RefRO<PlayerData>, RefRW<PhysicsVelocity>>().WithEntityAccess())
        {
            PlayerManagedAccess managedAccess = SystemAPI.ManagedAPI.GetComponent<PlayerManagedAccess>(entity);
            velocity.ValueRW.Linear = playerData.ValueRO.MovementSpeed * managedAccess.ManagedLocalPlayer.GetPlayerInput().TargetDirection;
        }
    }
}
