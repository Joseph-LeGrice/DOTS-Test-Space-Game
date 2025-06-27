using System;
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

    private GameObjectPool m_targetPool;

    private void Awake()
    {
        m_targetPool = new GameObjectPool(m_prefab, gameObject.layer, transform, 5);
    }

    public void UpdateTargets(List<TargetData> targets)
    {
        m_targetPool.SetActiveObjectCount(targets.Count);
        var activeTargets = m_targetPool.GetCurrentActiveObjects();
        for (int i = 0; i < targets.Count; i++)
        {
            TargetInstance ti = activeTargets[i].GetComponent<TargetInstance>();
            ti.SetSelected(targets[i].IsTargeting);
            ti.SetCanTargetAhead(targets[i].CanTargetAhead);
            ti.transform.position = targets[i].Position;
        }
    }
}
