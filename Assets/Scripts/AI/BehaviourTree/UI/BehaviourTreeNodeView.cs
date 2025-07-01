using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourTreeNodeView : VisualElement
{
    private int m_nodeIndex;
    private SerializedObject m_behaviourTree;
    private VisualElement m_connectionInElement;
    private VisualElement m_contentElement;
    private VisualElement m_connectionOutElement;

    public BehaviourTreeNodeView()
    {
        style.backgroundColor = Color.aquamarine;
        style.borderTopColor = style.borderBottomColor = style.borderLeftColor = style.borderRightColor = Color.black;
        style.borderTopWidth = style.borderBottomWidth = style.borderLeftWidth = style.borderRightWidth = 2.0f;
        style.flexGrow = 0.0f;
        style.flexShrink = 1.0f;
        style.width = new Length(256.0f, LengthUnit.Pixel);
        style.height = new Length(256.0f, LengthUnit.Pixel);

        m_connectionInElement = new VisualElement();
        m_connectionInElement.style.backgroundColor = Color.red;
        
        m_connectionInElement.style.flexGrow= 0.0f;
        m_connectionInElement.style.flexShrink = 0.0f;
        m_connectionInElement.style.width = 32.0f;
        m_connectionInElement.style.height = 16.0f;
        
        m_connectionInElement.style.position = Position.Relative;
        m_connectionInElement.style.alignSelf = Align.Center;
        m_connectionInElement.style.top = 0.0f;
        
        hierarchy.Add(m_connectionInElement);

        m_contentElement = new VisualElement();
        m_contentElement.style.flexShrink = 1.0f;
        m_contentElement.style.flexGrow = 0.0f;
        
        hierarchy.Add(m_contentElement);
        
        m_connectionOutElement = new VisualElement();
        m_connectionOutElement.style.backgroundColor = Color.red;
        
        m_connectionOutElement.style.flexGrow= 0.0f;
        m_connectionOutElement.style.flexShrink = 0.0f;
        m_connectionOutElement.style.width = 32.0f;
        m_connectionOutElement.style.height = 16.0f;
        
        m_connectionOutElement.style.position = Position.Relative;
        m_connectionOutElement.style.alignSelf = Align.Center;
        m_connectionOutElement.style.bottom = 0.0f;
        
        hierarchy.Add(m_connectionOutElement);
        
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
        m_connectionOutElement.visible = node.AcceptsConnectionOut();

        Label title = new Label(node.GetNodeName());
        title.style.color = Color.black;
        title.style.flexGrow = 0.0f;
        m_contentElement.Add(title);
        
        foreach (SerializedProperty childProperty in nodeSerialized)
        {
            var field = new PropertyField(childProperty, childProperty.name);
            field.Bind(m_behaviourTree);
            m_contentElement.Add(field);
        }
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
