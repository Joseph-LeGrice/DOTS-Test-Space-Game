using Unity.Entities;
using UnityEngine;

public struct Explosion : IComponentData
{
    public int FrameDelay;
    public float Force;
    public float Radius;
    public float Damage;
}

public class ExplosionBaker : MonoBehaviour
{
    public float Radius;
    public float Damage;
    public float Force;
    public float Lifetime;
    // TODO: Configure Falloff
    
    public class Baker : Baker<ExplosionBaker>
    {
        public override void Bake(ExplosionBaker authoring)
        {
            Entity self = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(self, new Explosion()
            {
                Damage = authoring.Damage,
                Radius = authoring.Radius,
                Force = authoring.Force,
                FrameDelay = 1,
            });
            AddComponent(self, new QueueForCleanup(authoring.Lifetime));
        }
    }
}

