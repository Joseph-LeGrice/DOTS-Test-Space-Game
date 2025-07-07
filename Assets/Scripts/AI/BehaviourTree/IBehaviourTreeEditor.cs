using UnityEditor;

public interface IBehaviourTreeEditor
{
    public int GetNodeCount();
    public BehaviourTreeNode GetNode(int index);
    public void AddNode(BehaviourTreeNode node);
    public int IndexOf(BehaviourTreeNode node);
    public void DeleteNode(int nodeIndex);
}

public class SerializedObjectBehaviourTreeEditor : IBehaviourTreeEditor
{
    private readonly SerializedObject m_behaviourTree;

    public SerializedObjectBehaviourTreeEditor(SerializedObject behaviourTree)
    {
        m_behaviourTree = behaviourTree;
    }
    
    public BehaviourTreeNode GetNode(int index)
    {
        m_behaviourTree.Update();
        var allNodes = m_behaviourTree.FindProperty(nameof(BehaviourTree.m_allNodes));
        if (allNodes.arraySize > index)
        {
            var arrayValue = allNodes.GetArrayElementAtIndex(index);
            return (BehaviourTreeNode)arrayValue.managedReferenceValue;
        }

        return null;
    }
    
    public int GetNodeCount()
    {
        m_behaviourTree.Update();
        var allNodes = m_behaviourTree.FindProperty(nameof(BehaviourTree.m_allNodes));
        return allNodes.arraySize;
    }

    public int IndexOf(BehaviourTreeNode node)
    {
        m_behaviourTree.Update();
        var allNodes = m_behaviourTree.FindProperty(nameof(BehaviourTree.m_allNodes));
        for (int i = 0; i < allNodes.arraySize; i++)
        {
            BehaviourTreeNode other = (BehaviourTreeNode)allNodes.GetArrayElementAtIndex(i).managedReferenceValue;
            if (other.m_nodeReference == node.m_nodeReference)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    public void AddNode(BehaviourTreeNode node)
    {
        m_behaviourTree.Update();
        var nextNodeReference = m_behaviourTree.FindProperty(nameof(BehaviourTree.m_nextNodeReference));
        var allNodes = m_behaviourTree.FindProperty(nameof(BehaviourTree.m_allNodes));
        node.m_nodeReference = nextNodeReference.intValue;
        nextNodeReference.intValue = nextNodeReference.intValue + 1;
        allNodes.InsertArrayElementAtIndex(allNodes.arraySize);
        allNodes.GetArrayElementAtIndex(allNodes.arraySize - 1).managedReferenceValue = node;
        m_behaviourTree.ApplyModifiedProperties();
    }

    public void DeleteNode(int nodeIndex)
    {
        m_behaviourTree.Update();
        var allNodes = m_behaviourTree.FindProperty(nameof(BehaviourTree.m_allNodes));
        if (allNodes.arraySize > nodeIndex)
        {
            allNodes.DeleteArrayElementAtIndex(nodeIndex);
            m_behaviourTree.ApplyModifiedProperties();
        }
    }
}
