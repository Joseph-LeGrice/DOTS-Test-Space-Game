using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random; 

class MonoAsteroidField : MonoBehaviour
{
    public AsteroidSettings Settings;
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
            Settings = authoring.Settings.GetBlobDataReference(this),
            AsteroidFieldRadius = authoring.AsteroidFieldRadius,
            AsteroidFieldDensity = authoring.AsteroidFieldDensity,
        });
        
        DynamicBuffer<AsteroidBufferData> asteroidData = AddBuffer<AsteroidBufferData>(e);
        asteroidData.Length = authoring.AsteroidFieldDensity;
        
        int numTypes = authoring.Settings.AsteroidTypes.Count;
        
        Random r = new Random(1);
        for (int i = 0; i < authoring.AsteroidFieldDensity; i++)
        {
            int asteroidTypeIndex = r.NextInt(numTypes); 
            int numMeshes = authoring.Settings.AsteroidTypes[asteroidTypeIndex].PossibleMeshHashes.Count;
            int meshIndex = r.NextInt(numMeshes);
            asteroidData[i] = new AsteroidBufferData()
            {
                Type = asteroidTypeIndex,
                MeshIndex = meshIndex,
                State = true,
                LocalPosition = authoring.AsteroidFieldRadius * r.NextFloat3(),
            };
        }
    }
}
