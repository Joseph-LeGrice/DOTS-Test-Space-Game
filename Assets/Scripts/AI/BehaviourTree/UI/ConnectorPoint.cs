using Unity.Properties;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorPoint : VisualElement
{
    private bool m_connectsIn;
    
    public ConnectorPoint(ConnectorStateHandler connectorStateHandler, bool connectsIn)
    {
        m_connectsIn = connectsIn;
        
        AddToClassList(BehaviourTreeStyleSelectors.ConnectorPoint);
        
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
