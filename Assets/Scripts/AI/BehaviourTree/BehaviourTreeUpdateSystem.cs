using Unity.Burst;
using Unity.Entities;

public struct BurstableBehaviourTreeNode
{
    public delegate int DoActionDelegate(ref BurstableBehaviourTree behaviourTree, ref ECSBehaviourTreeBlackboard blackboard);
    
    public int NodeReferenceIndex;
    public BlobArray<byte> NodeData;
    public FunctionPointer<DoActionDelegate> DoAction;
}

public struct BurstableBehaviourTree
{
    public int InitialNodeReferenceIndex;
    public BlobArray<BurstableBehaviourTreeNode> m_allNodes;

    public void Execute(ref ECSBehaviourTreeBlackboard blackboard)
    {
        m_allNodes[InitialNodeReferenceIndex].DoAction.Invoke(ref this, ref blackboard);
    }

    public /* ref */ BurstableBehaviourTreeNode GetNode(int nodeReference)
    {
        foreach (var node in m_allNodes.ToArray()) // Probably pretty slow
        {
            if (node.NodeReferenceIndex == nodeReference)
            {
                return node;
            }
        }
        return default(BurstableBehaviourTreeNode);
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
