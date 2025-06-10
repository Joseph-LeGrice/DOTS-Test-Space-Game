using Unity.Entities;

[System.Serializable]
public struct ThrusterSetup
{
    public float ForwardThrustersAcceleration;
    public float LateralThrustersAcceleration;
    public float ReverseThrustersAcceleration;
}

public struct PlayerData : IComponentData
{
    public ThrusterSetup DefaultMovement;
    public ThrusterSetup ADSMovement;
    
    public float MaximumVelocity;
    public float VelocityDamperDecelerationDefault;
    public float VelocityDamperDecelerationADS;
    public float MaxTurnSpeed;
    public float MaxRollSpeed;
    
}

public class PlayerManagedAccess : IComponentData
{
    public ManagedLocalPlayer ManagedLocalPlayer;
}

public struct ShipHardpointBufferElement : IBufferElementData
{
    public bool Enabled;
    public Entity Self;
}
