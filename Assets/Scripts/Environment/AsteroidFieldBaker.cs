using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class AsteroidFieldBaker : MonoBehaviour
{
    public AsteroidSettingsBaker AsteroidSettings;
    public float AsteroidFieldRadius;
    public int AsteroidFieldDensity;

    public class Baker : Baker<AsteroidFieldBaker>
    {
        public override void Bake(AsteroidFieldBaker authoring)
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
}
