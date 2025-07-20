using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public struct BehaviourTreeConditionalNodeBurstable
{
    public int m_conditionalNode;
    public int m_actionNode;
    
    [BurstCompile]
    [AOT.MonoPInvokeCallback(typeof(BurstableBehaviourTreeNode.DoActionDelegate))]
    public static BehaviourActionResult BurstableDoAction(ref BurstableBehaviourTree behaviourTree,
        ref BurstableBehaviourTreeNode node, ref ECSDataAccessor ecsDataAccessor, ref DynamicBuffer<ECSBehaviourTreeBlackboardValue> blackboardValueBuffer)
    {
        ref BehaviourTreeConditionalNodeBurstable data = ref node.GetNodeDataReference<BehaviourTreeConditionalNodeBurstable>();
        var result = behaviourTree.GetNode(data.m_conditionalNode).Execute(ref behaviourTree, ref ecsDataAccessor, ref blackboardValueBuffer);
        
        if (result == BehaviourActionResult.Success)
        {
            return behaviourTree.GetNode(data.m_actionNode).Execute(ref behaviourTree, ref ecsDataAccessor, ref blackboardValueBuffer);
        }
        else
        {
            return BehaviourActionResult.Failure;
        }
    }
}

[System.Serializable]
public class BehaviourTreeConditionalNode : BehaviourTreeNodeImplementation
{
    [SerializeField]
    [BehaviourNodeReference]
    private int m_conditionalNode;
    [SerializeField]
    [BehaviourNodeReference]
    private int m_actionNode;

    public override string GetNodeName()
    {
        return "Conditional Node";
    }
    
    public override BehaviourActionResult DoActionManaged(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard)
    {
        var result = behaviourTree.GetNode(m_conditionalNode).DoAction(behaviourTree, ref blackboard);
        
        if (result == BehaviourActionResult.Success)
        {
            return behaviourTree.GetNode(m_actionNode).DoAction(behaviourTree, ref blackboard);
        }
        else
        {
            return BehaviourActionResult.Failure;
        }
    }

    public override void PopulateBurstable(ref BlobBuilder builder, ref BurstableBehaviourTreeNode node)
    {
        ref BehaviourTreeConditionalNodeBurstable data = ref AllocateNodeData<BehaviourTreeConditionalNodeBurstable>(ref builder, ref node);
        data.m_actionNode = m_actionNode;
        data.m_conditionalNode = m_conditionalNode;
        node.DoActionBurstable = BurstCompiler.CompileFunctionPointer<BurstableBehaviourTreeNode.DoActionDelegate>(BehaviourTreeConditionalNodeBurstable.BurstableDoAction);
    }
}
