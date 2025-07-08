using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;


public readonly partial struct ShipAspect : IAspect
{
    public readonly Entity Self;
    public readonly RefRW<ShipInput> ShipInput;
    public readonly RefRW<ShipMovementData> ShipMovementData;
    public readonly RefRW<ShipBoosterState> PlayerBoostState;
    public readonly RefRW<PhysicsVelocity> Velocity;
    public readonly RefRW<PhysicsMass> PhysicsMass;
    public readonly RefRO<Damageable> Damageable;
    public readonly RefRO<LocalToWorld> LocalToWorld;
    public readonly DynamicBuffer<ShipHardpointReference> ShipHardpoints;
    public readonly DynamicBuffer<DetectedTarget> DetectedTargets;
    public readonly RefRO<TargetDetector> TargetDetector;
}

public struct ShipInput : IComponentData
{
    public float Throttle;
    public float2 StrafeThrusters;
    public float2 AngularThrottle;
    public bool IsAttacking;
    public bool IsADS;
    public bool IsBoosting;
    public bool LinearDampersActive;
    public bool AngularDampersActive;
    public float RollDirection;
    public bool TargetSelectAhead;

    public static ShipInput Default => new ShipInput()
    {
        LinearDampersActive = true,
        AngularDampersActive = true,
    };
}

[System.Serializable]
public struct ThrusterSetup
{
    public float ForwardThrustersAcceleration;
    public float LateralThrustersAcceleration;
    public float ReverseThrustersAcceleration;
    
    public float MaxTurnAcceleration;
    public float MaxTurnSpeed;
    
    public float MaxRollAcceleration;
    public float MaxRollSpeed;
    
    public float AngularDamperDeceleration;
    public float VelocityDamperDeceleration;
}

public struct ShipMovementData : IComponentData
{
    public ThrusterSetup DefaultMovement;
    public ThrusterSetup ADSMovement;
    
    public float MaximumVelocity;
    
    public float BoostTime;
    public float BoostRechargeTime;
    public float BoostAcceleration;
    public float BoostMaximumVelocity;
}

public struct ShipBoosterState : IComponentData
{
    private float m_currentBoostTime;
    private bool m_boostRecharging;
    
    public bool IsBoosting()
    {
        return !m_boostRecharging && m_currentBoostTime > 0.0f;
    }

    public float GetBoostAcceleration(float maxAcceleration, float boostTime)
    {
        float boostAmount = math.unlerp(0.0f, boostTime, m_currentBoostTime);
        return maxAcceleration * boostAmount;
    }
    
    public bool TryBoostPerformed(float boostTime)
    {
        if (!m_boostRecharging && m_currentBoostTime <= 0.0f)
        {
            m_currentBoostTime = boostTime;
            return true;
        }
        return false;
    }

    public void UpdateBoost(float dt, float boostRechargeTime)
    {
        if (m_boostRecharging)
        {
            m_currentBoostTime += dt;
            if (m_currentBoostTime >= boostRechargeTime)
            {
                m_boostRecharging = false;
                m_currentBoostTime = 0.0f;
            }
        }
        else if (m_currentBoostTime > 0.0f)
        {
            m_currentBoostTime -= dt;
            if (m_currentBoostTime <= 0.0f)
            {
                m_currentBoostTime = 0.0f;
                m_boostRecharging = true;
            }
        }
    }
}

public struct ShipHardpointReference : IBufferElementData
{
    public Entity Self;
}

public struct ShipHardpointInstance : IComponentData
{
    public Entity RelatedRigidbodyEntity;
    public Entity WeaponInstanceEntity;
    public bool IsFiring;
    public float AimDistance; 
    public float3 TargetLocalForward;
}
