using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct ThrusterData
{
    public float Acceleration;
    public float MaximumVelocity;
}

public struct PlayerData : IComponentData
{
    public ThrusterData ForwardThrusters;
    public ThrusterData LateralThrusters;
    public ThrusterData ReverseThrusters;
    
    public float TurnSpeed;
    public float VelocityDamperDeceleration;
}

public class PlayerManagedAccess : IComponentData
{
    public ManagedLocalPlayer ManagedLocalPlayer;
}
