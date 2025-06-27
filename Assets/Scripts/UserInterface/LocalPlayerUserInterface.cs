using System.Collections.Generic;
using Unity.Physics.GraphicsIntegration;
using UnityEngine;

public class TargetData
{
    public bool IsTargeting;
    public bool CanTargetAhead;
    public Vector3 WorldPosition;
    public Vector3 UIPosition;
    public bool IsVisibleOnScreen;
}

public class LocalPlayerUserInterface : MonoBehaviour
{
    [SerializeField]
    private Camera m_uiCamera;
    [SerializeField]
    private Camera m_playerCamera;
    [SerializeField]
    private CrosshairLayout m_crosshair;
    [SerializeField]
    private TurnAccelerationDisplay m_accelerationDisplay;
    [SerializeField]
    private LocalPlayerHUD m_localPlayerHUD;
    [SerializeField]
    private TargetDisplay m_targetDisplay;
    [SerializeField]
    private Radar m_radar;
    [SerializeField]
    private Transform m_shipAim;
    [SerializeField]
    private Color m_uiColor;

    private bool m_initialised;
    
    public bool HasCreated()
    {
        return m_initialised;
    }

    private Vector3 GetUIPosition(Vector3 worldPosition, out bool isVisible)
    {
        Vector3 screenPosition = m_playerCamera.WorldToScreenPoint(worldPosition);
        Vector3 position = m_uiCamera.ScreenToWorldPoint(screenPosition);
        isVisible = position.z > 0.0f;
        // position.z = 50.0f;
        return position;
    }

    public LocalPlayerHUD GetHUD()
    {
        return m_localPlayerHUD;
    }
    
    public void SetShipAim(Vector3 worldPosition)
    {
        Vector3 uiPosition = GetUIPosition(worldPosition, out bool isVisible);
        m_shipAim.position = uiPosition;
    }

    public void SetHardpointAim(int hardpointIndex, Vector3 worldPosition)
    {
        Vector3 uiPosition = GetUIPosition(worldPosition, out bool isVisible);
        m_crosshair.GetDotTransform(hardpointIndex).position = uiPosition;
    }

    public void SetAngularThrottle(Vector2 angularThrottle)
    {
        m_accelerationDisplay.SetAngularThrottle(angularThrottle);
    }

    public void SetTargets(ManagedLocalPlayer localPlayer, List<TargetData> targetData)
    {
        List<TargetData> newTargets = new List<TargetData>();
        foreach (TargetData td in targetData)
        {
            td.UIPosition = GetUIPosition(td.WorldPosition, out bool isVisibleOnScreen);
            td.IsVisibleOnScreen = isVisibleOnScreen;
            newTargets.Add(td);
        }
        m_targetDisplay.UpdateTargets(newTargets);
        m_radar.UpdateTargets(localPlayer, newTargets);
    }

    public void Initialize(int shipHardpointsLength, float detectionRangeSquared)
    {
        m_radar.SetMaxDetectionRange(Mathf.Sqrt(detectionRangeSquared));
        m_accelerationDisplay.Initialise(m_uiColor);
        m_crosshair.RefreshCrosshairs(shipHardpointsLength, m_uiColor);
        m_initialised = true;
    }
}
