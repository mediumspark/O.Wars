using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.UI; 

public class MainMenu : MonoBehaviour
{
    public CinemachineFreeLook Cam;
    public MenuItem SelectedMenuItem;
    NewInput inputActions;

    private void Awake()
    {
        inputActions = new NewInput();

        inputActions.NormalEvent.Select.performed += ctx =>
        {
            RaycastHit hit;
            Ray publicWorldPos = GameManager.CachedCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(publicWorldPos, out hit))
            {
                Transform hitboi = hit.transform;
                if (hitboi.TryGetComponent(out MenuItem intem))
                {
                    intem.OnPress();
                }
            }
            else
            {
                UnfocusCam();
            }
        };

        inputActions.NormalEvent.Deselect.performed += ctx => UnfocusCam(); 
    }

    public void UnfocusCam()
    {
        try
        {
            SelectedMenuItem.FocusPoint.Priority = 2;
            SelectedMenuItem.SubMenu.gameObject.SetActive(false);
            SelectedMenuItem = null;
        }
        catch
        {
            Debug.LogWarning("Selected Menu unassaigned");
        }
    }

    private void OnEnable()
    {
        inputActions.Enable(); 
    }

    private void OnDisable()
    {
        inputActions.Disable(); 
    }
}
