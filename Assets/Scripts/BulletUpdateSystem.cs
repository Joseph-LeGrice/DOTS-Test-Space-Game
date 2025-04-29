using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

struct Bullet : IComponentData
{
    public float3 Velocity;
    public float Lifetime;
}

partial struct BulletUpdateSystem : ISystem
{
    private EntityArchetype m_bulletArchetype;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        NativeArray<ComponentType> componentTypes = new NativeArray<ComponentType>(3, Allocator.Temp);
        componentTypes[0] = ComponentType.ReadWrite<LocalToWorld>();
        componentTypes[1] = ComponentType.ReadWrite<RenderMesh>();
        componentTypes[2] = ComponentType.ReadWrite<Bullet>();
        
        m_bulletArchetype = state.EntityManager.CreateArchetype(componentTypes);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
