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
    public int m_nodeReference;
    [SerializeField]
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
    
    public BehaviourTreeNode GetNode(int i)
    {
        foreach (BehaviourTreeNode n in m_allNodes)
        {
            if (n.m_nodeReference == i)
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
