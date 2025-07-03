using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorPoint : VisualElement
{
    private bool m_connectsIn;
    
    public ConnectorPoint(ConnectorStateHandler connectorStateHandler, bool connectsIn)
    {
        m_connectsIn = connectsIn;
        
        float size = 8.0f;
        float lineThickness = 2.0f;
        
        style.flexGrow = 0.0f;
        style.flexShrink = 0.0f;
        style.marginLeft = 8.0f;
        style.marginRight = 8.0f;
        style.alignSelf = Align.Center;
        style.width = style.height = size;
        style.borderTopColor = style.borderBottomColor =
            style.borderRightColor = style.borderLeftColor = Color.grey;
        style.borderTopWidth = style.borderBottomWidth =
            style.borderLeftWidth = style.borderRightWidth = lineThickness;
        style.borderTopLeftRadius = style.borderTopRightRadius =
            style.borderBottomLeftRadius = style.borderBottomRightRadius = size;
        
        this.AddManipulator(new ConnectorManipulator(connectorStateHandler, this));
    }

    public bool ConnectsIn()
    {
        return m_connectsIn;
    }

    public void ConnectTo(ConnectorPoint targetConnector)
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
