using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree
{
    public class BehaviourTreeBlackboard
    {
        public class BlackboardValue
        {
            public string Key;
            public object Value;
        }

        public List<BlackboardValue> m_blackboardValues;
    }
    
    public enum ActionResult
    {
        Success,
        Failure,
        InProgress,
    };
    
    public abstract class BehaviourTreeActionNode
    {
        public abstract ActionResult DoAction();
    }
    
    public class BehaviourTreeSequentialNode : BehaviourTreeActionNode
    {
        [SerializeField]
        private List<BehaviourTreeActionNode> m_actionNodes = new List<BehaviourTreeActionNode>();
        
        private int m_currentActionIndex;
        
        public override ActionResult DoAction()
        {
            while (m_currentActionIndex < m_actionNodes.Count)
            {
                var result = m_actionNodes[m_currentActionIndex].DoAction();
                if (result == ActionResult.Success)
                {
                    m_currentActionIndex++;
                }
                else
                {
                    if (result == ActionResult.Failure)
                    {
                        m_currentActionIndex = 0;
                    }
                    return result;
                }
            }

            m_currentActionIndex = 0;
            return ActionResult.Success;
        }
    }

    public class BehaviourTreeConditionalNode : BehaviourTreeActionNode
    {
        [SerializeField]
        private BehaviourTreeActionNode m_conditionalNode;
        [SerializeField]
        private BehaviourTreeActionNode m_actionNode;
        
        public override ActionResult DoAction()
        {
            var conditionalResult = m_conditionalNode.DoAction();
            if (conditionalResult == ActionResult.Success)
            {
                return m_actionNode.DoAction();
            }
            else
            {
                return conditionalResult;
            }
        }
    }
}
