using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public static class NodeTypes
{
    public class BehaviourNodeTypeData
    {
        public string NodeName;
        public Type NodeType;

        public static BehaviourNodeTypeData Create(Type nodeType)
        {
            return new BehaviourNodeTypeData()
            {
                NodeName = nodeType.Name,
                NodeType = nodeType
            };
        }
    }
        
    private static List<BehaviourNodeTypeData> s_allNodeTypes;
    public static List<BehaviourNodeTypeData> AllNodeTypes => s_allNodeTypes;
    
    static NodeTypes()
    {
        s_allNodeTypes = new List<BehaviourNodeTypeData>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(BehaviourTreeNodeImplementation)) && !type.IsAbstract)
                {
                    s_allNodeTypes.Add(BehaviourNodeTypeData.Create(type));
                }
            }
        }
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
        BehaviourTreeNode instance = new BehaviourTreeNode();
        instance.m_nodePosition = new Vector2(style.left.value.value, style.top.value.value);
        instance.m_nodeImplementation = (BehaviourTreeNodeImplementation)Activator.CreateInstance(NodeTypes.AllNodeTypes[i].NodeType);
        m_rootWindow.GetSerializedBehaviourTree().AddNode(instance);
        m_rootWindow.CloseContextMenu();
        evt.StopPropagation();
        m_rootWindow.RefreshAll();
    }
}
