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
        m_targetElement.transform.position = from.worldBound.center + m_offset;
        m_connectorLine.visible = true;
        m_connectorLine.SetFromTo(from, m_targetElement);
        m_connectorLine.MarkDirtyRepaint();
    }
    
    public void SetTargetPosition(Vector2 position)
    {
        m_targetElement.transform.position = position + m_offset;
        m_connectorLine.MarkDirtyRepaint();
    }

    public void DeactivateView()
    {
        m_connectorLine.visible = false;
    }

    public void SetOffset(Vector2 offset)
    {
        m_connectorLine.SetOffset(offset);
    }
}
