using UnityEngine;
using SocketIOClient;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine.SceneManagement;
using Core.Config;
using Features.Lobby.Integration;

[DisallowMultipleComponent]
public class SocketManagerDT : MonoBehaviour
{
    public static SocketManagerDT Instance { get; private set; }
    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    public ManagerDT manager;
    RandomHistoryDT historyRef;
    bool _isInitialFocus = true;
    public string currentRoundId;

    private void Awake()
    {
        historyRef = FindObjectOfType<RandomHistoryDT>();
        manager = FindObjectOfType<ManagerDT>();
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
        var uri = new Uri(ServerConfig.SocketUrl + "/dragonTiger");
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
            OnSocketConnected.Invoke();
            Debug.Log("Socket connected DT");
        };

        socket.OnDisconnected += (sender, e) =>
        {
        };

        socket.Connect();

        socket.On("gameResult", (response) =>
        {
            MainThreadDispatcher.Enqueue(() =>
            {

                Result(response);
            });


        });

        socket.On("botsData", (response) =>
       {
           MainThreadDispatcher.Enqueue(() =>
           {
               Debug.Log(gameObject.name);

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

        JArray jsonArray = JArray.Parse(response);

        // Assuming the first element in the array is the object you need
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        string gamePhase = (string)gameStatus["gamePhase"];

        int remainingTime = (int)gameStatus["timeLeft"];

        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();

        long roundStartTime = (long)gameStatus["roundStartTime"];
        currentRoundId = roundStartTime.ToString();
        JArray botDataArray = (JArray)gameStatus["botsData"];
        JArray charactersArray = (JArray)gameStatus["history"];
        char[] characters = charactersArray.ToObject<char[]>(); // Convert JArray to char[]
        if (characters.Length != 0)
        {

            if (manager == null)
            {
                manager = FindObjectOfType<ManagerDT>();
            }
            if (gamePhase != "Betting")
            {
                manager.EnableWaitingPanel();
                historyRef.GenerateHistoryData(characters);
            }
            else
            {
                manager.StartBetting(remainingTime, roundStartTime, seeds);
                historyRef.GenerateHistoryData(characters);

            }
        }

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


        if (manager == null)
        {
            manager = FindObjectOfType<ManagerDT>();
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
                    manager = FindObjectOfType<ManagerDT>();
                }
                manager.InitializeBots(bots, roundStartTime);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
        }



    }

    public void Result(SocketIOResponse response)
    {
        Debug.Log("result : " + response);
        try
        {
            JArray jsonResponseArray = JArray.Parse(response.ToString());
            JObject firstObject = jsonResponseArray[0] as JObject;
            firstObject.TryGetValue("dragonCard", out JToken dCard);
            firstObject.TryGetValue("tigerCard", out JToken tCard);
            firstObject.TryGetValue("winner", out JToken winner);
            JArray botWinA = (JArray)firstObject["botWinnerArray"];

            int[] botWinArray = botWinA.ToObject<int[]>();
            string dragonCard = dCard.ToObject<string>();
            string tigerCard = tCard.ToObject<string>();
            Char Winner = winner.ToObject<Char>();

            manager.DisplayCards(dragonCard, tigerCard, Winner, botWinArray);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing response: " + ex.Message);
        }
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
    public void SendBetDataToServer(int betOn, float betAmount)
    {
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
            { "betOn", betOn },
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
        };

        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("clearAllBet", jsonData);
    }

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
        Debug.Log("hurra");
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
        TogglePanel(currentlyOpenPanel);
        var data = new Dictionary<string, object>
        {
            {"token", UserDetail.Token},
            { "item", item},
            { "player", player}
        };

        string jsonData = JsonConvert.SerializeObject(data);
        Debug.Log("throwing item " + jsonData);
        socket.Emit("throwItem", jsonData);
    }


}


