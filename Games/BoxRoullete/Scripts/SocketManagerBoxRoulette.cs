using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Config;
using Features.Lobby.Integration;

public class SocketManagerBoxRoulette : MonoBehaviour
{
    public static SocketManagerBoxRoulette Instance { get; private set; }

    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    ManagerBRoullete manager;
    bool _isInitialFocus = true;
    public string roomName;
    public string currentRoundId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        manager = FindObjectOfType<ManagerBRoullete>();

        var uri = new Uri(ServerConfig.SocketUrl + "/boxRoulette");
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
            socket.Emit("joinRoom", roomName);
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


    public void InitGameStatusUpdate(string response)
    {
        Debug.Log("init gameStatus : " + response);
        JArray jsonArray = JArray.Parse(response);
        JObject gameStatus = (JObject)jsonArray[0];
        string gamePhase = (string)gameStatus["gamePhase"];

        int remainingTime = (int)gameStatus["timeLeft"];

        JArray stringArray = (JArray)gameStatus["history"];

        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();

        long serverStartTime = (long)gameStatus["roundStartTime"];

        int[] winNum = stringArray.ToObject<int[]>(); // Convert JArray to char[]
        manager.HistoryUpdate(winNum);

        if (winNum.Length != 0)
        {


            if (gamePhase == "Betting")
            {
                if (manager == null)
                {
                    manager = FindObjectOfType<ManagerBRoullete>();
                }
                if (remainingTime > 3)
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
                else
                {
                    manager.EnableWaitingPanel();
                }

            }
            else
            {
                manager.EnableWaitingPanel();
            }
        }



    }


    public void GameStatusUpdate(string response)
    {

        Debug.Log("Game status : " + response);
        JArray jsonArray = JArray.Parse(response);

        JObject gameStatus = (JObject)jsonArray[0];
        string gamePhase = (string)gameStatus["gamePhase"];
        int remainingTime = (int)gameStatus["timeLeft"];
        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();

        long serverStartTime = (long)gameStatus["roundStartTime"];


        if (gamePhase == "Betting")
        {
            manager.StartBetting(remainingTime, serverStartTime, seeds);
        }
    }

    public void Result(string response)
    {
        JArray jsonArray = JArray.Parse(response);

        // Assuming the first element in the array is the object you need
        JObject gameResult = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        int winIndex = (int)gameResult["winSymbol"];
        int randVal = (int)gameResult["randVal"];
        if (manager == null)
        {
            manager = FindObjectOfType<ManagerBRoullete>();
        }
        manager.ShowResult(winIndex, randVal);
    }
    public void SendBetDataToServer(int betOn, float betAmount)
    {
        Debug.Log("sending bet data : " + betOn + " bet amount : " + betAmount);
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
            { "betOn", betOn },
            { "roomName", roomName },
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

