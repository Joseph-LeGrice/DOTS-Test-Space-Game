using Unity.Entities;
using UnityEngine;

class MonoAsteroidField : MonoBehaviour
{
    public AsteroidSettings Settings;
    public float AsteroidFieldRadius;
    public float AsteroidFieldDensity;
}

class MonoAsteroidFieldBaker : Baker<MonoAsteroidField>
{
    public override void Bake(MonoAsteroidField authoring)
    {
        Entity e = GetEntity(TransformUsageFlags.WorldSpace);
        AddComponent(e, new AsteroidField()
        {
            AsteroidSettings = authoring.Settings.GetBlobDataReference(this),
            AsteroidFieldRadius = authoring.AsteroidFieldRadius,
            AsteroidFieldDensity = authoring.AsteroidFieldDensity,
        });
    }
}
