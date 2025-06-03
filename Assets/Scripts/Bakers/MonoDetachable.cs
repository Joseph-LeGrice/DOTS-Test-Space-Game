using Unity.Entities;
using UnityEngine;

public class MonoDetachable : MonoBehaviour
{
}

public class MonoDetachableBaker : Baker<MonoDetachable>
{
    public override void Bake(MonoDetachable authoring)
    {
        Entity self = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(self, new RequestPhysicCollisionFilterUpdate(PhysicsConfiguration.GetDamageReceiverFilter()));
    }
}
