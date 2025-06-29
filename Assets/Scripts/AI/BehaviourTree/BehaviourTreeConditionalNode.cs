using UnityEngine;

public class BehaviourTreeConditionalNode : BehaviourTreeActionNode
{
    [SerializeField]
    private BehaviourTreeConditionNode m_conditionalNode;
    [SerializeField]
    private BehaviourTreeActionNode m_actionNode;
    
    public override BehaviourActionResult DoAction(ref BehaviourTreeBlackboard blackboard)
    {
        if (m_conditionalNode.IsValid(ref blackboard))
        {
            return m_actionNode.DoAction(ref blackboard);
        }
        else
        {
            return BehaviourActionResult.Failure;
        }
    }
}
