using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    public Vector3 TargetDirection { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool VelocityDampersActive { get; private set; } = true;
    public bool IsBraking { get; private set; }
    public bool IsBoosting { get; private set; }

    private InputSystem_Actions m_inputActions;
    
    private void Awake()
    {
        m_inputActions = new InputSystem_Actions();
        m_inputActions.Player.AddCallbacks(this);
        m_inputActions.Player.Enable();
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        TargetDirection = context.ReadValue<Vector3>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        IsAttacking = context.ReadValueAsButton();
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        IsBoosting = context.ReadValueAsButton();
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        IsBraking = context.ReadValueAsButton();
    }

    public void OnVelocityDampers(InputAction.CallbackContext context)
    {
        if (context.action.WasPressedThisFrame())
        {
            VelocityDampersActive = !VelocityDampersActive;
        }
    }
}
