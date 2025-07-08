using System.Collections.Generic;
using UnityEngine;

public struct BehaviourTreeBlackboard
{
    private bool m_initialized;
    private Dictionary<string, object> m_references;
    private Dictionary<string, Vector3> m_vector3Values;

    private void CheckInitialised()
    {
        if (!m_initialized)
        {
            m_initialized = true;
            m_references = new Dictionary<string, object>();
            m_vector3Values = new Dictionary<string, Vector3>();
        }
    }
    
    public void SetReference<T>(string identifier, T value)
    {
        CheckInitialised();
        m_references[identifier] = value;
    }
    
    public T GetReference<T>(string identifier)
    {
        CheckInitialised();
        return (T)m_references[identifier];
    }

    public void SetVector3(string identifier, Vector3 value)
    {
        CheckInitialised();
        m_vector3Values[identifier] = value;
    }
    
    public Vector3 GetVector3(string identifier)
    {
        CheckInitialised();
        return m_vector3Values[identifier];
    }
}
