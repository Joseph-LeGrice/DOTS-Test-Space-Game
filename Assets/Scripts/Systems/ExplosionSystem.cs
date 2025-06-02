using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
partial struct ExplosionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        
        RefRW<PhysicsWorldSingleton> physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>();
        ComponentLookup<Damageable> damageableLookup = SystemAPI.GetComponentLookup<Damageable>();
        
        foreach (var (localToWorld, explosion, self) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<Explosion>>().WithEntityAccess())
        {
            NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
            bool didHitAny = physicsWorld.ValueRW.OverlapSphere(
                localToWorld.ValueRO.Position,
                explosion.ValueRO.Radius,
                ref outHits,
                new CollisionFilter()
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                },
                QueryInteraction.Default
            );
            
            if (didHitAny)
            {
                foreach (DistanceHit distanceHit in outHits)
                {
                    float3 delta = distanceHit.Position - localToWorld.ValueRO.Position;
                    float t = 1.0f; //1.0f - (math.length(delta) / explosion.ValueRO.Radius); // TODO: Configure Falloff
                    physicsWorld.ValueRW.PhysicsWorld.ApplyImpulse(
                        distanceHit.RigidBodyIndex,
                        explosion.ValueRO.Force * t * math.normalize(delta),
                        distanceHit.Position
                    );
                    
                    if (damageableLookup.HasComponent(distanceHit.Entity))
                    {
                        Damageable d = damageableLookup[distanceHit.Entity];
                        d.CurrentHealth -= t * explosion.ValueRO.Damage;
                        damageableLookup[distanceHit.Entity] = d;
                    }
                }
            }

            outHits.Dispose();
            ecb.RemoveComponent<Explosion>(self);
        }
        
        state.Dependency.Complete();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
