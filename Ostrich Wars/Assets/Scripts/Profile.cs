using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Profile : MonoBehaviour
{
    public List<UnitSO> UnitInventory = new List<UnitSO>();
    public Dictionary<string, List<UnitSO>> SavedTeams = new Dictionary<string, List<UnitSO>>(); 
    public List<UnitSO> CurrentTeam = new List<UnitSO>();

    public List<SpellSO> SpellsInventory = new List<SpellSO>();
    public List<SpellSO> CurrenDeck = new List<SpellSO>();

    public int _BaseUnitPity, _BaseSpellPity;
    public int UnitPity, SpellPity;

    public int Shards; //Things used to purchase units
}
