using UnityEngine;

public class BehaviourTreeRotationNode : BehaviourTreeNodeImplementation
{
    [SerializeField]
    private float m_rotationSpeedDegrees = 30.0f;
    
    public override string GetNodeName()
    {
        return "Rotation Node";
    }

    public override BehaviourActionResult DoAction(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard)
    {
        Vector3 targetDirection = blackboard.GetVector3("TargetDirection");
        Transform transform = blackboard.GetReference<Transform>("Transform");

        Vector3 currentForward = transform.forward;
        float forwardAngleDegrees = Vector3.Angle(currentForward, targetDirection);
        
        if (forwardAngleDegrees < 0.1f)
        {
            return BehaviourActionResult.Success;
        }
        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetDirection), m_rotationSpeedDegrees * Time.deltaTime);
            
        return BehaviourActionResult.InProgress;
    }
}
