using UnityEngine;

public class BehaviourTreeRunner : MonoBehaviour
{
    [SerializeField]
    private BehaviourTree m_behaviourTree;
    
    private void Update()
    {
        Debug.Log("Frame Start");
        m_behaviourTree.Execute();
    }
}
