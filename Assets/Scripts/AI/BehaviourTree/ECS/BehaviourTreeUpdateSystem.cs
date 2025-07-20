using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct ECSBehaviourTreeBlackboardValue : IBufferElementData
{
    public FixedString128Bytes Identifier;
    public unsafe fixed byte Data[128];
}

public static class ECSBehaviourTreeBlackboardValueHelpers
{
    private static unsafe ECSBehaviourTreeBlackboardValue* FindValue(this DynamicBuffer<ECSBehaviourTreeBlackboardValue> buffer, string identifier)
    {

        foreach (var keyValue in buffer)
        {
            if (keyValue.Identifier == identifier)
            {
                return &keyValue;
            }
        }
        throw new Exception("Value not found in blackboard value buffer with identifier: " + identifier);
    }

    public static Entity GetEntity(this DynamicBuffer<ECSBehaviourTreeBlackboardValue> buffer, string identifier)
    {
        unsafe
        {
            return *(Entity*)buffer.FindValue(identifier)->Data[0];
        }
    }

    public static float3 GetFloat3(this DynamicBuffer<ECSBehaviourTreeBlackboardValue> buffer, string identifier)
    {
        unsafe
        {
            return *(float3*)buffer.FindValue(identifier)->Data[0];
        }
    }
}

public struct ECSDataAccessor
{
    public float DeltaTime;
    private ArchetypeChunk m_chunk;
    private NativeHashMap<FixedString128Bytes, DynamicComponentTypeHandle> m_componentHandleLookup;

    public static ECSDataAccessor From(ArchetypeChunk chunk,
        NativeHashMap<FixedString128Bytes, DynamicComponentTypeHandle> componentHandleLookup) => new() 
    {
        m_chunk = chunk,
        m_componentHandleLookup = componentHandleLookup
    };
    
    public NativeArray<T> GetComponentData<T>(string identifier) where T : unmanaged, IComponentData
    {
        unsafe
        {
            if (m_componentHandleLookup.TryGetValue(identifier, out DynamicComponentTypeHandle handle))
            {
                return m_chunk.GetDynamicComponentDataArrayReinterpret<T>(ref handle, sizeof(T));
            }
        }
        throw new Exception("ComponentData not found in ECSDataAccessor with identifier: " + identifier);
    }
}

[BurstCompile]
public struct BehaviourTreeUpdateJob : IJobChunk
{
    public SharedComponentTypeHandle<ECSBehaviourTree> m_behaviourTree;
    public BufferTypeHandle<ECSBehaviourTreeBlackboardValue> m_blackboardBuffers;
    public NativeHashMap<FixedString128Bytes, DynamicComponentTypeHandle> m_componentHandleLookup;
    
    public float DeltaTime;
    
    [BurstCompile]
    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        ECSBehaviourTree chunkBehaviourTree = chunk.GetSharedComponent(m_behaviourTree);
        ref var behaviourTree = ref chunkBehaviourTree.BehaviourTree.Value;

        BufferAccessor<ECSBehaviourTreeBlackboardValue> blackboards = chunk.GetBufferAccessor(ref m_blackboardBuffers);

        ECSDataAccessor ecsDataAccessor = ECSDataAccessor.From(chunk, m_componentHandleLookup);
        ecsDataAccessor.DeltaTime = DeltaTime;
        
        var entityEnum = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
        while(entityEnum.NextEntityIndex(out int entity))
        {
            DynamicBuffer<ECSBehaviourTreeBlackboardValue> thisBlackboard = blackboards[entity];
            behaviourTree.Execute(ref ecsDataAccessor, ref thisBlackboard);
        }
    }
}

public partial struct BehaviourTreeUpdateSystem : ISystem // TODO: Code generation this to create DynamicComponentTypeHandle for every component the every behaviour tree uses
{
    private NativeHashMap<FixedString128Bytes, DynamicComponentTypeHandle> m_componentHandleLookup;

    public void OnCreate(ref SystemState state)
    {
        // ECSBehaviourTree chunkBehaviourTree = new ECSBehaviourTree();// chunk.GetSharedComponent(m_behaviourTree);
        // ref var behaviourTree = ref chunkBehaviourTree.BehaviourTree.Value;
        //
        // m_componentHandleLookup = new NativeHashMap<FixedString128Bytes, DynamicComponentTypeHandle>();
        // for (int i = 0; i < behaviourTree.m_ecsTypeInfo.Length; i++)
        // {
        //     ref var t = ref behaviourTree.m_ecsTypeInfo[i];
        //     m_componentHandleLookup[t.Identifier] = state.GetDynamicComponentTypeHandle(t.ComponentDataType);
        // }
    }
    
    public void OnUpdate(ref SystemState state)
    {
        BehaviourTreeUpdateJob job = new BehaviourTreeUpdateJob();
        job.m_behaviourTree = SystemAPI.GetSharedComponentTypeHandle<ECSBehaviourTree>();
        job.m_componentHandleLookup = m_componentHandleLookup;
        job.DeltaTime = SystemAPI.Time.DeltaTime;
        // state.Dependency = job.Schedule(state.Dependency);
    }
}

