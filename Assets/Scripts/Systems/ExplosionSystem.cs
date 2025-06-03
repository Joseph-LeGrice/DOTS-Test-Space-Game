using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
partial struct ExplosionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency.Complete();
        
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        
        RefRW<PhysicsWorldSingleton> physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>();
        ComponentLookup<Damageable> damageableLookup = SystemAPI.GetComponentLookup<Damageable>();
        ComponentLookup<LocalToWorld> localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        ComponentLookup<PhysicsVelocity> velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>();
        ComponentLookup<PhysicsMass> massLookup = SystemAPI.GetComponentLookup<PhysicsMass>();
        
        foreach (var (localToWorld, explosion, self) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<Explosion>>().WithEntityAccess())
        {
            NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
            bool didHitAny = physicsWorld.ValueRW.OverlapSphere(
                localToWorld.ValueRO.Position,
                explosion.ValueRO.Radius,
                ref outHits,
                PhysicsConfiguration.GetDamageDealerFilter(),
                QueryInteraction.Default
            );
            
            if (didHitAny)
            {
                foreach (DistanceHit distanceHit in outHits)
                {
                    float3 delta = distanceHit.Position - localToWorld.ValueRO.Position;
                    float t = 1.0f; //1.0f - (math.length(delta) / explosion.ValueRO.Radius); // TODO: Configure Falloff

                    if (velocityLookup.HasComponent(distanceHit.Entity) && massLookup.HasComponent(distanceHit.Entity))
                    {
                        var localToWorldHit = localToWorldLookup[distanceHit.Entity];
                        var v = velocityLookup[distanceHit.Entity];
                        v.ApplyImpulse(
                            massLookup[distanceHit.Entity],
                            localToWorldHit.Position,
                            localToWorldHit.Rotation,
                            explosion.ValueRO.Force * t * math.normalize(delta),
                            distanceHit.Position
                        );
                        velocityLookup[distanceHit.Entity] = v;
                    }
                    
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
