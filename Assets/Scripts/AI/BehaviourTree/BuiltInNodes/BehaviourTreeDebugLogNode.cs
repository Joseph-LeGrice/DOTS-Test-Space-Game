using UnityEngine;

[System.Serializable]
public class BehaviourTreeDebugLogNode : BehaviourTreeNodeImplementation
{
    [SerializeField]
    private string m_message;

    public override string GetNodeName()
    {
        return "Debug Log Node";
    }
    
    public override BehaviourActionResult DoAction(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard)
    {
        Debug.Log(m_message);
        return BehaviourActionResult.Success;
    }
}
