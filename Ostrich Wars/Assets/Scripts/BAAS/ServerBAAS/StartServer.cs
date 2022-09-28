#if UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab; 
using PlayFab.MultiplayerAgent.Model;
using Mirror;
using System;
using PlayFab.Networking;
using System.Linq; 

public class StartServer : MonoBehaviour
{
	private List<ConnectedPlayer> _connectedPlayers;
	public NetworkBehaviour OnlineBattle; 

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
		// get the port that the server will listen to
		// We *have to* do it on process mode, since there might be more than one game server instances on the same VM and we want to avoid port collision
		// On container mode, we can omit the below code and set the port directly, since each game server instance will run on its own network namespace. However, below code will work as well
		// we have to do that on process
		var connInfo = PlayFabMultiplayerAgentAPI.GetGameServerConnectionInfo();
		// make sure the ListeningPortKey is the same as the one configured in your Build settings (either on LocalMultiplayerAgent or on MPS)
		const string ListeningPortKey = "game_port";
		var portInfo = connInfo.GamePortsConfiguration.Where(x => x.Name == ListeningPortKey);
		if (portInfo.Count() > 0)
		{
			Debug.Log(string.Format("port with name {0} was found in GSDK Config Settings.", ListeningPortKey));
			UnityNetworkServer.Instance.Port = portInfo.Single().ServerListeningPort;
		}
		else
		{
			string msg = string.Format("Cannot find port with name {0} in GSDK Config Settings. If you are running locally, make sure the LocalMultiplayerAgent is running and that the MultiplayerSettings.json file includes correct name as a GamePort Name. If you are running this sample in the cloud, make sure you have assigned the correct name to the port", ListeningPortKey);
			Debug.LogError(msg);
			throw new Exception(msg);
		}

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
#endif