using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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

    public override void PopulateBurstable(ref BlobBuilder builder, ECSTypeRegister ecsTypeRegister, ref BurstableBehaviourTreeNode node)
    {
        ref BehaviourTreeRotationNodeBurstable data =
            ref AllocateNodeData<BehaviourTreeRotationNodeBurstable>(ref builder, ref node);
        data.m_rotationSpeedDegrees = m_rotationSpeedDegrees;
        node.DoActionBurstable = BurstCompiler.CompileFunctionPointer<BurstableBehaviourTreeNode.DoActionDelegate>(BehaviourTreeRotationNodeBurstable.BurstableDoAction);
        ecsTypeRegister.RegisterAccess<LocalToWorld>(ComponentType.AccessMode.ReadOnly);
        ecsTypeRegister.RegisterAccess<LocalTransform>(ComponentType.AccessMode.ReadWrite);
    }
}

[BurstCompile]
public struct BehaviourTreeRotationNodeBurstable
{
    public float m_rotationSpeedDegrees;
    
    [BurstCompile]
    [AOT.MonoPInvokeCallback(typeof(BurstableBehaviourTreeNode.DoActionDelegate))]
    public static BehaviourActionResult BurstableDoAction(ref BurstableBehaviourTree behaviourTree,
        ref BurstableBehaviourTreeNode node, ref ECSDataAccessor ecsDataAccessor, ref DynamicBuffer<ECSBehaviourTreeBlackboardValue> blackboardValueBuffer)
    {
        NativeArray<LocalToWorld> m_localToWorldLookup = ecsDataAccessor.GetComponentData<LocalToWorld>();
        NativeArray<LocalTransform> m_localTransformLookup = ecsDataAccessor.GetComponentData<LocalTransform>();
        
        float3 targetDirection = math.normalize(blackboardValueBuffer.GetFloat3("TargetDirection"));
        Entity e = blackboardValueBuffer.GetEntity("TargetEntity");
        LocalToWorld l2w = m_localToWorldLookup[e.Index]; // TODO: Decide on whether or not we can do this 
        
        float3 currentForward = l2w.Value.Forward();
        float forwardAngleDegrees = math.degrees(math.acos(math.dot(currentForward, targetDirection)));
        
        if (forwardAngleDegrees < 0.1f)
        {
            return BehaviourActionResult.Success;
        }
        
        LocalTransform lt = m_localTransformLookup[e.Index];
        
        ref BehaviourTreeRotationNodeBurstable data = ref node.GetNodeDataReference<BehaviourTreeRotationNodeBurstable>();
        quaternion rotation = MathHelpers.RotateTowards(currentForward, targetDirection, data.m_rotationSpeedDegrees * ecsDataAccessor.DeltaTime);
        lt.Rotate(rotation.value);

        m_localTransformLookup[e.Index] = lt;
        
        return BehaviourActionResult.InProgress;
    }
}