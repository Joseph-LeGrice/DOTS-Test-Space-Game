using System;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    [SerializeField]
    private float m_displayRadius;
    [SerializeField]
    private GameObject m_markerPrefab;
    [SerializeField]
    private Transform m_markerParent;
    
    private float m_detectionRangeSquared;
    private GameObjectPool m_targetPool;

    private void Awake()
    {
        m_targetPool = new GameObjectPool(m_markerPrefab, gameObject.layer, m_markerParent, 5);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 2.0f * m_displayRadius);
    }

    public void SetMaxDetectionRange(float detectionRangeSquared)
    {
        m_detectionRangeSquared = detectionRangeSquared;
    }

    public void UpdateTargets(ManagedLocalPlayer localPlayer, List<TargetData> targets)
    {
        m_targetPool.SetActiveObjectCount(targets.Count);
        var activeBlips = m_targetPool.GetCurrentActiveObjects();
        for (int i=0; i<targets.Count; i++)
        {
            TargetData td = targets[i];
            Vector3 playerDelta = td.WorldPosition - localPlayer.GetPlayerTrackedTransform().position;
            
            float t = m_displayRadius * Mathf.Clamp01(playerDelta.sqrMagnitude / m_detectionRangeSquared);
            Vector3 localOffset = localPlayer.GetPlayerTrackedTransform().worldToLocalMatrix.MultiplyPoint3x4(playerDelta.normalized);
            
            PooledGameObject radarBlip = activeBlips[i];
            radarBlip.GetComponent<RadarBlip>().SetSelected(td.IsTargeting);
            radarBlip.transform.localPosition = t * localOffset;
        }
    }

    private void OnDestroy()
    {
        m_targetPool.Dispose();
    }
}
