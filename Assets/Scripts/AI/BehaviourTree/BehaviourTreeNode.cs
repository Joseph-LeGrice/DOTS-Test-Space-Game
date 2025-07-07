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
    internal BehaviourTreeNodeImplementation m_nodeImplementation;
    
    public event System.EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    public void NotifyPropertyChanged(string property)
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    public BehaviourActionResult DoAction(BehaviourTree behaviourTree)
    {
        return m_nodeImplementation.DoAction(behaviourTree);
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
    public abstract BehaviourActionResult DoAction(BehaviourTree behaviourTree);

    public virtual BurstableBehaviourTreeNode GetBurstable()
    {
        Debug.LogWarning("Node not currently burstable");
        return default(BurstableBehaviourTreeNode);
    }
}