using UnityEngine;
using UnityEditor;

public class BehaviourTreeEditorWindow : EditorWindow
{
    private BehaviourTreeWindow m_mainWindow;
    
    public static void Open(SerializedObject behaviourTree)
    {
        BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
        wnd.titleContent = new GUIContent("Behaviour Tree Editor");
        wnd.Create(behaviourTree.targetObject);
    }

    private void Create(Object behaviourTree)
    {
        if (m_mainWindow != null)
        {
            rootVisualElement.Remove(m_mainWindow);
            m_mainWindow = null;
        }
        m_mainWindow = new BehaviourTreeWindow((BehaviourTree)behaviourTree);
        rootVisualElement.Add(m_mainWindow);
    }
}
