using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DeckManager
{
    private static List<SpellInstance> _playerHand = new List<SpellInstance>(); 
    private static List<SpellInstance> _enemyHand = new List<SpellInstance>(); 

    public static void DrawCard(bool isPlayer, List<SpellSO> DeckInstance)
    {
        var nextCard = new System.Random().Next(DeckInstance.Count - 1);
        var Card = DeckInstance[nextCard];

        SpellInstance spell = GameObject.Instantiate(Card.Prefab).GetComponent<SpellInstance>();
        spell.SpellBase = Card;
        spell.UpdateSpell();
        
        if (isPlayer)
        {
            spell.transform.SetParent(GameObject.Find("Hand").transform, false);
            _playerHand.Add(spell); 
        }
        else
        {
            _enemyHand.Add(spell);
        }

        DeckInstance.RemoveAt(nextCard); 
    }

    /// <summary>
    /// Placeholders just in case Enemy AI needs access to its own hand
    /// </summary>
    #region EnemyUseCard
    private static void EnemyUseCard() { }
    private static void EnemyCardValueCalc() { }
    #endregion
}
