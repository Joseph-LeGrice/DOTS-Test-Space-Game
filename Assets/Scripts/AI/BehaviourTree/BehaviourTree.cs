using System.Collections.Generic;
using UnityEngine;

public enum BehaviourActionResult
{
    Success,
    Failure,
    InProgress,
};

public abstract class BehaviourTreeActionNode
{
    public abstract BehaviourActionResult DoAction(ref BehaviourTreeBlackboard blackboard);
}

public abstract class BehaviourTreeConditionNode
{
    public abstract bool IsValid(ref BehaviourTreeBlackboard blackboard);
}

