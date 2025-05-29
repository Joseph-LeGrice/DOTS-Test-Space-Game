using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random; 

class MonoAsteroidField : MonoBehaviour
{
    public MonoAsteroidSettings AsteroidSettings;
    public float AsteroidFieldRadius;
    public int AsteroidFieldDensity;
}

class MonoAsteroidFieldBaker : Baker<MonoAsteroidField>
{
    public override void Bake(MonoAsteroidField authoring)
    {
        Entity e = GetEntity(TransformUsageFlags.WorldSpace);
        
        AddComponent(e, new AsteroidField()
        {
            AsteroidFieldRadius = authoring.AsteroidFieldRadius,
            AsteroidFieldDensity = authoring.AsteroidFieldDensity,
        });
        
        DynamicBuffer<AsteroidBufferData> asteroidData = AddBuffer<AsteroidBufferData>(e);
        
        Random r = new Random(1);
        for (int i = 0; i < authoring.AsteroidFieldDensity; i++)
        {
            int numTypes = authoring.AsteroidSettings.AsteroidTypes.Count;
            int asteroidTypeIndex = r.NextInt(numTypes);
            var asteroidTypeInfo = authoring.AsteroidSettings.AsteroidTypes[asteroidTypeIndex];
            asteroidData.Add(new AsteroidBufferData()
            {
                AsteroidType = asteroidTypeIndex,
                State = true,
                RotationAxis = r.NextFloat3(),
                RotationSpeed = r.NextFloat(asteroidTypeInfo.MinRotateSpeed, asteroidTypeInfo.MaxRotateSpeed),
                LocalPosition = authoring.AsteroidFieldRadius * r.NextFloat3(),
            });
        }
    }
}
