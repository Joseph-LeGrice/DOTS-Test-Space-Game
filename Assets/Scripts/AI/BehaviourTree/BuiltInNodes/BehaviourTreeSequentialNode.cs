using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public struct BurstableSequentialNodeData
{
    public BlobArray<int> ActionNodes;
    
    [BurstCompile]
    [AOT.MonoPInvokeCallback(typeof(BurstableBehaviourTreeNode.DoActionDelegate))]
    public static BehaviourActionResult BurstableDoAction(ref BurstableBehaviourTree behaviourTree, ref BurstableBehaviourTreeNode node, ref ECSBehaviourTreeBlackboard blackboard)
    {
        ref BurstableSequentialNodeData nodeDataRef = ref node.GetNodeDataReference<BurstableSequentialNodeData>();
        
        for (int i=0; i<nodeDataRef.ActionNodes.Length; i++)
        {
            var actionNodeRef = nodeDataRef.ActionNodes[i];
            var result = behaviourTree.GetNode(actionNodeRef).Execute(ref behaviourTree, ref blackboard);
            
            if (result == BehaviourActionResult.InProgress)
            {
                return BehaviourActionResult.InProgress;
            }
            
            if (result == BehaviourActionResult.Failure)
            {
                return BehaviourActionResult.Failure;
            }
        }
        return BehaviourActionResult.Success;
    }
}

[System.Serializable]
public class BehaviourTreeSequentialNode : BehaviourTreeNodeImplementation
{
    [SerializeField]
    [BehaviourNodeReference]
    private List<int> m_actionNodes;

    public override string GetNodeName()
    {
        return "Sequential Node";
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
            
            if (result == BehaviourActionResult.Failure)
            {
                return BehaviourActionResult.Failure;
            }
        }

        return BehaviourActionResult.Success;
    }
    
    public override void PopulateBurstable(ref BlobBuilder builder, ref BurstableBehaviourTreeNode node)
    {
        ref BurstableSequentialNodeData nodeDataRef = ref AllocateNodeData<BurstableSequentialNodeData>(ref builder, ref node);
        BlobBuilderArray<int> actionNodes = builder.Allocate(ref nodeDataRef.ActionNodes, m_actionNodes.Count);
        for (int i = 0; i < m_actionNodes.Count; i++)
        {
            actionNodes[i] = m_actionNodes[i];
        }
        node.DoActionBurstable = BurstCompiler.CompileFunctionPointer<BurstableBehaviourTreeNode.DoActionDelegate>(BurstableSequentialNodeData.BurstableDoAction);
    }
}
