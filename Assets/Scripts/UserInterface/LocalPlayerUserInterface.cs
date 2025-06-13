using UnityEngine;

public class LocalPlayerUserInterface : MonoBehaviour
{
    [SerializeField]
    private Camera m_uiCamera;
    [SerializeField]
    private CrosshairLayout m_crosshair;
    [SerializeField]
    private TurnAccelerationDisplay m_accelerationDisplay;
    [SerializeField]
    private Transform m_shipAim;
    [SerializeField]
    private Color m_uiColor;

    private bool m_initialised;
    
    public bool HasCreated()
    {
        return m_initialised;
    }

    private Vector3 GetUIPosition(Vector3 screenPosition)
    {
        Vector3 position = m_uiCamera.ScreenToWorldPoint(screenPosition);
        position.z = 50.0f;
        return position;
    }
    
    public void SetShipAim(Vector2 screenPosition)
    {
        Vector3 position = GetUIPosition(screenPosition);
        m_shipAim.position = position;
    }

    public void SetHardpointAim(int hardpointIndex, Vector2 screenPosition)
    {
        Vector3 worldPosition = GetUIPosition(screenPosition);
        m_crosshair.GetDotTransform(hardpointIndex).position = worldPosition;
    }

    public void SetAcceleration(Vector2 direction, float accelerationNormalised)
    {
        m_accelerationDisplay.SetAcceleration(direction, accelerationNormalised);
    }

    public void Initialize(int shipHardpointsLength)
    {
        m_accelerationDisplay.Initialise(m_uiColor);
        m_crosshair.RefreshCrosshairs(shipHardpointsLength, m_uiColor);
        m_initialised = true;
    }
}
