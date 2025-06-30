using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(BehaviourTree))]
public class BehaviourTreeEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        
        Button b = new Button(AddNew<BehaviourTreeSequentialNode>);
        b.text = "Add Sequential Node";
        root.Add(b);
        
        Button b1 = new Button(AddNew<BehaviourTreeConditionalNode>);
        b1.text = "Add Conditional Node";
        root.Add(b1);
        
        return root;
    }

    private void AddNew<T>() where T : BehaviourTreeNode, new()
    {
        serializedObject.Update();
        SerializedProperty nextNodeReference = serializedObject.FindProperty("m_nextNodeReference");
        T newInstance = new T
        {
            m_nodeReference = nextNodeReference.intValue
        };
        nextNodeReference.intValue = nextNodeReference.intValue + 1;
        
        SerializedProperty allNodes = serializedObject.FindProperty("m_allNodes");
        allNodes.InsertArrayElementAtIndex(allNodes.arraySize);
        var nodeElement = allNodes.GetArrayElementAtIndex(allNodes.arraySize - 1);
        nodeElement.managedReferenceValue = newInstance;
        
        serializedObject.ApplyModifiedProperties();
    }
}
