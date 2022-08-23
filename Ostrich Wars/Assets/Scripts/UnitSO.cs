using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct UnitStats
{
    public int Health, Attack, Defence, SpellAttack, SpellDefence, Speed;
    [Range(0, 5)]
    public int ManaPips;
    public StatusEffects _currenStatusEffect;

}

[CreateAssetMenu(fileName = "New Unit", menuName = "Unit")]
public class UnitSO : InventoryObject
{
    public UnitStats UnitBaseStats;

    //Prefabs 
    public GameObject Prefab; 
    public PassiveAbility Passive; 

    public string Description;

    public UnitRarity Rarity; 
}
