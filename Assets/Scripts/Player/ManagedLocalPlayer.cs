using System;
using UnityEngine;

public class ManagedLocalPlayer : MonoBehaviour
{
    [SerializeField]
    private Transform m_playerTrackedTransform;
    [SerializeField]
    private LocalPlayerUserInterface m_userInterface;
    
    [SerializeField]
    private GameObject m_mainCamera;
    [SerializeField]
    private GameObject m_adsCamera;

    [SerializeField]
    private float m_lookSensitivity = 20.0f;
    [SerializeField]
    private float m_lookSensitivityADS = 20.0f;
    [SerializeField]
    private float m_rollSensitivity = 1.0f;
    [SerializeField]
    private float m_throttleAdjustSensitivity = 1.0f;
    [SerializeField]
    private float m_throttleDeadzone = 0.5f;
    
    
    public LocalPlayerUserInterface GetUserInterface()
    {
        return m_userInterface;
    }
    
    public void UpdatePlayerPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        m_playerTrackedTransform.SetPositionAndRotation(position, rotation);
    }

    public void SetADS(bool isADS)
    {
        m_mainCamera.SetActive(!isADS);
        m_adsCamera.SetActive(isADS);
    }
    
    public Transform GetPlayerTrackedTransform()
    {
        return m_playerTrackedTransform;
    }

    public float GetLookSensitivity()
    {
        return m_lookSensitivity;
    }

    public float GetLookSensitivityADS()
    {
        return m_lookSensitivityADS;
    }

    public float GetRollSensitivity()
    {
        return m_rollSensitivity;
    }

    public float GetThrottleSensitivity()
    {
        return m_throttleAdjustSensitivity;
    }

    public float GetThrottleDeadzone()
    {
        return m_throttleDeadzone;
    }
}
