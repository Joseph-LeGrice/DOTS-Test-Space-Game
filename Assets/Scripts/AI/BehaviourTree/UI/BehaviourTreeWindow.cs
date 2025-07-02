using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourTreeWindow : VisualElement
{
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
    
    private BehaviourTree m_behaviourTree;
    private List<BehaviourNodeTypeData> m_allNodeTypes;
    private Dictionary<int, BehaviourTreeNodeView> m_nodeViewLookup = new Dictionary<int, BehaviourTreeNodeView>();
    private List<ConnectorLineView> m_allConnectors = new List<ConnectorLineView>();

    public BehaviourTreeWindow(BehaviourTree behaviourTree)
    {
        m_behaviourTree = behaviourTree;
        
        m_allNodeTypes = new List<BehaviourNodeTypeData>();
        m_allNodeTypes.Add(BehaviourNodeTypeData.Create<BehaviourTreeSequentialNode>());
        m_allNodeTypes.Add(BehaviourNodeTypeData.Create<BehaviourTreeConditionalNode>());
        
        style.height = new Length(100.0f, LengthUnit.Percent);

        VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("BehaviourTreeEditor");
        visualTree.CloneTree(this);
        
        VisualTreeAsset nodeButton = Resources.Load<VisualTreeAsset>("BehaviourTreeNodeButton");
        ListView nodeListView = this.Q<ListView>("NodeList");
        nodeListView.itemsSource = m_allNodeTypes;
        nodeListView.makeItem = nodeButton.CloneTree; 
        nodeListView.bindItem = BindCreateNodeButton; 
        nodeListView.RefreshItems();

        VisualElement nodeRoot = this.Q<VisualElement>("NodeInstanceRoot");
        var allNodes = m_behaviourTree.GetNodes();
        for (int i=0; i<allNodes.Count; i++)
        {
            BehaviourTreeNode node = allNodes[i];
            BehaviourTreeNodeView treeNodeViewInstance = new BehaviourTreeNodeView(this, i);
            nodeRoot.Add(treeNodeViewInstance);
            m_nodeViewLookup[node.m_nodeReference] = treeNodeViewInstance;
        }
        
        // Create all connections
        for (int i = 0; i < allNodes.Count; i++)
        {
            var sourceNode = allNodes[i];
            var sourceNodeFields = sourceNode.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo sourceField in sourceNodeFields)
            {
                BehaviourNodeReferenceAttribute nodeReference = (BehaviourNodeReferenceAttribute)Attribute.GetCustomAttribute(sourceField, typeof(BehaviourNodeReferenceAttribute));
                if (nodeReference != null && !nodeReference.ConnectsIn)
                {
                    var targetValue = sourceField.GetValue(sourceNode);
                    if (targetValue is int)
                    {
                        CreateConnector(sourceNode.m_nodeReference, (int)targetValue, sourceField.Name);
                    }
                    else if (targetValue is IEnumerable<int>)
                    {
                        foreach (int targetValueReference in (IEnumerable<int>)targetValue)
                        {
                            CreateConnector(sourceNode.m_nodeReference, targetValueReference, sourceField.Name); // Need a new identifier for sourceField.Name that includes array element
                        }
                    }
                }
            }
        }
        
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        
        // TODO: Drag / zoom handler
    }

    public BehaviourTree GetSerializedBehaviourTree()
    {
        return m_behaviourTree;
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        RepaintConnectors();
    }

    public void RepaintConnectors()
    {
        foreach (ConnectorLineView clv in m_allConnectors)
        {
            clv.MarkDirtyRepaint();
        }
    }

    private void CreateConnector(int sourceReference, int targetReference, string connectionOutId)
    {
        if (m_nodeViewLookup.TryGetValue(sourceReference, out BehaviourTreeNodeView sourceNodeView) &&
            m_nodeViewLookup.TryGetValue(targetReference, out BehaviourTreeNodeView targetNodeView))
        {
            VisualElement nodeRoot = this.Q<VisualElement>("NodeInstanceRoot");
            ConnectorLineView newConnection = new ConnectorLineView(
                sourceNodeView.GetConnectionOutElement(connectionOutId),
                targetNodeView.GetConnectionInElement()
            );
            nodeRoot.Add(newConnection);
            m_allConnectors.Add(newConnection);
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
