using System;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    [Serializable]
    public class DisplayRadiusSetup
    {
        public float m_range;
        public float m_radiusBreakpoint;
    }
    
    [SerializeField]
    private float m_displayRadius;
    [SerializeField]
    private List<DisplayRadiusSetup> m_radiusSetup = new List<DisplayRadiusSetup>();
    [SerializeField]
    private GameObject m_markerPrefab;
    [SerializeField]
    private Transform m_markerParent;
    
    private float m_detectionRange;
    private GameObjectPool m_targetPool;

    private void Awake()
    {
        m_targetPool = new GameObjectPool(m_markerPrefab, gameObject.layer, m_markerParent, 5);
        m_radiusSetup.Sort((a, b) => a.m_radiusBreakpoint.CompareTo(b.m_radiusBreakpoint));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 2.0f * m_displayRadius);
    }

    public void SetMaxDetectionRange(float detectionRange)
    {
        m_detectionRange = detectionRange;
    }

    public void UpdateTargets(ManagedLocalPlayer localPlayer, List<TargetData> targets)
    {
        m_targetPool.SetActiveObjectCount(targets.Count);
        var activeBlips = m_targetPool.GetCurrentActiveObjects();
        for (int i=0; i<targets.Count; i++)
        {
            TargetData td = targets[i];
            Vector3 playerDelta = td.WorldPosition - localPlayer.GetPlayerTrackedTransform().position;
            
            float t = 0.0f;
            float targetDistance = playerDelta.magnitude;
            
            for (int ii=0; ii<m_radiusSetup.Count; ii++)
            {
                float radiusBreakpointMin = m_radiusSetup[ii].m_radiusBreakpoint;
                float rangeMin = m_radiusSetup[ii].m_range;
                
                float radiusBreakpointMax = 1.0f;
                float rangeMax = m_detectionRange;
                
                if (ii < m_radiusSetup.Count - 1)
                {
                    radiusBreakpointMax = m_radiusSetup[ii + 1].m_radiusBreakpoint;
                    rangeMax = m_radiusSetup[ii + 1].m_range;
                }
                
                t += (radiusBreakpointMax - radiusBreakpointMin) * Mathf.InverseLerp(rangeMin, rangeMax, targetDistance);
            }
            
            Vector3 localOffset = localPlayer.GetPlayerTrackedTransform().worldToLocalMatrix.MultiplyVector(playerDelta.normalized);
            
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
