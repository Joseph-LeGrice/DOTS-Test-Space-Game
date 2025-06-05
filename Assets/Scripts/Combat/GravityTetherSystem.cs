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
                    End = localToWorldSelf.Position + gravityTether.MaxRange * localToWorldSelf.Forward,
                    Filter = PhysicsConfiguration.GetGravityTetherFilter()
                };
                
                if (physicsWorld.CastRay(rayInput, out RaycastHit hit))
                {
                    if (hit.RigidBodyIndex >= 0 && hit.Entity != gravityTether.SourceRigidbodyEntity)
                    {
                        Entity jointEntity = ecb.CreateEntity();

                        float distance = math.max(hit.Fraction * gravityTether.MaxRange, gravityTether.MinDistance);
                        BodyFrame bf1 = new BodyFrame()
                        {
                            Position = float3.zero,
                            Axis = new float3(1, 0, 0),
                            PerpendicularAxis = new float3(0, 1, 0)
                        };
                        BodyFrame bf2 = new BodyFrame()
                        {
                            Position = new float3(0.0f, 0.0f, -distance),
                            Axis = new float3(1, 0, 0),
                            PerpendicularAxis = new float3(0, 1, 0)
                        };
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
