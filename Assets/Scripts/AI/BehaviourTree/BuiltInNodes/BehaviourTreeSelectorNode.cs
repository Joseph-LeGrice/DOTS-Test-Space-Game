using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public struct BehaviourTreeSelectorNodeBurstable
{
    public BlobArray<int> m_actionNodes;

    [BurstCompile]
    [AOT.MonoPInvokeCallback(typeof(BurstableBehaviourTreeNode.DoActionDelegate))]
    public static BehaviourActionResult BurstableDoAction(ref BurstableBehaviourTree behaviourTree,
        ref BurstableBehaviourTreeNode node, ref ECSBehaviourTreeBlackboard blackboard)
    {
        ref BehaviourTreeSelectorNodeBurstable data =
            ref node.GetNodeDataReference<BehaviourTreeSelectorNodeBurstable>();
        
        for (int i = 0; i < data.m_actionNodes.Length; i++)
        {
            var result = behaviourTree.GetNode(data.m_actionNodes[i]).Execute(ref behaviourTree, ref blackboard);
            
            if (result == BehaviourActionResult.InProgress)
            {
                return BehaviourActionResult.InProgress;
            }
            
            if (result == BehaviourActionResult.Success)
            {
                return BehaviourActionResult.Success;
            }
        }
        
        return BehaviourActionResult.Failure;
    }
}

[System.Serializable]
public class BehaviourTreeSelectorNode : BehaviourTreeNodeImplementation
{
    [SerializeField]
    [BehaviourNodeReference]
    private List<int> m_actionNodes;

    public override string GetNodeName()
    {
        return "Selector Node";
    }
    
    public override BehaviourActionResult DoActionManaged(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard)
    {
        for (int i = 0; i < m_actionNodes.Count; i++)
        {
            var result = behaviourTree.GetNode(m_actionNodes[i]).DoAction(behaviourTree, ref blackboard);
            
            if (result == BehaviourActionResult.InProgress)
            {
                return BehaviourActionResult.InProgress;
            }
            
            if (result == BehaviourActionResult.Success)
            {
                return BehaviourActionResult.Success;
            }
        }
        
        return BehaviourActionResult.Failure;
    }

    public override void PopulateBurstable(ref BlobBuilder builder, ref BurstableBehaviourTreeNode node)
    {
        ref BehaviourTreeSelectorNodeBurstable data = ref AllocateNodeData<BehaviourTreeSelectorNodeBurstable>(ref builder, ref node);
        BlobBuilderArray<int> actionNodes = builder.Allocate(ref data.m_actionNodes, m_actionNodes.Count);
        for (int i = 0; i < m_actionNodes.Count; i++)
        {
            actionNodes[i] = m_actionNodes[i];
        }
        node.DoActionBurstable = BurstCompiler.CompileFunctionPointer<BurstableBehaviourTreeNode.DoActionDelegate>(BehaviourTreeSelectorNodeBurstable.BurstableDoAction);
    }
}
