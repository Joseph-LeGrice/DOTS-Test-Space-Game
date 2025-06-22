using Unity.Entities;
using UnityEngine;

public struct PlayerTag : IComponentData
{
    public Entity ControllingShip;
}

public class PlayerManagedAccess : IComponentData
{
    public ManagedLocalPlayer ManagedLocalPlayer;
}

class PlayerBaker : MonoBehaviour
{
    [SerializeField]
    private ShipBaker ControllingShip;
    
    public class Baker : Baker<PlayerBaker>
    {
        public override void Bake(PlayerBaker authoring)
        {
            Entity mainEntity = GetEntity(TransformUsageFlags.None);
            
            AddComponent(mainEntity, new PlayerTag()
            {
                ControllingShip = GetEntity(authoring.ControllingShip, TransformUsageFlags.Dynamic),
            });
            AddComponentObject(mainEntity, new PlayerManagedAccess()
            {
                ManagedLocalPlayer = ManagedSceneAccess.Instance.GetPlayer(),
            });
        }
    }
}

