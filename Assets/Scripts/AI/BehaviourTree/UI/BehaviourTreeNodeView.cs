using System.Collections.Generic;
using System.Reflection;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public interface IMovableElement
{
    public BehaviourTreeWindow GetBehaviourTreeWindow();
    public void SetPosition(Vector2 newPosition);
}

public class BehaviourTreeNodeView : VisualElement, IMovableElement
{
    private BehaviourTreeWindow m_behaviourTreeWindow;
    private int m_nodeIndex;
    
    private Label m_title;
    private ConnectorPoint m_connectionInElement;
    private VisualElement m_contentElement;
    
    private Dictionary<string, VisualElement> m_fieldElementLookup = new Dictionary<string, VisualElement>();
    
    public BehaviourTreeNodeView(BehaviourTreeWindow window, int nodeArrayIndex)
    {
        m_behaviourTreeWindow = window;
        m_nodeIndex = nodeArrayIndex;
        
        AddToClassList(BehaviourTreeStyleSelectors.NodeView);

        VisualElement headerElement = new VisualElement();
        headerElement.style.flexDirection = FlexDirection.Row;

        BehaviourTreeNode node = GetNode();
        transform.position = node.m_nodePosition;
        
        m_connectionInElement = new ConnectorPoint(m_behaviourTreeWindow.GetConnectorStateHandler(), true);
        m_connectionInElement.dataSource = node;
        m_connectionInElement.dataSourcePath = PropertyPath.FromName(nameof(BehaviourTreeNode.m_nodeReference));
        headerElement.Add(m_connectionInElement);
        
        m_title = new Label();
        if (node.m_nodeImplementation != null)
        {
            m_title.text = node.m_nodeImplementation.GetNodeName();
            m_title.style.color = Color.black;
        }
        else
        {
            m_title.text = "MISSING REFERENCE";
            m_title.style.color = Color.red;
        }
        m_title.style.flexGrow = 0.0f;
        headerElement.Add(m_title);
        
        Button deleteButton = new Button(DeleteSelf);
        deleteButton.text = "X";
        headerElement.Add(deleteButton);
        
        hierarchy.Add(headerElement);

        m_contentElement = new VisualElement();
        m_contentElement.style.flexShrink = 0.0f;
        m_contentElement.style.flexGrow = 1.0f;
        
        hierarchy.Add(m_contentElement);

        if (node.m_nodeImplementation != null)
        {
            FieldInfo[] nodeFields = node.m_nodeImplementation.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo childProperty in nodeFields)
            {
                var nodeReference =
                    (BehaviourNodeReferenceAttribute)System.Attribute.GetCustomAttribute(childProperty,
                        typeof(BehaviourNodeReferenceAttribute));
                if (nodeReference != null && !nodeReference.ConnectsIn)
                {
                    var field = new ConnectorPointField(m_behaviourTreeWindow.GetConnectorStateHandler(),
                        childProperty.Name,
                        typeof(IEnumerable<System.Int32>).IsAssignableFrom(childProperty.FieldType)
                    );
                    field.dataSource = node;
                    field.dataSourcePath = PropertyPath.AppendName(PropertyPath.FromName("m_nodeImplementation"), childProperty.Name);
                    m_contentElement.Add(field);
                    m_fieldElementLookup[childProperty.Name] = field;
                }
                else
                {
                    if (System.Attribute.GetCustomAttribute(childProperty, typeof(HideInInspector)) != null)
                    {
                        continue;
                    }

                    switch (childProperty.FieldType.Name)
                    {
                        default:
                            Debug.LogError("Property type \"" + childProperty.FieldType.Name + "\" not yet supported!");
                            break;
                        case "Int32":
                            InitField(new IntegerField(), childProperty);
                            break;
                        case "String":
                            InitField(new TextField(), childProperty);
                            break;
                        case "Vector2":
                            InitField(new Vector2Field(), childProperty);
                            break;
                        case "Single":
                            InitField(new FloatField(), childProperty);
                            break;
                        case "Boolean":
                            InitField(new Toggle(), childProperty);
                            break;
                    }
                }
            }
        }

        this.AddManipulator(new MouseDragManipulator());
    }

    private void DeleteSelf()
    {
        m_behaviourTreeWindow.GetSerializedBehaviourTree().DeleteNode(m_nodeIndex);
        m_behaviourTreeWindow.RefreshAll();
    }

    private void InitField<T>(BaseField<T> field, FieldInfo childProperty)
    {
        field.style.color = Color.black;
        field.SetBinding(nameof(field.value), new DataBinding
        {
            dataSource = GetNode().m_nodeImplementation,
            dataSourcePath = PropertyPath.FromName(childProperty.Name)
        });
        field.label = childProperty.Name;
        m_contentElement.Add(field);
    }

    public BehaviourTreeWindow GetBehaviourTreeWindow()
    {
        return m_behaviourTreeWindow;
    }

    public void SetPosition(Vector2 newPosition)
    {
        BehaviourTreeNode node = GetNode();
        if (node != null)
        {
            node.m_nodePosition = newPosition;
        }
    }

    public VisualElement GetConnectionInElement()
    {
        return m_connectionInElement;
    }

    public VisualElement GetConnectionOutElement(string fieldName, int i)
    {
        return m_fieldElementLookup[fieldName].Q<ConnectorPointField>().GetConnector(i);
    }

    private BehaviourTreeNode GetNode()
    {
        return m_behaviourTreeWindow.GetSerializedBehaviourTree().GetNode(m_nodeIndex);
    }
}
