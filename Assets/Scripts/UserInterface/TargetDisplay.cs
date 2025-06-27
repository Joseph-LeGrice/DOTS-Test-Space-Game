using System;
using System.Collections.Generic;
using UnityEngine;

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
        int visibleTargets = 0;
        foreach (TargetData td in targets)
        {
            if (td.IsVisibleOnScreen)
            {
                visibleTargets++;
            }
        }
        m_targetPool.SetActiveObjectCount(visibleTargets);
        
        var activeTargets = m_targetPool.GetCurrentActiveObjects();
        for (int i = 0; i < targets.Count; i++)
        {
            TargetData td = targets[i];
            if (td.IsVisibleOnScreen)
            {
                TargetInstance ti = activeTargets[i].GetComponent<TargetInstance>();
                ti.SetSelected(td.IsTargeting);
                ti.SetCanTargetAhead(td.CanTargetAhead);
                ti.transform.position = td.UIPosition;
            }
        }
    }

    private void OnDestroy()
    {
        m_targetPool.Dispose();
    }
}
