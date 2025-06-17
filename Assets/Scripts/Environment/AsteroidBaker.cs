using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public struct Asteroid : IComponentData
{
}

public class AsteroidBaker : MonoBehaviour
{
    public float Health = 100.0f;
    public float Size;
    public int NumberOfDetachables;
    public GameObject DetachablePrefab;
    public GameObject DestroyExplosionEffect;
    
    public class Baker : Baker<AsteroidBaker>
    {
        public override void Bake(AsteroidBaker authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Asteroid>(e);
            AddComponent(e, new RequestPhysicCollisionFilterUpdate(PhysicsConfiguration.GetDamageReceiverFilter()));

            Damageable d = Damageable.WithHealth(authoring.Health);
            d.SpawnOnDestroy = GetEntity(authoring.DestroyExplosionEffect, TransformUsageFlags.Dynamic);
            AddComponent(e, d);

            DynamicBuffer<DetachablePart> dp = AddBuffer<DetachablePart>(e);
            for (int i=0; i<authoring.NumberOfDetachables; i++)
            {
                dp.Add(new DetachablePart()
                {
                    DetachableEntityPrefab = GetEntity(authoring.DetachablePrefab, TransformUsageFlags.Dynamic),
                    LocalTransform = Matrix4x4.TRS(
                        authoring.Size * UnityEngine.Random.insideUnitSphere,
                        Quaternion.Euler(UnityEngine.Random.insideUnitSphere), 
                        new float3(1.0f)
                    ),
                });
            }
            
            AddComponent<Targetable>(e);
        }
    }
}
