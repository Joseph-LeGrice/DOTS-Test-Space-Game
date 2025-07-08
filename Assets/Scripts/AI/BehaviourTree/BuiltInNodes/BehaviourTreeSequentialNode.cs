using System.Collections.Generic;
using UnityEngine;

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
    
    public override BehaviourActionResult DoAction(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard)
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
}
