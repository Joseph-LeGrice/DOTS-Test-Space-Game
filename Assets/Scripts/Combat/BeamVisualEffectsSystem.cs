using Unity.Entities;
using UnityEngine.VFX;

public partial class BeamVisualEffectsSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(EntityManager.WorldUnmanaged);
        
        foreach (var beamSource in SystemAPI.Query<RefRO<BeamSource>>())
        {
            if (SystemAPI.HasComponent<Disabled>(beamSource.ValueRO.BeamFXEntity))
            {
                ecb.RemoveComponent<Disabled>(beamSource.ValueRO.BeamFXEntity);
            }
            VisualEffect vfx = SystemAPI.ManagedAPI.GetComponent<VisualEffect>(beamSource.ValueRO.BeamFXEntity);
            vfx.enabled = beamSource.ValueRO.IsFiring;
            if (beamSource.ValueRO.HasHit)
            {
                vfx.SetFloat("Length", beamSource.ValueRO.HitDistance);
            }
            else
            {
                vfx.SetFloat("Length", beamSource.ValueRO.MaxRange);
            }
        }
    }
}
