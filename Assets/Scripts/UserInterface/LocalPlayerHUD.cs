using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalPlayerHUD : MonoBehaviour
{
    [SerializeField]
    private UIDocument m_uiDocument;

    private void Awake()
    {
        VisualElement healthValue = m_uiDocument.rootVisualElement.Q("HealthValue");
        VisualElement shieldValue = m_uiDocument.rootVisualElement.Q("ShieldValue");
        VisualElement linearDampersValue = m_uiDocument.rootVisualElement.Q("LinearDampersValue");
        
        foreach (VisualElement thrusterCursor in m_uiDocument.rootVisualElement.Query("ThrusterCursor").Build())
        {
            
        }

        int numThrusterUiElements = 10;
        for (int i = 0; i < numThrusterUiElements; i++)
        {
            VisualElement thrusterValue = m_uiDocument.rootVisualElement.Q("ThrusterValue" + i);
        }
    }
}
