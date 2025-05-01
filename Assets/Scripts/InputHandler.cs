using System;
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
    
    [SerializeField]
    private float m_cameraLookSpeed;
    [SerializeField]
    private float m_cameraRollSpeed;
    [SerializeField]
    private Transform m_cameraTransform;

    private Vector2 m_cameraEuler;
    private Vector2 m_cameraMoveDirection;
    private float m_rollDirection;

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
        Vector2 input = context.ReadValue<Vector2>();
        m_cameraMoveDirection.x = -input.y;
        m_cameraMoveDirection.y = input.x;
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        m_rollDirection = -context.ReadValue<float>();
    }

    private void Update()
    {
        m_cameraEuler.x = (m_cameraEuler.x + m_cameraMoveDirection.x * m_cameraLookSpeed * Time.unscaledDeltaTime + 360.0f) % 360.0f;
        m_cameraEuler.y = (m_cameraEuler.y + m_cameraMoveDirection.y * m_cameraLookSpeed * Time.unscaledDeltaTime + 360.0f) % 360.0f;
        
        Vector2 adjustedEuler = m_cameraEuler;
        adjustedEuler.x = (adjustedEuler.x - 180.0f) * Mathf.Deg2Rad;
        adjustedEuler.y = (adjustedEuler.y - 180.0f) * Mathf.Deg2Rad;
        
        float xzHypotenuse = Mathf.Cos(adjustedEuler.x);
        Vector3 forward = new Vector3(
            xzHypotenuse * Mathf.Sin(adjustedEuler.y),
            Mathf.Sin(adjustedEuler.x),
            xzHypotenuse * Mathf.Cos(adjustedEuler.y)
        );

        Vector3 up = m_cameraTransform.up;
        float rollDelta = m_rollDirection * m_cameraRollSpeed * Time.unscaledDeltaTime;
        up = Quaternion.AngleAxis(rollDelta, forward) * up;
        // Vector3.OrthoNormalize(ref forward, ref up);
        
        m_cameraTransform.localRotation = Quaternion.LookRotation(forward, up);
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
