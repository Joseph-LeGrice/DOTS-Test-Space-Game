using Unity.Entities;
using UnityEngine;

class MonoProjectileSourceConfiguration : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public float ProjectileSpeed;
    public float FireRate;
}

class ProjectileSourceConfigurationBaker : Baker<MonoProjectileSourceConfiguration>
{
    public override void Bake(MonoProjectileSourceConfiguration authoring)
    {
        Entity mainEntity = GetEntity(TransformUsageFlags.None);
        AddComponent(mainEntity, new ProjectileSourceConfiguration()
        {
            ProjectilePrefab = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic),
            ProjectileSpeed = authoring.ProjectileSpeed,
            ProjectileSpawnRate = authoring.FireRate,
        });
    }
}
