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

    /// <summary>
    /// Target of Unit Strike or Spell
    /// </summary>
    protected UnitInstance _targetCache;

    protected NewInput inputActions;

    [SerializeField]
    protected GameObject _playerUI;

    //TurnOrder of the Currently Acting Units
    protected UnitInstance[] _turnOrder;
    public List<UnitInstance> TurnOrder => _turnOrder.ToList();

    /// <summary>
    /// ActionQueued is marked when the player selects an option  
    /// </summary>
    public bool ActionQueued = false;

    //Currently Acting Unit
    protected UnitInstance _currentActingUnit;
    public UnitInstance CurrentUnit {
        get => _currentActingUnit;
        set => _currentActingUnit = value;
            }
    protected UnitAnimation _currentUnitAnimations => _currentActingUnit.UIA;

    [SerializeField]
    protected Transform _playerBattleSpace, _enemyBattleSpace;

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

    public bool ActiveAnimation = false;

    #region Unity Methods
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
                if (hit.transform.TryGetComponent(out IGameplayInteractable Unit))
                {
                    Unit.OnPress();
                }
            }
        };
    }

    protected virtual void Update()
    {
        if(_currentActingUnit != null)
            _playerUI.SetActive(_currentActingUnit.PlayerOwned && !ActiveAnimation);
    }

    protected virtual void OnEnable()
    {
        inputActions.Enable();
    }

    protected void OnDisable()
    {
        inputActions.Disable();
    }
    #endregion

    #region Virtual Methods
    protected virtual void SetBattleUnits() { }
    public virtual void StartBattle() { }
    protected virtual void UnitCollection() { }
    protected virtual UnitInstance[] TurnOrderCalc() { return new UnitInstance[0]; }
    protected virtual void NextTurnInCycle(UnitInstance UnitForIndex) { }
    public virtual void EndTurn() { }
    public virtual void AddStarterListeners(UnitInstance CurrentUnit) {
        OnTurnEnd.AddListener(() => EndTurn());
    }
    public virtual void AddActionRecurringlListeners(UnitInstance CurrentUnit) {
        //ReAdding Essential Listeners
        OnAttack.AddListener(() =>
        {
            _currentUnitAnimations.AttackTrigger();
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
    protected virtual void ClearActionEvents()
    {
        OnAttack.RemoveAllListeners();
        SpellCast.RemoveAllListeners();
        OnPass.RemoveAllListeners();
    }
    public virtual void EndTurnAfterSpellCast()
    {
        OnTurnEnd.Invoke();
    }
    #endregion

    #region Events and Delegates
    /// <summary>
    /// Called when a new unit begins it's phase
    /// </summary>
    [HideInInspector]
    public UnityEvent OnTurnStart;

    /// <summary>
    /// Called When a unit selections an action and then completes that action
    /// </summary>
    public delegate void OnActionTakenDelegate(UnitInstance target);
    public OnActionTakenDelegate OnActionTaken;

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
    public UnityEvent SpellCast = new UnityEvent();

    /// <summary>
    /// Called when unit uses AttackCommand((requiresAuthority = false)
    /// </summary>
    [HideInInspector]
    public UnityEvent OnAttack;

    /// <summary>
    /// Called When Unit uses Pass Commmand
    /// </summary>
    [HideInInspector]
    public UnityEvent OnPass;
    #endregion

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
}
