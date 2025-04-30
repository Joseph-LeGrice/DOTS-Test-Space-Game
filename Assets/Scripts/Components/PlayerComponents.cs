using Unity.Entities;
using UnityEngine;

public struct PlayerData : IComponentData
{
    public float MovementSpeed;
}

public class PlayerManagedAccess : IComponentData
{
    public GameObject CameraGameObject;
}
