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
        Entity mainEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(mainEntity, new PlayerData() { MovementSpeed = authoring.Speed });
        AddComponentObject(mainEntity, new PlayerManagedAccess()
        {
            PlayerInput = ManagedSceneAccess.Instance.GetInputHandler(),
            CameraGameObject = ManagedSceneAccess.Instance.GetMainCamera().gameObject
        });
    }
}
