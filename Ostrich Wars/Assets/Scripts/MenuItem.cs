using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine; 


public class MenuItem : MonoBehaviour, IGameplayInteractable
{
    public Canvas SubMenu; 

    [HideInInspector]
    public CinemachineFreeLook FocusPoint;

    [HideInInspector]
    public MainMenu Menu; 

    private void Awake()
    {
        FocusPoint = GetComponentInChildren<CinemachineFreeLook>();
        Menu = FindObjectOfType<MainMenu>();
        FocusPoint.m_Transitions.m_OnCameraLive.AddListener(SubmenuActivate); 
    }


    public void OnPress() {
        FocusPoint.Priority = 10;
        Menu.SelectedMenuItem = this; 
    }

    public void SubmenuActivate(ICinemachineCamera a, ICinemachineCamera b)
    {
        SubMenu.gameObject.SetActive(true); 
    }
}
