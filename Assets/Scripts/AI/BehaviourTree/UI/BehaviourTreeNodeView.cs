using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourTreeNodeView : VisualElement
{
    private int m_nodeIndex;
    private SerializedObject m_behaviourTree;
    
    private Label m_title;
    private VisualElement m_connectionInElement;
    private VisualElement m_contentElement;
    
    public BehaviourTreeNodeView()
    {
        style.position = Position.Absolute;
        style.backgroundColor = Color.aquamarine;
        style.borderTopColor = style.borderBottomColor = style.borderLeftColor = style.borderRightColor = Color.black;
        style.borderTopWidth = style.borderBottomWidth = style.borderLeftWidth = style.borderRightWidth = 2.0f;
        style.flexGrow = 1.0f;
        style.flexShrink = 0.0f;
        style.width = new Length(256.0f, LengthUnit.Pixel);

        VisualElement headerElement = new VisualElement();
        headerElement.style.flexDirection = FlexDirection.Row;

        float connectorMargin = 8.0f;
        m_connectionInElement = new ConnectorPointView();
        m_connectionInElement.style.marginLeft = connectorMargin;
        m_connectionInElement.style.marginRight = connectorMargin;
        headerElement.Add(m_connectionInElement);
        
        m_title = new Label();
        m_title.style.color = Color.black;
        m_title.style.flexGrow = 0.0f;
        headerElement.Add(m_title);
        
        hierarchy.Add(headerElement);

        m_contentElement = new VisualElement();
        m_contentElement.style.flexShrink = 0.0f;
        m_contentElement.style.flexGrow = 1.0f;
        
        hierarchy.Add(m_contentElement);
        
        this.AddManipulator(new MouseDragManipulator());
    }

    private SerializedProperty GetNode()
    {
        return m_behaviourTree.FindProperty("m_allNodes").GetArrayElementAtIndex(m_nodeIndex);
    }

    public void SetNode(SerializedObject behaviourTree, int i)
    {
        m_nodeIndex = i;
        m_behaviourTree = behaviourTree;
        
        SerializedProperty nodeSerialized = GetNode();
        BehaviourTreeNode node = (BehaviourTreeNode)nodeSerialized.managedReferenceValue;

        transform.position = node.m_nodePosition;
        m_connectionInElement.visible = node.AcceptsConnectionIn();

        m_title.text = node.GetNodeName();
        
        var field = new PropertyField(nodeSerialized);
        field.Bind(m_behaviourTree);
        m_contentElement.Add(field);
        
        // foreach (SerializedProperty childProperty in nodeSerialized)
        // {
        //     var field = new PropertyField(childProperty, childProperty.name);
        //     field.Bind(m_behaviourTree);
        //     m_contentElement.Add(field);
        // }
    }

    public void SetPosition(Vector2 newPosition)
    {
        SerializedProperty nodeSerialized = GetNode();
        if (nodeSerialized != null)
        {
            nodeSerialized.serializedObject.Update();
            nodeSerialized.FindPropertyRelative("m_nodePosition").vector2Value = newPosition;
            nodeSerialized.serializedObject.ApplyModifiedProperties();
        }
    }
}
