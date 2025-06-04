using UnityEngine;

public class ManagedLocalPlayer : MonoBehaviour
{
    [SerializeField]
    private Transform m_playerTrackedTransform;
    [SerializeField]
    private Transform m_cameraPositionTransform;
    [SerializeField]
    private InputHandler m_playerInput;

    public void GetCameraAimDirection(out Vector3 forward, out Vector3 up)
    {
        forward = m_cameraPositionTransform.forward;
        up = m_cameraPositionTransform.up;
    }
    
    public void UpdatePlayerPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        m_playerTrackedTransform.SetPositionAndRotation(position, rotation);
        m_cameraPositionTransform.position = position;
    }
    
    public InputHandler GetPlayerInput()
    {
        return m_playerInput;
    }
}
