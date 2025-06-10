using Unity.Entities;
using Unity.Mathematics;

[System.Serializable]
public struct ThrusterSetup
{
    public float ForwardThrustersAcceleration;
    public float LateralThrustersAcceleration;
    public float ReverseThrustersAcceleration;
}

public struct PlayerData : IComponentData
{
    public ThrusterSetup DefaultMovement;
    public ThrusterSetup ADSMovement;
    
    public float MaximumVelocity;
    public float VelocityDamperDecelerationDefault;
    public float VelocityDamperDecelerationADS;
    public float MaxTurnSpeed;
    public float MaxRollSpeed;
    
    public float BoostTime;
    public float BoostRechargeTime;
    public float BoostAcceleration;
    public float BoostMaximumVelocity;
}

public struct PlayerBoosterState : IComponentData
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

public class PlayerManagedAccess : IComponentData
{
    public ManagedLocalPlayer ManagedLocalPlayer;
}

public struct ShipHardpointBufferElement : IBufferElementData
{
    public bool Enabled;
    public Entity Self;
}
