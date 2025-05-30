using Unity.Entities;
using UnityEngine;

class MonoProjectile : MonoBehaviour
{
    public GameObject ImpactEffectPrefab;
    public float ImpactDamage;
}

class MonoProjectileBaker : Baker<MonoProjectile>
{
    public override void Bake(MonoProjectile authoring)
    {
        Entity mainEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(mainEntity, new Prefab());
        AddComponent(mainEntity, new Projectile());
        AddComponent<MarkForCleanup>(mainEntity);
        AddComponent(mainEntity, ImpactDamage.WithFlatDamage(authoring.ImpactDamage, GetEntity(authoring.ImpactEffectPrefab, TransformUsageFlags.Renderable)));
    }
}
