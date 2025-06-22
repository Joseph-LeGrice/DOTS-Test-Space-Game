using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class LocalPlayerInputSystem : SystemBase
{
    private InputAction m_Player_Movement;
    private InputAction m_Player_Roll;
    private InputAction m_Player_Look;
    private InputAction m_Player_Attack;
    private InputAction m_Player_ADS;
    private InputAction m_Player_Boost;
    private InputAction m_Player_ToggleVelocityDampers;
    private InputAction m_Player_SelectAhead;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        
        InputActionMap playerActionMap = ManagedSceneAccess.Instance.GetInputActionAsset().FindActionMap("Player", throwIfNotFound: true);
        playerActionMap.Enable();
        
        m_Player_Movement = playerActionMap.FindAction("Movement", throwIfNotFound: true);
        m_Player_Roll = playerActionMap.FindAction("Roll", throwIfNotFound: true);
        m_Player_Look = playerActionMap.FindAction("Look", throwIfNotFound: true);
        m_Player_Attack = playerActionMap.FindAction("Attack", throwIfNotFound: true);
        m_Player_ADS = playerActionMap.FindAction("ADS", throwIfNotFound: true);
        m_Player_Boost = playerActionMap.FindAction("Boost", throwIfNotFound: true);
        m_Player_ToggleVelocityDampers = playerActionMap.FindAction("ToggleVelocityDampers", throwIfNotFound: true);
        m_Player_SelectAhead = playerActionMap.FindAction("SelectAhead", throwIfNotFound: true);
    }

    protected override void OnUpdate()
    {
        ComponentLookup<ShipInput> shipInputLookup = SystemAPI.GetComponentLookup<ShipInput>();
        foreach (var (localPlayer, self) in SystemAPI.Query<RefRO<PlayerTag>>().WithEntityAccess())
        {
            RefRW<ShipInput> shipInput = shipInputLookup.GetRefRW(localPlayer.ValueRO.ControllingShip);
            shipInput.ValueRW.TargetDirection = m_Player_Movement.ReadValue<Vector3>();
            
            Vector2 look = m_Player_Look.ReadValue<Vector2>();
            shipInput.ValueRW.LookDelta = new float2(look.x, -look.y);
            
            shipInput.ValueRW.IsAttacking = m_Player_Attack.inProgress;
            if (m_Player_ADS.triggered)
            {
                ManagedLocalPlayer managedLocalPlayer = SystemAPI.ManagedAPI.GetComponent<ManagedLocalPlayer>(self);
                shipInput.ValueRW.IsADS = !shipInput.ValueRW.IsADS;
                managedLocalPlayer.SetADS(shipInput.ValueRW.IsADS);
            }
            shipInput.ValueRW.IsBoosting = m_Player_Boost.triggered;
            shipInput.ValueRW.TargetSelectAhead = m_Player_SelectAhead.triggered;
            if (m_Player_ToggleVelocityDampers.triggered)
            {
                shipInput.ValueRW.LinearDampersActive = !shipInput.ValueRW.LinearDampersActive;
                // shipInput.ValueRW.RollDampersActive = !shipInput.ValueRW.RollDampersActive;
            }
            shipInput.ValueRW.RollDirection = -m_Player_Roll.ReadValue<float>();
        }
    }

    protected override void OnStopRunning()
    {
        base.OnStopRunning();
        
        InputActionMap playerActionMap = ManagedSceneAccess.Instance.GetInputActionAsset().FindActionMap("Player", throwIfNotFound: true);
        playerActionMap.Disable();
    }
}
