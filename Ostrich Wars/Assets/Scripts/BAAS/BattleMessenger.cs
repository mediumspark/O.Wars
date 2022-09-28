using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using TMPro;
using System.Linq;
using Mirror; 
using PlayFab;
using UnityEngine.InputSystem;
using UnityEngine.UI; 

public class BattleMessenger : NetworkBehaviour
{
    public OnlineBattleStateManager BM;

    public List<UnitInstance> Turn_Order; 

    NewInput inputActions;

    private void OnGUI()
    {
        if (GUILayout.Button("Say YO!")){
            Debug.Log("Yo");
        }
    }

    private void Awake()
    {
        inputActions = new NewInput(); 
        BM = BattleStateManager.instance as OnlineBattleStateManager;

        inputActions.NormalEvent.Select.performed += ctx =>
        {
            RaycastHit hit;
            Ray publicWorldPos = GameManager.CachedCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(publicWorldPos, out hit))
            {
                if (hit.transform.TryGetComponent(out IGameplayInteractable Unit) && BM.CurrentUnit.PlayerOwned)
                {
                    Unit.OnPress();
                    CmdAssignTarget(BM.TurnOrder.ToList().IndexOf(BM.Target));
                }
            }
        };

        BM.AttakButton.onClick.AddListener(AttackButtonCommand);
        BM.Passbutton.onClick.AddListener(CmdPass);
        GameManager.instance.OnBeginBattle();

    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        //Sync Turn order
        CmdSyncTurnOrder();
    }

    private void OnEnable()
    {
        inputActions.Enable(); 
    }

    private void OnDisable()
    {
        inputActions.Disable(); 
    }

    [Command(requiresAuthority =false)]
    private void CmdSyncTurnOrder()
    {
        RpcSyncTurnOrder(); 
    }

    [ClientRpc]
    private void RpcSyncTurnOrder()
    {
        List<UnitInstance> _turnOrder = new List<UnitInstance>(); 
        _turnOrder.AddRange(BM.TurnOrder);
        BM.TurnOrder.Clear();
        BM.TurnOrder.AddRange(_turnOrder);
        BM.CurrentUnit = _turnOrder[0];
        Turn_Order = _turnOrder; 
        Debug.Log("Sync List");
    }
    
    [Command(requiresAuthority = false)]
    private void CmdAssignTarget(int index)
    {
        RpcAssignTarget(index); 
    }

    [ClientRpc]
    private void RpcAssignTarget(int index)
    {
        BM.targetIndex = index;
        BM.Target = BM.TurnOrder[BM.targetIndex]; 
    }

    [ClientRpc]
    private void OnAttackMethod()
    {
        Debug.Log("Attacking");

        BM.ClearActionQueue();

        BM.OnAttack.AddListener(() => BM.Target.TakeDaamge(BM.CurrentUnit.CurrStats.Attack - Mathf.FloorToInt(BM.Target.CurrStats.Defence / 2) + 1));

        BM.OnActionTaken += ctx => RPConAttack();
        BM.OnActionTaken += ctx => RPConEnd();

        if (BM.Target != null)
        {
            BM.ActiveAnimation = true;
            RPConActionTaken(BM.Target);
        }
        else
            Debug.Log("Select Target");
        CmdNextTurn(); 
    }

    [Command(requiresAuthority =false)]
    private void CmdNextTurn() => RpcNextTurn(); 

    [ClientRpc]
    public void RpcNextTurn()
    {
        BM.ActionQueued = false;
        BM.ClearActionQueue();
        BM.AddActionRecurringlListeners(BM.CurrentUnit); 

        BM.unitIndex++;
        //Debug.Log($"Past Unit {BM.unitIndex} aka {BM.CurrentUnit.name}");
        if (BM.unitIndex >= BM.TurnOrder.Count)
        {
            BM.unitIndex = 0;
        }
        //Debug.Log($"Present {BM.unitIndex} aka {BM.CurrentUnit.name}");
    }


    [Command(requiresAuthority = false)]
    public void AttackButtonCommand()
    {
        OnAttackMethod(); 
    }

    #region Event RPCs  
    /// <summary>
    /// Client Friendly Turn Start
    /// </summary>
    public void RPConStart()
    {
        BM.OnTurnStart?.Invoke();
    }
    
    public void BattleStart()
    {
        RPConStart();
    }

    public void RPConActionTaken(UnitInstance target)
    {
        BM.OnActionTaken?.Invoke(target);
    }

    public void RPConEnd()
    {
        BM.OnTurnEnd?.Invoke();
    }

    public void RPConUnitDies(UnitInstance target)
    {
        BM.OnUnitDeath?.Invoke(target);
    }

    public void RPConSpellcast()
    {
        BM.SpellCast?.Invoke();
    }

    public void RPConAttack()
    {
        BM.OnAttack?.Invoke();
    }

    public void RPConPass()
    {
        BM.OnPass?.Invoke();
    }

    [Command(requiresAuthority =false)]
    public void CmdPass()
    {
        RPConPass(); 
    }
    #endregion

}