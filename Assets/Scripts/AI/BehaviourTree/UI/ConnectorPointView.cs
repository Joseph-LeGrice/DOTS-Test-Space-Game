using UnityEngine;
using UnityEngine.UIElements;

public class ConnectorPointView : VisualElement
{
    private Label m_nameLabel;
    private VisualElement m_connectorPoint;
    
    public ConnectorPointView(bool connectsIn)
    {
        float size = 8.0f;
        float lineThickness = 2.0f;
        
        style.flexGrow = 1.0f;
        style.flexShrink = 1.0f;
        style.flexDirection = FlexDirection.Row;
        style.height = new Length(size, LengthUnit.Pixel);
        
        m_nameLabel = new Label();
        m_nameLabel.style.flexGrow = 1.0f;
        m_nameLabel.style.flexShrink= 1.0f;
        m_nameLabel.enabledSelf = false;
        Add(m_nameLabel);
        
        m_connectorPoint = new VisualElement();
        m_connectorPoint.name = "ConnectorPointView";
        m_connectorPoint.style.paddingRight = 8.0f;
        
        m_connectorPoint.style.flexGrow = 0.0f;
        m_connectorPoint.style.flexShrink = 0.0f;
        // connectorPoint.style.alignSelf = Align.Center;
        m_connectorPoint.style.width = style.height = size;
        m_connectorPoint.style.borderTopColor = m_connectorPoint.style.borderBottomColor =
            m_connectorPoint.style.borderRightColor = m_connectorPoint.style.borderLeftColor = Color.grey;
        m_connectorPoint.style.borderTopWidth = m_connectorPoint.style.borderBottomWidth =
            m_connectorPoint.style.borderLeftWidth = m_connectorPoint.style.borderRightWidth = lineThickness;
        m_connectorPoint.style.borderTopLeftRadius = m_connectorPoint.style.borderTopRightRadius =
            m_connectorPoint.style.borderBottomLeftRadius = m_connectorPoint.style.borderBottomRightRadius = size;
        
        m_connectorPoint.AddManipulator(new ConnectorManipulator());
        
        Add(m_connectorPoint);
    }
    
    public ConnectorPointView(string label, bool connectsIn) : this(connectsIn)
    {
        m_nameLabel.text = label;
        m_nameLabel.enabledSelf = true;
    }

    public VisualElement GetConnectorPoint()
    {
        return m_connectorPoint;
    }
}
