using Unity.Entities;
using UnityEngine;

public struct GravityTether : IComponentData
{
    public bool IsFiring;
    public float MaxRange;
    public float MinDistance;
    public Entity SourceRigidbodyEntity;
    public Entity BeamFXEntity;
}

public struct GravityTetherJoint : IComponentData
{
    public Entity OwnerEntity;
    public Entity AttachedEntity;

    public GravityTetherJoint(Entity ownerEntity, Entity attachedEntity)
    {
        OwnerEntity = ownerEntity;
        AttachedEntity = attachedEntity;
    }
}

public class GravityTetherBaker : MonoBehaviour
{
    public float MaxRange;
    public float MinDistance;
    public Rigidbody SourceEntityRigidbody;
    public GameObject BeamFX;
    
    public class Baker : Baker<GravityTetherBaker>
    {
        public override void Bake(GravityTetherBaker authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new GravityTether()
            {
                MaxRange = authoring.MaxRange,
                MinDistance = authoring.MinDistance,
                SourceRigidbodyEntity = GetEntity(authoring.SourceEntityRigidbody, TransformUsageFlags.Dynamic),
                BeamFXEntity = GetEntity(authoring.BeamFX, TransformUsageFlags.Dynamic), 
            });
        }
    }
}
