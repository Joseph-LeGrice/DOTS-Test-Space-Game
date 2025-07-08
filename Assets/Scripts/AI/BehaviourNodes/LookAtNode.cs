using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LookAtNode : BehaviourTreeNodeImplementation
{
    private Vector3 m_targetDirection; // TODO: Get via blackboard?
    private ShipAspect m_shipAspect; // TODO: Get via something else?
    
    public override string GetNodeName()
    {
        return "Look at";
    }

    public override BehaviourActionResult DoAction(BehaviourTree behaviourTree)
    {
        float3 currentForward = m_shipAspect.LocalToWorld.ValueRO.Forward;
        float forwardAngleDegrees = math.degrees(MathHelpers.GetAngle(currentForward, m_targetDirection));
        
        if (forwardAngleDegrees < 2.5f)
        {
            return BehaviourActionResult.Success;
        }
        
        float3 forwardAxisLocal = m_shipAspect.LocalToWorld.ValueRO.Value.TransformDirection(
            math.cross(currentForward, m_targetDirection)
        );

        float throttleMagnitude = math.clamp(math.unlerp(2.0f, 30.0f, math.abs(forwardAngleDegrees)), 0, 1);
        
        float2 throttle = new float2(forwardAxisLocal.y, forwardAxisLocal.x);
        throttle = throttleMagnitude * math.normalize(throttle);
        
        m_shipAspect.ShipInput.ValueRW.AngularThrottle = throttle;
            
        return BehaviourActionResult.InProgress;
    }
}
