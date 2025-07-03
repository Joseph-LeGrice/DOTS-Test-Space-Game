using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorPointView : VisualElement
{
    private bool m_connectsIn;
    private Label m_nameLabel;
    private VisualElement m_connectorPoint;
    
    public ConnectorPointView(ConnectorStateHandler connectorStateHandler, bool connectsIn)
    {
        m_connectsIn = connectsIn;
        
        float size = 8.0f;
        float lineThickness = 2.0f;
        
        style.flexDirection = FlexDirection.Row;
        
        m_nameLabel = new Label();
        m_nameLabel.style.color = Color.black;
        m_nameLabel.style.flexGrow = 1.0f;
        m_nameLabel.style.flexShrink= 1.0f;
        m_nameLabel.enabledSelf = false;
        Add(m_nameLabel);
        
        m_connectorPoint = new VisualElement();
        m_connectorPoint.style.flexGrow = 0.0f;
        m_connectorPoint.style.flexShrink = 0.0f;
        m_connectorPoint.style.marginLeft = 8.0f;
        m_connectorPoint.style.marginRight = 8.0f;
        m_connectorPoint.style.alignSelf = Align.Center;
        m_connectorPoint.style.width = m_connectorPoint.style.height = size;
        m_connectorPoint.style.borderTopColor = m_connectorPoint.style.borderBottomColor =
            m_connectorPoint.style.borderRightColor = m_connectorPoint.style.borderLeftColor = Color.grey;
        m_connectorPoint.style.borderTopWidth = m_connectorPoint.style.borderBottomWidth =
            m_connectorPoint.style.borderLeftWidth = m_connectorPoint.style.borderRightWidth = lineThickness;
        m_connectorPoint.style.borderTopLeftRadius = m_connectorPoint.style.borderTopRightRadius =
            m_connectorPoint.style.borderBottomLeftRadius = m_connectorPoint.style.borderBottomRightRadius = size;
        
        m_connectorPoint.AddManipulator(new ConnectorManipulator(connectorStateHandler, this));
        
        Add(m_connectorPoint);
    }
    
    public ConnectorPointView(ConnectorStateHandler connectorStateHandler, string label, bool connectsIn) : this(connectorStateHandler, connectsIn)
    {
        m_nameLabel.text = label;
        m_nameLabel.enabledSelf = true;
    }

    public VisualElement GetConnectorPoint()
    {
        return m_connectorPoint;
    }

    public bool ConnectsIn()
    {
        return m_connectsIn;
    }

    public void ConnectTo(ConnectorPointView targetConnector)
    {
        if (targetConnector.ConnectsIn())
        {
            var targetDataSource = targetConnector.GetHierarchicalDataSourceContext();
            PropertyContainer.TryGetValue(targetDataSource.dataSource, targetDataSource.dataSourcePath, out int targetId);

            var thisDataSource = GetHierarchicalDataSourceContext();
            PropertyContainer.SetValue(thisDataSource.dataSource, thisDataSource.dataSourcePath, targetId);
        }
    }
}
