using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TurnAccelerationDisplay : MonoBehaviour
{
    [SerializeField]
    private Sprite m_dotSprite;

    [SerializeField]
    private int m_numberOfSprites;
    
    [SerializeField]
    private float m_minRadius;
    [SerializeField]
    private float m_maxRadius;

    [SerializeField]
    private Vector2 m_startSize;
    [SerializeField]
    private Vector2 m_endSize;

    private List<SpriteRenderer> m_createdSprites;
    private Color m_uiColor;
    
    
    public void Initialise(Color uiColor)
    {
        m_uiColor = uiColor;
        m_createdSprites = new List<SpriteRenderer>();
        
        for (int i = 0; i < m_numberOfSprites; i++)
        {
            float t = (float)i / m_numberOfSprites;
            
            GameObject go = new GameObject("Sprite");
            go.transform.parent = transform;
            go.layer = gameObject.layer;
            go.transform.localPosition = Vector3.Lerp(new Vector3(0.0f, m_minRadius, 0.0f), new Vector3(0.0f, m_maxRadius, 0.0f), t);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = m_dotSprite;
            sr.color = new Color(m_uiColor.r, m_uiColor.g, m_uiColor.b, 0.0f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = Vector2.Lerp(m_startSize, m_endSize, t);
            m_createdSprites.Add(sr);
        }
    }

    public void SetAngularThrottle(Vector2 angularThrottle)
    {
        float throttleSprite = m_numberOfSprites * math.cmax(math.abs(angularThrottle));
        for (int i = 0; i < m_numberOfSprites; i++)
        {
            float a = Mathf.Clamp01(throttleSprite - i);
            m_createdSprites[i].color = new Color(m_uiColor.r, m_uiColor.g, m_uiColor.b, a);
        }

        Vector3 direction = new Vector3(
            angularThrottle.y,
            -angularThrottle.x,
            0.0f
        );
        transform.rotation = Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.up, direction), Vector3.forward);
    }
}
