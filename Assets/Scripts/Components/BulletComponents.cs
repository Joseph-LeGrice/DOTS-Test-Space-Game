using Unity.Entities;
using Unity.Mathematics;

public struct Bullet : IComponentData
{
    public float3 CurrentVelocity;
    public float Speed;
    public float Lifetime;

    public Bullet(float Speed, float Lifetime)
    {
        this.Speed = Speed;
        this.Lifetime = Lifetime;
        this.CurrentVelocity = 0.0f;
    }
}

public struct BulletSource : IComponentData
{
    public Entity Prefab;
    public float SpawnRate;
    public float3 SpawnPosition;
    public float3 BulletFireDirection;
    public float NextSpawnTime;
}
