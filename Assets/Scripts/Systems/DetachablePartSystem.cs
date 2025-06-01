using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct DetachParts : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter m_ecbWriter;
    [ReadOnly]
    public ComponentLookup<LocalTransform> m_localTransformLookup;
    // public ComponentLookup<PhysicsVelocity> m_physicsVelocityLookup;
    public Random m_random;
    
    private void Execute(Entity self, in Damageable d, in DynamicBuffer<DetachablePart> detachableParts)
    {
        if (d.CurrentHealth < 0.0f)
        {
            foreach (DetachablePart detachablePart in detachableParts)
            {
                m_ecbWriter.RemoveComponent<Parent>(0, detachablePart.DetachableEntity);
                // https://docs.unity3d.com/Packages/com.unity.entities@1.3/manual/transforms-comparison.html
                
                LocalTransform localTransform = m_localTransformLookup[detachablePart.DetachableEntity];

                if (detachablePart.EffectPrefab != Entity.Null)
                {
                    Entity effect = m_ecbWriter.Instantiate(0, detachablePart.EffectPrefab);
                    m_ecbWriter.SetComponent(0, effect, LocalTransform.FromPosition(localTransform.Position));
                }
                
                // m_ecbWriter.AddComponent<PhysicsVelocity>(1, detachablePart.DetachableEntity);
                // m_ecbWriter.SetComponent(0, detachablePart.DetachableEntity, new PhysicsVelocity()
                // {
                //     Linear = localTransform.Forward() * m_random.NextFloat(detachablePart.ImpulseForceMinimum, detachablePart.ImpulseForceMaximum),
                //     Angular = m_random.NextFloat(detachablePart.AngularForceMinimum, detachablePart.AngularForceMaximum),
                // });
                
                // PhysicsVelocity pv = m_physicsVelocityLookup[detachablePart.DetachableEntity];
                // pv.Linear = localTransform.Forward() * m_random.NextFloat(detachablePart.ImpulseForceMinimum, detachablePart.ImpulseForceMaximum);
                // pv.Angular = m_random.NextFloat(detachablePart.AngularForceMinimum, detachablePart.AngularForceMaximum);
                // m_physicsVelocityLookup[detachablePart.DetachableEntity] = pv;
            }
        }
    }
}

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct DetachablePartSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecbForDetach = new EntityCommandBuffer(Allocator.TempJob);
        //
        // new DamageableUpdate()
        // {
        //     m_ecbWriter = ecbForDetach.AsParallelWriter(),
        //     m_detachablePartsLookup = SystemAPI.GetBufferLookup<DetachablePart>(),
        //     m_localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
        //     // m_physicsVelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
        //     m_random = new Random((uint)SystemAPI.Time.ElapsedTime + 100),
        // }.Schedule();
        //
        // state.Dependency.Complete();
        //
        // ecbForDetach.Playback(state.EntityManager);
        // ecbForDetach.Dispose();
        
        var m_detachablePartsLookup = SystemAPI.GetBufferLookup<DetachablePart>();
        foreach (var (d, detachableParts, self) in SystemAPI.Query<RefRO<Damageable>, DynamicBuffer<DetachablePart>>().WithEntityAccess())
        {
            if (d.ValueRO.CurrentHealth < 0.0f && m_detachablePartsLookup.HasBuffer(self))
            {
                foreach (DetachablePart detachablePart in detachableParts)
                {
                    ecbForDetach.RemoveComponent<Parent>(detachablePart.DetachableEntity);
                }
            }
        }

        ecbForDetach.Playback(state.EntityManager);
        ecbForDetach.Dispose();
    }
}
