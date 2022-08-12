using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class ProfileToUI : MonoBehaviour
{
    LocalProfile profile;

    [Header("Main Menu")]
    [SerializeField]
    TextMeshProUGUI Currency_1;
    [SerializeField]
    TextMeshProUGUI Currency_2, Currency_3, Currency_4;

    [Header("StoreMenu")]
    [SerializeField]
    TextMeshProUGUI FreeCurrencyStoreText;
    [SerializeField]
    TextMeshProUGUI PremiumCurrencyStoreText;

    public static int PC, MC, SC, FC; 

    private void Awake()
    {
        LoginManager.OnLogin += SetCurrencyUI;
    }

    private void Update()
    {
        Currency_1.text = $"{PC}";
        Currency_2.text = $"{MC} / 30";
        Currency_3.text = $"{SC} / 20";
        Currency_4.text = $"{FC}";

        FreeCurrencyStoreText.text = Currency_4.text;
        PremiumCurrencyStoreText.text = Currency_1.text;
    }

    private void SetCurrencyUI()
    {
        profile = GetComponent<LocalProfile>();
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnRequestAccepted, OnError);
    }

    private void OnRequestAccepted(GetUserInventoryResult obj)
    {
        PC = obj.VirtualCurrency["PC"];
        MC = obj.VirtualCurrency["MC"];
        SC = obj.VirtualCurrency["SC"];
        FC = obj.VirtualCurrency["FC"];

    }

    private void OnError(PlayFabError obj)
    {
        Debug.LogWarning(obj.ErrorMessage); 
    }
}
