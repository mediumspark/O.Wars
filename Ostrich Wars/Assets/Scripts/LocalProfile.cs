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
    public List<UnitSO> CurrentTeam = new List<UnitSO>();

    public List<SpellSO> SpellsInventory = new List<SpellSO>();
    public List<SpellSO> CurrenDeck = new List<SpellSO>();

    public int _BaseUnitPity, _BaseSpellPity;
    public int UnitPity, SpellPity;

    public void SetProfile()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }   

    private void OnDataRecieved(GetUserDataResult result)
    {
        bool Datacheck = 
            result.Data.ContainsKey("SavedTeams") &&
            result.Data.ContainsKey("CurrentTeam") &&
            result.Data.ContainsKey("CurrenDeck") &&
            result.Data.ContainsKey("UnitPity") &&
            result.Data.ContainsKey("SpellPity");

        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnInvRecieved, OnError); 


        if (result.Data != null && Datacheck)
        {
            CurrentTeam = TeamEncoding.InventoryDecoder<UnitSO>(result.Data["CurrentTeam"].Value);
            CurrenDeck = TeamEncoding.InventoryDecoder<SpellSO>(result.Data["CurrenDeck"].Value);
            UnitPity = Convert.ToInt32(result.Data["UnitPity"].Value);
            SpellPity = Convert.ToInt32(result.Data["SpellPity"].Value);
            SavedTeams = TeamEncoding.DecodeSavedTeams(result.Data["SavedTeams"].Value);
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
                {"CurrentTeam", TeamEncoding.InventoryEncoder(CurrentTeam)},
                {"CurrenDeck", TeamEncoding.InventoryEncoder(CurrenDeck)},
                {"UnitPity", UnitPity.ToString()},
                {"SpellPity", SpellPity.ToString()},
                {"SavedTeams", TeamEncoding.EncodeSavedTeams(SavedTeams)}

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


    public void AddToDeck(SpellSO newSpell)
    {
        if(CurrenDeck.Count < 20)
        {
            CurrenDeck.Add(newSpell);
        }
        else
        {
            Debug.Log("DeckLimit Reached");
        }
    }

    public void RemoveFromDeck(SpellSO spell)
    {
        CurrenDeck.Remove(spell); 
    }
    

    public void AddToTeam(UnitSO newUnit, int Position)
    {
        if (Position == 1 || Position == 2 || Position == 0)
        {
            CurrentTeam.Insert(Position, newUnit);
        }
        else
        {
            Debug.Log("Not valid team position");
        }
    }

    public void RemoveFromTeam(int Position)
    {
        try
        {
            if ((Position == 1 || Position == 2 || Position == 0) && CurrentTeam[Position] != null)
            {
                CurrentTeam.RemoveAt(Position);
            }
            else
            {
                Debug.Log("Not a valid team Position");
            }
        }
        catch
        {
            Debug.LogWarning("Warning: Issue at removal, likely index issue");
        }

    }

    public void SaveTeam(string name)
    {
        SavedTeams.Add(name, CurrentTeam);
    }

}

public static class TeamEncoding
{
    public static string InventoryEncoder<T>(List<T> Inventory) where T : InventoryObject
    {
        string InventoryCode = "";
        char InventoryDelimiter = ';';

        foreach (InventoryObject thing in Inventory)
        {
            InventoryCode += thing.InvCode + InventoryDelimiter;
        }

        return InventoryCode;
    }

    public static string EncodeSavedTeams(Dictionary<string, List<UnitSO>> Teams)
    {
        string fullsavedteams = "";
        char delim = '|';
        foreach (string name in Teams.Keys)
        {
            fullsavedteams += delim + name + delim;

            fullsavedteams += InventoryEncoder(Teams[name]);
        }
        return fullsavedteams;
    }

    public static Dictionary<string, List<UnitSO>> DecodeSavedTeams(string Teams)
    {
        string tmpNameInc = "";
        string tmpName = "";
        string tmpTeam = "";
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

    public static List<T> InventoryDecoder<T>(string Code) where T : InventoryObject
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
}

