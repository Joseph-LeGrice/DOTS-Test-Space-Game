using System;
using UnityEngine;

public class BehaviourTreeRunner : MonoBehaviour
{
    [SerializeField]
    private BehaviourTree m_behaviourTree;
    [SerializeField]
    private Vector3 m_targetDirection;

    private BehaviourTreeBlackboard m_blackboard;

    private void Start()
    {
        m_blackboard.SetReference("Transform", transform);
    }

    private void Update()
    {
        m_blackboard.SetVector3("TargetDirection", m_targetDirection);
        
        m_behaviourTree.Execute(ref m_blackboard);
    }
}
