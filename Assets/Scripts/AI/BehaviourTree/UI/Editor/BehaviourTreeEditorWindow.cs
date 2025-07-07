using UnityEngine;
using UnityEditor;

public class BehaviourTreeEditorWindow : EditorWindow
{
    private BehaviourTreeWindow m_mainWindow;
    
    public static void Open(SerializedObject behaviourTree)
    {
        BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
        wnd.titleContent = new GUIContent("Behaviour Tree Editor");
        wnd.Create(behaviourTree);
    }

    private void Create(SerializedObject behaviourTree)
    {
        if (m_mainWindow != null)
        {
            rootVisualElement.Remove(m_mainWindow);
            m_mainWindow = null;
        }
        SerializedObjectBehaviourTreeEditor editor = new SerializedObjectBehaviourTreeEditor(behaviourTree);
        m_mainWindow = new BehaviourTreeWindow(editor);
        rootVisualElement.Add(m_mainWindow);
    }
}
