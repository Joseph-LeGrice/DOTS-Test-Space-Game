using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(BehaviourNodeReferenceAttribute))]
public class BehaviourNodeReferenceAttributeDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new VisualElement();
        root.style.paddingRight = 8.0f;
        
        root.style.flexDirection = FlexDirection.Row;
        Label nameLabel = new Label();
        nameLabel.style.flexGrow = 1.0f;
        nameLabel.style.flexShrink= 1.0f;
        nameLabel.text = property.displayName;
        root.Add(nameLabel);

        root.Add(new ConnectorPointView());
        
        return root;
    }
}
