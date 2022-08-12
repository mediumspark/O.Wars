using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.UI;
using System.Linq; 

public class ShopItem : MonoBehaviour
{
    public void Buy(int cost, string CurrencyTag)
    {
        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = CurrencyTag,
            Amount = cost
        };

        PlayFabClientAPI.SubtractUserVirtualCurrency(request, OnPurchaseSuccess, OnError);
    }

    void OnPurchaseSuccess(ModifyUserVirtualCurrencyResult result)
    {
        ProfileToUI.PC = result.Balance;
        Debug.Log("Purchase Successful");
    }

    void OnError(PlayFabError error)
    {
        Debug.Log(error.ErrorMessage);
    }
}

public class StoreUI : MonoBehaviour
{
    public GameObject StoreObjectPrefab; 
    public List<GridLayoutGroup> ContentMenus = new List<GridLayoutGroup>();
    public Sprite currencySprite, energySprite, ticketSprite;     

    public void PreloadStorePages()
    {
        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), OnAccepted, OnError);
    }

    private void OnError(PlayFabError obj)
    {
        Debug.LogWarning(obj.ErrorMessage);
    }

    private void OnAccepted(GetCatalogItemsResult obj)
    {
        foreach(CatalogItem item in obj.Catalog)
        {
            try
            {
                if (item.Tags.Contains("Currency"))
                {
                    CreateEntry(item, ContentMenus[3].transform, currencySprite);
                }
                if (item.Tags.Contains("Energy"))
                {
                    CreateEntry(item, ContentMenus[2].transform, energySprite);                  
                }
                if (item.Tags.Contains("Ticket"))
                {
                    CreateEntry(item, ContentMenus[0].transform, ticketSprite);
                }
            }
            catch
            {
                //No Tags
            }
        }
    }

    private GameObject CreateEntry(CatalogItem item, Transform Parent, Sprite DisplayImage)
    {
        GameObject First = Instantiate(StoreObjectPrefab, Parent);
        First.name = item.DisplayName;
        ShopItem SI = First.AddComponent<ShopItem>();

        GameObject Image = First.transform.GetChild(0).gameObject;
        Image.GetComponent<Image>().sprite = DisplayImage;
        Image.transform.SetParent(First.transform);
        Image.transform.localScale = Vector3.one;
        

        GameObject TextDescr = First.transform.GetChild(1).gameObject;
        TextMeshProUGUI descrname = TextDescr.GetComponent<TextMeshProUGUI>();
        descrname.text = item.DisplayName;
        descrname.enableAutoSizing = true;
        descrname.enableWordWrapping = false; 
        TextDescr.transform.SetParent(First.transform);
        TextDescr.transform.localScale = Vector3.one;


        GameObject TextCost =First.transform.GetChild(2).gameObject;
        TextMeshProUGUI descrcost = TextCost.GetComponent<TextMeshProUGUI>();
        descrcost.text = $"{item.VirtualCurrencyPrices[item.VirtualCurrencyPrices.First().Key]}";
        descrcost.enableAutoSizing = true;
        descrcost.alignment = TextAlignmentOptions.Center;
        descrcost.fontStyle = FontStyles.Bold;
        descrcost.alignment = TextAlignmentOptions.Top; 
        descrcost.enableWordWrapping = false; 
        TextCost.transform.SetParent(First.transform);
        TextCost.transform.localScale = Vector3.one;

        string currency = item.VirtualCurrencyPrices.First().Key;
        int cost = Convert.ToInt32(item.VirtualCurrencyPrices[item.VirtualCurrencyPrices.First().Key]); 


        GameObject PurchaseButton = First.transform.GetChild(3).gameObject;
        PurchaseButton.GetComponent<Button>().onClick.AddListener(() => 
        {
            SI.Buy(cost, currency);
        });

        TextMeshProUGUI BuyText = PurchaseButton.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        BuyText.transform.localScale = Vector3.one;
        BuyText.text = "Buy";
        BuyText.enableAutoSizing = true;
        BuyText.transform.SetParent(PurchaseButton.transform);
        BuyText.transform.localScale = Vector3.one;
        BuyText.color = Color.black;
        BuyText.alignment = TextAlignmentOptions.Center;

        return First;        
    }

    private void HideStores()
    {
        foreach(GridLayoutGroup layout in ContentMenus)
        {
            layout.gameObject.SetActive(false);     
        }
    }

    private void Awake()
    {
        HideStores();
        LoginManager.OnLogin += PreloadStorePages; 
    }

    public void OnShowStonesStore()
    {
        HideStores();
        ContentMenus[0].gameObject.SetActive(true);
    }

    public void OnShowEquipmentStore()
    {
        HideStores();
        ContentMenus[1].gameObject.SetActive(true);
    }

    public void OnShowPacksStore()
    {
        HideStores();
        ContentMenus[2].gameObject.SetActive(true);
    }

    public void OnShowCurrencyStore()
    {
        HideStores();
        ContentMenus[3].gameObject.SetActive(true);
    }

    public void OnShowPlayerMarket()
    {
        HideStores();
        ContentMenus[4].gameObject.SetActive(true);
    }
}
