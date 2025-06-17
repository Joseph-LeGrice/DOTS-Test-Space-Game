using UnityEngine;

public class TargetInstance : MonoBehaviour
{
    [SerializeField]
    private GameObject m_selectedHierarchy;
    [SerializeField]
    private GameObject m_canTargetAhead;
    [SerializeField]
    private GameObject m_deselectedHierarchy;

    private bool m_isSelected;
    
    public void SetSelected(bool isSelected)
    {
        m_isSelected = isSelected;
        m_selectedHierarchy.SetActive(isSelected);
        m_deselectedHierarchy.SetActive(!isSelected);
    }

    public void SetCanTargetAhead(bool canTargetAhead)
    {
        m_canTargetAhead.SetActive(canTargetAhead && !m_isSelected);
    }
}
