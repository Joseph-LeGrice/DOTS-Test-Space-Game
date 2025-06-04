using Unity.Entities;
using UnityEngine;

public class BeamSourceBaker : MonoBehaviour
{
    public class Baker : Baker<BeamSourceBaker>
    {
        public override void Bake(BeamSourceBaker authoring)
        {
            Entity mainEntity = GetEntity(TransformUsageFlags.None);
        }
    }
}

