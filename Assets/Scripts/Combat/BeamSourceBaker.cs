using Unity.Entities;
using UnityEngine;

public struct BeamSource : IComponentData
{
    public Entity BeamFXEntity;
    public bool IsFiring;
    public float MaxRange;
    public bool HasHit;
    public float HitDistance;
    public float DamagePerSecond;
}

public class BeamSourceBaker : MonoBehaviour
{
    public float MaxRange;
    public float DamagePerSecond;
    public GameObject BeamFX;
    
    public class Baker : Baker<BeamSourceBaker>
    {
        public override void Bake(BeamSourceBaker authoring)
        {
            Entity mainEntity = GetEntity(TransformUsageFlags.None);
            AddComponent(mainEntity, new BeamSource()
            {
                BeamFXEntity = GetEntity(authoring.BeamFX, TransformUsageFlags.Dynamic),
                MaxRange = authoring.MaxRange,
                DamagePerSecond = authoring.DamagePerSecond
            });
        }
    }
}
