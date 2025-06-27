using UnityEngine;

public class RadarBlip : MonoBehaviour
{
    [SerializeField]
    private GameObject m_selectedHierarchy;
    [SerializeField]
    private LineRenderer m_lineRenderer;
    [SerializeField]
    private Transform m_faceCameraPlaneHierachy;
    [SerializeField]
    private Transform m_lineBaseMarkerHierachy;

    public void SetSelected(bool isSelected)
    {
        m_selectedHierarchy.SetActive(isSelected);
    }
    
    private void Update()
    {
        Vector3 localOffsetY = new Vector3(0.0f, -transform.localPosition.y, 0.0f);
        m_lineRenderer.SetPosition(1, localOffsetY);
        m_lineBaseMarkerHierachy.localPosition = localOffsetY;
        
        // m_faceCameraPlaneHierachy.rotation
    }
}
