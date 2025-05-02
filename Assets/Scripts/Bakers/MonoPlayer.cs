using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

class MonoPlayer : MonoBehaviour
{
    public ThrusterData ForwardThrusters;
    public ThrusterData LateralThrusters;
    public ThrusterData ReverseThrusters;
    public float VelocityDamperDeceleration;
    public float TurnSpeedDegreesPerSecond;
}

class MonoPlayerBaker : Baker<MonoPlayer>
{
    public override void Bake(MonoPlayer authoring)
    {
        Entity mainEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(mainEntity, new PlayerData()
        {
            ForwardThrusters = authoring.ForwardThrusters,
            LateralThrusters = authoring.LateralThrusters,
            ReverseThrusters = authoring.ReverseThrusters,
            VelocityDamperDeceleration = authoring.VelocityDamperDeceleration,
            TurnSpeed = authoring.TurnSpeedDegreesPerSecond,
        });
        AddComponentObject(mainEntity, new PlayerManagedAccess()
        {
            ManagedLocalPlayer = ManagedSceneAccess.Instance.GetPlayer(),
        });
    }
}
