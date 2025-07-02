using UnityEngine;
using UnityEditor;

public class BehaviourTreeEditorWindow : EditorWindow
{
    public static void Open(SerializedObject behaviourTree)
    {
        BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
        wnd.titleContent = new GUIContent("Behaviour Tree Editor");
        wnd.rootVisualElement.Add(new BehaviourTreeWindow((BehaviourTree)behaviourTree.targetObject));
    }
}
