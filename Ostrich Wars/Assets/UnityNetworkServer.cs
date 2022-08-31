using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

namespace PlayFab.Networking
{
    public class UnityNetworkServer : MonoBehaviour
    {
		public PlayerEvent OnPlayerAdded = new PlayerEvent();
		public PlayerEvent OnPlayerRemoved = new PlayerEvent();

		public int MaxConnections = 100;
		public int Port = 7777;

		public NetworkManager _networkManager;

		public List<UnityNetworkConnection> Connections
		{
			get { return _connections; }
			private set { _connections = value; }
		}
		private List<UnityNetworkConnection> _connections = new List<UnityNetworkConnection>();

		public class PlayerEvent : UnityEvent<string> { }

		void Awake()
		{
			AddRemoteServerListeners();
		}

		private Action<NetworkConnectionToClient, ErrorMessage> OnError; 
		private Action<NetworkConnectionToClient, ConnectMessage> OnConnect; 
		private Action<NetworkConnectionToClient, DisconnectMessage> OnDisconnect; 
		private Action<NetworkConnectionToClient, ReceiveAuthenticateMessage> Authenticate; 

		private void AddRemoteServerListeners()
		{
			OnError += (a,b)=> OnServerError();
			OnConnect += (a,b)=> OnServerConnect(a);
			OnDisconnect += (a,b)=> OnServerDisconnect(a);
			Authenticate += (a, b) => OnReceiveAuthenticate(a); 

			Debug.Log("[UnityNetworkServer].AddRemoteServerListeners");
			NetworkServer.RegisterHandler(OnConnect, false); //Connect
			NetworkServer.RegisterHandler(OnDisconnect, false); //Disconnect
			NetworkServer.RegisterHandler(OnError, false); //Error
			NetworkServer.RegisterHandler(Authenticate,false); //On Authenticate
		}

		public void StartServer()
		{
			NetworkServer.Listen(Port);
		}

		private void OnApplicationQuit()
		{
			NetworkServer.Shutdown();
		}

		private void OnReceiveAuthenticate(NetworkConnectionToClient netMsg)
		{
			byte[] nil = new byte[1];
			NetworkReader reader = new NetworkReader(nil);

			var conn = _connections.Find(c => c.ConnectionId == netMsg.connectionId);
			if (conn != null)
			{
				var message = reader.Read<ReceiveAuthenticateMessage>();
				conn.PlayFabId = message.PlayFabId;
				conn.IsAuthenticated = true;
				OnPlayerAdded.Invoke(message.PlayFabId);
			}
		}

		private void OnServerConnect(NetworkConnectionToClient netMsg)
		{
			Debug.LogWarning("Client Connected");
			var conn = _connections.Find(c => c.ConnectionId == netMsg.connectionId);
			if (conn == null)
			{
				_connections.Add(new UnityNetworkConnection()
				{
					Connection = netMsg,
					ConnectionId = netMsg.connectionId,
					LobbyId = PlayFabMultiplayerAgentAPI.SessionConfig.SessionId
				});
			}
		}

		private void OnServerError()
		{
			byte[] nil = new byte[1]; 
			NetworkReader netMsg = new NetworkReader(nil);

			try
			{
				var error = netMsg.Read<ErrorMessage>();
				if (error.value != 0)
				{
					Debug.Log(string.Format("Unity Network Connection Status: code - {0}", error.value));
				}
			}
			catch (Exception)
			{
				Debug.Log("Unity Network Connection Status, but we could not get the reason, check the Unity Logs for more info.");
			}
		}

		private void OnServerDisconnect(NetworkConnectionToClient netMsg)
		{
			var conn = _connections.Find(c => c.ConnectionId == netMsg.connectionId);
			if (conn != null)
			{
				if (!string.IsNullOrEmpty(conn.PlayFabId))
				{
					OnPlayerRemoved.Invoke(conn.PlayFabId);
				}
				_connections.Remove(conn);
			}
		}

	}

	[Serializable]
	public class UnityNetworkConnection
	{
		public bool IsAuthenticated;
		public string PlayFabId;
		public string LobbyId;
		public int ConnectionId;
		public NetworkConnection Connection;
	}

	public struct CustomGameServerMessageTypes
	{
		public const short ReceiveAuthenticate = 900;
		public const short ShutdownMessage = 901;
		public const short MaintenanceMessage = 902;
	}

	public struct ReceiveAuthenticateMessage : NetworkMessage
	{
		public string PlayFabId;
	}

	public struct ShutdownMessage : NetworkMessage { }
	public struct ConnectMessage : NetworkMessage{  }
	public struct DisconnectMessage : NetworkMessage{  }

	public struct ErrorMessage : NetworkMessage
	{
		public byte value;

		public ErrorMessage(byte v, NetworkReader r, NetworkConnection con)
		{
			value = v;
		}

	}
}