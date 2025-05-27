using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct AsteroidField : IComponentData
{
    public bool IsCreated;
    public BlobAssetReference<AsteroidSettingsBlob> Settings;
    public float AsteroidFieldRadius;
    public float AsteroidFieldDensity;

    public NativeArray<int> AsteroidType;
    public NativeArray<int> AsteroidMeshIndex;
    public NativeArray<float3> AsteroidLocalPositions;
    public NativeArray<bool> AsteroidStates;
}

public struct Asteroid : IComponentData
{
    public float3 TumbleAxis;
    public float TumbleSpeed;
}
