using Unity.Entities;
using UnityEngine;

public class BeamSourceBaker : MonoBehaviour
{
    public float MaxRange;
    public float DamagePerSecond;
    
    public class Baker : Baker<BeamSourceBaker>
    {
        public override void Bake(BeamSourceBaker authoring)
        {
            Entity mainEntity = GetEntity(TransformUsageFlags.None);
            AddComponent(mainEntity, new BeamSource()
            {
                MaxRange = authoring.MaxRange,
                DamagePerSecond = authoring.DamagePerSecond
            });
        }
    }
}
