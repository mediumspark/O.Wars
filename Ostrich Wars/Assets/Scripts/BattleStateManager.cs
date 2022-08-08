using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.InputSystem; 

public class BattleStateManager : MonoBehaviour
{
    ///Player Lists from their selected Inventories
    private static List<UnitSO> _playerUnit, _enemyUnits;
    //Units Currently on the field
    private List<UnitInstance> _activePlayerUnits = new List<UnitInstance>();
    private List<UnitInstance> _activeEnemyUnits = new List<UnitInstance>();

    //Currently Acting Unit
    private static UnitInstance _currentActingUnit;
    public static UnitInstance CurrentUnit => _currentActingUnit;
    private static UnitAnimation _currentUnitAnimations => _currentActingUnit.UIA; 

    //TurnOrder of the Currently Acting Units
    private UnitInstance[] _turnOrder;

    [SerializeField]
    private Transform _playerBattleSpace, _enemyBattleSpace;

    [SerializeField]
    private GameObject _playerUI;

    [SerializeField]
    private EnemyAI _aIPlayer; 
    public EnemyAI SetAI { set => _aIPlayer = value;  }
    /// <summary>
    /// ActionQueued is marked when the player selects an option  
    /// </summary>
    public static bool ActionQueued = false; 

    /// <summary>
    /// Target of Unit Strike or Spell
    /// </summary>
    private static UnitInstance _targetCache;

    private NewInput inputActions;

    /// <summary>
    /// Decks
    /// </summary>
    private static List<SpellSO> _playerDeck, _enemyDeck;
    [SerializeField]
    private List<SpellSO> _playerDeckInstance, _enemyDeckInstance; 

    private void Awake()
    {
        inputActions = new NewInput();

        inputActions.NormalEvent.Select.performed += ctx =>
        {
            RaycastHit hit;
            Ray publicWorldPos = GameManager.CachedCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(publicWorldPos, out hit))
            {
                if(hit.transform.TryGetComponent(out IGameplayInteractable Unit))
                {
                    Unit.OnPress(); 
                }
            }
        };
    }

    public static bool ActiveAnimation = false;

    private void OnEnable()
    {
        inputActions.Enable(); 
    }

    private void OnDisable()
    {
        inputActions.Disable(); 
    }

    public static UnitInstance Target 
    {
        get => _targetCache;
        set 
        {
            _targetCache = value;
            if (ActionQueued)
            {
                OnActionTaken.Invoke(Target); 
            }
        } 
    }


    public static void SetBattleComponents(List<UnitSO> PlayerUnit, List<UnitSO> EnemyUnits, List<SpellSO> PlayerDeck, List<SpellSO> EnemyDeck)
    {
        _playerDeck = PlayerDeck; 
        _playerUnit = PlayerUnit;
        _enemyDeck = EnemyDeck; 
        _enemyUnits = EnemyUnits;
        FindObjectOfType<BattleStateManager>().StartBattle(); 
    }

    /// <summary>
    /// When the Battle Begins
    /// Summon Units
    /// Determine TurnOrder 
    /// Any Passives that take place on Turn Starts
    /// </summary>
    public void StartBattle()
    {
        _playerDeckInstance.AddRange(_playerDeck);
        _enemyDeckInstance.AddRange(_enemyDeck); 

        UnitCollection();

        _turnOrder = TurnOrderCalc();

        _currentActingUnit = _turnOrder[0];

        #region Adding Listeners
        OnTurnEnd.AddListener(() => EndTurn());
        OnTurnStart.AddListener(() =>
        {
            if (!_currentActingUnit.PlayerOwned)
            {
                Debug.Log("EnemyTurn");
                _aIPlayer.ExecuteDescision(_currentActingUnit, _activeEnemyUnits.Concat(_activePlayerUnits).ToList());
            }
            else
            {
                ActiveAnimation = false; 
            }
        });

        OnTurnStart.AddListener(() =>
        {
            if(_enemyDeck.Count > 0)
            {
                DeckManager.DrawCard(false, _enemyDeckInstance); 
            }

            if(_playerDeck.Count > 0)
            {
                DeckManager.DrawCard(true, _playerDeckInstance);
            }
        });
        OnTurnEnd.AddListener(() =>
        {

            if (Target != null && !Target.isAlive)
            {
                OnUnitDeath += ctx => UnitDied(Target);
                OnUnitDeath += ctx => VictoryCheck();
                Target.UIA.DieTrigger(); 
                OnUnitDeath.Invoke(Target); 
            }

            if (!_currentActingUnit.isAlive)
            {
                OnUnitDeath += ctx => UnitDied(_currentActingUnit);
                OnUnitDeath += ctx => VictoryCheck();
                OnUnitDeath.Invoke(_currentActingUnit);
                _currentUnitAnimations.DieTrigger(); 
            }

        });
        AddActionRecurringlListeners();
        #endregion

        OnTurnStart.Invoke(); 
    }

    private void Update()
    {
        _playerUI.SetActive(_currentActingUnit.PlayerOwned && !ActiveAnimation);
    }

    /// <summary>
    /// Summons units from player and enemy inventory
    /// </summary>
    private void UnitCollection()
    {
        for (int UnitInterator = 0; UnitInterator <= 3; UnitInterator++)
        {
            
            UnitInstance newPlayerUnit = UnitInterator < _playerUnit.Count || UnitInterator == 0? 
                Instantiate(_playerUnit[UnitInterator].Prefab.GetComponent<UnitInstance>(), _playerBattleSpace.GetChild(UnitInterator)) : null;

            UnitInstance newEnemyUnit = UnitInterator < _enemyUnits.Count || UnitInterator == 0?
                Instantiate(_enemyUnits[UnitInterator].Prefab.GetComponent<UnitInstance>(), _enemyBattleSpace.GetChild(UnitInterator)) : null;


            if (newPlayerUnit != null)
            {
                newPlayerUnit.PlayerOwned = true; 
                newPlayerUnit.UnitBase = _playerUnit[UnitInterator];
                _activePlayerUnits.Add(newPlayerUnit);
            }

            if(newEnemyUnit != null)
            {
                newEnemyUnit.UnitBase = _enemyUnits[UnitInterator];
                _activeEnemyUnits.Add(newEnemyUnit);
            }
        }
    }

    #region Unity Events

    /// <summary>
    /// Called when a new unit begins it's phase
    /// </summary>
    [HideInInspector]
    public UnityEvent OnTurnStart;

    /// <summary>
    /// Called When a unit selections an action and then completes that action
    /// </summary>
    public delegate void OnActionTakenDelegate(UnitInstance target);
    public static OnActionTakenDelegate OnActionTaken; 

    /// <summary>
    /// Called when that action is completed but before the next unit has been chosen
    /// </summary>
    [HideInInspector]
    public UnityEvent OnTurnEnd;

    /// <summary>
    /// Called when a unit has died and is removed from play
    /// </summary>
    public delegate void OnDeathDelegate(UnitInstance target);
    [HideInInspector]
    public OnDeathDelegate OnUnitDeath;

    /// <summary>
    /// Called when a unit casts a spell 
    /// </summary>
    [HideInInspector]
    public static UnityEvent SpellCast = new UnityEvent();

    /// <summary>
    /// Called when unit uses AttackCommand
    /// </summary>
    [HideInInspector]
    public UnityEvent OnAttack;

    /// <summary>
    /// Called When Unit uses Pass Commmand
    /// </summary>
    [HideInInspector]
    public UnityEvent OnPass;
    #endregion

    /// <summary>
    /// TEMP: 
    /// Testing simple turned based -> Static turns based on speed tiers, no ui representation
    /// Creates the turn list
    /// </summary>
    private UnitInstance[] TurnOrderCalc()
    {
        List<UnitInstance> AllCurrentUnits = new List<UnitInstance>();
        AllCurrentUnits.AddRange(_activeEnemyUnits);
        AllCurrentUnits.AddRange(_activePlayerUnits);
        return AllCurrentUnits.OrderByDescending(u => u.CurrStats.Speed).ToArray(); 
    }

    public void VictoryCheck()
    {
        Debug.Log("Victory Check");
        bool AlliesDead = _activePlayerUnits.ToArray().Length == 0;
        bool EnemiesDead = _activeEnemyUnits.ToArray().Length == 0;

        if (!AlliesDead && EnemiesDead)
        {
            //Player WIn
            Debug.Log("You Win!");
        }
        else if (AlliesDead && !EnemiesDead)
        {
            //Enemies Win
            Debug.Log("You Lose!");
        }
        else if (AlliesDead && EnemiesDead)
        {
            //Draw
            Debug.Log("Draw?");
        }
        
    }

    /// <summary>
    /// TEMP: 
    /// Testing simple turned based -> Static turns based on speed tiers, no ui representation
    /// Goes to next unit
    /// </summary>
    private void NextTurnInCycle()
    {
        int unitIndex = _turnOrder.ToList().IndexOf(_currentActingUnit);

        unitIndex++; 

        if(unitIndex >= _turnOrder.Length)
        {
            unitIndex = 0;
        }

        _currentActingUnit = _turnOrder[unitIndex];

        OnTurnStart.Invoke();
    }

    public void EndTurn()
    {
        ActionQueued = false;
        //Will need to remove any additional listeners
        ClearActionEvents();
        AddActionRecurringlListeners();
        NextTurnInCycle();
    }

    private void AddActionRecurringlListeners()
    {
        //ReAdding Essential Listeners
        OnAttack.AddListener(() =>
        {
            _currentUnitAnimations.AttackTrigger();
        });

        OnPass.AddListener(() =>
        {
            //This shouldn't work
            _currentActingUnit.CurrStats.ManaPips++;
            OnTurnEnd.Invoke();
        });

        SpellCast.AddListener(() =>
        {
            _currentUnitAnimations.CastTrigger(); 
        });

    }

    public void PlayHitAnimation(ParticleSystem AttackEffect)
    {
        var go = Instantiate(AttackEffect, Target.transform);
        go.Play();
    }

    //Used Externally so clicking a new spell clears all other spells from the ActionTaken delegate
    public static void ClearActionQueue()
    {
        OnActionTaken = null; 
    }

    private void ClearActionEvents()
    {
        OnAttack.RemoveAllListeners();
        SpellCast.RemoveAllListeners();
        OnPass.RemoveAllListeners();
    }

    //Used Externally so that the spell animation can play out
    public static void EndTurnAfterSpellCast()
    {
        FindObjectOfType<BattleStateManager>().OnTurnEnd.Invoke(); 
    }

    /// <summary>
    /// For UI Attack Button
    /// </summary>
    public void AttackButtonPress()
    {
        ClearActionQueue();
        
        OnAttack.AddListener(() => Target.TakeDaamge(CurrentUnit.CurrStats.Attack - Mathf.FloorToInt(Target.CurrStats.Defence / 2) + 1));

        OnActionTaken += ctx => OnAttack.Invoke();
        OnActionTaken += ctx => OnTurnEnd.Invoke();

        if (Target != null)
        {
            ActiveAnimation = true;
            OnActionTaken.Invoke(Target);
        }
        else
            Debug.Log("Select Target");

    }

    /// <summary>
    /// On unit death the unit is removed from the field, from the turn order
    /// and it's gameobject is destroyed
    /// </summary>
    /// <param name="Unit"></param>
    public void UnitDied(UnitInstance Unit)
    {
        if(Unit.PlayerOwned)
        {
            _activePlayerUnits.Remove(Unit); 
        }
        else
        {
            _activeEnemyUnits.Remove(Unit); 
        }

        _turnOrder.ToList().Remove(Unit); 
        

        if (Unit == _currentActingUnit)
            OnTurnEnd.Invoke(); 

        Destroy(Unit);

    }

    /// <summary>
    /// For UI Pass Button
    /// </summary>
    public void OnPassButtonPress()
    {
        OnPass.Invoke(); 
    }
}
