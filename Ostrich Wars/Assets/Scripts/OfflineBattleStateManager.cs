using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class OfflineBattleStateManager : BattleStateManager 
{
    ///Player Lists from their selected Inventories
    private List<UnitSO> _playerUnit = new List<UnitSO>();
    private List<UnitSO> _enemyUnits = new List<UnitSO>();
    //Units Currently on the field
    private List<UnitInstance> _activePlayerUnits = new List<UnitInstance>();
    private List<UnitInstance> _activeEnemyUnits = new List<UnitInstance>();

    public new List<UnitInstance> EnemiesActive => _activeEnemyUnits;
    public new List<UnitInstance> AllUnits
    {
        get
        {
            List<UnitInstance> UIL = new List<UnitInstance>();

            UIL.AddRange(_activePlayerUnits);
            UIL.AddRange(_activeEnemyUnits);
            return UIL;
        }
    }

    private UnitInstance _currentUnit;
    public new UnitInstance CurrentUnit => _currentUnit;
    private UnitAnimation _currentUnitAnimations => _currentUnit.UIA;

    [SerializeField]
    private List<SpellSO> _playerDeck = new List<SpellSO>();
    [SerializeField]
    private List<SpellSO> _enemyDeck = new List<SpellSO>();
    [SerializeField]
    private List<SpellSO> _playerDeckInstance = new List<SpellSO>();
    [SerializeField]
    private List<SpellSO> _enemyDeckInstance = new List<SpellSO>();

    [SerializeField]
    private EnemyAI _aIPlayer;
    public EnemyAI SetAI { set => _aIPlayer = value; }

    protected override void Awake()
    {
        base.Awake();
        instance = this; 
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetOfflineBattleComponents(
            GameManager.instance.Player.CurrentTeam,
            GameManager.instance.Other.CurrentTeam,
            GameManager.instance.Player.CurrenDeck,
            GameManager.instance.Other.CurrenDeck
            );
    }

    protected override void Update()
    {
        _playerUI.SetActive(_currentUnit.PlayerOwned && !ActiveAnimation);
    }

    protected override void UnitCollection()
    {
        for (int UnitInterator = 0; UnitInterator <= 3; UnitInterator++)
        {

            UnitInstance newPlayerUnit = UnitInterator < _playerUnit.Count || UnitInterator == 0 ?
                Instantiate(_playerUnit[UnitInterator].Prefab.GetComponent<UnitInstance>(), _playerBattleSpace.GetChild(UnitInterator)) : null;

            UnitInstance newEnemyUnit = UnitInterator < _enemyUnits.Count || UnitInterator == 0 ?
                Instantiate(_enemyUnits[UnitInterator].Prefab.GetComponent<UnitInstance>(), _enemyBattleSpace.GetChild(UnitInterator)) : null;


            if (newPlayerUnit != null)
            {
                newPlayerUnit.PlayerOwned = true;
                newPlayerUnit.UnitBase = _playerUnit[UnitInterator];
                _activePlayerUnits.Add(newPlayerUnit);
            }

            if (newEnemyUnit != null)
            {
                newEnemyUnit.UnitBase = _enemyUnits[UnitInterator];
                _activeEnemyUnits.Add(newEnemyUnit);
            }
        }
    }


    public void SetOfflineBattleComponents(List<UnitSO> PlayerUnit, List<UnitSO> EnemyUnits, List<SpellSO> PlayerDeck, List<SpellSO> EnemyDeck)
    {
        _playerDeck = PlayerDeck; 
        _playerUnit = PlayerUnit;
        _enemyDeck = EnemyDeck;
        _enemyUnits = EnemyUnits;

        StartBattle();
    }


    public void OfflineAttackButtonPress()
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

    public void OfflinePassButtonPress()
    {
        OnPass.Invoke();
    }

    private UnitInstance[] TurnOrderCalc()
    {
        List<UnitInstance> AllCurrentUnits = new List<UnitInstance>();
        AllCurrentUnits.AddRange(_activeEnemyUnits);
        AllCurrentUnits.AddRange(_activePlayerUnits);
        return AllCurrentUnits.OrderByDescending(u => u.CurrStats.Speed).ToArray();
    }

    protected override void SetBattleUnits()
    {
        _playerDeckInstance.AddRange(_playerDeck);
        _enemyDeckInstance.AddRange(_enemyDeck);

        UnitCollection();

        _turnOrder = TurnOrderCalc();
        _currentUnit = _turnOrder[0];

    }

    public override void StartBattle()
    {
        SetBattleUnits();

        AddStarterListeners(_currentUnit);
        AddActionRecurringlListeners(_currentUnit);
        OnTurnStart.Invoke(); 
    }

    public override void AddStarterListeners(UnitInstance CurrentUnit)
    {
        base.AddStarterListeners(CurrentUnit);
        OnTurnStart.AddListener(() =>
        {
            if (!_currentUnit.PlayerOwned)
            {
                Debug.Log("EnemyTurn");
                _aIPlayer?.ExecuteDescision(_currentUnit, _activeEnemyUnits.Concat(_activePlayerUnits).ToList());
            }
            else
            {
                ActiveAnimation = false;
            }
        });
    }

    protected override void NextTurnInCycle(UnitInstance UnitForIndex)
    {
        int unitIndex = _turnOrder.ToList().IndexOf(UnitForIndex);

        unitIndex++;

        if (unitIndex >= _turnOrder.Length)
        {
            unitIndex = 0;
        }

        _currentUnit = _turnOrder[unitIndex];

        OnTurnStart.Invoke();
    }


}
