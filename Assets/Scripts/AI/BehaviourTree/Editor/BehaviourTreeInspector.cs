using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(BehaviourTree))]
public class BehaviourTreeInspector : Editor
{
    public VisualTreeAsset m_InspectorUXML;
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        
        Button b = new Button(OpenBehaviourTree);
        b.text = "Open";
        root.Add(b);
        
        return root;
    }

    private void OpenBehaviourTree()
    {
        BehaviourTreeEditorWindow.Open(serializedObject);
    }
}
