using Unity.Burst;
using Unity.Entities;

public struct BurstableBehaviourTreeNode
{
    public delegate int DoActionDelegate(ref ECSBehaviourTreeBlackboard blackboard);
    
    public int NodeReferenceIndex;
    public BlobArray<byte> NodeData;
    public FunctionPointer<DoActionDelegate> DoAction;
}

public struct BurstableBehaviourTree
{
    public BlobArray<BurstableBehaviourTreeNode> m_allNodes;

    public int ExecuteNode(int i, ref ECSBehaviourTreeBlackboard blackboard)
    {
        foreach (BurstableBehaviourTreeNode n in m_allNodes.ToArray())
        {
            if (n.NodeReferenceIndex == i)
            {
                return n.DoAction.Invoke(ref blackboard);
            }
        }
        return 0;
    }
}

public struct ECSBehaviourTreeBlackboard : IComponentData
{
    
}

public struct ECSBehaviourTree : IComponentData
{
    public int m_currentActionIndex;
    public BlobAssetReference<BurstableBehaviourTree> BehaviourTree; // Shared Component?
}

public partial struct BehaviourTreeUpdateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (ecsBehaviourTree, blackboard) in SystemAPI.Query<RefRW<ECSBehaviourTree>, RefRW<ECSBehaviourTreeBlackboard>>())
        {
            ref BurstableBehaviourTree behaviourTree = ref ecsBehaviourTree.ValueRO.BehaviourTree.Value;
            ecsBehaviourTree.ValueRW.m_currentActionIndex = behaviourTree.ExecuteNode(ecsBehaviourTree.ValueRO.m_currentActionIndex, ref blackboard.ValueRW);
        }
    }
}
