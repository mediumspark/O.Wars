using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro; 

public class LoginManager : MonoBehaviour
{

    public TextMeshProUGUI MessageField; 
    public TMP_InputField emailIn;
    public TMP_InputField passwordIn; 

    public void RegisterButton()
    {
        if(passwordIn.text.Length < 6)
        {
            //Password too short text 
            return; 
        }

        var request = new RegisterPlayFabUserRequest
        {
            Email = emailIn.text,
            Password = passwordIn.text,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    public void LoginButton()
    {
        Login(); 
    }

    public void PasswordResetButton()
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = emailIn.text,
            TitleId = "CB67B"
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError); 
    }

    private void OnPasswordReset(SendAccountRecoveryEmailResult obj)
    {
        MessageField.text = "Password Reset Sent";
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Registered and Logged in");
    }

    // Start is called before the first frame update
    void Start()
    {
        Login();
    }

    void Login()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = emailIn.text,
            Password = passwordIn.text
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnSuccess, OnError);
    }

    void OnSuccess(LoginResult result)
    {
        Debug.Log("Successfully login");
    }

    void OnError(PlayFabError ErrorResult)
    {
        MessageField.text = ErrorResult.ErrorMessage; 
        Debug.LogWarning(ErrorResult.ErrorMessage);
        Debug.LogWarning(ErrorResult.GenerateErrorReport()); 
    }
}
