using UnityEngine;

public class ManagedSceneAccess : MonoSingleton<ManagedSceneAccess>
{
    [SerializeField]
    private ManagedLocalPlayer m_mainLocalPlayer;

    public ManagedLocalPlayer GetPlayer()
    {
        return m_mainLocalPlayer;
    }
}
