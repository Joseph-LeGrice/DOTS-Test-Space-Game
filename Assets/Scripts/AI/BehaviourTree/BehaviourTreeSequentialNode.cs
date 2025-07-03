using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BehaviourTreeSequentialNode : BehaviourTreeNode
{
    [SerializeField]
    [BehaviourNodeReference]
    private List<int> m_actionNodes;
    [SerializeField]
    [HideInInspector]
    private int m_currentActionIndex; // This is state..
    
    public override BehaviourActionResult DoAction(BehaviourTree behaviourTree)
    {
        while (m_currentActionIndex < m_actionNodes.Count)
        {
            var result = behaviourTree.GetNode(m_actionNodes[m_currentActionIndex]).DoAction(behaviourTree);
            if (result == BehaviourActionResult.Success)
            {
                m_currentActionIndex++;
            }
            else
            {
                if (result == BehaviourActionResult.Failure)
                {
                    m_currentActionIndex = 0;
                }
                return result;
            }
        }

        m_currentActionIndex = 0;
        return BehaviourActionResult.Success;
    }

    public override BurstableBehaviourTreeNode GetBurstable()
    {
        return new BurstableBehaviourTreeNode();
    }

    public override string GetNodeName()
    {
        return "Sequential Node";
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
