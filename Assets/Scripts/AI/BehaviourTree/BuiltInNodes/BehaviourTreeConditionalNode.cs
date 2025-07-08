using UnityEngine;

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
    
    public override BehaviourActionResult DoAction(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard)
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
}
