using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MonoAsteroid : MonoBehaviour
{
    public float Health = 100.0f;
    public float Size;
    public int NumberOfDetachables;
    public GameObject DetachablePrefab;
}

public class MonoAsteroidBaker : Baker<MonoAsteroid>
{
    public override void Bake(MonoAsteroid authoring)
    {
        Entity e = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<Asteroid>(e);
        AddComponent(e, Damageable.WithHealth(authoring.Health));
        
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
    }
}