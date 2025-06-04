using Unity.Entities;
using UnityEngine;

public class DetachableBaker : MonoBehaviour
{
    public class Baker : Baker<DetachableBaker>
    {
        public override void Bake(DetachableBaker authoring)
        {
            Entity self = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(self, new RequestPhysicCollisionFilterUpdate(PhysicsConfiguration.GetDamageReceiverFilter()));
        }
    }
}

