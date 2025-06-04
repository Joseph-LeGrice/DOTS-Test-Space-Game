using Unity.Entities;
using UnityEngine;

class PlayerBaker : MonoBehaviour
{
    public ThrusterData ForwardThrusters;
    public ThrusterData LateralThrusters;
    public ThrusterData ReverseThrusters;
    public float VelocityDamperDeceleration;
    public float TurnSpeedDegreesPerSecond;
    
    public GameObject[] ShipHardpoints;
    
    public class Baker : Baker<PlayerBaker>
    {
        public override void Bake(PlayerBaker authoring)
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
            
            DynamicBuffer<ShipHardpointBufferElement> shbe = AddBuffer<ShipHardpointBufferElement>(mainEntity);
            foreach (GameObject sh in authoring.ShipHardpoints)
            {
                shbe.Add(new ShipHardpointBufferElement()
                {
                    Enabled = sh.activeSelf,
                    Self = GetEntity(sh, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}

