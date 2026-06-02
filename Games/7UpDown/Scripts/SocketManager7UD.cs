using UnityEngine;
using SocketIOClient;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using SocketIOClient.Newtonsoft.Json;
using System.IO;
using Core.Config;
using Features.Lobby.Integration;

public class SocketManager7UD : MonoBehaviour
{
    public static SocketManager7UD Instance { get; private set; }

    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    Manager7UD manager;
    RandomHistory7UD historyRef;
    bool _isInitialFocus = true;
    public string currentRoundId;



    private void Awake()
    {
        historyRef = FindObjectOfType<RandomHistory7UD>();
        manager = FindObjectOfType<Manager7UD>();
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
        manager = FindObjectOfType<Manager7UD>();

        var uri = new Uri(ServerConfig.SocketUrl + "/sevenUpDown");
        Debug.Log("uri : " + uri);
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

        socket.On("throwItemAnim", (response) =>
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                Debug.Log("throwItemAnim : " + response);
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
        Debug.Log("response : " + response);

        JArray jsonArray = JArray.Parse(response);

        // Assuming the first element in the array is the object you need
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        string gamePhase = (string)gameStatus["gamePhase"];

        int remainingTime = (int)gameStatus["timeLeft"];
        Debug.Log("remaining time: " + remainingTime);
        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();
        long serverStartTime = (long)gameStatus["roundStartTime"];
        JArray botDataArray = (JArray)gameStatus["botsData"];
        currentRoundId = serverStartTime.ToString();
        JArray charactersArray = (JArray)gameStatus["history"];
        char[] characters = charactersArray.ToObject<char[]>(); // Convert JArray to char[]
        if (characters.Length != 0)
        {


            if (gamePhase != "Betting")
            {
                manager.EnableWaitingPanel();
                historyRef.GenerateHistoryData(characters);
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
                historyRef.GenerateHistoryData(characters);

            }
        }
        manager.CheckForPendingBets();
        try
        {
            List<BotData> bots = JsonConvert.DeserializeObject<List<BotData>>(botDataArray.ToString());

            // Access the first list of bots
            if (bots.Count > 0)
            {


                if (manager == null)
                {
                    manager = FindObjectOfType<Manager7UD>();
                }
                manager.InitializeBots(bots, serverStartTime);



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

        // Assuming the first element in the array is the object you need
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object

        int remainingTime = (int)gameStatus["timeLeft"];
        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();
        long roundStartTime = (long)gameStatus["roundStartTime"];
        currentRoundId = roundStartTime.ToString();
        JArray botDataArray = (JArray)gameStatus["botsData"];


        if (manager == null)
        {
            manager = FindObjectOfType<Manager7UD>();
        }

        manager.StartBetting(remainingTime, roundStartTime, seeds);


        Debug.Log("deserialisation for : \n" + botDataArray.ToString());
        try
        {
            List<BotData> bots = JsonConvert.DeserializeObject<List<BotData>>(botDataArray.ToString());

            // Access the first list of bots
            if (bots.Count > 0)
            {


                if (manager == null)
                {
                    manager = FindObjectOfType<Manager7UD>();
                }
                manager.InitializeBots(bots, roundStartTime);



            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
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

    public void Result(string response)
    {
        JArray jsonArray = JArray.Parse(response);

        // Assuming the first element in the array is the object you need
        JObject gameResult = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        int dice1Val = (int)gameResult["dice1"]; ;

        int dice2Val = (int)gameResult["dice2"];
        char winner = (char)gameResult["winner"];
        JArray botWinA = (JArray)gameResult["botWinnerArray"];

        int[] botWinArray = botWinA.ToObject<int[]>();


        manager.DisplayWinner(dice1Val, dice2Val, winner, botWinArray);
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
    public void ClearAllBets()
    {
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
        };

        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("clearAllBet", jsonData);
    }

    public void SendGameStatusRequest()
    {
        Debug.Log("Asking for game Status");
        socket.Emit("GameStatusRequest");
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
            {"userId", BootstrapLobbyAdapter.GetUserId()},
            { "item", item},
            { "player", player}
        };

        string jsonData = JsonConvert.SerializeObject(data);
        Debug.Log("throwing item " + jsonData);

        socket.Emit("throwItem", jsonData);
    }
}
