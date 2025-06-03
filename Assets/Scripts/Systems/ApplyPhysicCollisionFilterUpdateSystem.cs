using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;

public struct RequestPhysicCollisionFilterUpdate : IComponentData
{
    public CollisionFilter CollisionFilter;

    public RequestPhysicCollisionFilterUpdate(CollisionFilter cf)
    {
        CollisionFilter = cf;
    }
}

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial struct ApplyPhysicCollisionFilterUpdateSystem : ISystem
{
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // var ecb = new EntityCommandBuffer(Allocator.TempJob);
        // var job = new ApplyPhysicCollisionFilterUpdateJob
        // {
        //     ECB = ecb.AsParallelWriter()
        // };
        //
        // state.Dependency = job.ScheduleParallel(state.Dependency);
        //
        // state.Dependency.Complete();
        // ecb.Playback(state.EntityManager);
        // ecb.Dispose();

        var ECB = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (physicsCollider, request, entity) in SystemAPI
                     .Query<RefRO<PhysicsCollider>, RefRO<RequestPhysicCollisionFilterUpdate>>().WithEntityAccess())
        {
            var colliderCopy = physicsCollider.ValueRO.Value.Value.Clone();
            colliderCopy.Value.SetCollisionFilter(request.ValueRO.CollisionFilter);

            PhysicsCollider newCollider = colliderCopy.AsComponent();
            ECB.SetComponent(entity, newCollider);
            ECB.RemoveComponent<RequestPhysicCollisionFilterUpdate>(entity);
        }
        
        state.Dependency.Complete();
        ECB.Playback(state.EntityManager);
        ECB.Dispose();
    }

    // [BurstCompile]
    // private partial struct ApplyPhysicCollisionFilterUpdateJob : IJobEntity
    // {
    //     public EntityCommandBuffer.ParallelWriter ECB;
    //
    //     public void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, ref PhysicsCollider physicsCollider, in RequestPhysicCollisionFilterUpdate request)
    //     {
    //         var colliderCopy = physicsCollider.Value.Value.Clone();
    //         colliderCopy.Value.SetCollisionFilter(request.CollisionFilter);
    //
    //         PhysicsCollider newCollider = colliderCopy.AsComponent();
    //         ECB.SetComponent(entityIndex, entity, newCollider);
    //         ECB.RemoveComponent<RequestPhysicCollisionFilterUpdate>(entityIndex, entity);
    //     }
    // }
}