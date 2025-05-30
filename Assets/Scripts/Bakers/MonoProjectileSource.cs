using Unity.Entities;
using UnityEngine;

class MonoProjectileSource : MonoBehaviour
{
    public MonoProjectileSourceConfiguration projectileSourceConfiguration;
    public Rigidbody RelatedRigidbody;
}

class ProjectileSourceBaker : Baker<MonoProjectileSource>
{
    public override void Bake(MonoProjectileSource authoring)
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
