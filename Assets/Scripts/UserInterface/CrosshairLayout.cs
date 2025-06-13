using System;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairLayout : MonoBehaviour
{
    [SerializeField]
    private Sprite m_dotSprite;
    [SerializeField]
    private float m_lineThickness = 0.05f;
    [SerializeField]
    private float m_lineLength = 2.0f;
    [SerializeField]
    private float m_centerGap = 1.0f;

    private GameObject m_crosshairParent;
    private List<SpriteRenderer> m_createdAimDots = new List<SpriteRenderer>();
    private List<SpriteRenderer> m_createdCrosshairs = new List<SpriteRenderer>();
    
    public Transform GetDotTransform(int dotIndex)
    {
        return m_createdAimDots[dotIndex].transform;
    }
    
    public void RefreshCrosshairs(int numberOfDots, Color uiColor)
    {
        Destroy(m_crosshairParent);
        
        foreach (SpriteRenderer sr in m_createdAimDots)
        {
            Destroy(sr.gameObject);
        }
        m_createdAimDots.Clear();
        
        foreach (SpriteRenderer sr in m_createdCrosshairs)
        {
            Destroy(sr.gameObject);
        }
        m_createdCrosshairs.Clear();
        
        m_crosshairParent = new GameObject("Crosshair Parent");
        m_crosshairParent.layer = gameObject.layer;
        m_crosshairParent.transform.parent = transform;
        
        int numCrosshairs = 3;
        for (int i = 0; i < numCrosshairs; i++)
        {
            GameObject go  = new GameObject("Crosshair");
            go.layer = gameObject.layer;
            go.transform.parent = m_crosshairParent.transform;
            
            float a = 2.0f * Mathf.PI * i / numCrosshairs;
            float l = m_centerGap + 0.5f * m_lineLength;
            go.transform.localPosition = new Vector3(l * Mathf.Sin(a), l * Mathf.Cos(a), 0.0f);
            go.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -Mathf.Rad2Deg * a);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = m_dotSprite;
            sr.color = uiColor;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(m_lineThickness, m_lineLength);
            
            m_createdCrosshairs.Add(sr);
        }

        for (int i = 0; i < numberOfDots; i++)
        {
            GameObject go  = new GameObject("Dot");
            go.layer = gameObject.layer;
            go.transform.parent = transform;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = m_dotSprite;
            sr.color = uiColor;
            
            m_createdAimDots.Add(sr);
        }
    }
}
