using UnityEngine;

public class BehaviourTreeDebugLogNode : BehaviourTreeNodeImplementation
{
    [SerializeField]
    private string m_message;

    public override string GetNodeName()
    {
        return "Debug Log Node";
    }
    
    public override BehaviourActionResult DoAction(BehaviourTree behaviourTree)
    {
        Debug.Log(m_message);
        return BehaviourActionResult.Success;
    }
}
