using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions m_inputActions;

    private void Awake()
    {
        m_inputActions = new InputSystem_Actions();
        m_inputActions.Player.AddCallbacks(this);
        m_inputActions.Player.Enable();
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnVelocityDampers(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }
}
