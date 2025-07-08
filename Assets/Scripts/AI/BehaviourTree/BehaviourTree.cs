using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public interface IBehaviourTreeEditor
{
    public int GetNodeCount();
    public BehaviourTreeNode GetNode(int index);
    public void AddNode(BehaviourTreeNode node);
    public int IndexOf(BehaviourTreeNode node);
    public void DeleteNode(int nodeIndex);
    BehaviourTreeValueReference GetInitialNode();
}

[CreateAssetMenu(menuName = "BehaviourTree Asset")]
public class BehaviourTree : BlobAssetScriptableObject<BurstableBehaviourTree>
{
    [SerializeField]
    [SerializeReference]
    internal List<BehaviourTreeNode> m_allNodes = new List<BehaviourTreeNode>();
    [SerializeField]
    [SerializeReference]
    internal BehaviourTreeValueReference m_initialNode = new BehaviourTreeValueReference();
    [SerializeField]
    internal int m_nextNodeReference = 1;

    public BehaviourActionResult Execute(ref BehaviourTreeBlackboard blackboard)
    {
        return GetNode(m_initialNode.m_nodeReference).DoAction(this, ref blackboard);
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
