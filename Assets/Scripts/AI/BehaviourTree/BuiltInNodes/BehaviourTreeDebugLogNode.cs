using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public struct BehaviourTreeDebugLogNodeBurstable
{
    public BlobString m_message;
    
    [BurstCompile]
    [AOT.MonoPInvokeCallback(typeof(BurstableBehaviourTreeNode.DoActionDelegate))]
    public static BehaviourActionResult BurstableDoAction(ref BurstableBehaviourTree behaviourTree,
        ref BurstableBehaviourTreeNode node, ref ECSDataAccessor ecsDataAccessor, ref DynamicBuffer<ECSBehaviourTreeBlackboardValue> blackboardValueBuffer)
    {
        ref BehaviourTreeDebugLogNodeBurstable data = ref node.GetNodeDataReference<BehaviourTreeDebugLogNodeBurstable>();
        Debug.Log(data.m_message.ToString());
        return BehaviourActionResult.Success;
    }
}

[System.Serializable]
public class BehaviourTreeDebugLogNode : BehaviourTreeNodeImplementation
{
    [SerializeField]
    private string m_message;

    public override string GetNodeName()
    {
        return "Debug Log Node";
    }
    
    public override BehaviourActionResult DoActionManaged(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard)
    {
        Debug.Log(m_message);
        return BehaviourActionResult.Success;
    }

    public override void PopulateBurstable(ref BlobBuilder builder, ref BurstableBehaviourTreeNode node)
    {
        ref BehaviourTreeDebugLogNodeBurstable data = ref AllocateNodeData<BehaviourTreeDebugLogNodeBurstable>(ref builder, ref node);
        builder.AllocateString(ref data.m_message, m_message);
        node.DoActionBurstable = BurstCompiler.CompileFunctionPointer<BurstableBehaviourTreeNode.DoActionDelegate>(BehaviourTreeDebugLogNodeBurstable.BurstableDoAction);
    }
}
