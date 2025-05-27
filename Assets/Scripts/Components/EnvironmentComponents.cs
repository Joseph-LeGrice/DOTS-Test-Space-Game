using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct AsteroidField : IComponentData
{
    public bool IsCreated;
    public BlobAssetReference<AsteroidSettingsBlob> Settings;
    public float AsteroidFieldRadius;
    public float AsteroidFieldDensity;

}

public struct AsteroidBufferData : IBufferElementData
{
    public int Type;
    public int MeshIndex;
    public float3 LocalPosition;
    public bool State;
}

public struct Asteroid : IComponentData
{
    public float3 TumbleAxis;
    public float TumbleSpeed;
}
