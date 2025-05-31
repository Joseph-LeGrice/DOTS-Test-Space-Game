using Unity.Entities;

public struct Damageable : IComponentData
{
    public float CurrentHealth;
    public float MaxHealth;

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

public struct DetachablePart : IBufferElementData
{
    public Entity EffectPrefab;
    public Entity DetachableEntity;
    public float ImpulseForceMinimum;
    public float ImpulseForceMaximum;
    public float AngularForceMinimum;
    public float AngularForceMaximum;
}
