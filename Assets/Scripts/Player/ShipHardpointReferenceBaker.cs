using Unity.Entities;
using UnityEngine;

public class ShipHardpointReferenceBaker : MonoBehaviour
{
    public Rigidbody RelatedRigidbody;
    public GameObject EquippedWeapon;
    
    class Baker : Baker<ShipHardpointReferenceBaker>
    {
        public override void Bake(ShipHardpointReferenceBaker authoring)
        {
            Entity self = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(self, new ShipHardpointInstance()
            {
                WeaponInstanceEntity = GetEntity(authoring.EquippedWeapon, TransformUsageFlags.Dynamic),
                RelatedRigidbodyEntity = GetEntity(authoring.RelatedRigidbody.gameObject, TransformUsageFlags.Dynamic),
                IsFiring = false,
            });
        }
    }
}
