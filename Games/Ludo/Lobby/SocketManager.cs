using UnityEngine;
using SocketIOClient;
using System;
using System.Collections.Generic;
using SocketIOClient.Newtonsoft.Json;
using Newtonsoft.Json;
using Features.Lobby.Integration;

public class SocketManager : MonoBehaviour
{
    public static SocketManager Instance;

    public static SocketIOUnity socket;
    public string serverUrl = "http://127.0.0.1:3000";

    private Action onConnectedCallback;
    public ServerResponse serverResponseRef;
    public LobbyUI lobbyUI;

    void Awake()
    {
        serverUrl = Const.Server_Url;
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        serverResponseRef = FindFirstObjectByType<ServerResponse>();
        InitSocket();
    }

    public void Start()
    {
        lobbyUI = FindFirstObjectByType<LobbyUI>();
    }

    private void InitSocket()
    {
        Debug.Log("initialized");
        var uri = new Uri(serverUrl);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", "UNITY" }
            },
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.On("test", (r) => { Debug.Log("test data : " + r); });

        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Connected to Socket.IO server!");
            LocalPlayer.playerId = socket.Id;
            LocalPlayer.playerId = socket.Id;
            LocalPlayer.profilePic = socket.Id;
            GameLiveData.instance.mySocketId = socket.Id;
            onConnectedCallback?.Invoke();
        };

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.LogWarning("Disconnected from Socket.IO server.");
        };



        socket.OnConnected += (sender, e) =>
        {
            socket.OnUnityThread("roomCreated", response =>
            {
                Debug.Log("roomCreated triggered!");
            });
        };


        socket.OnError += (sender, e) =>
        {
            Debug.LogError("Socket Error: " + e);
        };

        socket.On("test", (response) =>// listening for change in game status
        {
            Debug.Log("Socket test successful with : " + response.ToString());
        });

        serverResponseRef.InitializeListners(socket);
    }

    public void ConnectToServer(Action onConnected = null)
    {
        Debug.Log("wanna connect to server");
        onConnectedCallback = onConnected;

        if (!socket.Connected)
        {
            socket.Connect();
        }
        else
        {
            onConnected?.Invoke();
        }
    }

    public void CreateAndJoinRoom()
    {
        Debug.LogError("yahha hum json serialize nahi kr rhe hai data ko");
        Debug.Log("creating room with : " + socket.Id);
        var data = new Dictionary<string, object>
        {
            {"playerId", BootstrapLobbyAdapter.GetUserId()},
            { "profile", UserDetail.profileImageIndex},
            {"playerWallet", BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f},
            {"playerName", UserDetail.UserName},
            {"roomAmount", 99},
            { "roomLimit", 4}
        };
        Emit("createRoom", data);
    }

    public void JoinRoom(string roomId)
    {
        Debug.Log("sent join Req to server " + roomId);

        var data = new Dictionary<string, object>
        {
            {"roomId", roomId},
            { "playerId", BootstrapLobbyAdapter.GetUserId()},
            { "profile", UserDetail.profileImageIndex},
            {"playerWallet", BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f},
            {"playerName", UserDetail.UserName},
        };
        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("joinRoom", jsonData);
    }

    public void Start_Matchmaking(int roomAmount)
    {
        Debug.Log("sent join Req to server " + BootstrapLobbyAdapter.GetUserId());

        var data = new Dictionary<string, object>
        {
            {"playerId", BootstrapLobbyAdapter.GetUserId()},
            {"profile", UserDetail.profileImageIndex},
            {"playerWallet", BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f},
            {"playerName", UserDetail.UserName},
            {"playersCount", GameLiveData.instance.roomLength},
            {"roomAmount", roomAmount}
        };

        socket.Emit(ServerRequestApi.MATCH_MAKING.ToString(), data);
    }

    public void LeaveGame()
    {
        Debug.Log("leaving game with : " + socket.Id);
        socket.Emit(ServerRequestApi.LEAVE_MATCH.ToString());
    }

    public void Emit(string eventName, object data = null)
    {
        if (data != null)
            socket.Emit(eventName, data);
        else
            socket.Emit(eventName);
    }

    public void On(string eventName, Action<SocketIOResponse> callback)
    {
        socket.On(eventName, callback);
    }

    public void OnUnityThread(string eventName, Action<SocketIOResponse> callback)
    {
        socket.OnUnityThread(eventName, callback);
    }

    public void Disconnect()
    {
        socket.Disconnect();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }
}
