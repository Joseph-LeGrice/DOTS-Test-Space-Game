using Unity.Entities;
using UnityEngine;

class MonoPlayer : MonoBehaviour
{
    public float Speed;
}

class MonoPlayerBaker : Baker<MonoPlayer>
{
    public override void Bake(MonoPlayer authoring)
    {
        GameObject cameraGameObject = GameObject.FindGameObjectWithTag("MainCamera");
        Entity mainEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(mainEntity, new PlayerData() { MovementSpeed = authoring.Speed });
        AddComponentObject(mainEntity, new PlayerManagedAccess() { CameraGameObject = cameraGameObject });
    }
}
