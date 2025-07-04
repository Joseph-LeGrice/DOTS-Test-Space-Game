using UnityEngine;
using UnityEngine.UIElements;

public class AddNodeContextMenuManipulator : MouseManipulator
{
    private BehaviourTreeWindow m_window;
    private int m_pressedButton = -1;

    public AddNodeContextMenuManipulator(BehaviourTreeWindow behaviourTreeWindow)
    {
        m_window = behaviourTreeWindow;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseUpEvent>(OnClickUp);
        target.RegisterCallback<MouseDownEvent>(OnClickDown);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseUpEvent>(OnClickUp);
        target.UnregisterCallback<MouseDownEvent>(OnClickDown);
    }

    private void OnClickDown(MouseDownEvent mouseDownEvent)
    {
        m_pressedButton = mouseDownEvent.button;
    }
    
    private void OnClickUp(MouseUpEvent mouseUpEvent)
    {
        if (m_pressedButton == mouseUpEvent.button)
        {
            if (mouseUpEvent.button == 1)
            {
                m_window.OpenContextMenu(mouseUpEvent.localMousePosition);
            }
            else if (mouseUpEvent.button == 0)
            {
                m_window.CloseContextMenu();
            }
        }

        m_pressedButton = -1;
    }
}
