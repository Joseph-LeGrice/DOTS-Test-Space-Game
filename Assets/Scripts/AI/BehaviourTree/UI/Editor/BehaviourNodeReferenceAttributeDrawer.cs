using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(BehaviourNodeReferenceAttribute))]
public class BehaviourNodeReferenceAttributeDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        return new ConnectorPointView(property.displayName, ((BehaviourNodeReferenceAttribute)attribute).ConnectsIn);
    }
}
