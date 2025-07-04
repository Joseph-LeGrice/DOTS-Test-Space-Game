using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourTreeWindow : VisualElement
{
    
    private BehaviourTree m_behaviourTree;
    private ConnectorStateHandler m_connectorStateHandler;
    private Dictionary<int, BehaviourTreeNodeView> m_nodeViewLookup = new Dictionary<int, BehaviourTreeNodeView>();
    private List<ConnectorLine> m_allConnectors = new List<ConnectorLine>();
    private AddNodeContextMenu m_contextMenu;

    public BehaviourTreeWindow(BehaviourTree behaviourTree)
    {
        m_behaviourTree = behaviourTree;
        style.height = new Length(100.0f, LengthUnit.Percent);
        
        VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("BehaviourTreeEditor");
        visualTree.CloneTree(this);

        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        
        this.AddManipulator(new AddNodeContextMenuManipulator(this));
        // TODO: Drag / zoom handler
    }

    public ConnectorStateHandler GetConnectorStateHandler()
    {
        return m_connectorStateHandler;
    }

    public BehaviourTree GetSerializedBehaviourTree()
    {
        return m_behaviourTree;
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
        
        int i = m_behaviourTree.GetNodes().IndexOf(node);
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
        
        foreach (var sourceNode in m_behaviourTree.GetNodes())
        {
            var sourceNodeFields = sourceNode.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo sourceField in sourceNodeFields)
            {
                BehaviourNodeReferenceAttribute nodeReference = (BehaviourNodeReferenceAttribute)Attribute.GetCustomAttribute(sourceField, typeof(BehaviourNodeReferenceAttribute));
                if (nodeReference != null && !nodeReference.ConnectsIn)
                {
                    var targetValue = sourceField.GetValue(sourceNode);
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
    
    private void OnAttachToPanel(AttachToPanelEvent evt)
    {
        VisualElement nodeRoot = this.Q<VisualElement>("NodeInstanceRoot");
        m_connectorStateHandler = new ConnectorStateHandler(this);
        
        var allNodes = m_behaviourTree.GetNodes();
        for (int i = 0; i < allNodes.Count; i++)
        {
            BehaviourTreeNode node = allNodes[i];
            BehaviourTreeNodeView treeNodeViewInstance = new BehaviourTreeNodeView(this, i);
            nodeRoot.Add(treeNodeViewInstance);
            m_nodeViewLookup[node.m_nodeReference] = treeNodeViewInstance;
        }
        
        RefreshConnectors();
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
}
