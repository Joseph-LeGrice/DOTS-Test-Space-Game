using Unity.Entities;
using UnityEngine.VFX;

public partial class BeamVisualEffectsSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var (beamSource, self) in SystemAPI.Query<RefRO<BeamSource>>().WithEntityAccess())
        {
            VisualEffect vfx = SystemAPI.ManagedAPI.GetComponent<VisualEffect>(self);
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
