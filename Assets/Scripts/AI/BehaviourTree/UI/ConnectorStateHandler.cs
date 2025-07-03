using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorStateHandler
{
    private bool m_isActive;

    private BehaviourTreeWindow m_rootWindow;
    
    private ConnectorPoint m_sourceConnector;
    private ConnectorPoint m_targetConnector;
    
    private ConnectorLine m_previewConnectorLine;
    private VisualElement m_dummyTargetElement;

    public ConnectorStateHandler(BehaviourTreeWindow rootWindow)
    {
        m_rootWindow = rootWindow;
        
        m_dummyTargetElement = new VisualElement();
        m_dummyTargetElement.visible = false;
        m_dummyTargetElement.style.position = Position.Absolute;
        m_dummyTargetElement.style.width = m_dummyTargetElement.style.height = new Length(1.0f, LengthUnit.Pixel);
        
        m_previewConnectorLine = new ConnectorLine();
        
        VisualElement root = rootWindow.Q<VisualElement>("NodeInstanceRoot");
        root.Add(m_dummyTargetElement);
        root.Add(m_previewConnectorLine);
        
        m_previewConnectorLine.visible = false;
    }

    public bool IsActive()
    {
        return m_isActive;
    }

    public void SetTargetConnector(ConnectorPoint targetConnector)
    {
        if (targetConnector == null || targetConnector.ConnectsIn())
        {
            m_targetConnector = targetConnector;
        }
    }

    public ConnectorPoint GetTargetConnector()
    {
        return m_targetConnector;
    }
    
    public void ActivateView(ConnectorPoint sourceConnector)
    {
        m_isActive = true;
        m_sourceConnector = sourceConnector;
        m_dummyTargetElement.style.top = sourceConnector.worldBound.center.y + m_previewConnectorLine.GetScreenOffset().y;
        m_dummyTargetElement.style.left = sourceConnector.worldBound.center.x + m_previewConnectorLine.GetScreenOffset().x;
        m_previewConnectorLine.visible = true;
        m_previewConnectorLine.SetFromTo(sourceConnector, m_dummyTargetElement);
        m_previewConnectorLine.MarkDirtyRepaint();
    }
    
    public void SetTargetPosition(Vector2 position)
    {
        if (m_targetConnector != null)
        {
            m_dummyTargetElement.style.top = m_targetConnector.worldBound.center.y + m_previewConnectorLine.GetScreenOffset().y;
            m_dummyTargetElement.style.left = m_targetConnector.worldBound.center.x + m_previewConnectorLine.GetScreenOffset().x;
        }
        else
        {
            m_dummyTargetElement.style.top = position.y + m_previewConnectorLine.GetScreenOffset().y;
            m_dummyTargetElement.style.left = position.x + m_previewConnectorLine.GetScreenOffset().x;
        }
        m_previewConnectorLine.MarkDirtyRepaint();
    }

    public void DeactivateView()
    {
        m_isActive = false;
        m_previewConnectorLine.visible = false;

        if (m_targetConnector != null)
        {
            m_sourceConnector.ConnectTo(m_targetConnector);
            m_rootWindow.RefreshConnectors();
        }
        m_sourceConnector = m_targetConnector = null;
    }
}
