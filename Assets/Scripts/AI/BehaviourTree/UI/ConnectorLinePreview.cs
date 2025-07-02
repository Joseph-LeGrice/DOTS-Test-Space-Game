using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorLinePreview
{
    private VisualElement m_targetElement;
    private ConnectorLine m_connectorLine;
    private Vector2 m_offset;

    public ConnectorLinePreview(VisualElement root)
    {
        m_targetElement = new VisualElement();
        m_targetElement.visible = false;
        m_targetElement.style.position = Position.Absolute;
        m_targetElement.style.width = m_targetElement.style.height = new Length(1.0f, LengthUnit.Pixel);
        
        root.Add(m_targetElement);
        
        m_connectorLine = new ConnectorLine();
        root.Add(m_connectorLine);
        
        m_connectorLine.visible = false;
    }

    public void ActivateView(VisualElement from)
    {
        m_targetElement.style.top = from.worldBound.center.y + m_offset.y;
        m_targetElement.style.left = from.worldBound.center.x + m_offset.x;
        m_connectorLine.visible = true;
        m_connectorLine.SetFromTo(from, m_targetElement);
        m_connectorLine.MarkDirtyRepaint();
    }
    
    public void SetTargetPosition(Vector2 position)
    {
        m_targetElement.style.top = position.y + m_offset.y;
        m_targetElement.style.left = position.x + m_offset.x;
        m_connectorLine.MarkDirtyRepaint();
    }

    public void DeactivateView()
    {
        m_connectorLine.visible = false;
    }

    public void SetOffset(Vector2 offset)
    {
        m_offset = offset;
        m_connectorLine.SetOffset(offset);
    }
}
