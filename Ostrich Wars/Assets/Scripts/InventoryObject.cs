using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InventoryObject : ScriptableObject
{
    [SerializeField]
    private string _invCode;
    public string InvCode => _invCode;
    public Sprite Portrait; 

    #if UNITY_EDITOR
    public void CreateInvCode()
    {
        EditorUtility.SetDirty(this);
        if (string.IsNullOrEmpty(_invCode))
        {
            _invCode = SetInvCode(8); 
        }
    }

    private string SetInvCode(int length)
    {
        string InvCode = GenerateInvCode(8);
        if (!GameManager.InventoryCodes.Contains(InvCode))
        {
            GameManager.InventoryCodes.Add(InvCode);
            return InvCode;
        }
        else
        {
            SetInvCode(length);
            return "";
        }
    }

    private string GenerateInvCode(int length)
    {
        System.Random random = new System.Random(); 
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    #endif

    private void OnDestroy()
    {
        GameManager.InventoryCodes.Remove(_invCode); 
    }

}


