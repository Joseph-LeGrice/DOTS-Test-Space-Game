using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalPlayerHUD : MonoBehaviour
{
    [SerializeField]
    private UIDocument m_uiDocument;

    private VisualElement m_thrusterDisplayRoot;
    
    private Label m_healthValue;
    private Label m_shieldValue;
    private Label m_linearDampersValue;
    
    private List<VisualElement> m_thrusterCursors;
    private VisualElement[] m_thrusterValues;
    
    private void Awake()
    {
        m_healthValue = m_uiDocument.rootVisualElement.Q<Label>("HealthValue");
        m_shieldValue = m_uiDocument.rootVisualElement.Q<Label>("ShieldValue");
        m_linearDampersValue = m_uiDocument.rootVisualElement.Q<Label>("LinearDampersValue");
        m_thrusterCursors = new List<VisualElement>(m_uiDocument.rootVisualElement.Query("ThrusterCursor").Build());

        int numThrusterUiElements = 10;
        m_thrusterValues = new VisualElement[numThrusterUiElements];
        for (int i = 0; i < numThrusterUiElements; i++)
        {
            m_thrusterValues[i] = m_uiDocument.rootVisualElement.Q("ThrusterValue" + i);
        }

        m_thrusterDisplayRoot = m_uiDocument.rootVisualElement.Q("ThrusterDisplayRoot");
    }

    public void UpdateHUD(ref ShipAspect ship)
    {
        m_healthValue.text = 100.0f * (ship.Damageable.ValueRO.CurrentHealth / ship.Damageable.ValueRO.MaxHealth) + "%";
        m_shieldValue.text = 100.0f * (ship.Damageable.ValueRO.CurrentHealth / ship.Damageable.ValueRO.MaxHealth) + "%";

        float displayHeight = m_thrusterDisplayRoot.resolvedStyle.height;
        float thrusterCursorT = ship.ShipInput.ValueRO.Throttle;
        if (ship.ShipInput.ValueRO.Throttle < 0.0f)
        {
            thrusterCursorT += 1.0f;
        }
        
        foreach (VisualElement thrusterCursor in m_thrusterCursors)
        {
            Vector3 translation = thrusterCursor.transform.position;
            translation.y = -thrusterCursorT * (displayHeight - thrusterCursor.resolvedStyle.height);
            thrusterCursor.transform.position = translation;
        }

        float3 localVelocity = ship.LocalToWorld.ValueRO.Value.InverseTransformDirection(ship.Velocity.ValueRO.Linear);
        float thrusterValueT = localVelocity.z / ship.ShipMovementData.ValueRO.MaximumVelocity;
        thrusterValueT = thrusterValueT * m_thrusterValues.Length;
        bool isReverse = thrusterValueT < 0.0f;
        thrusterValueT = math.abs(thrusterValueT);
        
        for (int i = 0; i < m_thrusterValues.Length; i++)
        {
            var tint = m_thrusterValues[i].resolvedStyle.unityBackgroundImageTintColor;
            float thrusterValueI = i;
            if (isReverse)
            {
                thrusterValueI = m_thrusterValues.Length - i;
            }
            tint.a = Mathf.Clamp(thrusterValueT - thrusterValueI, 0.0f, 1.0f);

            m_thrusterValues[i].style.unityBackgroundImageTintColor = tint;
        }
    }
}
