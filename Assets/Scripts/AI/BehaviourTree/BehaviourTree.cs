using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public enum BehaviourActionResult
{
    Success,
    Failure,
    InProgress,
};

[System.Serializable]
public abstract class BehaviourTreeNode
{
    [SerializeField]
    [BehaviourNodeReference(true)]
    public int m_nodeReference;
    [SerializeField]
    [HideInInspector]
    public Vector2 m_nodePosition;
    
    public abstract BehaviourActionResult DoAction(BehaviourTree behaviourTree);
    public abstract BurstableBehaviourTreeNode GetBurstable();

    public abstract string GetNodeName();
    public abstract bool AcceptsConnectionIn();
    public abstract bool AcceptsConnectionOut();
}

[CreateAssetMenu(menuName = "BehaviourTree Asset")]
public class BehaviourTree : BlobAssetScriptableObject<BurstableBehaviourTree>
{
    [SerializeField]
    [SerializeReference]
    private List<BehaviourTreeNode> m_allNodes;
    [SerializeField]
    private int m_nextNodeReference;

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
    
    public IReadOnlyList<BehaviourTreeNode> GetNodes()
    {
        return m_allNodes;
    }

    protected override void PopulateBlob(IBaker baker, BlobBuilder builder, ref BurstableBehaviourTree blobData)
    {
    }
}
