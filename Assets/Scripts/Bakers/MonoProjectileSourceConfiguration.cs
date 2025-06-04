using Unity.Entities;
using UnityEngine;

class MonoProjectileSourceConfiguration : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public GameObject ImpactEffectPrefab;
    public float ProjectileSpeed = 250.0f;
    public float ImpactDamage = 10.0f;
    public float FireRate = 0.2f;
    public float Lifetime = 3.0f;
}

class ProjectileSourceConfigurationBaker : Baker<MonoProjectileSourceConfiguration>
{
    public override void Bake(MonoProjectileSourceConfiguration authoring)
    {
        Entity mainEntity = GetEntity(TransformUsageFlags.None);
        AddComponent(mainEntity, new ProjectileSourceConfiguration()
        {
            ProjectilePrefab = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic),
            ImpactEffectPrefab = GetEntity(authoring.ImpactEffectPrefab, TransformUsageFlags.Dynamic),
            ProjectileDamage = authoring.ImpactDamage,
            ProjectileSpeed = authoring.ProjectileSpeed,
            ProjectileSpawnRate = authoring.FireRate,
            ProjectileLifetime = authoring.Lifetime,
        });
    }
}
