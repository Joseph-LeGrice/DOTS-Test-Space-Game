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
        // if (this.edgeWidth <= 0)
        //     return;
        // this.UpdateRenderPoints();
        // if (this.m_RenderPoints.Count == 0)
        //     return;
        // Color inputColor = this.inputColor;
        // Color outputColor = this.outputColor;
        // Color col1 = inputColor * this.playModeTintColor;
        // Color col2 = outputColor * this.playModeTintColor;
        // uint count = (uint) this.m_RenderPoints.Count;
        // Painter2D painter2D = mgc.painter2D;
        // float num1 = (float) this.edgeWidth;
        // float alpha = 1f;
        // UnityEditor.Experimental.GraphView.GraphView graphView = this.m_GraphView;
        // float num2 = graphView != null ? graphView.scale : 1f;
        // if ((double) this.edgeWidth * (double) num2 < 1.75)
        // {
        //     alpha = (float) ((double) this.edgeWidth * (double) num2 / 1.75);
        //     num1 = 1.75f / num2;
        // }
        // EdgeControl.k_Gradient.SetKeys(new GradientColorKey[2]
        // {
        //     new GradientColorKey(col2, 0.0f),
        //     new GradientColorKey(col1, 1f)
        // }, new GradientAlphaKey[1]
        // {
        //     new GradientAlphaKey(alpha, 0.0f)
        // });
        // painter2D.BeginPath();
        // painter2D.strokeGradient = EdgeControl.k_Gradient;
        // painter2D.lineWidth = num1;
        // painter2D.MoveTo(this.m_RenderPoints[0]);
        // for (int index = 1; (long) index < (long) count; ++index)
        //     painter2D.LineTo(this.m_RenderPoints[index]);
        // painter2D.Stroke();
        
        Painter2D painter2D = mgc.painter2D;
        painter2D.BeginPath();
        painter2D.strokeColor = Color.white;
        painter2D.lineWidth = 2.0f;
        
        painter2D.MoveTo(m_fromNode.worldBound.center + m_screenOffset);
        painter2D.LineTo(m_toNode.worldBound.center + m_screenOffset);
        
        painter2D.Stroke();
        painter2D.ClosePath();
    }

    public void SetOffset(Vector2 offset)
    {
        m_screenOffset = offset;
    }
}
