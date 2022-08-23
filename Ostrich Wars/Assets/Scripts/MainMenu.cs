using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.UI; 

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance; 
    public CinemachineVirtualCamera Cam;
    public MenuItem SelectedMenuItem;
    NewInput inputActions;

    private void Awake()
    {
        instance = this; 
        inputActions = new NewInput();

        inputActions.NormalEvent.Select.performed += ctx =>
        {
            RaycastHit hit;
            //Returns true if using mouse
            bool MouseOrTap = Mouse.current != null;
            var InputValue = MouseOrTap ? Mouse.current.position.ReadValue() : Touchscreen.current.position.ReadValue();
            Ray publicWorldPos = GameManager.CachedCamera.ScreenPointToRay(InputValue);
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
