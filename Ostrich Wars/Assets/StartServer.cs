using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ServerModels;
using PlayFab; 
using PlayFab.MultiplayerAgent.Model;
using Mirror;
using System;
using PlayFab.Networking; 

public class StartServer : MonoBehaviour
{
	private List<ConnectedPlayer> _connectedPlayers;

	public UnityNetworkServer UNetServer;

	private bool playFabDebugging = false; 
	// Start is called before the first frame update
	void Start()
    {
		StartRemoteServer(); 
    }

	private void StartRemoteServer()
	{
		Debug.Log("[ServerStartUp].StartRemoteServer");
		_connectedPlayers = new List<ConnectedPlayer>();
		PlayFabMultiplayerAgentAPI.Start();
		PlayFabMultiplayerAgentAPI.ReadyForPlayers();
		PlayFabMultiplayerAgentAPI.IsDebugging = playFabDebugging;
		PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
		PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
		PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
		PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;

		UNetServer.OnPlayerAdded.AddListener(OnPlayerAdded);
		UNetServer.OnPlayerRemoved.AddListener(OnPlayerRemoved);

		StartCoroutine(ReadyForPlayers());
		StartCoroutine(ShutdownServerInXTime());
	}

	private void OnMaintenance(DateTime? NextScheduledMaintenanceUtc)
	{
		Debug.LogFormat("Maintenance scheduled for: {0}", NextScheduledMaintenanceUtc.Value.ToLongDateString());
		foreach (var conn in UNetServer.Connections)
		{
			conn.Connection.Send(new MaintenanceMessage()
			{
				ScheduledMaintenanceUTC = (DateTime)NextScheduledMaintenanceUtc
			}, CustomGameServerMessageTypes.ShutdownMessage);
		}
	}
	
	private void CheckPlayerCountToShutdown()
	{
		if (_connectedPlayers.Count <= 0)
		{
			StartShutdownProcess();
		}
	}

	private void OnPlayerAdded(string playfabId)
	{
		_connectedPlayers.Add(new ConnectedPlayer(playfabId));
		PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
	}

	private void OnPlayerRemoved(string playfabId)
	{
		ConnectedPlayer player = _connectedPlayers.Find(x => x.PlayerId.Equals(playfabId, StringComparison.OrdinalIgnoreCase));
		_connectedPlayers.Remove(player);
		PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
		CheckPlayerCountToShutdown();
	}

	IEnumerator ReadyForPlayers()
	{
		yield return new WaitForSeconds(.5f);
		PlayFabMultiplayerAgentAPI.ReadyForPlayers();
	}

	IEnumerator ShutdownServerInXTime()
	{
		yield return new WaitForSeconds(300f);
		StartShutdownProcess();
	}


	private void OnShutdown()
	{
		StartShutdownProcess();
	}

	private void StartShutdownProcess()
	{
		Debug.Log("Server is shutting down");
		foreach (var conn in UNetServer.Connections)
		{
			conn.Connection.Send(new ShutdownMessage(), CustomGameServerMessageTypes.ShutdownMessage);
		}
		StartCoroutine(ShutdownServer());
	}

	private void OnServerActive()
	{
		UNetServer.StartServer();
		Debug.Log("Server Started From Agent Activation");
	}

	IEnumerator ShutdownServer()
	{
		yield return new WaitForSeconds(5f);
		Application.Quit();
	}

	public struct MaintenanceMessage : NetworkMessage
	{
		public DateTime ScheduledMaintenanceUTC;
	}

	private void OnAgentError(string error)
	{
		Debug.Log(error);
	}
}
