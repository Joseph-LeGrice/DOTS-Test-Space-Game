using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial struct GravityTetherSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        
        var playerData = SystemAPI.GetSingleton<PlayerData>();
        var physicsWorldRef = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>();
        var physicsWorld = physicsWorldRef.ValueRO;
        
        foreach (var (gravityTetherRef, localToWorld, self) in SystemAPI.Query<RefRW<GravityTether>, RefRO<LocalToWorld>>().WithEntityAccess())
        {
            var existingJointEntity = Entity.Null;
            foreach (var (activeTether, je) in SystemAPI.Query<RefRO<GravityTetherJoint>>().WithEntityAccess())
            {
                if (activeTether.ValueRO.OwnerEntity == self)
                {
                    existingJointEntity = je;
                    break;
                }
            }
            
            var gravityTether = gravityTetherRef.ValueRO;
            if (gravityTether.IsFiring && existingJointEntity == Entity.Null)
            {
                RaycastInput rayInput = new RaycastInput()
                {
                    Start = localToWorld.ValueRO.Position,
                    End = localToWorld.ValueRO.Position + gravityTether.MaxRange * playerData.AimDirection,
                    Filter = PhysicsConfiguration.GetDamageDealerFilter()
                };
                
                if (physicsWorld.CastRay(rayInput, out RaycastHit hit))
                {
                    if (hit.RigidBodyIndex >= 0 && hit.Entity != gravityTether.SourceRigidbodyEntity)
                    {
                        Entity jointEntity = ecb.CreateEntity();
                        ecb.AddComponent(jointEntity, PhysicsJoint.CreateBallAndSocket(new float3(), new float3()));
                        ecb.AddComponent(jointEntity, new PhysicsConstrainedBodyPair(gravityTether.SourceRigidbodyEntity, hit.Entity, true));
                        ecb.AddSharedComponent(jointEntity, new PhysicsWorldIndex(0));
                        ecb.AddComponent(jointEntity, new GravityTetherJoint(self, hit.Entity));
                    }
                }
            }
            else if (!gravityTether.IsFiring && existingJointEntity != Entity.Null)
            {
                ecb.DestroyEntity(existingJointEntity);
            }
        }
    }
}
