using Unity.Entities;
using Unity.Mathematics;

public struct Projectile : IComponentData
{
    public float3 Velocity;
    public float Lifetime;

    public Projectile(float Lifetime)
    {
        this.Lifetime = Lifetime;
        this.Velocity = 0.0f;
    }
}

public struct ProjectileSourceConfiguration : IComponentData
{
    public Entity ProjectilePrefab;
    public float ProjectileSpeed;
    public float ProjectileSpawnRate;
}

public struct ProjectileSource : IComponentData
{
    public Entity ProjectileWeaponEntity;
    public Entity RelatedRigidbodyEntity;
    public float NextSpawnTime;
    public bool IsFiring;
}
