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

public class SocketManagerBo5 : MonoBehaviour
{
    public static SocketManagerBo5 Instance { get; private set; }

    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    BestOf5 manager;
    bool _isInitialFocus = true;
    TrendGeneratorBestOf5 historyRef;
    public string currentRoundId;


    private void Awake()
    {
        manager = FindObjectOfType<BestOf5>();
        historyRef = FindObjectOfType<TrendGeneratorBestOf5>();
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
        var uri = new Uri(ServerConfig.SocketUrl + "/bestOf5");
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
            MainThreadDispatcher.DequeueAll();
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
                if (!manager.waitingPanel.activeInHierarchy)
                {
                    Result(response.ToString());
                }
            });


        });

        socket.On("throwItemAnim", (response) =>
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                ThrowItemAnim(response);
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
        Debug.Log("init : " + response);
        JArray jsonArray = JArray.Parse(response);

        // Assuming the first element in the array is the object you need
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        string gPhase = (string)gameStatus["gamePhase"];
        int remainingTime = (int)gameStatus["timeLeft"];
        JArray historyArray = (JArray)gameStatus["history"];
        int[][] history = historyArray.ToObject<int[][]>(); // Convert JArray to char[]
        string[][] gameCardsArray = gameStatus["gameCardsArray"].ToObject<string[][]>();
        JArray botDataArray = (JArray)gameStatus["botsData"];
        long roundStartTime = (long)gameStatus["roundStartTime"];
        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();

        if (history.Length != 0)
        {


            if (gPhase == "Result")
            {

                manager.EnableWaitingPanel();
                historyRef.GenerateHistoryData(history);
            }
            else
            {
                manager.Banker_Players_CardArray_Update(gameCardsArray);

                Debug.Log("gamePhase betting");
                manager.StartBetting(remainingTime, roundStartTime, seeds);
                historyRef.GenerateHistoryData(history);

            }
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
                    manager = FindObjectOfType<BestOf5>();
                }
                manager.InitializeBots(bots, roundStartTime);


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
        string[][] gameCardsArray = gameStatus["gameCardsArray"].ToObject<string[][]>();


        manager.Banker_Players_CardArray_Update(gameCardsArray);
        manager.StartBetting(remainingTime, roundStartTime, seeds);
        currentRoundId = roundStartTime.ToString();


        List<BotData> bots = JsonConvert.DeserializeObject<List<BotData>>(botDataArray.ToString());

        // Access the first list of bots
        if (bots.Count > 0)
        {


            if (manager == null)
            {
                manager = FindObjectOfType<BestOf5>();
            }
            manager.InitializeBots(bots, roundStartTime);
        }

        try
        {

        }
        catch (Exception ex)
        {
            Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
        }



    }



    public void Result(string response)
    {
        JArray jsonArray = JArray.Parse(response);

        // Assuming the first element in the array is the object you need
        JObject gameResult = (JObject)jsonArray[0];
        int[] winner = gameResult["winner"].ToObject<int[]>();
        string[][] gameCardsArray = gameResult["gameCardsArray"].ToObject<string[][]>();
        manager.Banker_Players_CardArray_Update(gameCardsArray);
        manager.ShowResult(winner);
    }
    public void SendBetDataToServer(int betOn, float betAmount)
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




    public void ThrowItemAnim(SocketIOResponse response)
    {
        Debug.Log("throw response : " + response);
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



    /// <summary>
    /// ///////////////////////////////////////////////////
    /// </summary>
    /// 


    GameObject currentlyOpenPanel;

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
        Debug.Log("throwing ");
        TogglePanel(currentlyOpenPanel);
        var data = new Dictionary<string, object>
        {
            {"token", UserDetail.Token},
            { "item", item},
            { "player", player}
        };

        string jsonData = JsonConvert.SerializeObject(data);

        socket.Emit("throwItem", jsonData);
    }


}
