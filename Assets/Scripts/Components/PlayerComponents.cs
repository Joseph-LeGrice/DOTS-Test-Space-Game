using Unity.Entities;
using UnityEngine;

public struct PlayerData : IComponentData
{
    public float MovementSpeed;
    public float TurnSpeed;
}

public class PlayerManagedAccess : IComponentData
{
    public ManagedLocalPlayer ManagedLocalPlayer;
}
