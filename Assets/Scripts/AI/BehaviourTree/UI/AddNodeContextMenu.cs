using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class NodeTypes
{
    public class BehaviourNodeTypeData
    {
        public string NodeName;

        public static BehaviourNodeTypeData Create<T>() where T : BehaviourTreeNode
        {
            return new BehaviourNodeTypeData()
            {
                NodeName = typeof(T).Name
            };
        }
    }
        
    private static List<BehaviourNodeTypeData> s_allNodeTypes;
    public static List<BehaviourNodeTypeData> AllNodeTypes => s_allNodeTypes;
    
    static NodeTypes()
    {
        s_allNodeTypes = new List<BehaviourNodeTypeData>();
        //TODO: Use reflection to get all types
        s_allNodeTypes.Add(BehaviourNodeTypeData.Create<BehaviourTreeSequentialNode>());
        s_allNodeTypes.Add(BehaviourNodeTypeData.Create<BehaviourTreeConditionalNode>());
    }
}

public class AddNodeContextMenu : VisualElement
{
    
    public AddNodeContextMenu()
    {
        VisualTreeAsset nodeButton = Resources.Load<VisualTreeAsset>("BehaviourTreeNodeButton");
        ListView nodeListView = new ListView();
        nodeListView.itemsSource = NodeTypes.AllNodeTypes;
        nodeListView.makeItem = nodeButton.CloneTree;
        nodeListView.bindItem = BindCreateNodeButton;
        nodeListView.RefreshItems();
        Add(nodeListView);
    }
    
    private void BindCreateNodeButton(VisualElement button, int i)
    {
        button.Q<Button>().text = NodeTypes.AllNodeTypes[i].NodeName;
        // ((Button)button).RegisterCallback();
    }

    private void AddNew<T>() where T : BehaviourTreeNode, new()
    {
        // serializedObject.Update();
        // SerializedProperty nextNodeReference = serializedObject.FindProperty("m_nextNodeReference");
        // T newInstance = new T
        // {
        //     m_nodeReference = nextNodeReference.intValue
        // };
        // nextNodeReference.intValue = nextNodeReference.intValue + 1;
        //
        // SerializedProperty allNodes = serializedObject.FindProperty("m_allNodes");
        // allNodes.InsertArrayElementAtIndex(allNodes.arraySize);
        // var nodeElement = allNodes.GetArrayElementAtIndex(allNodes.arraySize - 1);
        // nodeElement.managedReferenceValue = newInstance;
        //
        // serializedObject.ApplyModifiedProperties();
    }
}
