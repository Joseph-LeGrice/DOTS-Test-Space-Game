using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class MonoAsteroid : MonoBehaviour
{
}

public class MonoAsteroidBaker : Baker<MonoAsteroid>
{
    public override void Bake(MonoAsteroid authoring)
    {
        Entity e = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<Asteroid>(e);
    }
}