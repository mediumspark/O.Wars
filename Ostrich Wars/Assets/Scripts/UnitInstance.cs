using UnityEngine;

public interface IGameplayInteractable
{
    public void OnPress();
}

public class UnitInstance : MonoBehaviour, IGameplayInteractable
{
    private UnitSO _unitBaseSO; 

    public UnitSO UnitBase 
    {
        get => _unitBaseSO;
        set
        {
            _unitBaseSO = value; 
            CurrStats = UnitBase.UnitBaseStats;
        }
    }

    public UnitAnimation UIA => GetComponentInChildren<UnitAnimation>(); 

    public UnitStats CurrStats;

    public bool PlayerOwned;

    private StatusEffects _currenStatusEffect = StatusEffects.normal;
    public StatusEffects CurrentStatus
    {
        get => _currenStatusEffect;
        set
        {
            _currenStatusEffect = value;
            StatusPing(_currenStatusEffect); 
        }
    }

    public bool CurrentlyAnimated = false;

    public bool isAlive = true; 
    
    public void StatusPing(StatusEffects Effect)
    {
        if(_currenStatusEffect == StatusEffects.normal || Effect == StatusEffects.normal)
        {
            _currenStatusEffect = Effect; 
        }//Cannot convert one status to another status i.e. Blessed Units cannot be Feared or heavy
        //However Units can be converted back to Normal 

        //Status effects, switch statement
        switch (_currenStatusEffect)
        {
            case StatusEffects.Blessed:
                CurrStats.SpellDefence *=2;
                CurrStats.SpellAttackBonus *= 2; 
                break;
            case StatusEffects.Burned:
                CurrStats.Attack /= 2; 
                break;
            case StatusEffects.Fear:
                CurrStats.Defence /= 2;
               break;
            case StatusEffects.Cursed:
                break;
            case StatusEffects.Heavy:
                CurrStats.Speed /= 2; 
                break;
            case StatusEffects.normal:
                int CurrHealth = CurrStats.Health;
                CurrStats = _unitBaseSO.UnitBaseStats;
                CurrStats.Health = CurrHealth; 
                break;
            default:
                Debug.LogWarning("Something has gone wrong when setting a status effect");
                break;//Something has gone wrong if it defaults

        }
    }

    private void OnMouseEnter()
    {
        //hover highlight
    }

    public void OnPress()
    {
        Debug.Log($"{name} clicked");

        //TODO: Add Valid Target Check
        BattleStateManager.Target = this; 
    }

    private void OnMouseExit()
    {
        //End hover highlight
    }

    public void TakeDaamge(int damage)
    {
        int health = CurrStats.Health;
        health -= damage; 
        CurrStats.Health = health; 

        if(CurrStats.Health <= 0)
        {
            isAlive = false; 
        }
    }
}
