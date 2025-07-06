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
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            Add(container);
            
            DataSourceContext thisDataSource = GetHierarchicalDataSourceContext();
            if (PropertyContainer.TryGetValue(thisDataSource.dataSource, thisDataSource.dataSourcePath,
                    out IEnumerable<int> idList))
            {
                int i = 0;
                foreach (var _ in idList)
                {
                    var connectorPointContainer = new VisualElement();
                    connectorPointContainer.style.flexDirection = FlexDirection.Row;
                    connectorPointContainer.dataSourcePath = PropertyPath.FromIndex(i);

                    int ii = i;
                    var removeButton = new Button();
                    removeButton.text = "-";
                    removeButton.RegisterCallback<ClickEvent, int>(RemoveConnectorElement, ii);
                    connectorPointContainer.Add(removeButton);

                    var connectorPoint = new ConnectorPoint(m_connectorStateHandler, false);
                    connectorPointContainer.Add(connectorPoint);
                    m_connectorPoints.Add(connectorPoint);

                    container.Add(connectorPointContainer);

                    i++;
                }
            }

            var addButton = new Button(AddConnectorElement);
            addButton.text = "+";
            container.Add(addButton);
        }
        else
        {
            var connectorPoint = new ConnectorPoint(m_connectorStateHandler, false);
            Add(connectorPoint);
            m_connectorPoints.Add(connectorPoint);
        }
    }

    private void AddConnectorElement()
    {
        DataSourceContext thisDataSource = GetHierarchicalDataSourceContext();
        if (PropertyContainer.TryGetValue(thisDataSource.dataSource, thisDataSource.dataSourcePath,
                out IList<int> idList))
        {
            idList.Add(-1);
            PropertyContainer.SetValue(thisDataSource.dataSource, thisDataSource.dataSourcePath, idList);
        }
        else if (PropertyContainer.TryGetValue(thisDataSource.dataSource, thisDataSource.dataSourcePath,
                     out int[] idArray))
        {
            Debug.LogError("TODO");
        }
        
        m_connectorStateHandler.GetRootWindow().RefreshNode((BehaviourTreeNode)GetHierarchicalDataSourceContext().dataSource);
    }

    private void RemoveConnectorElement(ClickEvent evt, int i)
    {
        DataSourceContext thisDataSource = GetHierarchicalDataSourceContext();
        if (PropertyContainer.TryGetValue(thisDataSource.dataSource, thisDataSource.dataSourcePath,
                out IList<int> idList))
        {
            idList.RemoveAt(i);
            PropertyContainer.SetValue(thisDataSource.dataSource, thisDataSource.dataSourcePath, idList);
        }
        else if (PropertyContainer.TryGetValue(thisDataSource.dataSource, thisDataSource.dataSourcePath,
                out int[] idArray))
        {
            Debug.LogError("TODO");
        }
        
        m_connectorStateHandler.GetRootWindow().RefreshNode((BehaviourTreeNode)GetHierarchicalDataSourceContext().dataSource);
    }

    public VisualElement GetConnector(int i)
    {
        return m_connectorPoints[i];
    }
}
