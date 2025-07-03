using UnityEngine;

[System.Serializable]
public class BehaviourTreeConditionalNode : BehaviourTreeNode
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
    
    public override BehaviourActionResult DoAction(BehaviourTree behaviourTree)
    {
        var result = behaviourTree.GetNode(m_conditionalNode).DoAction(behaviourTree);
        
        if (result == BehaviourActionResult.Success)
        {
            return behaviourTree.GetNode(m_actionNode).DoAction(behaviourTree);
        }
        else
        {
            return BehaviourActionResult.Failure;
        }
    }

    public override BurstableBehaviourTreeNode GetBurstable()
    {
        return new BurstableBehaviourTreeNode();
    }

    public override bool AcceptsConnectionIn()
    {
        return true;
    }

    public override bool AcceptsConnectionOut()
    {
        return true;
    }
}
