using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourTreeWindow : VisualElement
{
    private IBehaviourTreeEditor m_behaviourTree;
    
    private AddNodeContextMenu m_contextMenu;
    private BehaviourTreeValueReferenceView m_initialNodeValueReference;
    
    private Dictionary<int, BehaviourTreeNodeView> m_nodeViewLookup = new Dictionary<int, BehaviourTreeNodeView>();
    
    private ConnectorStateHandler m_connectorStateHandler;
    private List<ConnectorLine> m_allConnectors = new List<ConnectorLine>();
    

    public BehaviourTreeWindow(IBehaviourTreeEditor behaviourTree)
    {
        m_behaviourTree = behaviourTree;
        
        styleSheets.Add(Resources.Load<StyleSheet>("BehaviourTreeStyles"));
        style.height = new Length(100.0f, LengthUnit.Percent);

        VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("BehaviourTreeEditor");
        visualTree.CloneTree(this);

        m_connectorStateHandler = new ConnectorStateHandler(this);

        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        
        this.AddManipulator(new AddNodeContextMenuManipulator(this));
        // TODO: Drag / zoom handler
    }

    public ConnectorStateHandler GetConnectorStateHandler()
    {
        return m_connectorStateHandler;
    }

    public IBehaviourTreeEditor GetSerializedBehaviourTree()
    {
        return m_behaviourTree;
    }

    public void RefreshAll()
    {
        if (m_initialNodeValueReference != null)
        {
            m_initialNodeValueReference.RemoveFromHierarchy();
        }
        
        m_initialNodeValueReference = new BehaviourTreeValueReferenceView(this);
        m_initialNodeValueReference.dataSource = m_behaviourTree.GetInitialNode();
        m_initialNodeValueReference.Bind(m_connectorStateHandler);
        Add(m_initialNodeValueReference);
        
        foreach (var nodeView in m_nodeViewLookup.Values)
        {
            nodeView.RemoveFromHierarchy();
        }
        m_nodeViewLookup.Clear();
        
        VisualElement nodeRoot = this.Q<VisualElement>("NodeInstanceRoot");
        
        int nodeCount = m_behaviourTree.GetNodeCount();
        for (int i = 0; i < nodeCount; i++)
        {
            BehaviourTreeNode node = m_behaviourTree.GetNode(i);
            BehaviourTreeNodeView treeNodeViewInstance = new BehaviourTreeNodeView(this, i);
            nodeRoot.Add(treeNodeViewInstance);
            m_nodeViewLookup[node.m_nodeReference] = treeNodeViewInstance;
        }
        
        RefreshConnectors();
    }

    public void OpenContextMenu(Vector2 position)
    {
        if (m_contextMenu == null)
        {
            m_contextMenu = new AddNodeContextMenu(this);
            VisualElement nodeRoot = this.Q<VisualElement>("NodeContextMenu");
            nodeRoot.Add(m_contextMenu);
        }

        m_contextMenu.style.top = position.y;
        m_contextMenu.style.left = position.x;
    }
    
    public void CloseContextMenu()
    {
        if (m_contextMenu != null)
        {
            m_contextMenu.RemoveFromHierarchy();
            m_contextMenu = null;
        }
    }

    public void RefreshNode(BehaviourTreeNode node)
    {
        m_nodeViewLookup[node.m_nodeReference].RemoveFromHierarchy();
        
        int i = m_behaviourTree.IndexOf(node);
        BehaviourTreeNodeView treeNodeViewInstance = new BehaviourTreeNodeView(this, i);
        VisualElement nodeRoot = this.Q<VisualElement>("NodeInstanceRoot");
        nodeRoot.Add(treeNodeViewInstance);
        m_nodeViewLookup[node.m_nodeReference] = treeNodeViewInstance;
        
        RefreshConnectors();
    }
    
    public void RefreshConnectors()
    {
        foreach (ConnectorLine connectorLine in m_allConnectors)
        {
            connectorLine.RemoveFromHierarchy();
        }
        m_allConnectors.Clear();

        CreateConnector(m_initialNodeValueReference);
        
        int nodeCount = m_behaviourTree.GetNodeCount();
        for (int i=0; i<nodeCount; i++)
        {
            BehaviourTreeNode sourceNode = m_behaviourTree.GetNode(i);

            if (sourceNode.m_nodeImplementation != null)
            {
                FieldInfo[] sourceNodeFields = sourceNode.m_nodeImplementation.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (FieldInfo sourceField in sourceNodeFields)
                {
                    BehaviourNodeReferenceAttribute nodeReference = (BehaviourNodeReferenceAttribute)Attribute.GetCustomAttribute(sourceField, typeof(BehaviourNodeReferenceAttribute));
                    if (nodeReference != null && !nodeReference.ConnectsIn)
                    {
                        var targetValue = sourceField.GetValue(sourceNode.m_nodeImplementation);
                        if (targetValue is int)
                        {
                            CreateConnector(sourceNode.m_nodeReference, (int)targetValue, sourceField.Name, 0);
                        }
                        else if (targetValue is IEnumerable<int>)
                        {
                            int connectionOutIndex = 0;
                            foreach (int targetValueReference in (IEnumerable<int>)targetValue)
                            {
                                CreateConnector(sourceNode.m_nodeReference, targetValueReference, sourceField.Name, connectionOutIndex);
                                connectionOutIndex++;
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void OnAttachToPanel(AttachToPanelEvent evt)
    {
        RefreshAll();
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        RepaintConnectors();
    }

    public void RepaintConnectors()
    {
        foreach (ConnectorLine clv in m_allConnectors)
        {
            clv.MarkDirtyRepaint();
        }
    }

    private void CreateConnector(int sourceReference, int targetReference, string connectionOutId, int connectionOutIndex)
    {
        if (m_nodeViewLookup.TryGetValue(sourceReference, out BehaviourTreeNodeView sourceNodeView) &&
            m_nodeViewLookup.TryGetValue(targetReference, out BehaviourTreeNodeView targetNodeView))
        {
            VisualElement connectorLineRoot = this.Q<VisualElement>("NodeConnectorLines");
            ConnectorLine newConnection = new ConnectorLine(
                sourceNodeView.GetConnectionOutElement(connectionOutId, connectionOutIndex),
                targetNodeView.GetConnectionInElement()
            );
            connectorLineRoot.Add(newConnection);
            m_allConnectors.Add(newConnection);
        }
    }

    private void CreateConnector(BehaviourTreeValueReferenceView valueReferenceView)
    {
        if (m_nodeViewLookup.TryGetValue(valueReferenceView.GetTarget(), out BehaviourTreeNodeView targetNodeView))
        {
            VisualElement connectorLineRoot = this.Q<VisualElement>("NodeConnectorLines");
            ConnectorLine newConnection = new ConnectorLine(
                valueReferenceView.GetConnectionOutElement(),
                targetNodeView.GetConnectionInElement()
            );
            connectorLineRoot.Add(newConnection);
            m_allConnectors.Add(newConnection);
        }
    }
}
