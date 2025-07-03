using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorLine : VisualElement
{
    private VisualElement m_fromNode;
    private VisualElement m_toNode;
    private Vector2 m_screenOffset;

    public ConnectorLine()
    {
        style.position = Position.Absolute;
        style.flexGrow = 0.0f;
        style.flexShrink = 0.0f;
        style.height = new Length(100.0f, LengthUnit.Percent);
        style.width = new Length(100.0f, LengthUnit.Percent);
        pickingMode = PickingMode.Ignore;
        generateVisualContent += OnGenerateVisualContent;
    }

    public ConnectorLine(VisualElement from, VisualElement to) : this()
    {
        m_fromNode = from;
        m_toNode = to;
    }

    public void SetFromTo(VisualElement from, VisualElement to)
    {
        m_fromNode = from;
        m_toNode = to;
    }
    
    private void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        Painter2D painter2D = mgc.painter2D;
        painter2D.BeginPath();
        painter2D.strokeColor = Color.white;
        painter2D.lineWidth = 2.0f;

        m_screenOffset = new Vector2(0, -parent.worldBound.y);
        painter2D.MoveTo(m_fromNode.worldBound.center + m_screenOffset);
        painter2D.LineTo(m_toNode.worldBound.center + m_screenOffset);
        
        painter2D.Stroke();
        painter2D.ClosePath();
    }

    public Vector2 GetScreenOffset()
    {
        return m_screenOffset;
    }
}
