using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using UnityEngine.InputSystem; 

public class OnlineBattleStateManager : BattleStateManager
{
    public UnityEngine.UI.Button AttakButton, Passbutton; 

    ///Player Lists from their selected Inventories
    private readonly SyncList<UnitSO> _playerUnit = new SyncList<UnitSO>();
    private readonly SyncList<UnitSO> _enemyUnits = new SyncList<UnitSO>();
    //Units Currently on the field
    private readonly SyncList<UnitInstance> _activePlayerUnits = new SyncList<UnitInstance>();
    
    private readonly SyncList<UnitInstance> _activeEnemyUnits = new SyncList<UnitInstance>();
    //tmp
    public SyncList<UnitInstance> p_Units => _activePlayerUnits;
    public SyncList<UnitInstance> e_Units => _activeEnemyUnits;

    private readonly new SyncList<UnitInstance> _turnOrder = new SyncList<UnitInstance>();
    public new SyncList<UnitInstance> TurnOrder => _turnOrder;

    public SyncList<UnitInstance> EnemiesActive => _activeEnemyUnits;
    public SyncList<UnitInstance> AllUnits  { 
        get {
            SyncList<UnitInstance> UIL = new SyncList<UnitInstance>();

            UIL.AddRange(_activePlayerUnits);
            UIL.AddRange(_activeEnemyUnits);
            return UIL; 
        }
    }

    [SyncVar]
    public int unitIndex = 0;

    [SyncVar]
    public int targetIndex;

    /// <summary>
    /// Decks
    /// </summary>
    private readonly SyncList<SpellSO> _playerDeck = new SyncList<SpellSO>();
    private readonly SyncList<SpellSO> _enemyDeck = new SyncList<SpellSO>();

    private readonly SyncList<SpellSO> _playerDeckInstance = new SyncList<SpellSO>();
    private readonly SyncList<SpellSO> _enemyDeckInstance = new SyncList<SpellSO>();

    protected override void Awake()
    {
        instance = this;

        inputActions = new NewInput();
    }

    public void SetOnlineBattleComponents(List<UnitSO> PlayerUnit, List<UnitSO> EnemyUnits, List<SpellSO> PlayerDeck, List<SpellSO> EnemyDeck)
    {
        Debug.Log("Set Lists");
        
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

    protected override void SetBattleUnits()
    {
        //Debug.Log("BattleUnits Set");
        _playerDeckInstance.AddRange(_playerDeck);
        _enemyDeckInstance.AddRange(_enemyDeck);

        UnitCollection();
        
        _turnOrder.Clear();
        units.Clear(); 

        _turnOrder.AddRange(TurnOrderCalc().ToList());
        units.AddRange(_turnOrder); 
        _currentActingUnit = _turnOrder[unitIndex];

    } 

    private void AddListeners()
    {
        AddStarterListeners(_currentActingUnit);
        AddActionRecurringlListeners(_currentActingUnit);
    }

    /// <summary>
    /// When the Battle Begins
    /// Summon Units
    /// Determine TurnOrder 
    /// Any Passives that take place on Turn Starts
    /// </summary>
    public override void StartBattle()
    {
        SetBattleUnits();
        AddListeners(); 
    }
    
    /// <summary>
    /// Summons units from player and enemy inventory
    /// </summary>
    protected override void UnitCollection()
    {
        for (int UnitInterator = 0; UnitInterator < 3; UnitInterator++)
        {
            if(_playerBattleSpace.GetChild(UnitInterator).childCount == 1)
            {
                CreateUnit(UnitInterator, true, _playerBattleSpace.GetChild(UnitInterator), _playerUnit[UnitInterator].Prefab.GetComponent<UnitInstance>());
            }


            if (_enemyBattleSpace.GetChild(UnitInterator).childCount == 1)
            {
                CreateUnit(UnitInterator, false, _enemyBattleSpace.GetChild(UnitInterator), _enemyUnits[UnitInterator].Prefab.GetComponent<UnitInstance>());
            }
        }
    }


    public void CreateUnit(int position, bool playerOwned, Transform  parent, UnitInstance Unit)
    {
        UnitInstance newPlayerUnit = position < _playerUnit.Count || position == 0 ?
            Instantiate(Unit, parent) : null;

        if (newPlayerUnit != null)
        {
            newPlayerUnit.PlayerOwned = playerOwned;
            newPlayerUnit.UnitBase = _playerUnit[position];
            _activePlayerUnits.Add(newPlayerUnit);
        }
    }

    public UnitInstance[] o_TurnOrderCalc() => TurnOrderCalc(); 

    public List<UnitInstance> units; 
    /// <summary>
    /// Testing simple turned based -> Static turns based on speed tiers, no ui representation
    /// Creates the turn list
    /// </summary>
    protected override UnitInstance[] TurnOrderCalc()
    {
        List<UnitInstance> Units = new List<UnitInstance>();
        Units.AddRange(_activeEnemyUnits); 
        Units.AddRange(_activePlayerUnits);

        Units.OrderByDescending(u => u.CurrStats.Speed).ToArray();
        for (int i = 0; i < Units.Count; i++)
        {
            if (i + 1 < Units.Count && Units[i].CurrStats.Speed == AllUnits[i + 1].CurrStats.Speed)
            {
                int Roll_1 = Random.Range(0, 1000);
                int Roll_2 = Random.Range(0, 1000);
                if (Roll_1 < Roll_2)
                {
                    UnitInstance tmp = Units[i];
                    Units[i] = Units[i + 1];
                    Units[i + 1] = tmp;
                }
            }
        }
        return Units.ToArray(); 
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

    protected override void Update()
    {
        if(CurrentUnit != null)
        {
            _playerUI.SetActive(_currentActingUnit.PlayerOwned && !ActiveAnimation);
            CurrentUnit = TurnOrder[unitIndex];
        }
    }

    public override void EndTurn()
    {
        ActionQueued = false;
        //Will need to remove any additional listeners
        ClearActionEvents();
        AddActionRecurringlListeners(_currentActingUnit);
        NextTurnInCycle(_currentActingUnit);
    }

    public override void AddStarterListeners(UnitInstance CurrentUnit)
    {
        base.AddStarterListeners(CurrentUnit);
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
                OnUnitDeath += ctx => RpcUnitDied(Target);
                OnUnitDeath += ctx => VictoryCheck();
                Target.UIA.DieTrigger();
                OnUnitDeath.Invoke(Target);
            }

            if (CurrentUnit != null && !CurrentUnit.isAlive)
            {
                OnUnitDeath += ctx => RpcUnitDied(CurrentUnit);
                OnUnitDeath += ctx => VictoryCheck();
                OnUnitDeath.Invoke(CurrentUnit);
                _currentUnitAnimations.DieTrigger();
            }
        });
    }

    /// <summary>
    /// On unit death the unit is removed from the field, from the turn order
    /// and it's gameobject is destroyed
    /// </summary>
    /// <param name="Unit"></param>
    public void RpcUnitDied(UnitInstance Unit)
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


        if (Unit == instance.CurrentUnit)
            OnTurnEnd.Invoke();

        Destroy(Unit);
    }

    /// <summary>
    /// For UI Pass Button
    /// </summary>
    public void OnlinePassButtonPress()
    {
        OnPass.Invoke();
    }

}