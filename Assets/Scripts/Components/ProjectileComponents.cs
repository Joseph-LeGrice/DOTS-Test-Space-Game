using Unity.Entities;
using Unity.Mathematics;

public struct Projectile : IComponentData
{
    public float3 Velocity;
}

public struct ProjectileSourceConfiguration : IComponentData
{
    public Entity ProjectilePrefab;
    public float ProjectileSpeed;
    public float ProjectileSpawnRate;
    public float ProjectileLifetime;
}

public struct ProjectileSource : IComponentData
{
    public Entity ProjectileWeaponEntity;
    public Entity RelatedRigidbodyEntity;
    public float NextSpawnTime;
    public bool IsFiring;
}
