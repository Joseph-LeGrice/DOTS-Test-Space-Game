using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct ThrusterData
{
    public float Acceleration;
}

[System.Serializable]
public struct ThrusterSetup
{
    public ThrusterData ForwardThrusters;
    public ThrusterData LateralThrusters;
    public ThrusterData ReverseThrusters;
    public float MaximumVelocity;
}

public struct PlayerData : IComponentData
{
    public ThrusterSetup DefaultMovement;
    public ThrusterSetup ADSMovement;
    
    public float MaxTurnSpeed;
    public float MaxRollSpeed;
    
    public float VelocityDamperDeceleration;

    public bool IsADS;
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
