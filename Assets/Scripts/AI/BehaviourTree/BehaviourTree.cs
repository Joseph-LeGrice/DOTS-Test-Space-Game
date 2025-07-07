using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
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

    public virtual bool AcceptsConnectionIn()
    {
        return true;
    }

    public virtual bool AcceptsConnectionOut()
    {
        return false;
    }
}

[CreateAssetMenu(menuName = "BehaviourTree Asset")]
public class BehaviourTree : BlobAssetScriptableObject<BurstableBehaviourTree>
{
    [SerializeField]
    [SerializeReference]
    internal List<BehaviourTreeNode> m_allNodes = new List<BehaviourTreeNode>();
    [SerializeField]
    internal int m_nextNodeReference = 1;

    public int Execute(int i)
    {
        return -1;
        // return GetNode(i).DoAction(this);
    }
    
    public BehaviourTreeNode GetNode(int nodeReference)
    {
        foreach (BehaviourTreeNode n in m_allNodes)
        {
            if (n.m_nodeReference == nodeReference)
            {
                return n;
            }
        }
        return null;
    }
    
    protected override void PopulateBlob(IBaker baker, BlobBuilder builder, ref BurstableBehaviourTree blobData)
    {
    }
}
