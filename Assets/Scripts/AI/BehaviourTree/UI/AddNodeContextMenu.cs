using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AddNodeContextMenu : VisualElement
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
    
    private List<BehaviourNodeTypeData> m_allNodeTypes;
    
    public AddNodeContextMenu()
    {
        m_allNodeTypes = new List<BehaviourNodeTypeData>();
        m_allNodeTypes.Add(BehaviourNodeTypeData.Create<BehaviourTreeSequentialNode>());
        m_allNodeTypes.Add(BehaviourNodeTypeData.Create<BehaviourTreeConditionalNode>());
        
        VisualTreeAsset nodeButton = Resources.Load<VisualTreeAsset>("BehaviourTreeNodeButton");
        ListView nodeListView = this.Q<ListView>("NodeList");
        nodeListView.itemsSource = m_allNodeTypes;
        nodeListView.makeItem = nodeButton.CloneTree;
        nodeListView.bindItem = BindCreateNodeButton;
        nodeListView.RefreshItems();
    }
    
    
    private void BindCreateNodeButton(VisualElement button, int i)
    {
        button.Q<Button>().text = m_allNodeTypes[i].NodeName;
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
