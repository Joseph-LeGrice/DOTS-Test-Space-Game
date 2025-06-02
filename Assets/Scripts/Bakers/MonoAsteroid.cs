using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class MonoAsteroid : MonoBehaviour
{
    public float Health = 100.0f;

    public GameObject DetachablePrefab;
    public List<Transform> Detachables = new List<Transform>();
}

public class MonoAsteroidBaker : Baker<MonoAsteroid>
{
    public override void Bake(MonoAsteroid authoring)
    {
        Entity e = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<Asteroid>(e);
        AddComponent(e, Damageable.WithHealth(authoring.Health));
        if (authoring.Detachables.Count > 0)
        {
            DynamicBuffer<DetachablePart> dp = AddBuffer<DetachablePart>(e);
            foreach (Transform d in authoring.Detachables)
            {
                dp.Add(new DetachablePart()
                {
                    DetachableEntityPrefab = GetEntity(authoring.DetachablePrefab, TransformUsageFlags.Dynamic),
                    LocalTransform = Matrix4x4.TRS(d.localPosition, d.localRotation, d.localScale),
                });
            }
        }
    }
}