using UnityEngine;

public class ManagedSceneAccess : MonoSingleton<ManagedSceneAccess>
{
    // To support local multiplayer, have a class containing a camera / input handler per player 
    
    [SerializeField]
    private Camera m_mainCamera;
    [SerializeField]
    private InputHandler m_inputHandler;

    public InputHandler GetInputHandler()
    {
        return m_inputHandler;
    }
    
    public Camera GetMainCamera()
    {
        return m_mainCamera;
    }
}
