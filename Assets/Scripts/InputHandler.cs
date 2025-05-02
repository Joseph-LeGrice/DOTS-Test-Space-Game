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

    private Vector2 m_cameraMoveDirection;
    private float m_rollDirection;

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
        m_cameraMoveDirection.x = -input.y;
        m_cameraMoveDirection.y = input.x;
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        m_rollDirection = -context.ReadValue<float>();
    }

    private void Update()
    {
        float pitchDelta = m_cameraMoveDirection.x * m_cameraLookSpeed * Time.unscaledDeltaTime;
        float yawDelta = m_cameraMoveDirection.y * m_cameraLookSpeed * Time.unscaledDeltaTime;

        Vector3 forward = m_cameraTransform.forward;
        forward = Quaternion.AngleAxis(pitchDelta, m_cameraTransform.right) * forward;
        forward = Quaternion.AngleAxis(yawDelta, m_cameraTransform.up) * forward;

        Vector3 up = m_cameraTransform.up;
        Vector3.OrthoNormalize(ref forward, ref up);
        float rollDelta = m_rollDirection * m_cameraRollSpeed * Time.unscaledDeltaTime;
        up = Quaternion.AngleAxis(rollDelta, forward) * up;
        
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
