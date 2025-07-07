using UnityEngine;

public class BehaviourTreeRunner : MonoBehaviour
{
    [SerializeField]
    private BehaviourTree m_behaviourTree;
    
    private void Update()
    {
        m_behaviourTree.Execute();
    }
}
