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
    }

    public void UpdateHUD(ref ShipAspect ship)
    {
        m_healthValue.text = 100.0f * (ship.Damageable.ValueRO.CurrentHealth / ship.Damageable.ValueRO.MaxHealth) + "%";
        m_shieldValue.text = 100.0f * (ship.Damageable.ValueRO.CurrentHealth / ship.Damageable.ValueRO.MaxHealth) + "%";

        foreach (VisualElement thrusterCursor in m_thrusterCursors)
        {
            StyleBackgroundPosition backgroundPositionY = thrusterCursor.style.backgroundPositionY;
            BackgroundPosition value = backgroundPositionY.value;
            value.offset.value = ((ship.ShipInput.ValueRO.TargetDirection.z + 1.0f) % 1.0f) * 100.0f;
            backgroundPositionY.value = value;
            thrusterCursor.style.backgroundPositionY = backgroundPositionY;
        }

        float3 localVelocity = ship.LocalToWorld.ValueRO.Value.InverseTransformDirection(ship.Velocity.ValueRO.Linear);
        float t = localVelocity.z / ship.ShipMovementData.ValueRO.MaximumVelocity;
        t = t * m_thrusterValues.Length;
        
        for (int i = 0; i < m_thrusterValues.Length; i++)
        {
            var tint = m_thrusterValues[i].style.unityBackgroundImageTintColor;
            Color c = tint.value;
            c.a = Mathf.Clamp(t - i, 0.0f, 1.0f);
            tint.value = c;
            m_thrusterValues[i].style.unityBackgroundImageTintColor = tint;
        }
    }
}
