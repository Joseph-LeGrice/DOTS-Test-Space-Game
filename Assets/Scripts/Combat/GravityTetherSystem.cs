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
        var localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        
        foreach (var (gravityTetherRef, self) in SystemAPI.Query<RefRW<GravityTether>>().WithEntityAccess())
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
                var localToWorldSelf = localToWorldLookup[self];
                RaycastInput rayInput = new RaycastInput()
                {
                    Start = localToWorldSelf.Position,
                    End = localToWorldSelf.Position + gravityTether.MaxRange * playerData.AimDirection,
                    Filter = PhysicsConfiguration.GetGravityTetherFilter()
                };
                
                if (physicsWorld.CastRay(rayInput, out RaycastHit hit))
                {
                    if (hit.RigidBodyIndex >= 0 && hit.Entity != gravityTether.SourceRigidbodyEntity)
                    {
                        var localToWorldTarget = localToWorldLookup[hit.Entity];
                        
                        
                        Entity jointEntity = ecb.CreateEntity();
                        
                        var pivot = localToWorldSelf.Position + 0.5f * (localToWorldTarget.Position - localToWorldSelf.Position);
                        
                        RigidTransform rt1 = new RigidTransform(quaternion.identity, localToWorldSelf.Value.InverseTransformPoint(pivot));
                        RigidTransform rt2 = new RigidTransform(localToWorldTarget.Rotation, localToWorldTarget.Value.InverseTransformPoint(pivot));
                        BodyFrame bf1 = new BodyFrame(rt1);
                        BodyFrame bf2 = new BodyFrame(rt2);
                        PhysicsJoint pj = PhysicsJoint.CreateFixed(bf1, bf2);
                        ecb.AddComponent(jointEntity, pj);
                        
                        ecb.AddComponent(jointEntity, new PhysicsConstrainedBodyPair(gravityTether.SourceRigidbodyEntity, hit.Entity, false));
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
