using Unity.Entities;
using Unity.Mathematics;

public struct Damageable : IComponentData
{
    public float CurrentHealth;
    public float MaxHealth;
    public Entity SpawnOnDestroy;

    public static Damageable WithHealth(float health)
    {
        return new Damageable()
        {
            CurrentHealth = health,
            MaxHealth = health,
        };
    }
}

public struct ImpactDamage : IComponentData
{
    public float FlatDamage;
    public Entity ImpactEffectEntity;

    public static ImpactDamage WithFlatDamage(float damage, Entity impactEffect)
    {
        return new ImpactDamage()
        {
            FlatDamage = damage,
            ImpactEffectEntity = impactEffect,
        };
    }
}

public struct Explosion : IComponentData
{
    public float Force;
    public float Radius;
    public float Damage;
}

public struct DetachablePart : IBufferElementData
{
    public Entity EffectPrefab;
    public Entity DetachableEntityPrefab;
    public float4x4 LocalTransform;
}
