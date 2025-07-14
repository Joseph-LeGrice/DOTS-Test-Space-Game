using System;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

public struct BurstableBehaviourTreeNode
{
    public delegate BehaviourActionResult DoActionDelegate(ref BurstableBehaviourTree behaviourTree, ref BurstableBehaviourTreeNode node, ref ECSBehaviourTreeBlackboard blackboard);
    
    public int NodeReferenceIndex;
    public BlobArray<byte> NodeData;
    public FunctionPointer<DoActionDelegate> DoActionBurstable;

    public BehaviourActionResult Execute(ref BurstableBehaviourTree behaviourTree, ref ECSBehaviourTreeBlackboard blackboard)
    {
        return DoActionBurstable.Invoke(ref behaviourTree, ref this, ref blackboard);
    }

    public unsafe ref T GetNodeDataReference<T>() where T : unmanaged
    {
        return ref UnsafeUtility.AsRef<T>((byte*)NodeData.GetUnsafePtr());
    }
}

public struct BurstableBehaviourTree
{
    public int InitialNodeReferenceIndex;
    public BlobArray<BurstableBehaviourTreeNode> m_allNodes;

    public BehaviourActionResult Execute(ref ECSBehaviourTreeBlackboard blackboard)
    {
        return GetNode(InitialNodeReferenceIndex).Execute(ref this, ref blackboard);
    }

    public ref BurstableBehaviourTreeNode GetNode(int nodeReference)
    {
        for (int i=0; i<m_allNodes.Length; i++)
        {
            if (m_allNodes[i].NodeReferenceIndex == nodeReference)
            {
                return ref m_allNodes[i];
            }
        }
        throw new System.Exception("Node not found");
    }
}

public struct ECSBehaviourTreeBlackboard : IComponentData
{
    
}

public struct ECSBehaviourTree : IComponentData
{
    public BlobAssetReference<BurstableBehaviourTree> BehaviourTree;

    public static ECSBehaviourTree From(BlobAssetReference<BurstableBehaviourTree> behaviourTreeBlobReference)
    {
        return new ECSBehaviourTree()
        {
            BehaviourTree = behaviourTreeBlobReference
        };
    }
}

public partial struct BehaviourTreeUpdateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (ecsBehaviourTree, blackboard) in SystemAPI.Query<RefRW<ECSBehaviourTree>, RefRW<ECSBehaviourTreeBlackboard>>())
        {
            ref BurstableBehaviourTree behaviourTree = ref ecsBehaviourTree.ValueRO.BehaviourTree.Value;
            behaviourTree.Execute(ref blackboard.ValueRW);
        }
    }
}
