using UnityEngine;

[CreateAssetMenu(fileName = "LoginSettings", menuName ="Login")]
public class LoginSettings: ScriptableObject
{
    public string USRN, PSWD, EMAIL;
    public bool AUTOLOGIN; 
}
