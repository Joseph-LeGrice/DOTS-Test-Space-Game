using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UserInterface : MonoBehaviour
{
    private UIDocument m_uiDocument;

    private void Awake()
    {
        m_uiDocument = GetComponent<UIDocument>();
    }
}
