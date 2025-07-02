using UnityEngine;
using UnityEngine.UIElements;

public class MouseDragManipulator : MouseManipulator
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
            m_mouseOffset = evt.localMousePosition;
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
            
            ((BehaviourTreeNodeView)target).SetPosition(target.transform.position);
        }
    }
    
    private void OnMove(MouseMoveEvent evt)
    {
        if (m_isDragging)
        {
            Vector2 newPosition = (Vector2)target.transform.position + evt.mouseDelta;
            target.transform.position = newPosition;
            evt.StopPropagation();
            
            ((BehaviourTreeNodeView)target).GetBehaviourTreeWindow().RepaintConnectors();
        }
    }
}
