using System.Collections.Generic;
using UnityEngine;

    
public class BehaviourTreeSequentialNode : BehaviourTreeActionNode
{
    [SerializeField]
    private List<BehaviourTreeActionNode> m_actionNodes = new List<BehaviourTreeActionNode>();
        
    private int m_currentActionIndex;
        
    public override BehaviourActionResult DoAction(ref BehaviourTreeBlackboard blackboard)
    {
        while (m_currentActionIndex < m_actionNodes.Count)
        {
            var result = m_actionNodes[m_currentActionIndex].DoAction(ref blackboard);
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
}