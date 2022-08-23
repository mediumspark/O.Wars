using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using TMPro;
using System.Linq;
using Unity.Netcode;
using PlayFab; 

public class BattleToServerMessenger : NetworkBehaviour
{
    public static BattleToServerMessenger instance;

    public BattleStateManager BM; 

    /// <summary>
    /// Player's BattleManager
    /// </summary>
    public List<UnitInstance> Units => BattleStateManager.instance.AllUnits; 
    
    private void Awake()
    {
        instance = this;
    }


    public void ConnectRemoteClient()
    {
    }


    [ServerRpc(RequireOwnership = false)]
    public void SubmitActionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        if (IsOwner)
        {
            BattleStateManager.instance.OnActionTaken.Invoke(BattleStateManager.instance.Target);
            RecieveNewStats();
        }
    }

    void RecieveNewStats()
    {
        Debug.Log("Sync Units");
    }

    void OnError(PlayFabError error)
    {
        Debug.LogWarning(error.GenerateErrorReport());
    }

}