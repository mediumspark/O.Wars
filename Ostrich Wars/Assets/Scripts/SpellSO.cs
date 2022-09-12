using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Spell", menuName = "Spell")]
public class SpellSO : InventoryObject
{
    public int Cost;
    //Prefab
    public SpellInstance Prefab; 
    public Ability SpellEffect; 
    public SpellRarity Rarity; 
}


