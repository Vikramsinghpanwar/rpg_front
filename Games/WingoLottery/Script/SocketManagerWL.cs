using SocketIOClient;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine.SceneManagement;
using UnityEngine;
using Core.Config;
using Features.Lobby.Integration;


public class SocketManagerWL : MonoBehaviour
{
    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    public static SocketManagerWL Instance { get; private set; }

    bool _isInitialFocus;
    public GameRuleManager manager;
    GameObject currentlyOpenPanel;

    RandomHistoryWL randomHistoryWL;
    public string currentRoundId;

    private void Awake()
    {
        manager = FindObjectOfType<GameRuleManager>();
        randomHistoryWL = FindObjectOfType<RandomHistoryWL>();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void OnDestroy()
    {
        Disconnect();
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
            if (socket != null)
            {
                socket.Disconnect();
            }
            _isInitialFocus = false;
        }
    }


    void Start()
    {
        var uri = new Uri(ServerConfig.SocketUrl + "/colorPrediction");
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
            Debug.Log("Disconnect to server");
        };

        socket.Connect();

        socket.On("initGameStatus", (response) =>
        {

            MainThreadDispatcher.Enqueue(() =>
            {
                InitGameStatusUpdate(response.ToString());
            });


        });

        socket.On("throwItemAnim", (response) =>
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                ThrowItemAnim(response);
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

        socket.On("gameResult", (response) =>
        {
            Debug.Log("result : " + response);
            MainThreadDispatcher.Enqueue(() =>
            {
                ProcessGameResult(response);
            });


        });

    }

    public void ClearAllBets()
    {
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
        };

        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("clearAllBet", jsonData);
    }


    public void InitGameStatusUpdate(string response)
    {
        Debug.Log("init : " + response);
        JArray jsonArray = JArray.Parse(response);
        JObject gamePhase = (JObject)jsonArray[0];

        string gameInProgress = (string)gamePhase["gamePhase"];
        int remainingTime = (int)gamePhase["timeLeft"];

        JArray historyArray = (JArray)gamePhase["history"];
        int[] history = historyArray.ToObject<int[]>();

        long roundStartTime = (long)gamePhase["roundStartTime"];
        currentRoundId = roundStartTime.ToString();

        JArray seedsArray = (JArray)gamePhase["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();

        JArray botDataArray = (JArray)gamePhase["botsData"];


        if (history.Length != 0)
        {
            randomHistoryWL.GenerateHistoryData(history);
        }


        if (gameInProgress == "Betting")
        {
            manager.StartBetting(remainingTime, roundStartTime, seeds);  // Start betting phase for 15 seconds
        }
        else if (gameInProgress == "Result")
        {

            manager.EnableWaitingPanel();

        }


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
                    manager = FindObjectOfType<GameRuleManager>();
                }
                manager.InitializeBots(bots, remainingTime);


            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
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

        JArray botDataArray = (JArray)gameStatus["botsData"];


        if (manager == null)
        {
            manager = FindObjectOfType<GameRuleManager>();
        }

        manager.StartBetting(remainingTime, roundStartTime, seeds);
        currentRoundId = roundStartTime.ToString();

        try
        {
            List<BotData> bots = JsonConvert.DeserializeObject<List<BotData>>(botDataArray.ToString());

            // Access the first list of bots
            if (bots.Count > 0)
            {


                if (manager == null)
                {
                    manager = FindObjectOfType<GameRuleManager>();
                }
                manager.InitializeBots(bots, roundStartTime);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
        }



    }


    void ProcessGameResult(SocketIOResponse response)
    {
        string rawResponse = response.ToString();
        Debug.Log("Received Game Result: " + rawResponse);
        JArray jsonResponseArray = JArray.Parse(rawResponse);
        JObject resultData = (JObject)jsonResponseArray[0];
        int winner = (int)resultData["winner"];
        List<int> botWinnerArray = resultData["botWinnerArray"].ToObject<List<int>>();


        Debug.Log("Game result: " + winner);
        manager.GameResult(winner);
        if (botWinnerArray.Count > 0)
        {
            Debug.Log("Bot winners: " + string.Join(",", botWinnerArray));
        }


    }

    public void SendBetDataToServer(int betOn, float betAmount)
    {
        Debug.Log("Sending bet data: Bet on " + betOn + " with amount " + betAmount);
        var data = new Dictionary<string, object>
        {
            { "betOn", betOn },
            { "betAmount", betAmount },
            {"userId", BootstrapLobbyAdapter.GetUserId()},
        };

        string jsonData = JsonConvert.SerializeObject(data);
        Debug.Log("JSON: " + jsonData);
        socket.Emit("sendData", jsonData);
    }

    public void ThrowItemAnim(SocketIOResponse response)
    {
        Debug.Log("result : " + response);
        try
        {
            JArray jsonResponseArray = JArray.Parse(response.ToString());
            JObject firstObject = jsonResponseArray[0] as JObject;
            firstObject.TryGetValue("player", out JToken playerNum);
            firstObject.TryGetValue("item", out JToken itemNum);

            int player = playerNum.ToObject<int>();
            int item = itemNum.ToObject<int>();

            manager.ThrowItemAnim(player, item);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing response: " + ex.Message);
        }
    }



    public void TogglePanel(GameObject selectedPanel)
    {


        // If the selected panel is already open, close it
        if (currentlyOpenPanel == selectedPanel)
        {
            selectedPanel.SetActive(false);
            currentlyOpenPanel = null; // Reset the currently open panel
        }
        else
        {
            // Close the currently open panel if any
            if (currentlyOpenPanel != null)
            {
                currentlyOpenPanel.SetActive(false);
            }

            // Open the selected panel
            selectedPanel.SetActive(true);
            currentlyOpenPanel = selectedPanel;
        }
    }

    public void Item1(int player)
    {
        ThrowItem(player, 1);
    }

    public void Item2(int player)
    {
        ThrowItem(player, 2);
    }

    public void Item3(int player)
    {
        ThrowItem(player, 3);

    }
    public void Item4(int player)
    {
        ThrowItem(player, 4);

    }
    public void Item5(int player)
    {
        ThrowItem(player, 5);

    }

    public void Item6(int player)
    {
        ThrowItem(player, 6);

    }

    void ThrowItem(int player, int item)
    {
        var data = new Dictionary<string, object>
        {
            {"token", UserDetail.Token},
            { "item", item},
            { "player", player}
        };
        Debug.Log("throw");
        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("throwItem", jsonData);
    }
}



