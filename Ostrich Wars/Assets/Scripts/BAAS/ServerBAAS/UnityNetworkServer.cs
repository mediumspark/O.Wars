#if UNITY_SERVER
namespace PlayFab.Networking
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Mirror;
    using UnityEngine.Events;

    public class UnityNetworkServer : NetworkManager
    {
        public static UnityNetworkServer Instance { get; private set; }

        public PlayerEvent OnPlayerAdded = new PlayerEvent();
        public PlayerEvent OnPlayerRemoved = new PlayerEvent();

        public int MaxConnections = 100;
        public int Port = 7777; // overwritten by the code in AgentListener.cs

        public List<UnityNetworkConnection> Connections
        {
            get { return _connections; }
            private set { _connections = value; }
        }
        private List<UnityNetworkConnection> _connections = new List<UnityNetworkConnection>();

        public class PlayerEvent : UnityEvent<string> { }

        // Use this for initialization
        public override void Awake()
        {
            base.Awake();
            Instance = this;
            NetworkServer.RegisterHandler<ReceiveAuthenticateMessage>(OnReceiveAuthenticate);
        }

        public void StartListen()
        {
            this.GetComponent<TelepathyTransport>().port = (ushort)Port;
            NetworkServer.Listen(MaxConnections);
        }

        public override void OnApplicationQuit()
        {
            NetworkServer.Shutdown();
            base.OnApplicationQuit();
        }

        private void OnReceiveAuthenticate(NetworkConnection nconn, ReceiveAuthenticateMessage message)
        {
            var conn = _connections.Find(c => c.ConnectionId == nconn.connectionId);
            if (conn != null)
            {
                conn.PlayFabId = message.PlayFabId;
                conn.IsAuthenticated = true;
                OnPlayerAdded.Invoke(message.PlayFabId);
            }
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);

            Debug.LogWarning("Client Connected");
            var uconn = _connections.Find(c => c.ConnectionId == conn.connectionId);
            if (uconn == null)
            {
                _connections.Add(new UnityNetworkConnection()
                {
                    Connection = conn,
                    ConnectionId = conn.connectionId,
                    LobbyId = PlayFabMultiplayerAgentAPI.SessionConfig.SessionId
                });
            }
        }

        public override void OnServerError(NetworkConnectionToClient conn, Exception ex)
        {
            base.OnServerError(conn, ex);

            Debug.Log(string.Format("Unity Network Connection Status: exception - {0}", ex.Message));
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);

            var uconn = _connections.Find(c => c.ConnectionId == conn.connectionId);
            if (uconn != null)
            {
                if (!string.IsNullOrEmpty(uconn.PlayFabId))
                {
                    OnPlayerRemoved.Invoke(uconn.PlayFabId);
                }
                _connections.Remove(uconn);
            }
        }
    }

    
}
#endif