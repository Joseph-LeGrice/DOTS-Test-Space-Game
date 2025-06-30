using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class BehaviourTreeEditorWindow : EditorWindow
{
    private SerializedObject m_behaviourTree;
    
    [System.Serializable]
    public class BehaviourNodeTypeData
    {
        public string NodeName;

        public static BehaviourNodeTypeData Create<T>() where T : BehaviourTreeNode
        {
            return new BehaviourNodeTypeData()
            {
                NodeName = typeof(T).Name
            };
        }
    }
    
    [SerializeField]
    private List<BehaviourNodeTypeData> m_allNodeTypes;
    
    public static void Open(SerializedObject behaviourTree)
    {
        // This method is called when the user selects the menu item in the Editor.
        BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
        wnd.titleContent = new GUIContent("Behaviour Tree Editor");
        wnd.m_behaviourTree = behaviourTree;

        // Limit size of the window.
        wnd.minSize = new Vector2(450, 200);
        wnd.maxSize = new Vector2(1920, 720);
    }
    
    public void CreateGUI()
    {
        m_allNodeTypes = new List<BehaviourNodeTypeData>();
        m_allNodeTypes.Add(BehaviourNodeTypeData.Create<BehaviourTreeSequentialNode>());
        m_allNodeTypes.Add(BehaviourNodeTypeData.Create<BehaviourTreeConditionalNode>());
        
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("afeeeff658b12d94f90257e46d6165f7"));
        var root = visualTree.Instantiate();
        root.style.height = new StyleLength(new Length(100.0f, LengthUnit.Percent));
        
        var nodeListView = root.Q<ListView>("NodeList");
        nodeListView.dataSource = this;
        // nodeListView.makeItem = 
        
        rootVisualElement.Add(root);
    }

    private void AddNew<T>() where T : BehaviourTreeNode, new()
    {
        // serializedObject.Update();
        // SerializedProperty nextNodeReference = serializedObject.FindProperty("m_nextNodeReference");
        // T newInstance = new T
        // {
        //     m_nodeReference = nextNodeReference.intValue
        // };
        // nextNodeReference.intValue = nextNodeReference.intValue + 1;
        //
        // SerializedProperty allNodes = serializedObject.FindProperty("m_allNodes");
        // allNodes.InsertArrayElementAtIndex(allNodes.arraySize);
        // var nodeElement = allNodes.GetArrayElementAtIndex(allNodes.arraySize - 1);
        // nodeElement.managedReferenceValue = newInstance;
        //
        // serializedObject.ApplyModifiedProperties();
    }
}
