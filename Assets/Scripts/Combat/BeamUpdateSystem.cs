using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public struct BeamSource : IComponentData
{
    public Entity BeamFXEntity;
    public bool IsFiring;
    public float MaxRange;
    public bool HasHit;
    public float HitDistance;
    public float DamagePerSecond;
}

[BurstCompile]
public partial struct CastBeams : IJobEntity
{
    public float m_deltaTime;
    [ReadOnly]
    public CollisionWorld m_collisionWorld;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Damageable> m_damageableLookup;

    private void Execute (ref BeamSource beamSource, in LocalToWorld localToWorld)
    {
        beamSource.HasHit = false;
        beamSource.HitDistance = beamSource.MaxRange;
        
        if (beamSource.IsFiring)
        {
            RaycastInput ri = new RaycastInput()
            {
                Start = localToWorld.Position,
                End = localToWorld.Position + beamSource.MaxRange * localToWorld.Forward,
                Filter = PhysicsConfiguration.GetDamageDealerFilter()
            };

            if (m_collisionWorld.CastRay(ri, out RaycastHit closestHit))
            {
                if (m_damageableLookup.HasComponent(closestHit.Entity))
                {
                    var d = m_damageableLookup[closestHit.Entity];
                    d.CurrentHealth -= beamSource.DamagePerSecond * m_deltaTime;
                    m_damageableLookup[closestHit.Entity] = d;
                }
                beamSource.HasHit = true;
                beamSource.HitDistance = math.length(closestHit.Position - localToWorld.Position);
            }
        }
    }
}

public partial struct BeamUpdateSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        new CastBeams()
        {
            m_deltaTime = SystemAPI.Time.DeltaTime,
            m_collisionWorld = physicsWorld.CollisionWorld,
            m_damageableLookup = SystemAPI.GetComponentLookup<Damageable>()
        }.ScheduleParallel();
    }
}
