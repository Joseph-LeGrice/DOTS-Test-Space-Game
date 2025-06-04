using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WriteGroup(typeof(LocalToWorld))]
public struct Projectile : IComponentData
{
    public float3 Velocity;
    public float FlatDamage;
    public Entity ImpactEffectEntity;
}

public struct ProjectileSourceConfiguration : IComponentData
{
    public Entity ProjectilePrefab;
    public Entity ImpactEffectPrefab;
    public float ProjectileSpeed;
    public float ProjectileSpawnRate;
    public float ProjectileLifetime;
    public float ProjectileDamage;
}

public struct ProjectileSource : IComponentData
{
    public Entity ProjectileWeaponEntity;
    public Entity RelatedRigidbodyEntity;
    public float NextSpawnTime;
    public bool IsFiring;
}
