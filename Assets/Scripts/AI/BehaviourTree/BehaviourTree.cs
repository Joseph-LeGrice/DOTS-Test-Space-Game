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
    
    protected override void PopulateBlob(IBaker baker, ref BlobBuilder builder, ref BurstableBehaviourTree tree)
    {
        tree.InitialNodeReferenceIndex = m_initialNode.m_nodeReference;

        ECSTypeRegister ecsTypeRegister = new ECSTypeRegister();
            
        var nodeArrayBuilder = builder.Allocate(ref tree.m_allNodes, m_allNodes.Count);
        for (int i=0; i<m_allNodes.Count; i++)
        {
            m_allNodes[i].PopulateBurstable(ref builder, ecsTypeRegister, ref nodeArrayBuilder[i]);
        }

        var ecsTypes = ecsTypeRegister.GetAllTypes();
        var ecsTypeInfoBuilder = builder.Allocate(ref tree.m_ecsTypeInfo, ecsTypes.Count);
        for (int i = 0; i < ecsTypes.Count; i++)
        {
            ecsTypeInfoBuilder[i] = ecsTypes[i];
        }
    }
}

public class ECSTypeRegister
{
    private Dictionary<string, ECSTypeInfo> m_types = new Dictionary<string, ECSTypeInfo>();

    public List<ECSTypeInfo> GetAllTypes()
    {
        return new List<ECSTypeInfo>(m_types.Values);
    }
    
    public void RegisterAccess<T>(ComponentType.AccessMode accessMode) where T : unmanaged, IComponentData
    {
        m_types[typeof(T).Name] = new ECSTypeInfo(typeof(T), accessMode);
    }
}
