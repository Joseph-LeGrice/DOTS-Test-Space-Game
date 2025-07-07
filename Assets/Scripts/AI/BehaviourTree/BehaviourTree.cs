using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

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
