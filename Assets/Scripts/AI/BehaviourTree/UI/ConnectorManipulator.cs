using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorManipulator : MouseManipulator
{
    private bool m_isDragging;
    private Vector2 m_mouseOffset;
    
    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseUpEvent>(OnUp);
        target.RegisterCallback<MouseDownEvent>(OnDown);
        target.RegisterCallback<MouseMoveEvent>(OnMove);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseUpEvent>(OnUp);
        target.UnregisterCallback<MouseDownEvent>(OnDown);
        target.UnregisterCallback<MouseMoveEvent>(OnMove);
    }

    private void OnDown(MouseDownEvent evt)
    {
        if (!m_isDragging)
        {
            m_isDragging = true;
            evt.StopPropagation();
            target.CaptureMouse();
        }
    }
    
    private void OnUp(MouseUpEvent evt)
    {
        if (m_isDragging)
        {
            m_isDragging = false;
            target.ReleaseMouse();
            evt.StopPropagation();
        }
    }
    
    private void OnMove(MouseMoveEvent evt)
    {
        if (m_isDragging)
        {
        }
    }
}
