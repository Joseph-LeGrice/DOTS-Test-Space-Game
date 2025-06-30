using System;
using UnityEngine;

public class BehaviourTreeRunner : MonoBehaviour
{
    [SerializeField]
    private BehaviourTree m_behaviourTree;

    private int m_currentActionIndex;
    
    private void Update()
    {
        m_currentActionIndex = m_behaviourTree.Execute(m_currentActionIndex);
    }
}
