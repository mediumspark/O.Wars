using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.InputSystem;
using Mirror;


public class BattleStateManager : NetworkBehaviour
{
    public static BattleStateManager instance; 

    ///Player Lists from their selected Inventories
    private readonly SyncList<UnitSO> _playerUnit = new SyncList<UnitSO>();
    private readonly SyncList<UnitSO> _enemyUnits = new SyncList<UnitSO>();
    //Units Currently on the field
    private readonly SyncList<UnitInstance> _activePlayerUnits = new SyncList<UnitInstance>();
    private readonly SyncList<UnitInstance> _activeEnemyUnits = new SyncList<UnitInstance>();

    public SyncList<UnitInstance> EnemiesActive => _activeEnemyUnits;
    public SyncList<UnitInstance> AllUnits  { 
        get {
            SyncList<UnitInstance> UIL = new SyncList<UnitInstance>();

            UIL.AddRange(_activePlayerUnits);
            UIL.AddRange(_activeEnemyUnits);
            return UIL; 
        }
    }

    //Currently Acting Unit
    private UnitInstance _currentActingUnit;
    [SyncVar(hook = nameof(SetNextUnit))]
    int unitIndex = 0; 
    public  UnitInstance CurrentUnit => _currentActingUnit;
    private UnitAnimation _currentUnitAnimations => _currentActingUnit.UIA; 

    //TurnOrder of the Currently Acting Units
    protected UnitInstance[] _turnOrder;
    public List<UnitInstance> TurnOrder => _turnOrder.ToList(); 

    [SerializeField]
    protected Transform _playerBattleSpace, _enemyBattleSpace;

    [SerializeField]
    protected GameObject _playerUI;

    /// <summary>
    /// ActionQueued is marked when the player selects an option  
    /// </summary>
    public bool ActionQueued = false; 

    /// <summary>
    /// Target of Unit Strike or Spell
    /// </summary>
    private UnitInstance _targetCache;

    private NewInput inputActions;

    /// <summary>
    /// Decks
    /// </summary>
    private readonly SyncList<SpellSO> _playerDeck = new SyncList<SpellSO>();
    private readonly SyncList<SpellSO> _enemyDeck = new SyncList<SpellSO>();

    [SerializeField]
    private readonly SyncList<SpellSO> _playerDeckInstance = new SyncList<SpellSO>();
    private readonly SyncList<SpellSO> _enemyDeckInstance = new SyncList<SpellSO>();

    protected virtual void Awake()
    {
        instance = this; 

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

    public bool ActiveAnimation = false;

    protected virtual void OnEnable()
    {
        inputActions.Enable(); 
    }

    protected void OnDisable()
    {
        inputActions.Disable(); 
    }

    public UnitInstance Target 
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


    public void SetOnlineBattleComponents(List<UnitSO> PlayerUnit, List<UnitSO> EnemyUnits, List<SpellSO> PlayerDeck, List<SpellSO> EnemyDeck)
    {
        _playerDeck.Clear(); 
        _playerDeck.AddRange(PlayerDeck);
        _playerUnit.Clear(); 
        _playerUnit.AddRange(PlayerUnit);
        _enemyDeck.Clear(); 
        _enemyDeck.AddRange(EnemyDeck);
        _enemyUnits.Clear(); 
        _enemyUnits.AddRange(EnemyUnits); 

        StartBattle(); 
    }

    protected virtual void SetBattleUnits()
    {
        _playerDeckInstance.AddRange(_playerDeck);
        _enemyDeckInstance.AddRange(_enemyDeck);

        UnitCollection();

        _turnOrder = TurnOrderCalc();
    }

    /// <summary>
    /// When the Battle Begins
    /// Summon Units
    /// Determine TurnOrder 
    /// Any Passives that take place on Turn Starts
    /// </summary>
    public virtual void StartBattle()
    {
        SetBattleUnits();
        #region Adding Listeners

        AddStarterListeners(_currentActingUnit); 
        AddActionRecurringlListeners(_currentActingUnit);
        _currentActingUnit = TurnOrder[0]; 
        #endregion
        StartBattle(); 
    }

    protected virtual void Update()
    {
        _playerUI.SetActive(_currentActingUnit.PlayerOwned && !ActiveAnimation);
    }

    /// <summary>
    /// Summons units from player and enemy inventory
    /// </summary>
    protected virtual void UnitCollection()
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
    /// Client Friendly Turn Start
    /// </summary>
    [ClientRpc]
    public void RPConStart()
    {
        OnTurnStart?.Invoke(); 
    }
    
    [Command(requiresAuthority =false)]
    public void BattleStart()
    {
        RPConStart(); 
    }

    /// <summary>
    /// Called When a unit selections an action and then completes that action
    /// </summary>
    public delegate void OnActionTakenDelegate(UnitInstance target);
    public OnActionTakenDelegate OnActionTaken; 

    [ClientRpc]
    public void RPConActionTaken(UnitInstance target)
    {
        OnActionTaken?.Invoke(target); 
    }

    /// <summary>
    /// Called when that action is completed but before the next unit has been chosen
    /// </summary>
    [HideInInspector]
    public UnityEvent OnTurnEnd;
    [ClientRpc]
    public void RPConEnd()
    {
        OnTurnEnd?.Invoke();
    }

    /// <summary>
    /// Called when a unit has died and is removed from play
    /// </summary>
    public delegate void OnDeathDelegate(UnitInstance target);
    [HideInInspector]
    public OnDeathDelegate OnUnitDeath;

    [ClientRpc]
    public void RPConUnitDies(UnitInstance target)
    {
        OnUnitDeath?.Invoke(target);
    }

    /// <summary>
    /// Called when a unit casts a spell 
    /// </summary>
    [HideInInspector]
    public UnityEvent SpellCast = new UnityEvent();
    [ClientRpc]
    public void RPConSpellcast()
    {
        SpellCast?.Invoke();
    }

    /// <summary>
    /// Called when unit uses AttackCommand((requiresAuthority = false)
    /// </summary>
    [HideInInspector]
    public UnityEvent OnAttack;
    [ClientRpc]
    public void RPConAttack()
    {
        OnAttack?.Invoke();
    }

    /// <summary>
    /// Called When Unit uses Pass Commmand
    /// </summary>
    [HideInInspector]
    public UnityEvent OnPass;
    [ClientRpc]
    public void RPConPass()
    {
        OnPass?.Invoke();
    }
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

    [ClientRpc]
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

    [ClientRpc]
    /// <summary>
    /// Testing simple turned based -> Static turns based on speed tiers, no ui representation
    /// Goes to next unit
    /// </summary>
    protected virtual void NextTurnInCycle(UnitInstance UnitForIndex)
    {
        int tempUnitIndex = _turnOrder.ToList().IndexOf(UnitForIndex);

        tempUnitIndex++; 

        if(tempUnitIndex >= _turnOrder.Length)
        {
            tempUnitIndex = 0;
        }

        unitIndex = tempUnitIndex; 
    }

    private void SetNextUnit(int oldUnit, int newUnit)
    {
        Debug.Log("Hook");
        _currentActingUnit = _turnOrder[newUnit];
        OnTurnStart.Invoke();

    }

    //ForTestingPurposes
    public void NextTurnButton() => NextTurnInCycle(_currentActingUnit); 

    [ClientRpc]
    public virtual void EndTurn()
    {
        ActionQueued = false;
        //Will need to remove any additional listeners
        ClearActionEvents();
        AddActionRecurringlListeners(_currentActingUnit);
        NextTurnInCycle(_currentActingUnit);
    }

    public virtual void AddStarterListeners(UnitInstance CurrentUnit)
    {
        OnTurnEnd.AddListener(() => EndTurn());


        OnTurnStart.AddListener(() =>
        {
            if (_enemyDeck.Count > 0)
            {
                DeckManager.DrawCard(false, _enemyDeckInstance);
            }

            if (_playerDeck.Count > 0)
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
            if (CurrentUnit != null && !CurrentUnit.isAlive)
            {
                OnUnitDeath += ctx => UnitDied(CurrentUnit);
                OnUnitDeath += ctx => VictoryCheck();
                OnUnitDeath.Invoke(CurrentUnit);
                _currentUnitAnimations.DieTrigger();
            }

        });
    }

    public virtual void AddActionRecurringlListeners(UnitInstance CurrentUnit)
    {
        //ReAdding Essential Listeners
        OnAttack.AddListener(() =>
        {
            instance._currentUnitAnimations.AttackTrigger();

        });

        OnPass.AddListener(() =>
        {
            //This shouldn't work
            CurrentUnit.CurrStats.ManaPips++;

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
    public void ClearActionQueue()
    {
        OnActionTaken = null; 
    }

    protected void ClearActionEvents()
    {
        OnAttack.RemoveAllListeners();
        SpellCast.RemoveAllListeners();
        OnPass.RemoveAllListeners();
    }

    //Used Externally so that the spell animation can play out
    public void EndTurnAfterSpellCast()
    {
        OnTurnEnd.Invoke(); 
    }

    /// <summary>
    /// For UI Attack Button
    /// </summary>
    [Command(requiresAuthority =false)]
    public void OnlineAttackButtonPress()
    {
        OnAttackMethod();
    }

    [ClientRpc]
    private void OnAttackMethod()
    {
        Debug.Log("Attacking");

        ClearActionQueue();

        OnAttack.AddListener(() => Target.TakeDaamge(CurrentUnit.CurrStats.Attack - Mathf.FloorToInt(Target.CurrStats.Defence / 2) + 1));

        OnActionTaken += ctx => RPConAttack();
        OnActionTaken += ctx => RPConEnd();

        if (Target != null)
        {
            ActiveAnimation = true;
            RPConActionTaken(Target);
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


        if (Unit == instance._currentActingUnit)
            OnTurnEnd.Invoke();

        Destroy(Unit);

    }

    /// <summary>
    /// For UI Pass Button
    /// </summary>
    [Command]
    public void OnlinePassButtonPress()
    {
        RPConPass();
    }

}