using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class DatabaseAccessor
{

    //Load roaster from local database
    public static List<UnitSO> LoadFullUnitList()
    {
        return Resources.LoadAll<UnitSO>("ScriptableObjects/Units").ToList();
      
    }

    //Load roaster from local database
    public static List<SpellSO> LoadSpellList()
    {
        return Resources.LoadAll<SpellSO>("ScriptableObjects/Spells").ToList();
    }

    public static UnitSO LoadUnit(string name)
    {
        try {
            return Resources.Load<UnitSO>($"ScriptableObjects/Units/{name}");
        }
        catch
        {
            Debug.LogError("Unit Not Found");
            return null; 
        }
    }

    public static SpellSO LoadSpell(string name)
    {
        try {
            return Resources.Load<SpellSO>($"ScriptableObjects/Spells/{name}");
        }
        catch
        {
            Debug.LogError("Spell Not Found");
            return null; 
        }
    }
}
