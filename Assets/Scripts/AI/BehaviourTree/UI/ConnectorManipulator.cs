using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorManipulator : MouseManipulator
{
    private bool m_isDragging;
    private Vector2 m_mouseOffset;
    private readonly BehaviourTreeWindow m_behaviourTreeWindow;

    public ConnectorManipulator(BehaviourTreeWindow behaviourTreeWindow)
    {
        m_behaviourTreeWindow = behaviourTreeWindow;
    }
        
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
            m_behaviourTreeWindow.GetLinePreview().ActivateView(target);
        }
    }
    
    private void OnUp(MouseUpEvent evt)
    {
        if (m_isDragging)
        {
            m_isDragging = false;
            target.ReleaseMouse();
            evt.StopPropagation();
            m_behaviourTreeWindow.GetLinePreview().DeactivateView();
        }
    }
    
    private void OnMove(MouseMoveEvent evt)
    {
        if (m_isDragging)
        {
            m_behaviourTreeWindow.GetLinePreview().SetTargetPosition(evt.mousePosition);
        }
    }
}
