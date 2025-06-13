using System;
using UnityEngine;

public class ManagedLocalPlayer : MonoBehaviour
{
    [SerializeField]
    private Transform m_playerTrackedTransform;
    [SerializeField]
    private InputHandler m_playerInput;
    [SerializeField]
    private LocalPlayerUserInterface m_userInterface;
    
    [SerializeField]
    private Camera m_cinemachineCamera;
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

    public LocalPlayerUserInterface GetUserInterface()
    {
        return m_userInterface;
    }
    
    public Camera GetMainCamera()
    {
        return m_cinemachineCamera;
    }
    
    public void UpdatePlayerPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        m_playerTrackedTransform.SetPositionAndRotation(position, rotation);
    }

    public Transform GetPlayerTrackedTransform()
    {
        return m_playerTrackedTransform;
    }
    
    public InputHandler GetPlayerInput()
    {
        return m_playerInput;
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

    private void Update()
    {
        m_mainCamera.SetActive(!m_playerInput.IsADS);
        m_adsCamera.SetActive(m_playerInput.IsADS);
    }
}
