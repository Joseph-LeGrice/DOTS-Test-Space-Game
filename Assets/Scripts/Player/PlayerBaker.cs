using Unity.Entities;
using UnityEngine;

class PlayerBaker : MonoBehaviour
{
    public ThrusterSetup DefaultMovement;
    public ThrusterSetup ADSMovement;
    
    public float MaximumVelocity;
    public float VelocityDamperDecelerationDefault;
    public float VelocityDamperDecelerationADS;
    
    public float BoostTime;
    public float BoostRechargeTime;
    public float BoostAcceleration;
    public float BoostMaximumVelocity;
    
    public float MaxTurnSpeed;
    public float MaxRollSpeed;
    
    public GameObject[] ShipHardpoints;
    
    public class Baker : Baker<PlayerBaker>
    {
        public override void Bake(PlayerBaker authoring)
        {
            Entity mainEntity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(mainEntity, new PlayerData()
            {
                DefaultMovement = authoring.DefaultMovement,
                ADSMovement = authoring.ADSMovement,
                MaximumVelocity = authoring.MaximumVelocity,
                VelocityDamperDecelerationDefault = authoring.VelocityDamperDecelerationDefault,
                VelocityDamperDecelerationADS = authoring.VelocityDamperDecelerationADS,
                MaxTurnSpeed = authoring.MaxTurnSpeed,
                MaxRollSpeed = authoring.MaxRollSpeed,
                BoostTime = authoring.BoostTime,
                BoostRechargeTime = authoring.BoostRechargeTime,
                BoostAcceleration = authoring.BoostAcceleration,
                BoostMaximumVelocity = authoring.BoostMaximumVelocity,
            });
            AddComponentObject(mainEntity, new PlayerManagedAccess()
            {
                ManagedLocalPlayer = ManagedSceneAccess.Instance.GetPlayer(),
            });
            AddComponent<PlayerBoosterState>(mainEntity);
            
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

