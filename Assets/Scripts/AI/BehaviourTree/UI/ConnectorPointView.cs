using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorPointView : VisualElement
{
    public ConnectorPointView()
    {
        float lineThickness = 2.0f;
        float size = 8.0f;
        
        style.flexGrow = 0.0f;
        style.flexShrink = 0.0f;
        style.alignSelf = Align.Center;
        style.width = style.height = size;
        style.borderTopColor = style.borderBottomColor =
            style.borderRightColor = style.borderLeftColor = Color.grey;
        style.borderTopWidth = style.borderBottomWidth =
            style.borderLeftWidth = style.borderRightWidth = lineThickness;
        style.borderTopLeftRadius = style.borderTopRightRadius =
            style.borderBottomLeftRadius = style.borderBottomRightRadius = size;
        
        this.AddManipulator(new ConnectorManipulator());
    }
}
