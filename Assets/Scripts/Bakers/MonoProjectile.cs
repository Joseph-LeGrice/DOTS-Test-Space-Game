using Unity.Entities;
using UnityEngine;

class MonoProjectile : MonoBehaviour
{
    public GameObject ImpactEffectPrefab;
    public float ImpactDamage;
}

// class MonoProjectileBaker : Baker<MonoProjectile>
// {
//     public override void Bake(MonoProjectile authoring)
//     {
//         Entity mainEntity = GetEntity(TransformUsageFlags.Dynamic);
//         AddComponent(mainEntity, new Prefab());
//         AddComponent(mainEntity, new Projectile()
//         {
//             FlatDamage = authoring.ImpactDamage,
//             ImpactEffectEntity = GetEntity(authoring.ImpactEffectPrefab, TransformUsageFlags.Renderable)
//         });
//         AddComponent<MarkForCleanup>(mainEntity);
//     }
// }
