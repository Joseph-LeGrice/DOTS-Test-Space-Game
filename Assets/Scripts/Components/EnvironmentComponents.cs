using Unity.Entities;
using Unity.Mathematics;

public struct AsteroidField : IComponentData
{
    public bool IsCreated;
    public bool Initialized;
    public float AsteroidFieldRadius;
    public float AsteroidFieldDensity;
}

public struct AsteroidBufferData : IBufferElementData
{
    public bool Created;
    public int AsteroidType;
    public bool State;
    public float RotationSpeed;
    public float3 RotationAxis;
    public float3 LocalPosition;
}

public struct Asteroid : IComponentData
{
}

public struct AsteroidFieldSettings : IComponentData
{
}

public struct AsteroidTypeBufferData : IBufferElementData
{
    public Entity Prefab;
}
