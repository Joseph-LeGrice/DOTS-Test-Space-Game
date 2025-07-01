using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class BehaviourTreeEditorWindow : EditorWindow
{
    private static SerializedObject s_behaviourTree;
    
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
        s_behaviourTree = behaviourTree;
        
        BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
        wnd.titleContent = new GUIContent("Behaviour Tree Editor");
    }
    
    public void CreateGUI()
    {
        m_allNodeTypes = new List<BehaviourNodeTypeData>();
        m_allNodeTypes.Add(BehaviourNodeTypeData.Create<BehaviourTreeSequentialNode>());
        m_allNodeTypes.Add(BehaviourNodeTypeData.Create<BehaviourTreeConditionalNode>());
        
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("afeeeff658b12d94f90257e46d6165f7"));
        visualTree.CloneTree(rootVisualElement);
        
        VisualTreeAsset nodeButton = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("be27b30f9d53faf40969f3e569078b6b"));
        ListView nodeListView = rootVisualElement.Q<ListView>("NodeList");
        nodeListView.itemsSource = m_allNodeTypes;
        nodeListView.makeItem = nodeButton.CloneTree; 
        nodeListView.bindItem = BindCreateNodeButton; 
        nodeListView.RefreshItems();

        s_behaviourTree.Update();
        VisualElement nodeRoot = rootVisualElement.Q<VisualElement>("NodeInstanceRoot");
        SerializedProperty allNodes = s_behaviourTree.FindProperty("m_allNodes");
        for (int i=0; i<allNodes.arraySize; i++)
        {
            BehaviourTreeNodeView treeNodeViewInstance = new BehaviourTreeNodeView();
            treeNodeViewInstance.SetNode(s_behaviourTree, i);
            nodeRoot.Add(treeNodeViewInstance);
        }
    }

    private void BindCreateNodeButton(VisualElement button, int i)
    {
        button.Q<Button>().text = m_allNodeTypes[i].NodeName;
        // ((Button)button).RegisterCallback();
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
