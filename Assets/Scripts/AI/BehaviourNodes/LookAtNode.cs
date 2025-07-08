using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LookAtNode : BehaviourTreeNodeImplementation
{
    public override string GetNodeName()
    {
        return "Look at";
    }

    public override BehaviourActionResult DoAction(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard)
    {
        Vector3 targetDirection = blackboard.GetVector3("TargetDirection");
        ShipAspect shipAspect = blackboard.GetReference<ShipAspect>("ShipAspect");
        
        float3 currentForward = shipAspect.LocalToWorld.ValueRO.Forward;
        float forwardAngleDegrees = math.degrees(MathHelpers.GetAngle(currentForward, targetDirection));
        
        if (forwardAngleDegrees < 2.5f)
        {
            return BehaviourActionResult.Success;
        }
        
        float3 forwardAxisLocal = shipAspect.LocalToWorld.ValueRO.Value.InverseTransformDirection(
            math.cross(currentForward, targetDirection)
        );

        float throttleMagnitude = math.clamp(math.unlerp(2.0f, 30.0f, math.abs(forwardAngleDegrees)), 0, 1);
        
        float2 throttle = new float2(forwardAxisLocal.y, forwardAxisLocal.x);
        throttle = throttleMagnitude * math.normalize(throttle);
        
        shipAspect.ShipInput.ValueRW.AngularThrottle = throttle;
            
        return BehaviourActionResult.InProgress;
    }
}
