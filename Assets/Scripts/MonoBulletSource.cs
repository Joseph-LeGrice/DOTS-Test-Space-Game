using Unity.Entities;
using UnityEngine;

class MonoBulletSource : MonoBehaviour
{
    public GameObject Prefab;
    public float SpawnRate;
}

class BulletSourceBaker : Baker<MonoBulletSource>
{
    public override void Bake(MonoBulletSource authoring)
    {
        Entity mainEntity = GetEntity(TransformUsageFlags.None);
        AddComponent(mainEntity, new BulletSource()
        {
            Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
            SpawnRate = authoring.SpawnRate,
            SpawnPosition = authoring.transform.position,
            BulletFireDirection = authoring.transform.forward,
            NextSpawnTime = 0.0f
        });
    }
}
