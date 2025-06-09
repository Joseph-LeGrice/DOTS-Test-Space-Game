using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    public Vector3 TargetDirection { get; private set; }
    public Vector2 LookDelta { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool ADS { get; private set; } = true;
    public bool IsBraking { get; private set; }
    public bool IsBoosting { get; private set; }
    public float RollDirection { get; private set; }

    private InputSystem_Actions m_inputActions;
    
    private void Awake()
    {
        m_inputActions = new InputSystem_Actions();
        m_inputActions.Player.AddCallbacks(this);
        m_inputActions.Player.Enable();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        TargetDirection = context.ReadValue<Vector3>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        LookDelta = new Vector2(input.x, -input.y);
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        RollDirection = -context.ReadValue<float>();
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

    public void OnADS(InputAction.CallbackContext context)
    {
        ADS = context.ReadValueAsButton();
    }
}
