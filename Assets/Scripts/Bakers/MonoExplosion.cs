using Unity.Entities;
using UnityEngine;

public class MonoExplosion : MonoBehaviour
{
    public float Radius;
    public float Damage;
    public float Force;
    public float Lifetime;
    // TODO: Configure Falloff
}

public class MonoExplosionBaker : Baker<MonoExplosion>
{
    public override void Bake(MonoExplosion authoring)
    {
        Entity self = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(self, new Explosion()
        {
            Damage = authoring.Damage,
            Radius = authoring.Radius,
            Force = authoring.Force,
        });
        AddComponent(self, new QueueForCleanup(authoring.Lifetime));
    }
}
