using Unity.Entities;

public struct AsteroidField : IComponentData
{
    public BlobAssetReference<AsteroidSettingsBlob> AsteroidSettings;
    public float AsteroidFieldRadius;
    public float AsteroidFieldDensity;
}
