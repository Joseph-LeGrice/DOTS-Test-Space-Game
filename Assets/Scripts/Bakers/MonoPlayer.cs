using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

class MonoPlayer : MonoBehaviour
{
    public float ForwardSpeed;
    public float TurnSpeedDegreesPerSecond;
}

class MonoPlayerBaker : Baker<MonoPlayer>
{
    public override void Bake(MonoPlayer authoring)
    {
        Entity mainEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(mainEntity, new PlayerData()
        {
            MovementSpeed = authoring.ForwardSpeed,
            TurnSpeed = authoring.TurnSpeedDegreesPerSecond,
        });
        AddComponentObject(mainEntity, new PlayerManagedAccess()
        {
            ManagedLocalPlayer = ManagedSceneAccess.Instance.GetPlayer(),
        });
    }
}
