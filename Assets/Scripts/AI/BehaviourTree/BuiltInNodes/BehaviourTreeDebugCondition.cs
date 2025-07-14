using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public struct BehaviourTreeDebugConditionBurstable
{
    public bool m_condition;
    
    [BurstCompile]
    [AOT.MonoPInvokeCallback(typeof(BurstableBehaviourTreeNode.DoActionDelegate))]
    public static BehaviourActionResult BurstableDoAction(ref BurstableBehaviourTree behaviourTree,
        ref BurstableBehaviourTreeNode node, ref ECSBehaviourTreeBlackboard blackboard)
    {
        ref BehaviourTreeDebugConditionBurstable data = ref node.GetNodeDataReference<BehaviourTreeDebugConditionBurstable>();
        
        if (data.m_condition)
        {
            return BehaviourActionResult.Success;
        }
        else
        {
            return BehaviourActionResult.Failure;
        }
    }
}

[System.Serializable]
public class BehaviourTreeDebugCondition : BehaviourTreeNodeImplementation
{
    [SerializeField]
    private bool m_condition;

    public override string GetNodeName()
    {
        return "Debug Condition Node";
    }
    
    public override BehaviourActionResult DoActionManaged(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard)
    {
        if (m_condition)
        {
            return BehaviourActionResult.Success;
        }
        else
        {
            return BehaviourActionResult.Failure;
        }
    }

    public override void PopulateBurstable(ref BlobBuilder builder, ref BurstableBehaviourTreeNode node)
    {
        ref BehaviourTreeDebugConditionBurstable data =
            ref AllocateNodeData<BehaviourTreeDebugConditionBurstable>(ref builder, ref node);
        data.m_condition = m_condition;
        node.DoActionBurstable = BurstCompiler.CompileFunctionPointer<BurstableBehaviourTreeNode.DoActionDelegate>(BehaviourTreeDebugConditionBurstable.BurstableDoAction);
    }
}
