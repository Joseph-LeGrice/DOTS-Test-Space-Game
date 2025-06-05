using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.VFX;

public partial class GravityTetherVFXSystem : SystemBase 
{
    protected override void OnUpdate()
    {
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(EntityManager.WorldUnmanaged);

        var playerData = SystemAPI.GetSingleton<PlayerData>();
        var localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        
        foreach (var (gravityTetherRef, self) in SystemAPI.Query<RefRO<GravityTether>>().WithEntityAccess())
        {
            if (SystemAPI.HasComponent<Disabled>(gravityTetherRef.ValueRO.BeamFXEntity))
            {
                ecb.RemoveComponent<Disabled>(gravityTetherRef.ValueRO.BeamFXEntity);
            }
            
            Entity beamFxEntity = gravityTetherRef.ValueRO.BeamFXEntity;
            VisualEffect vfx = SystemAPI.ManagedAPI.GetComponent<VisualEffect>(beamFxEntity);
            vfx.enabled = gravityTetherRef.ValueRO.IsFiring;

            float3 tetherTargetPosition = localToWorldLookup[self].Position + gravityTetherRef.ValueRO.MaxRange * playerData.AimDirection;
            foreach (var tetherJoint in SystemAPI.Query<RefRO<GravityTetherJoint>>())
            {
                if (tetherJoint.ValueRO.OwnerEntity == self)
                {
                    var attachedLocalToWorld = localToWorldLookup[tetherJoint.ValueRO.AttachedEntity];
                    tetherTargetPosition = attachedLocalToWorld.Position;
                }
            }
            vfx.SetMatrix4x4("Attachment", float4x4.Translate(tetherTargetPosition));
        }
    }
}
