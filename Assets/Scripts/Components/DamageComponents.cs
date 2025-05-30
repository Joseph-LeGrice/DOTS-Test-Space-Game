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

    public static ImpactDamage WithFlatDamage(float damage)
    {
        return new ImpactDamage()
        {
            FlatDamage = damage
        };
    }
}
