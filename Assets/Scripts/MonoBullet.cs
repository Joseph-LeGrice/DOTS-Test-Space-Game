using Unity.Entities;
using UnityEngine;

class MonoBullet : MonoBehaviour
{
    public float Speed;
    public float Lifetime;
}

class MonoBulletBaker : Baker<MonoBullet>
{
    public override void Bake(MonoBullet authoring)
    {
        Entity mainEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(mainEntity, new Prefab());
        AddComponent(mainEntity, new Bullet(authoring.Speed, authoring.Lifetime));
    }
}
