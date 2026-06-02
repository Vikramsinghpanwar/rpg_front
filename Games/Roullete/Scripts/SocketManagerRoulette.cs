using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Config;
using Features.Lobby.Integration;

public class SocketManagerRoulette : MonoBehaviour
{
    public static SocketManagerRoulette Instance { get; private set; }
    public GameObject connectingPanel;
    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    ManagerRoullete manager;
    bool _isInitialFocus = true;
    public string roomName;
    public string currentRoundId;

    private void Awake()
    {
        manager = FindObjectOfType<ManagerRoullete>();

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
            if (!_isInitialFocus)
            {
                socket.Disconnect();
            }
            _isInitialFocus = false;
        }
    }

    void Start()
    {
        connectingPanel.SetActive(true);
        var uri = new Uri(ServerConfig.SocketUrl + "/roulette");
        //var uri = new Uri("http://localhost:2000");
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

        socket.On("gameStatus", (response) =>// listening for change in game status
        {
            //is in Resting period = gameStatus;
            MainThreadDispatcher.Enqueue(() =>
            {
                GameStatusUpdate(response.ToString());
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
        string seed = gameStatus["seed"].ToString();

        long serverStartTime = (long)gameStatus["roundStartTime"];
        currentRoundId = serverStartTime.ToString();

        JArray stringArray = (JArray)gameStatus["history"];
        string[] characters = stringArray.ToObject<string[]>(); // Convert JArray to char[]
        JArray botDataArray = (JArray)gameStatus["botsData"];
        manager.HistoryUpdate(characters);

        if (characters.Length != 0)
        {
            if (gamePhase == "Betting")
            {
                if (manager == null)
                {
                    manager = FindObjectOfType<ManagerRoullete>();
                }
                manager.StartBetting(remainingTime, serverStartTime, seed);

                //historyRef.GenerateHistoryData(characters);
            }
            else
            {
                manager.EnableWaitingPanel();

                //historyRef.GenerateHistoryData(characters);

            }
        }
        connectingPanel.SetActive(false);

        manager.CheckForPendingBets();

        try
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Populate
            };
            List<BotData> bots = JsonConvert.DeserializeObject<List<BotData>>(botDataArray.ToString(), settings);

            // Access the first list of bots
            if (bots.Count > 0)
            {


                if (manager == null)
                {
                    Debug.Log("null manager at socket manager roulette");
                    manager = FindObjectOfType<ManagerRoullete>();
                }
                manager.InitializeBots(bots, serverStartTime);


            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
        }

    }


    public void GameStatusUpdate(string response)
    {

        Debug.Log("my : " + response);
        JArray jsonArray = JArray.Parse(response);

        // Assuming the first element in the array is the object you need
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        string gamePhase = (string)gameStatus["gamePhase"];
        int remainingTime = (int)gameStatus["timeLeft"];
        Debug.Log("remaining time: " + remainingTime);
        string seed = gameStatus["seed"].ToString();
        JArray botDataArray = (JArray)gameStatus["botsData"];
        long serverStartTime = (long)gameStatus["roundStartTime"];
        currentRoundId = serverStartTime.ToString();

        if (gamePhase == "Betting")
        {

            manager.StartBetting(remainingTime, serverStartTime, seed);
            manager.CheckForPendingBets();

            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Populate
                };
                List<BotData> bots = JsonConvert.DeserializeObject<List<BotData>>(botDataArray.ToString(), settings);

                // Access the first list of bots
                if (bots.Count > 0)
                {
                    if (manager == null)
                    {
                        manager = FindObjectOfType<ManagerRoullete>();
                    }
                    manager.InitializeBots(bots, serverStartTime);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
            }

        }
    }

    public void Result(string response)
    {
        JArray jsonArray = JArray.Parse(response);
        // Assuming the first element in the array is the object you need
        JObject gameResult = (JObject)jsonArray[0];
        JArray botWinA = (JArray)gameResult["botWinnerArray"];
        int[] botWinArray = botWinA.ToObject<int[]>();
        int winNum = (int)gameResult["winNumber"];
        manager.DisplayResult(winNum, botWinArray);
    }
    public void SendBetDataToServer(int betOn, float betAmount)
    {
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
            { "betOn", betOn },
            { "roomName", roomName },
            { "betAmount", betAmount }

        };

        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("sendData", jsonData);
    }
    public void ClearAllBets()
    {
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
            { "roomName", roomName },
        };

        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("clearAllBet", jsonData);
    }


}

