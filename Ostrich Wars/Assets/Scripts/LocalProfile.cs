using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Linq;

public class LocalProfile : MonoBehaviour
{
    public List<UnitSO> UnitInventory = new List<UnitSO>();
    public Dictionary<string, List<UnitSO>> SavedTeams = new Dictionary<string, List<UnitSO>>(); 
    private List<UnitSO> currentTeam = new List<UnitSO>();
    public List<UnitSO> CurrentTeam
    {
        get => currentTeam; set
        {
            if(currentTeam.Count)
        }
    }

    public List<SpellSO> SpellsInventory = new List<SpellSO>();
    public List<SpellSO> CurrenDeck = new List<SpellSO>();

    public int _BaseUnitPity, _BaseSpellPity;
    public int UnitPity, SpellPity;

    public void SetProfile()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    public string InventoryEncoder<T>(List<T> Inventory) where T : InventoryObject
    {
        string InventoryCode = "";
        char InventoryDelimiter = ';';

        foreach(InventoryObject thing in Inventory)
        {
            InventoryCode += thing.InvCode + InventoryDelimiter;
        }

        return InventoryCode;
    }

    private string EncodeSavedTeams(Dictionary<string, List<UnitSO>> Teams)
    {
        string fullsavedteams = "";
        char delim = '|';
        foreach(string name in Teams.Keys)
        {
            fullsavedteams += delim + name + delim;

            fullsavedteams += InventoryEncoder(Teams[name]);
        }
        return fullsavedteams; 
    }

    private Dictionary<string, List<UnitSO>> DecodeSavedTeams(string Teams)
    {
        string tmpNameInc ="";
        string tmpName = "";
        string tmpTeam =""; 
        bool nameStart = false; 
        char NameDelim = '|';
        char InvenDeliter = ';';

        Dictionary<string, List<UnitSO>> TeamList = new Dictionary<string, List<UnitSO>>();

        try
        {
            foreach (char c in Teams)
            {
                if (c == NameDelim)
                {
                    if (!nameStart)
                    {
                        tmpNameInc += c;
                    }
                    else
                    {
                        tmpName = tmpNameInc;
                        tmpNameInc = "";
                        TeamList.Add(tmpName, new List<UnitSO>());
                        nameStart = false;
                    }//Created a new Entry for the teamlist
                }
                else
                {
                    tmpTeam += c;
                    if (c == InvenDeliter)
                    {
                        TeamList[tmpName] = InventoryDecoder<UnitSO>(tmpTeam);
                        tmpTeam = "";
                    }
                }
            }
        }
        catch
        {
            Debug.LogWarning("Issue loading saved teams");
        }

        return TeamList; 
    }

    public List<T> InventoryDecoder<T>(string Code) where T : InventoryObject
    {
        List<T> Inventory = new List<T>();
        List<T> AllObjects = Resources.LoadAll<T>("ScriptableObjects").ToList();
        char InventoryDelimiter = ';';
        string tmp = "";


        try
        {
            foreach (char c in Code)
            {
                if (c == InventoryDelimiter)
                {
                    Inventory.Add(AllObjects.Where(ctx => ctx.InvCode == tmp).FirstOrDefault());
                    tmp = "";
                }
                else
                {
                    tmp += c;
                }

            }
        }
        catch
        {
            Debug.Log("Empty Team");
        }
        return Inventory;
    }

    private void OnDataRecieved(GetUserDataResult obj)
    {
        bool Datacheck = 
            obj.Data.ContainsKey("SavedTeams") &&
            obj.Data.ContainsKey("CurrentTeam") &&
            obj.Data.ContainsKey("CurrenDeck") &&
            obj.Data.ContainsKey("UnitPity") &&
            obj.Data.ContainsKey("SpellPity");

        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnInvRecieved, OnError); 


        if (obj.Data != null && Datacheck)
        {
            CurrentTeam = InventoryDecoder<UnitSO>(obj.Data["CurrentTeam"].Value);
            CurrenDeck = InventoryDecoder<SpellSO>(obj.Data["CurrenDeck"].Value);
            UnitPity = Convert.ToInt32(obj.Data["UnitPity"].Value);
            SpellPity = Convert.ToInt32(obj.Data["SpellPity"].Value);
            SavedTeams = DecodeSavedTeams(obj.Data["SavedTeams"].Value);
        }

    }

    private void OnInvRecieved(GetUserInventoryResult obj)
    {
        UnitInventory = new List<UnitSO>();
        SpellsInventory = new List<SpellSO>(); 

        foreach(var inventoryobject in obj.Inventory)
        {
            if(inventoryobject.ItemClass == "UnitSO")
            {
                UnitInventory.Add(DatabaseAccessor.LoadUnit(inventoryobject.DisplayName));
            }
            else if(inventoryobject.ItemClass == "SpellSO")
            {
                SpellsInventory.Add(DatabaseAccessor.LoadSpell(inventoryobject.DisplayName));
            }
        }
    }

    public void SaveProfile()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"CurrentTeam", InventoryEncoder(CurrentTeam)},
                {"CurrenDeck", InventoryEncoder(CurrenDeck)},
                {"UnitPity", UnitPity.ToString()},
                {"SpellPity", SpellPity.ToString()},
                {"SavedTeams", EncodeSavedTeams(SavedTeams)}

            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSaved, OnError);

    }

    private void OnError(PlayFabError obj)
    {
        Debug.Log(obj.ErrorMessage);
    }

    private void OnDataSaved(UpdateUserDataResult obj)
    {
        Debug.Log("Deck Saved");
    }  
    

}
