using Unity.Entities;
using UnityEngine;

public struct EnemyAI : IComponentData
{
    public Entity ControllingShip;
}

public class EnemyAIBaker : MonoBehaviour
{
    [SerializeField]
    private ShipBaker m_enemyShip;
    
    public class Baker : Baker<EnemyAIBaker>
    {
        public override void Bake(EnemyAIBaker authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);
            AddComponent(e, new EnemyAI() { ControllingShip = GetEntity(authoring.m_enemyShip, TransformUsageFlags.Dynamic) });
        }
    }
}
