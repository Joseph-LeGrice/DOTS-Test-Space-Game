using UnityEngine;

public class BehaviourTreeDebugCondition : BehaviourTreeNode
{
    [SerializeField]
    private bool m_condition;

    public override string GetNodeName()
    {
        return "Debug Condition Node";
    }
    
    public override BehaviourActionResult DoAction(BehaviourTree behaviourTree)
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
}
