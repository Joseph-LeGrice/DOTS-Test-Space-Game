using Unity.Entities;
using UnityEngine;

public struct ProjectileSourceData : IComponentData
{
    public float NextSpawnTime;
    public Entity RelatedHardpoint;
}

public struct ProjectileSourceConfiguration : IComponentData
{
    public Entity FireNode;
    public Entity ProjectilePrefab;
    public Entity ImpactEffectPrefab;
    public float ProjectileSpeed;
    public float ProjectileSpawnRate;
    public float ProjectileLifetime;
    public float ProjectileDamage;
}

public class ProjectileSourceBaker : MonoBehaviour
{
    public ShipHardpointReferenceBaker RelatedHardpoint;
    public GameObject ProjectilePrefab;
    public GameObject ImpactEffectPrefab;
    public GameObject GimbalHierarchy;
    public GameObject FireNode;
    public float ProjectileSpeed = 250.0f;
    public float ImpactDamage = 10.0f;
    public float FireRate = 0.2f;
    public float Lifetime = 3.0f;
    
    public class Baker : Baker<ProjectileSourceBaker>
    {
        public override void Bake(ProjectileSourceBaker authoring)
        {
            Entity mainEntity = GetEntity(TransformUsageFlags.None);

            AddComponent(mainEntity, new ProjectileSourceData()
            {
                NextSpawnTime = 0.0f,
                RelatedHardpoint = GetEntity(authoring.RelatedHardpoint.gameObject, TransformUsageFlags.Dynamic) 
            });
            AddComponent(mainEntity, new ProjectileSourceConfiguration()
            {
                FireNode = GetEntity(authoring.FireNode, TransformUsageFlags.Dynamic),
                ProjectilePrefab = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic),
                ImpactEffectPrefab = GetEntity(authoring.ImpactEffectPrefab, TransformUsageFlags.Dynamic),
                ProjectileDamage = authoring.ImpactDamage,
                ProjectileSpeed = authoring.ProjectileSpeed,
                ProjectileSpawnRate = authoring.FireRate,
                ProjectileLifetime = authoring.Lifetime,
            });
            AddComponent(mainEntity, new Gimbal()
            {
                GimbalEntity = GetEntity(authoring.GimbalHierarchy, TransformUsageFlags.Dynamic)
            });
        }
    }
}

