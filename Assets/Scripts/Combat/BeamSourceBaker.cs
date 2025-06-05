using Unity.Entities;
using UnityEngine;
using UnityEngine.VFX;

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
