using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpellInstance : MonoBehaviour, IPointerClickHandler
{
    public SpellSO SpellBase;
    public int CurrentCost;
    public Ability SpellEffect;


    public void UpdateSpell()
    {
        SpellEffect = SpellBase.SpellEffect;
        CurrentCost = SpellBase.Cost; 
    }

    private void Cast(UnitInstance Target)
    {
        //ValidityCheck
        Debug.Log($"{Target.name} was struck with {name}");

        Ability A = Instantiate(SpellEffect);

        if (A is ActiveAbility)
        {
            A.transform.position = Target.transform.position; 
        }

        if(A is TravelCast)
        {
            A.transform.position = BattleStateManager.instance.CurrentUnit.transform.position; 
        }

        ToDiscard(); 

    }

    /// <summary>
    /// Places spells to the bottom of the deck after they've been cast
    /// </summary>
    public void ToDiscard()
    {
        Destroy(gameObject); 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        BattleStateManager.instance.ClearActionQueue();

        BattleStateManager.instance.ActionQueued = true;

        BattleStateManager.instance.SpellCast.AddListener(() => Cast(BattleStateManager.instance.Target));
        BattleStateManager.instance.OnActionTaken += ctx => BattleStateManager.instance.SpellCast.Invoke(); 

        Debug.Log("Spell Selected");

        if (BattleStateManager.instance.Target != null)
            BattleStateManager.instance.OnActionTaken.Invoke(BattleStateManager.instance.Target);
    }
    //Battle Manager has "End Turn After Spell Cast" Which is already implemented in order
    //to end the turn after the spell effect is finished not after the spell is selected
}