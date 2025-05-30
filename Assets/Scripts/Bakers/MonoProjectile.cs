using Unity.Entities;
using UnityEngine;

class MonoProjectile : MonoBehaviour
{
    public float Lifetime = 10.0f;
    public float ImpactDamage;
}

class MonoProjectileBaker : Baker<MonoProjectile>
{
    public override void Bake(MonoProjectile authoring)
    {
        Entity mainEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(mainEntity, new Prefab());
        AddComponent(mainEntity, Projectile.WithLifetime(authoring.Lifetime));
        AddComponent(mainEntity, ImpactDamage.WithFlatDamage(authoring.ImpactDamage));
    }
}
