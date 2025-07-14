using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public struct BehaviourTreeRotationNodeBurstable
{
    public float m_rotationSpeedDegrees;
    
    [BurstCompile]
    [AOT.MonoPInvokeCallback(typeof(BurstableBehaviourTreeNode.DoActionDelegate))]
    public static BehaviourActionResult BurstableDoAction(ref BurstableBehaviourTree behaviourTree,
        ref BurstableBehaviourTreeNode node, ref ECSBehaviourTreeBlackboard blackboard)
    {
        // Vector3 targetDirection = blackboard.GetVector3("TargetDirection");
        // Transform transform = blackboard.GetReference<Transform>("Transform");
        //
        // Vector3 currentForward = transform.forward;
        // float forwardAngleDegrees = Vector3.Angle(currentForward, targetDirection);
        //
        // if (forwardAngleDegrees < 0.1f)
        // {
        //     return BehaviourActionResult.Success;
        // }
        //
        // transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetDirection), m_rotationSpeedDegrees * Time.deltaTime);
        
        return BehaviourActionResult.InProgress;
    }
}

public class BehaviourTreeRotationNode : BehaviourTreeNodeImplementation
{
    [SerializeField]
    private float m_rotationSpeedDegrees = 30.0f;
    
    public override string GetNodeName()
    {
        return "Rotation Node";
    }

    public override BehaviourActionResult DoActionManaged(BehaviourTree behaviourTree, ref BehaviourTreeBlackboard blackboard)
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

    public override void PopulateBurstable(ref BlobBuilder builder, ref BurstableBehaviourTreeNode node)
    {
        ref BehaviourTreeRotationNodeBurstable data =
            ref AllocateNodeData<BehaviourTreeRotationNodeBurstable>(ref builder, ref node);
        data.m_rotationSpeedDegrees = m_rotationSpeedDegrees;
        node.DoActionBurstable = BurstCompiler.CompileFunctionPointer<BurstableBehaviourTreeNode.DoActionDelegate>(BehaviourTreeRotationNodeBurstable.BurstableDoAction);
    }
}
