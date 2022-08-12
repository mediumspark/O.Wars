using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gatcha : MonoBehaviour
{    
    private Dictionary<UnitRarity, List<UnitSO>> UnitList = new Dictionary<UnitRarity, List<UnitSO>>();
    private Dictionary<SpellRarity, List<SpellSO>> SpellList = new Dictionary<SpellRarity, List<SpellSO>>();

    /// <summary>
    /// Before starting the gatcha process we need to make sure we 
    /// have a cache for every possible Unit or spell that the player
    /// can get in the roll
    /// </summary>
    public void Start()
    {
        foreach(UnitSO unit in DatabaseAccessor.LoadFullUnitList())
        {
            if (UnitList.ContainsKey(unit.Rarity))
            {
                UnitList[unit.Rarity].Add(unit); 
            }
            else
            {
                UnitList.Add(unit.Rarity, new List<UnitSO>());
                UnitList[unit.Rarity].Add(unit); 
            }
        }

        foreach(SpellSO spell in DatabaseAccessor.LoadSpellList())
        {
            if (SpellList.ContainsKey(spell.Rarity))
            {
                SpellList[spell.Rarity].Add(spell);
            }
            else
            {
                SpellList.Add(spell.Rarity, new List<SpellSO>());
                SpellList[spell.Rarity].Add(spell);
            }
        }
    }

    /// <summary>
    /// For each Dice the first roll is for unit rarity and the second 
    /// is for the unit itself
    /// </summary>
    #region Dice Rolls
    private (UnitRarity, int) UnitDice(LocalProfile Player)
    {
        var UnitRarityRoll = new System.Random().Next();
        var UnitRoll = new System.Random().Next(UnitList[DecideUnitRarity(UnitRarityRoll, Player)].Count);

        return (DecideUnitRarity(UnitRarityRoll, Player), UnitRoll);
    }

    private (SpellRarity, int) SpellDice(LocalProfile Player)
    {
        var SpellRarityRoll = new System.Random().Next();
        var SpellRoll = new System.Random().Next(SpellList[DecideSpellRarity(SpellRarityRoll, Player)].Count);

        return (DecideSpellRarity(SpellRarityRoll, Player), SpellRoll);
    }

    //Check Unit Rarity Roll
    private UnitRarity DecideUnitRarity(int UnitRarityRoll, LocalProfile Player)
    {

        bool isCommon = UnitRarityRoll >= (int)UnitRarity.Common;
        bool isRare = !isCommon && UnitRarityRoll >= (int)UnitRarity.Rare;
        bool isLegend = !isCommon && !isRare && UnitRarityRoll >= (int)UnitRarity.Legend;

        if (Player.UnitPity <= 0 && UnitRarityRoll > 25)
        {
            Player.UnitPity = Player._BaseUnitPity;
            DecideUnitRarity(10, Player);
        }

        if (isCommon)
        {
            Player.UnitPity--;
            return UnitRarity.Common;
        }
        else if (isRare)
        {
            Player.UnitPity--;
            return UnitRarity.Rare;
        } else if (isLegend)
        {
            return UnitRarity.Legend;
        } 
        else
        {
            return UnitRarity.Diety;
        }

    }
    
    //Check Spell Rarity Roll
    private SpellRarity DecideSpellRarity(int SpellRarityRoll, LocalProfile Player)
    {
        bool isCommon = SpellRarityRoll >= (int)UnitRarity.Common;
        bool isUncommon = !isCommon && SpellRarityRoll >= (int)UnitRarity.Rare;
        bool isLegend = !isCommon && !isUncommon && SpellRarityRoll >= (int)UnitRarity.Legend;
        bool isThresh = !isCommon && !isUncommon && !isLegend && SpellRarityRoll >= (int)UnitRarity.Legend;

        if (Player.SpellPity <= 0 && SpellRarityRoll > 25)
        {
            Player.SpellPity = Player._BaseSpellPity;
            DecideUnitRarity(10, Player);
        }

        if (isCommon)
        {
            Player.SpellPity--; 
            return SpellRarity.Common;
        }
        else if (isUncommon)
        {
            Player.SpellPity--; 
            return SpellRarity.Uncommon;
        }
        else if (isLegend)
        {
            return SpellRarity.Hex;
        }
        else if(isThresh)
        {
            return SpellRarity.Thresh;
        }
        else
        {
            return SpellRarity.Miracle;
        }
    }

    #endregion

    public void SendToInventory(LocalProfile Player)
    {
        var Uroll = UnitDice(Player);
        var Sroll = SpellDice(Player);

        Player.UnitInventory.Add(UnitList[Uroll.Item1][Uroll.Item2]); 
        Player.SpellsInventory.Add(SpellList[Sroll.Item1][Sroll.Item2]); 
    }
}
