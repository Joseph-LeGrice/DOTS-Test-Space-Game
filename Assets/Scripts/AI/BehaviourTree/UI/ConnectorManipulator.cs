using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorManipulator : MouseManipulator
{
    private bool m_isDragging;
    private Vector2 m_mouseOffset;
    private readonly ConnectorStateHandler m_connectorStateHandler;
    private ConnectorPointView m_relatedConnector;

    public ConnectorManipulator(ConnectorStateHandler connectorStateHandler, ConnectorPointView connector)
    {
        m_connectorStateHandler = connectorStateHandler;
        m_relatedConnector = connector;
    }
        
    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseEnterEvent>(OnEnter);
        target.RegisterCallback<MouseLeaveEvent>(OnLeave);
        target.RegisterCallback<MouseUpEvent>(OnUp);
        target.RegisterCallback<MouseDownEvent>(OnDown);
        target.RegisterCallback<MouseMoveEvent>(OnMove);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseEnterEvent>(OnEnter);
        target.UnregisterCallback<MouseLeaveEvent>(OnLeave);
        target.UnregisterCallback<MouseUpEvent>(OnUp);
        target.UnregisterCallback<MouseDownEvent>(OnDown);
        target.UnregisterCallback<MouseMoveEvent>(OnMove);
    }

    private void OnEnter(MouseEnterEvent evt)
    {
        if (!m_isDragging && m_connectorStateHandler.IsActive())
        {
            m_connectorStateHandler.SetTargetConnector(m_relatedConnector);
        }
    }

    private void OnLeave(MouseLeaveEvent evt)
    {
        if (!m_isDragging && m_connectorStateHandler.GetTargetConnector() == m_relatedConnector)
        {
            m_connectorStateHandler.SetTargetConnector(null);
        }
    }
    
    private void OnDown(MouseDownEvent evt)
    {
        if (!m_isDragging)
        {
            m_isDragging = true;
            evt.StopPropagation();
            target.CaptureMouse();
            m_connectorStateHandler.ActivateView(m_relatedConnector);
        }
    }
    
    private void OnUp(MouseUpEvent evt)
    {
        if (m_isDragging)
        {
            m_isDragging = false;
            target.ReleaseMouse();
            evt.StopPropagation();
            m_connectorStateHandler.DeactivateView();
        }
    }
    
    private void OnMove(MouseMoveEvent evt)
    {
        if (m_isDragging)
        {
            m_connectorStateHandler.SetTargetPosition(evt.mousePosition);
        }
    }
}
