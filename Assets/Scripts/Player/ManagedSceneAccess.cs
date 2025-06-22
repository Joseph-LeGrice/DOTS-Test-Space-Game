using UnityEngine;
using UnityEngine.InputSystem;

public class ManagedSceneAccess : MonoSingleton<ManagedSceneAccess>
{
    [SerializeField]
    private ManagedLocalPlayer m_mainLocalPlayer;
    [SerializeField]
    private InputActionAsset m_inputActionAsset;

    public ManagedLocalPlayer GetPlayer()
    {
        return m_mainLocalPlayer;
    }

    public InputActionAsset GetInputActionAsset()
    {
        return m_inputActionAsset;
    }
}
