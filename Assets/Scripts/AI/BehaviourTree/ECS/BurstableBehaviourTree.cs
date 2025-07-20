using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

public struct BurstableBehaviourTreeNode
{
    public delegate BehaviourActionResult DoActionDelegate(ref BurstableBehaviourTree behaviourTree, ref BurstableBehaviourTreeNode node, ref ECSDataAccessor ecsDataAccessor, ref DynamicBuffer<ECSBehaviourTreeBlackboardValue> blackboard);
    
    public int NodeReferenceIndex;
    public BlobArray<byte> NodeData;
    public FunctionPointer<DoActionDelegate> DoActionBurstable;

    public BehaviourActionResult Execute(ref BurstableBehaviourTree behaviourTree, ref ECSDataAccessor ecsDataAccessor, ref DynamicBuffer<ECSBehaviourTreeBlackboardValue> blackboardValues)
    {
        return DoActionBurstable.Invoke(ref behaviourTree, ref this, ref ecsDataAccessor, ref blackboardValues);
    }

    public unsafe ref T GetNodeDataReference<T>() where T : unmanaged
    {
        return ref UnsafeUtility.AsRef<T>((byte*)NodeData.GetUnsafePtr());
    }
}

public struct ECSTypeInfo
{
    public FixedString128Bytes Identifier;
    public ComponentType ComponentDataType;
}

public struct BurstableBehaviourTree
{
    public int InitialNodeReferenceIndex;
    public BlobArray<BurstableBehaviourTreeNode> m_allNodes;
    public BlobArray<ECSTypeInfo> m_ecsTypeInfo;

    public BehaviourActionResult Execute(ref ECSDataAccessor ecsDataAccessor, ref DynamicBuffer<ECSBehaviourTreeBlackboardValue> blackboard)
    {
        return GetNode(InitialNodeReferenceIndex).Execute(ref this, ref ecsDataAccessor, ref blackboard);
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

public struct ECSBehaviourTree : ISharedComponentData // TODO: Perhaps make a chunk component?
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
