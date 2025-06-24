using Unity.Entities;
using UnityEngine;


class ShipBaker : MonoBehaviour
{
    public ThrusterSetup DefaultMovement;
    public ThrusterSetup ADSMovement;
    
    public float MaximumVelocity;
    
    public float BoostTime;
    public float BoostRechargeTime;
    public float BoostAcceleration;
    public float BoostMaximumVelocity;
    
    public GameObject[] ShipHardpoints;
    
    public class Baker : Baker<ShipBaker>
    {
        public override void Bake(ShipBaker authoring)
        {
            Entity mainEntity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent<ShipInput>(mainEntity, ShipInput.Default);
            AddComponent<Damageable>(mainEntity, Damageable.WithHealth(1000.0f));
            AddComponent(mainEntity, new ShipMovementData()
            {
                DefaultMovement = authoring.DefaultMovement,
                ADSMovement = authoring.ADSMovement,
                MaximumVelocity = authoring.MaximumVelocity,
                BoostTime = authoring.BoostTime,
                BoostRechargeTime = authoring.BoostRechargeTime,
                BoostAcceleration = authoring.BoostAcceleration,
                BoostMaximumVelocity = authoring.BoostMaximumVelocity,
            });
            AddComponent<ShipBoosterState>(mainEntity);
            
            DynamicBuffer<ShipHardpointReference> shbe = AddBuffer<ShipHardpointReference>(mainEntity);
            foreach (GameObject sh in authoring.ShipHardpoints)
            {
                shbe.Add(new ShipHardpointReference()
                {
                    Self = GetEntity(sh, TransformUsageFlags.Dynamic),
                });
            }
            
            AddComponent<Targetable>(mainEntity);
            AddComponent<TargetDetector>(mainEntity, new TargetDetector() { RangeSquared = Mathf.Pow(2500.0f, 2.0f)});
            AddBuffer<DetectedTarget>(mainEntity);
        }
    }
}

