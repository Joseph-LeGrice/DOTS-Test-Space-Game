using System;
using System.Collections.Generic;
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
    private List<BehaviourTreeNode> m_allNodes;
    [SerializeField]
    private int m_nextNodeReference = 1;

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
    
    public IList<BehaviourTreeNode> GetNodes()
    {
        return m_allNodes;
    }

    public void AddNode(BehaviourTreeNode node)
    {
        node.m_nodeReference = m_nextNodeReference;
        m_nextNodeReference++;
        m_allNodes.Add(node);
    }

    protected override void PopulateBlob(IBaker baker, BlobBuilder builder, ref BurstableBehaviourTree blobData)
    {
    }
}
