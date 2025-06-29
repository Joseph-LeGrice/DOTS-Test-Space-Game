using System.Collections.Generic;
using UnityEngine;

public struct BehaviourTreeBlackboard
{
    public class BlackboardValue
    {
        public string Key;
        public object Value;
    }

    public List<BlackboardValue> m_blackboardValues;
}