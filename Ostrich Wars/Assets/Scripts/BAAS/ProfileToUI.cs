using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

public partial class ProfileToUI : MonoBehaviour
{
    #region Vars
    public static ProfileToUI instance; 

    private LocalProfile profile;

    [Header("Main Menu")]
    [SerializeField]
    private TextMeshProUGUI Currency_1;
    [SerializeField]
    private TextMeshProUGUI Currency_2, Currency_3, Currency_4;

    [Header("StoreMenu")]
    [SerializeField]
    private TextMeshProUGUI FreeCurrencyStoreText;
    [SerializeField]
    private TextMeshProUGUI PremiumCurrencyStoreText;

    [Header("TeamMenu")]
    [SerializeField]
    private Transform TeamMenuTransform;
    [SerializeReference]
    private Camera MenuCamera; 


    [Header("TeamMenu")]
    [SerializeField]
    private GameObject UnitInventoryContentBox;
    [SerializeField]
    private GameObject Unit_1_Portrait, Unit_2_Portrait, Unit_3_Portrait;
    [SerializeField]
    private List<TextMeshProUGUI> UnitStatsText;
    /* 0. Health
     * 1. Attack
     * 2. Defence
     * 3. Sp. Attack
     * 4. Sp. Defence
     * 5. Mana
     * 6. Speed
     */

    private InventoryObject _selectedUnit;
    public InventoryObject Selected { get => _selectedUnit; set
        {
            _selectedUnit = value;
            UpdateUnitInfo();
        } 
    }

    [SerializeField]
    private GameObject UnitDisplayArea; 
    private GameObject UnitDisplay; 

    [Header("SpellMenu")]
    [SerializeField]
    private GameObject SpellInventoryContentBox;
    [SerializeField]
    private GameObject DeckContentBox, DeckImage;
    [SerializeField]
    private TextMeshProUGUI SpellName, SpellDescription; 
    [SerializeField]
    private SpellSO CurrentSpell;
    TeamSpace S1, S2, S3;
    static GameObject CardSelected;

    #endregion
    private void Awake()
    {
        LoginManager.OnLogin += SetCurrencyUI;
        instance = this;

        LoginManager.OnLogin += () =>
        {
            S1 = Unit_1_Portrait.AddComponent<TeamSpace>();
            S2 = Unit_2_Portrait.AddComponent<TeamSpace>();
            S3 = Unit_3_Portrait.AddComponent<TeamSpace>();
        };

        TeamMenuTransform.gameObject.AddComponent<TeamMenu>();
    }

    private void Update()
    {
        CurrencyUpdate();
    }

    #region Currency
    [HideInInspector]
    public int PC, MC, SC, FC;

    private void CurrencyUpdate()
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

#endregion
#region Team Management

    public void FillUnitContentBox (List<UnitSO> Inventory)
    {
        foreach(UnitSO unit in Inventory)
        {
            CreateNewClickableCard(unit); 
        }
    }

    private void CreateNewClickableCard<T>(T unit)where T: InventoryObject
    {
        GameObject go = new GameObject();
        go.transform.parent = UnitInventoryContentBox.transform;
        go.AddComponent<Image>().sprite = unit.Portrait;
        go.AddComponent<ClickableCard>().AssignedSO = unit;
        go.transform.localScale = Vector3.one;
    }

    public void ClearContentBox()
    {
        for(int start = 0, end = UnitInventoryContentBox.transform.childCount - 1; start <= end; start++)
        {
            Destroy(UnitInventoryContentBox.transform.GetChild(start).gameObject); 
        }
    }

    private void UpdateUnitInfo()
    {

        try
        {
            UnitSO su = _selectedUnit as UnitSO;

            Destroy(UnitDisplay);

            UnitDisplay = Instantiate(su.Prefab, UnitDisplayArea.transform);
            UnitDisplay.transform.LookAt(MenuCamera.transform);
            UnitDisplay.transform.localScale = Vector3.one * 2;

            UnitStatsText[0].text = $"Health  : {su.UnitBaseStats.Health}";
            UnitStatsText[1].text = $"Attack  : {su.UnitBaseStats.Attack}";
            UnitStatsText[2].text = $"Defence : {su.UnitBaseStats.Defence}";
            UnitStatsText[3].text = $"Sp Attk : {su.UnitBaseStats.SpellAttack}";
            UnitStatsText[4].text = $"Sp Def  : {su.UnitBaseStats.SpellDefence}";
            UnitStatsText[5].text = $"Speed   : {su.UnitBaseStats.Speed}";
            UnitStatsText[6].text = $"Mana    : {su.UnitBaseStats.ManaPips}";


        }
        catch
        {
            //Errors either destroying the display or setting skill text
        }

    }

    public void AddToTeam()
    {
        if (S1.CurrentUnit == null)
        {
            S1.AssignUnitToSpace();
            Selected = null; 
            Destroy(CardSelected);
        }
        else if (S2.CurrentUnit == null)
        {
            S2.AssignUnitToSpace();
            Selected = null; 
            Destroy(CardSelected);
        }
        else if (S3.CurrentUnit == null)
        {
            S3.AssignUnitToSpace();
            Selected = null; 
            Destroy(CardSelected);
        }
        else
            Debug.Log("Team Full");
    }

    public void RemoveFromSpace()
    {
        if (S3.CurrentUnit != null)
        {
            S3.RemoveFromSpace();
            S3.CurrentUnit = null;
        }
        else if (S2.CurrentUnit != null)
        {
            S2.RemoveFromSpace();
            S2.CurrentUnit = null;
        }
        else if (S1.CurrentUnit != null)
        {
            S1.RemoveFromSpace();
            S1.CurrentUnit = null;
        }
        else
            Debug.Log("Team Empty");

    }

    #endregion

    public void UpdateSpellInfo()
    {

    }


    private void OnError(PlayFabError obj)
    {
        Debug.LogWarning(obj.ErrorMessage); 
    }
}
