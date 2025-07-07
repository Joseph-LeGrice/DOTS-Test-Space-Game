using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourTreeValueReferenceView : VisualElement, IMovableElement
{
    private BehaviourTreeWindow m_behaviourTreeWindow;
    private ConnectorPointField m_connector;

    public BehaviourTreeValueReferenceView(BehaviourTreeWindow behaviourTreeWindow)
    {
        m_behaviourTreeWindow = behaviourTreeWindow;
        AddToClassList(BehaviourTreeStyleSelectors.NodeView);
        this.AddManipulator(new MouseDragManipulator());
    }

    public void Bind(ConnectorStateHandler connectorStateHandler)
    {
        transform.position = PropertyContainer.GetValue<BehaviourTreeValueReference, Vector2>(
            (BehaviourTreeValueReference)GetHierarchicalDataSourceContext().dataSource,
            PropertyPath.FromName(nameof(BehaviourTreeValueReference.m_nodePosition))
        );
        
        var connector = new ConnectorPointField(connectorStateHandler, "Start", false);
        connector.dataSourcePath = PropertyPath.FromName(nameof(BehaviourTreeValueReference.m_nodeReference));
        m_connector = connector;
        Add(connector);
    }

    public BehaviourTreeWindow GetBehaviourTreeWindow()
    {
        return m_behaviourTreeWindow;
    }

    public void SetPosition(Vector2 newPosition)
    {
        PropertyContainer.SetValue(
            GetHierarchicalDataSourceContext().dataSource,
            PropertyPath.FromName(nameof(BehaviourTreeValueReference.m_nodePosition)),
            newPosition
        );
    }

    public int GetTarget()
    {
        return PropertyContainer.GetValue<BehaviourTreeValueReference, int>(
            (BehaviourTreeValueReference)GetHierarchicalDataSourceContext().dataSource,
            PropertyPath.FromName(nameof(BehaviourTreeValueReference.m_nodeReference))
        );
    }

    public VisualElement GetConnectionOutElement()
    {
        return m_connector.GetConnector(0);
    }
}
