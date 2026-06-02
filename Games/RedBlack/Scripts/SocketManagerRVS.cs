using System.Collections;
using UnityEngine;
using SocketIOClient;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using SocketIOClient.Newtonsoft.Json;
using Core.Config;
using Features.Lobby.Integration;

public class SocketManagerRVS : MonoBehaviour
{
    public static SocketManagerRVS Instance { get; private set; }

    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    ManagerRedBlack manager;
    bool _isInitialFocus = true;
    public string currentRoundId;

    private void Awake()
    {
        manager = FindObjectOfType<ManagerRedBlack>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void OnDestroy()
    {
        if (socket != null)
        {
            socket.Disconnect();
            socket.Dispose();
            socket = null;
        }
        MainThreadDispatcher.DequeueAll();
    }
    public void Disconnect()
    {
        if (socket != null)
        {
            socket.Disconnect();
            socket.Dispose();
            socket = null;
        }
        MainThreadDispatcher.DequeueAll();
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private void OnApplicationPause(bool focus)
    {
        if (!focus)
        {
            if (!_isInitialFocus)
            {
                Debug.Log("Focused");
                MainThreadDispatcher.DequeueAll();
                //socket.Emit("GameStatusRequest");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else
        {
            socket.Disconnect();
            _isInitialFocus = false;
        }
    }

    void Start()
    {
        var uri = new Uri(ServerConfig.SocketUrl + "/redBlack");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", "UNITY" }
            },
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Connected to server");
            OnSocketConnected.Invoke();

        };

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("Disconnected from server");
        };

        socket.Connect();

        socket.On("gameResult", (response) =>
        {
            Debug.Log("result : " + response);
            MainThreadDispatcher.Enqueue(() =>
            {
                Result(response.ToString());
            });


        });



        socket.On("initGameStatus", (response) =>
        {

            MainThreadDispatcher.Enqueue(() =>
            {
                InitGameStatusUpdate(response.ToString());
            });


        });

        socket.On("startBetting", (response) =>// listening for change in game status
        {
            //is in Resting period = gameStatus;
            MainThreadDispatcher.Enqueue(() =>
            {
                StartBetting(response.ToString());
            });


        });
    }


    public void InitGameStatusUpdate(string response)
    {
        Debug.Log("response : " + response);

        JArray jsonArray = JArray.Parse(response);

        // Assuming the first element in the array is the object you need
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        string gamePhase = (string)gameStatus["gamePhase"];
        Debug.Log("game status: " + gamePhase);

        int remainingTime = (int)gameStatus["timeLeft"];
        Debug.Log("remaining time: " + remainingTime);

        JArray stringArray = (JArray)gameStatus["history"];

        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();

        long serverStartTime = (long)gameStatus["roundStartTime"];


        string[] characters = stringArray.ToObject<string[]>(); // Convert JArray to char[]
        manager.HistoryUpdate(characters);

        if (characters.Length != 0)
        {


            if (gamePhase != "Betting")
            {
                manager.EnableWaitingPanel();
            }
            else
            {
                if (remainingTime > 3)
                {
                    manager.StartBetting(remainingTime, serverStartTime, seeds);
                }
                else
                {
                    manager.EnableWaitingPanel();
                }
            }
        }



    }


    public void StartBetting(string response)
    {
        Debug.Log("start Betting : " + response);
        JArray jsonArray = JArray.Parse(response);

        JObject gameStatus = (JObject)jsonArray[0];

        int remainingTime = (int)gameStatus["timeLeft"];
        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();
        long roundStartTime = (long)gameStatus["roundStartTime"];


        manager.StartBetting(remainingTime, roundStartTime, seeds);
        currentRoundId = roundStartTime.ToString();




    }


    public void GameStatusUpdate(string response)
    {

        Debug.Log("my : " + response);
        JArray jsonArray = JArray.Parse(response);

        // Assuming the first element in the array is the object you need
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        bool gameInProgress = (bool)gameStatus["isRestingInit"];
        Debug.Log("game status: " + gameInProgress);

        int remainingTime = (int)gameStatus["timeLeft"];
        Debug.Log("remaining time: " + remainingTime);

        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();

        long serverStartTime = (long)gameStatus["roundStartTime"];


        if (gameInProgress)
        {

            manager.DeclareResult();

        }
        else
        {
        }
    }

    public void Result(string response)
    {
        JArray jsonArray = JArray.Parse(response);

        Debug.Log("idhar : " + response);
        // Assuming the first element in the array is the object you need
        JObject gameResult = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        string card = gameResult["resultCard"].ToString();
        manager.DisplayCard(card);
    }
    public void SendBetDataToServer(char betOn, float betAmount)
    {
        Debug.Log("sending bet data : " + betOn + " bet amount : " + betAmount);
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
            { "betOn", betOn },
            { "betAmount", betAmount }
        };

        string jsonData = JsonConvert.SerializeObject(data);
        Debug.Log("json : " + jsonData);
        socket.Emit("sendData", jsonData);
    }

    public void SendGameStatusRequest()
    {
        Debug.Log("Asking for game Status");
        socket.Emit("GameStatusRequest");
    }


}

