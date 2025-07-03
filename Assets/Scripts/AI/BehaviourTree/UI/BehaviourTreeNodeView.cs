using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Properties;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourTreeNodeView : VisualElement
{
    private BehaviourTreeWindow m_behaviourTreeWindow;
    private int m_nodeIndex;
    
    private Label m_title;
    private ConnectorPoint m_connectionInElement;
    private VisualElement m_contentElement;
    
    private Dictionary<string, VisualElement> m_fieldElementLookup = new Dictionary<string, VisualElement>();
    
    public BehaviourTreeNodeView(BehaviourTreeWindow window, int nodeArrayIndex)
    {
        m_behaviourTreeWindow = window;
        m_nodeIndex = nodeArrayIndex;
        
        style.position = Position.Absolute;
        style.backgroundColor = Color.aquamarine;
        style.borderTopColor = style.borderBottomColor = style.borderLeftColor = style.borderRightColor = Color.black;
        style.borderTopWidth = style.borderBottomWidth = style.borderLeftWidth = style.borderRightWidth = 2.0f;
        style.flexGrow = 1.0f;
        style.flexShrink = 0.0f;
        style.width = new Length(256.0f, LengthUnit.Pixel);

        VisualElement headerElement = new VisualElement();
        headerElement.style.flexDirection = FlexDirection.Row;

        BehaviourTreeNode node = GetNode();
        transform.position = node.m_nodePosition;
        
        m_connectionInElement = new ConnectorPoint(m_behaviourTreeWindow.GetConnectorStateHandler(), true);
        m_connectionInElement.dataSource = node;
        m_connectionInElement.dataSourcePath = PropertyPath.FromName(nameof(BehaviourTreeNode.m_nodeReference));
        m_connectionInElement.visible = node.AcceptsConnectionIn();
        headerElement.Add(m_connectionInElement);
        
        m_title = new Label();
        m_title.text = node.GetNodeName();
        m_title.style.color = Color.black;
        m_title.style.flexGrow = 0.0f;
        headerElement.Add(m_title);
        
        hierarchy.Add(headerElement);

        m_contentElement = new VisualElement();
        m_contentElement.style.flexShrink = 0.0f;
        m_contentElement.style.flexGrow = 1.0f;
        
        hierarchy.Add(m_contentElement);
        
        FieldInfo[] nodeFields = node.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (FieldInfo childProperty in nodeFields)
        {
            var nodeReference = (BehaviourNodeReferenceAttribute)Attribute.GetCustomAttribute(childProperty, typeof(BehaviourNodeReferenceAttribute));
            if (nodeReference != null && !nodeReference.ConnectsIn)
            {
                var field = new ConnectorPointField(m_behaviourTreeWindow.GetConnectorStateHandler(), childProperty);
                field.dataSource = node;
                field.dataSourcePath = PropertyPath.FromName(childProperty.Name);
                m_contentElement.Add(field);
                m_fieldElementLookup[childProperty.Name] = field;
            }
            
            //TODO: Support all other types, do some ugly switching for the types
            switch (childProperty.FieldType.Name)
            {
                default:
                    break;
                case "int":
                    CreateField<IntegerField, int>();
                    break;
                case "string":
                    CreateField<TextField, string>();
                    break;
            }
        }
        
        this.AddManipulator(new MouseDragManipulator());
    }

    private void CreateField<TField, TType>() where TField : BaseField<TType>
    {
        //TODO: Create a BaseField with generics and bind it to the correct field
        
        // var field = new PropertyField(childProperty, childProperty.name);
        // field.Bind(m_behaviourTreeWindow.GetSerializedBehaviourTree());
        // m_contentElement.Add(field);
        // m_fieldElementLookup[childProperty.name] = field;
    }

    public BehaviourTreeWindow GetBehaviourTreeWindow()
    {
        return m_behaviourTreeWindow;
    }

    public VisualElement GetConnectionInElement()
    {
        return m_connectionInElement;
    }

    public VisualElement GetConnectionOutElement(string fieldName, int i)
    {
        return m_fieldElementLookup[fieldName].Q<ConnectorPointField>().GetConnector(i);
    }

    private BehaviourTreeNode GetNode()
    {
        return m_behaviourTreeWindow.GetSerializedBehaviourTree().GetNodes()[m_nodeIndex];
    }

    public void SetPosition(Vector2 newPosition)
    {
        BehaviourTreeNode node = GetNode();
        if (node != null)
        {
            node.m_nodePosition = newPosition;
        }
    }

    public void Refresh()
    {
        throw new NotImplementedException();
    }
}
