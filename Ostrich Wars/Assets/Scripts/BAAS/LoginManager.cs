using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class LoginManager : MonoBehaviour
{
    [SerializeField]
    LoginSettings LoginSettings;

    public bool SaveCredintials
    {
        get => LoginSettings.AUTOLOGIN;
        set => LoginSettings.AUTOLOGIN = value;
    }
    
    public TextMeshProUGUI MessageField; 
    public TMP_InputField emailIn;
    public TMP_InputField passwordIn;

    private LocalProfile Player;
    private string SavedUSRN
    {
        get => LoginSettings.EMAIL;
        set => LoginSettings.EMAIL = value; 
    }
   
    private string SavedPS
    {
        get => LoginSettings.PSWD;
        set => LoginSettings.PSWD = value; 
    }

    [SerializeField]
    private Transform OutOfBattleScene, LoginScene;

    public delegate void LoginDelegate();
    public static LoginDelegate OnLogin;

    public static string SessionTicket;
    public static string EntityID;

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
        SessionTicket = result.SessionTicket;
        EntityID = result.EntityToken.Entity.Id;
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
        var Loginrequest = new LoginWithEmailAddressRequest
        {            
            Email = SaveCredintials && SavedUSRN != ""? SavedUSRN: emailIn.text,
            Password = SaveCredintials && SavedPS != ""? SavedPS : passwordIn.text,
        };       
        PlayFabClientAPI.LoginWithEmailAddress(Loginrequest, OnLoginSuccess, OnError);

    }


    void OnLoginSuccess(LoginResult result)
    {
        if (SaveCredintials)
        {
            SavedUSRN = emailIn.text != ""? emailIn.text : SavedUSRN;
            SavedPS = passwordIn.text != ""? passwordIn.text : SavedPS;
        }

        SessionTicket = result.SessionTicket;
        EntityID = result.EntityToken.Entity.Id;

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
