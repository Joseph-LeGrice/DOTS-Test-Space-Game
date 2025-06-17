using System.Collections.Generic;
using UnityEngine;

public class TargetData
{
    public bool IsTargeting;
    public bool CanTargetAhead;
    public Vector3 Position;
}

public class TargetDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject m_prefab;

    private List<TargetInstance> m_createdTargets = new List<TargetInstance>();
    
    public void UpdateTargets(List<TargetData> targets)
    {
        int diff = targets.Count - m_createdTargets.Count;

        int toAdd = Mathf.Max(diff, 0);
        for (int i = 0; i < toAdd; i++)
        {
            GameObject instance = Instantiate(m_prefab, transform);
            instance.layer = gameObject.layer;
            
            m_createdTargets.Add(instance.GetComponent<TargetInstance>());
        }
        
        int toRemove = Mathf.Abs(Mathf.Min(diff, 0));
        for (int i = 0; i < toRemove; i++)
        {
            Destroy(m_createdTargets[m_createdTargets.Count - 1].gameObject);
            m_createdTargets.RemoveAt(m_createdTargets.Count - 1);
        }

        for (int i = 0; i < targets.Count; i++)
        {
            m_createdTargets[i].SetSelected(targets[i].IsTargeting);
            m_createdTargets[i].SetCanTargetAhead(targets[i].CanTargetAhead);
            m_createdTargets[i].transform.position = targets[i].Position;
        }
    }
}
