using System;
using Unity.Entities;
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
        return m_nodeImplementation.DoActionManaged(behaviourTree, ref blackboard);
    }

    public void PopulateBurstable(ref BlobBuilder builder, ref BurstableBehaviourTreeNode node)
    {
        node.NodeReferenceIndex = m_nodeReference;
        m_nodeImplementation.PopulateBurstable(ref builder, ref node);
        
        if (node.DoActionBurstable.Value == IntPtr.Zero)
        {
            Debug.LogError(m_nodeImplementation.GetType().Name + ".DoActionBurstable is null!");
        }
    }
}

[System.Serializable]
public abstract class BehaviourTreeNodeImplementation
{
    public abstract string GetNodeName();
    public abstract BehaviourActionResult DoActionManaged(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard);

    protected unsafe ref T AllocateNodeData<T>(ref BlobBuilder builder, ref BurstableBehaviourTreeNode node) where T : unmanaged
    {
        var arrayAllocation = builder.Allocate(ref node.NodeData, sizeof(T));
        return ref *(T*)arrayAllocation.GetUnsafePtr();
    }
    
    public virtual void PopulateBurstable(ref BlobBuilder builder, ref BurstableBehaviourTreeNode node)
    {
        Debug.LogWarning("Node not currently burstable");
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