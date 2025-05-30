using Unity.Entities;
using UnityEngine;

class MonoProjectile : MonoBehaviour
{
    public float Lifetime = 10.0f;
}

class MonoProjectileBaker : Baker<MonoProjectile>
{
    public override void Bake(MonoProjectile authoring)
    {
        Entity mainEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(mainEntity, new Prefab());
        AddComponent(mainEntity, new Projectile(authoring.Lifetime));
    }
}
