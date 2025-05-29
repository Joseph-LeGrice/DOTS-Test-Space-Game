using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public struct AsteroidField : IComponentData
{
    public bool IsCreated;
    public bool Initialized;
    public float AsteroidFieldRadius;
    public float AsteroidFieldDensity;
}

public struct AsteroidBufferData : IBufferElementData
{
    public Entity Instance;
    public int AsteroidType;
    public bool State;
    public float RotationSpeed;
    public float3 RotationAxis;
    public float3 LocalPosition;
}

public struct Asteroid : IComponentData
{
    public float3 TumbleAxis;
    public float TumbleSpeed;
}

public struct AsteroidFieldSettings : IComponentData
{
}

public struct AsteroidTypeBufferData : IBufferElementData
{
    public Entity Prefab;
}
