using System.Collections.Generic;
using System.Reflection;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorPointField : VisualElement
{
    private VisualElement m_connectorPoint;
    private List<ConnectorPoint> m_connectorPoints = new List<ConnectorPoint>();

    private ConnectorStateHandler m_connectorStateHandler;
    private bool m_isList;
    
    public ConnectorPointField(ConnectorStateHandler connectorStateHandler, FieldInfo field)
    {
        style.flexDirection = FlexDirection.Row;
        
        var nameLabel = new Label();
        nameLabel.text = field.Name;
        nameLabel.style.color = Color.black;
        nameLabel.style.flexGrow = 1.0f;
        nameLabel.style.flexShrink= 1.0f;
        Add(nameLabel);

        m_connectorStateHandler = connectorStateHandler;
        m_isList = typeof(IEnumerable<System.Int32>).IsAssignableFrom(field.FieldType);
        
        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);  
    }

    private void OnAttachToPanel(AttachToPanelEvent evt)
    {
        if (m_isList)
        {
            // TODO: Buttons for adding/removing arra elements (or an intfield)
            
            DataSourceContext thisDataSource = GetHierarchicalDataSourceContext();
            PropertyContainer.TryGetValue(thisDataSource.dataSource, thisDataSource.dataSourcePath, out IEnumerable<int> idList);
            
            int i = 0;
            foreach (var _ in idList)
            {
                var connectorPoint = new ConnectorPoint(m_connectorStateHandler, false);
                connectorPoint.dataSourcePath = PropertyPath.FromIndex(i);
                Add(connectorPoint);
                m_connectorPoints.Add(connectorPoint);
                i++;
            }
        }
        else
        {
            var connectorPoint = new ConnectorPoint(m_connectorStateHandler, false);
            Add(connectorPoint);
            m_connectorPoints.Add(connectorPoint);
        }
    }

    public VisualElement GetConnector(int i)
    {
        return m_connectorPoints[i];
    }
}
