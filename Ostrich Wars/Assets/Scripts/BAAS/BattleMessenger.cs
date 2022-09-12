using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using TMPro;
using System.Linq;
using Mirror; 
using PlayFab;
using UnityEngine.InputSystem; 

public class BattleMessenger : NetworkBehaviour
{
    public static BattleMessenger instance;

    public BattleStateManager BM = BattleStateManager.instance; 

    /// <summary>
    /// Player's BattleManager
    /// </summary>
    public SyncList<UnitInstance> Units => BattleStateManager.instance.AllUnits; 
    
    private void Awake()
    {
        instance = this;
        //GameManager.instance.OnBeginBattle();
        //BM.OnAttack.AddListener(RecieveAction);
    }

    private void Update()
    {
        if (Keyboard.current.aKey.isPressed)
        {
            SubmitActionCommand(); 
        }
    }

    [ClientRpc]
    public void SubmitAttackCommand()
    {
        BM.OnlineAttackButtonPress(); 
    }

    [Command(requiresAuthority = false)]
    public void SubmitActionCommand()
    {
        Debug.Log("Sync Units is Command");
        RecieveAction(); 
    } //Server commits action on both clients

    [ClientRpc]
    void RecieveAction()
    {
    } //Sends action to the server
}