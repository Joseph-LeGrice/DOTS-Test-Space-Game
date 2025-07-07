using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum BehaviourActionResult
{
    Success,
    Failure,
    InProgress,
};

[System.Serializable]
public abstract class BehaviourTreeNode : INotifyBindablePropertyChanged 
{
    [SerializeField]
    [HideInInspector]
    [BehaviourNodeReference(true)]
    public int m_nodeReference = -1;
    [SerializeField]
    [HideInInspector]
    public Vector2 m_nodePosition;
    
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    public void NotifyPropertyChanged(string property)
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
    
    public abstract string GetNodeName();
    public abstract BehaviourActionResult DoAction(BehaviourTree behaviourTree);

    public virtual BurstableBehaviourTreeNode GetBurstable()
    {
        Debug.LogWarning("Node not currently burstable");
        return default(BurstableBehaviourTreeNode);
    }
}
