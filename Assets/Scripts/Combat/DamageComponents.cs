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

public struct Explosion : IComponentData
{
    public int FrameDelay;
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
