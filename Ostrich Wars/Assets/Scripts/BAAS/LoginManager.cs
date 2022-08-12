using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System;

public class LoginManager : MonoBehaviour
{
    public bool SaveCredintials { get; set; } 
    public TextMeshProUGUI MessageField; 
    public TMP_InputField emailIn;
    public TMP_InputField passwordIn;

    private LocalProfile Player;
    private string SavedUSRN = "", SavedPS = "";
    [SerializeField]
    private Transform OutOfBattleScene, LoginScene;

    public delegate void LoginDelegate();
    public static LoginDelegate OnLogin; 

    public void RegisterButton()
    {
        if(passwordIn.text.Length < 6)
        {
            MessageField.text = "Password is too short";
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
        if (SaveCredintials)
        {
            SavedUSRN = emailIn.text;
            SavedPS = passwordIn.text;
        }
        Debug.Log("Registered and Logged in");
    }

    // Start is called before the first frame update
    void Start()
    {
        Player = GetComponent<LocalProfile>(); 

        if(SavedUSRN != "" && SavedPS != "")
            Login();
    }

    void Login()
    {
        var request = new LoginWithEmailAddressRequest
        {            
            Email = SaveCredintials ? SavedUSRN: emailIn.text,
            Password = SaveCredintials ? SavedPS : passwordIn.text
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);

    }

    void OnLoginSuccess(LoginResult result)
    {
        if (SaveCredintials)
        {
            SavedUSRN = emailIn.text;
            SavedPS = passwordIn.text;
        }

        Player.SetProfile();

        LoginScene.gameObject.SetActive(false);
        OutOfBattleScene.gameObject.SetActive(true);

        OnLogin.Invoke();

        Debug.Log("Successfully login");
    }

    void OnError(PlayFabError ErrorResult)
    {
        MessageField.text = ErrorResult.ErrorMessage; 
        Debug.LogWarning(ErrorResult.ErrorMessage);
        Debug.LogWarning(ErrorResult.GenerateErrorReport()); 
    }

    public void OnFacebookButtonPressed()
    {
        PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest(), OnFBLoginSuccess, OnError);
    }

    private void OnFBLoginSuccess(LoginResult obj)
    {
        Debug.Log("Login");
    }

    public void OnGoogleButtonPressed()
    {
        PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest(), OnGoogleLogin, OnError); 
    }

    private void OnGoogleLogin(LoginResult obj)
    {
        Debug.Log("Login");
    }
}
