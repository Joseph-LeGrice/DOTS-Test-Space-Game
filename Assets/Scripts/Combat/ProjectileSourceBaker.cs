using Unity.Entities;
using UnityEngine;

class ProjectileSourceBaker : MonoBehaviour
{
    public ProjectileSourceConfigurationBaker projectileSourceConfiguration;
    public Rigidbody RelatedRigidbody;
    
    public class Baker : Baker<ProjectileSourceBaker>
    {
        public override void Bake(ProjectileSourceBaker authoring)
        {
            Entity mainEntity = GetEntity(TransformUsageFlags.None);
            
            Entity weaponEntity = Entity.Null;
            if (authoring.projectileSourceConfiguration != null)
            {
                weaponEntity = GetEntity(authoring.projectileSourceConfiguration.gameObject, TransformUsageFlags.Dynamic);
            }
            
            Entity rigidbodyEntity = Entity.Null;
            if (authoring.RelatedRigidbody != null)
            {
                rigidbodyEntity = GetEntity(authoring.RelatedRigidbody.gameObject, TransformUsageFlags.Dynamic);
            }

            AddComponent(mainEntity, new ProjectileSource()
            {
                ProjectileWeaponEntity = weaponEntity,
                RelatedRigidbodyEntity = rigidbodyEntity,
                NextSpawnTime = 0.0f
            });
        }
    }
}

