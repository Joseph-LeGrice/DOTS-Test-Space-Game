using UnityEngine;
using UnityEngine.UIElements;

public enum BehaviourActionResult
{
    Success,
    Failure,
    InProgress,
};

[System.Serializable]
public sealed class BehaviourTreeNode : INotifyBindablePropertyChanged 
{
    [SerializeField]
    [HideInInspector]
    [BehaviourNodeReference(true)]
    internal int m_nodeReference = -1;
    [SerializeField]
    [HideInInspector]
    internal Vector2 m_nodePosition;
    [SerializeField]
    [SerializeReference]
    internal BehaviourTreeNodeImplementation m_nodeImplementation;
    
    public event System.EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    public void NotifyPropertyChanged(string property)
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    public BehaviourActionResult DoAction(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard)
    {
        return m_nodeImplementation.DoAction(behaviourTree, ref blackboard);
    }

    public BurstableBehaviourTreeNode GetBurstable()
    {
        Debug.LogWarning("Node not currently burstable");
        return default(BurstableBehaviourTreeNode);
    }
}

[System.Serializable]
public abstract class BehaviourTreeNodeImplementation
{
    public abstract string GetNodeName();
    public abstract BehaviourActionResult DoAction(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard);

    public virtual BurstableBehaviourTreeNode GetBurstable()
    {
        Debug.LogWarning("Node not currently burstable");
        return default(BurstableBehaviourTreeNode);
    }
}

[System.Serializable]
public sealed class BehaviourTreeValueReference
{
    [SerializeField]
    [HideInInspector]
    internal Vector2 m_nodePosition;
    [SerializeField]
    [HideInInspector]
    [BehaviourNodeReference()]
    internal int m_nodeReference = -1;
}