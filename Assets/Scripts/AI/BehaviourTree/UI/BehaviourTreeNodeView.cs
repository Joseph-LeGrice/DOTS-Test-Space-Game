using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourTreeNodeView : VisualElement
{
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
        // style.height = new Length(128.0f, LengthUnit.Pixel);

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
    }

    public void SetNode(SerializedProperty nodeSerialized)
    {
        BehaviourTreeNode node = (BehaviourTreeNode)nodeSerialized.managedReferenceValue;

        m_connectionInElement.visible = node.AcceptsConnectionIn();
        m_connectionOutElement.visible = node.AcceptsConnectionOut();

        Label title = new Label(node.GetNodeName());
        title.style.color = Color.black;
        title.style.flexGrow = 0.0f;
        m_contentElement.Add(title);
        
        foreach (SerializedProperty childProperty in nodeSerialized)
        {
            var field = new PropertyField(childProperty, childProperty.name);
            field.Bind(nodeSerialized.serializedObject);
            m_contentElement.Add(field);
        }
    }
}
