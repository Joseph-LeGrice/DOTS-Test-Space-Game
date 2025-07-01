using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorLineView : VisualElement
{
    public ConnectorLineView()
    {
        this.generateVisualContent = this.generateVisualContent + new Action<MeshGenerationContext>(this.OnGenerateVisualContent);
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
    }
}
