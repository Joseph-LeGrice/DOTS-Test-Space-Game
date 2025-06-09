using System;
using UnityEngine;

public class ManagedLocalPlayer : MonoBehaviour
{
    [SerializeField]
    private Transform m_playerTrackedTransform;
    [SerializeField]
    private InputHandler m_playerInput;
    
    [SerializeField]
    private GameObject m_mainCamera;
    [SerializeField]
    private GameObject m_adsCamera;

    [SerializeField]
    private float m_lookSensitivity;
    
    public void UpdatePlayerPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        m_playerTrackedTransform.SetPositionAndRotation(position, rotation);
    }
    
    public InputHandler GetPlayerInput()
    {
        return m_playerInput;
    }

    public float GetLookSensitivity()
    {
        return m_lookSensitivity;
    }

    private void Update()
    {
        m_mainCamera.SetActive(!m_playerInput.ADS);
        m_adsCamera.SetActive(m_playerInput.ADS);
    }
}
