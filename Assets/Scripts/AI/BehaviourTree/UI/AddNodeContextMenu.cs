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
    private BehaviourTreeWindow m_rootWindow;
    
    public AddNodeContextMenu(BehaviourTreeWindow rootWindow)
    {
        m_rootWindow = rootWindow;
        
        style.position = Position.Absolute;
        style.width = new Length(256.0f, LengthUnit.Pixel);
        style.borderTopWidth = style.borderLeftWidth = style.borderBottomWidth = style.borderRightWidth = 2.0f;
        style.borderTopColor = style.borderLeftColor = style.borderBottomColor = style.borderRightColor = Color.white;
        style.backgroundColor = Color.black;
        
        // m_nodeButton
        
        ListView nodeListView = new ListView();
        nodeListView.itemsSource = NodeTypes.AllNodeTypes;
        nodeListView.makeItem = CreateNodeButton;
        nodeListView.bindItem = BindCreateNodeButton;
        nodeListView.RefreshItems();
        
        Add(nodeListView);
    }

    private VisualElement CreateNodeButton()
    {
        var nodeButton = new VisualElement();
        nodeButton.style.alignItems = Align.Center;
        nodeButton.style.justifyContent = Justify.Center;
        nodeButton.Add(new Label());
        return nodeButton;
    }

    private void BindCreateNodeButton(VisualElement button, int i)
    {
        button.Q<Label>().text = NodeTypes.AllNodeTypes[i].NodeName;
        button.userData = i;
        
        button.RegisterCallback<MouseUpEvent>(OnClickUp);
        button.RegisterCallback<MouseDownEvent>(OnClickDown);
        button.RegisterCallback<ClickEvent, VisualElement>(AddNew, button);
    }

    private void OnClickUp(MouseUpEvent evt)
    {
        evt.StopPropagation();
    }

    private void OnClickDown(MouseDownEvent evt)
    {
        evt.StopPropagation();
    }
    
    private void AddNew(ClickEvent evt, VisualElement source)
    {
        int i = (int)source.userData;
        Debug.Log("Create new: " + NodeTypes.AllNodeTypes[i].NodeName);
        m_rootWindow.CloseContextMenu();
        
        evt.StopPropagation();

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
