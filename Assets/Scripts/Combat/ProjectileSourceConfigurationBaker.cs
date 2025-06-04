using Unity.Entities;
using UnityEngine;

public class ProjectileSourceConfigurationBaker : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public GameObject ImpactEffectPrefab;
    public float ProjectileSpeed = 250.0f;
    public float ImpactDamage = 10.0f;
    public float FireRate = 0.2f;
    public float Lifetime = 3.0f;
    
    public class Baker : Baker<ProjectileSourceConfigurationBaker>
    {
        public override void Bake(ProjectileSourceConfigurationBaker authoring)
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
}

