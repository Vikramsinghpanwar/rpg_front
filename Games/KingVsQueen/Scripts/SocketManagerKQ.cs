using System.Collections;
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
using System.Threading.Tasks;
using Core.API;
using Core.Bootstrap;

public class SocketManagerKQ : MonoBehaviour
{
    public static SocketManagerKQ Instance { get; private set; }

    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    ManagerKQ manager;
    RandomHistoryKQ historyRef;
    bool _isInitialFocus = true;
    public string currentRoundId;
    bool _walletRefreshed;



    private void Awake()
    {
        manager = FindObjectOfType<ManagerKQ>();
        historyRef = FindObjectOfType<RandomHistoryKQ>();

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

    private void OnApplicationPause(bool focus)
    {
        if (!focus)
        {
            if (!_isInitialFocus)
            {
                MainThreadDispatcher.DequeueAll();
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

        var uri = new Uri(ServerConfig.SocketUrl + "/kingQueen");
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
            Debug.Log("connected");

        };

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("ddiss");
        };

        socket.Connect();

        socket.On("gameResult", (response) =>
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                Result(response.ToString());
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
            historyRef.GenerateHistoryData(characters);

            if (manager == null)
            {
                manager = FindObjectOfType<ManagerKQ>();
            }
            if (gamePhase != "Betting")
            {
                manager.EnableWaitingPanel();
            }
            else
            {
                manager.StartBetting(remainingTime, roundStartTime, seeds);
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


                if (manager == null)
                {
                    manager = FindObjectOfType<ManagerKQ>();
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
        _walletRefreshed = false;

        JArray jsonArray = JArray.Parse(response);

        // Assuming the first element in the array is the object you need
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object

        int remainingTime = (int)gameStatus["timeLeft"];
        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();
        long roundStartTime = (long)gameStatus["roundStartTime"];

        JArray botDataArray = (JArray)gameStatus["botsData"];


        if (manager == null)
        {
            manager = FindObjectOfType<ManagerKQ>();
        }
        currentRoundId = roundStartTime.ToString();

        manager.StartBetting(remainingTime, roundStartTime, seeds);


        try
        {
            List<BotData> bots = JsonConvert.DeserializeObject<List<BotData>>(botDataArray.ToString());

            // Access the first list of bots
            if (bots.Count > 0)
            {


                if (manager == null)
                {
                    manager = FindObjectOfType<ManagerKQ>();
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

    public void Result(string response)
    {
        Debug.Log("[KQ] Result received");
        try
        {
            JArray jsonArray = JArray.Parse(response);

            JObject gameResultWrapper = (JObject)jsonArray[0]["betResults"];
            if (gameResultWrapper == null)
            {
                Debug.LogError("Failed to parse 'betResults'. Check the response format.");
                return;
            }

            JObject gameResult = (JObject)gameResultWrapper["result"];

            List<string> kingCards = gameResultWrapper["kingCards"]?.ToObject<List<string>>() ?? new List<string>();
            List<string> queenCards = gameResultWrapper["queenCards"]?.ToObject<List<string>>() ?? new List<string>();

            char winner = (char)gameResult["winner"];
            string kingType = gameResult["kingType"].ToString();
            string queenType = gameResult["queenType"].ToString();

            Debug.Log("[KQ] Result animation starting");
            manager.BeginResultProcessing();
            manager.Result(kingCards, queenCards, winner, kingType, queenType);
            StartCoroutine(WaitForResultThenRefreshWallet());
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing response: " + ex.Message);
        }
    }

    private IEnumerator WaitForResultThenRefreshWallet()
    {
        yield return new WaitUntil(() => manager.IsResultAnimationComplete);

        Debug.Log("[KQ] Result animation completed");
        Debug.Log("[KQ] RefreshWalletAsync starting");

        if (_walletRefreshed)
        {
            Debug.Log("[KQ] Wallet refresh already performed this round, skipping duplicate");
            yield break;
        }
        _walletRefreshed = true;

        var refreshTask = CGSBetService.Instance.RefreshWalletAsync();

        while (!refreshTask.IsCompleted)
        {
            yield return null;
        }

        if (refreshTask.IsFaulted)
        {
            Debug.LogWarning($"[KQ] RefreshWalletAsync failed: {refreshTask.Exception?.Message}");
        }
        else
        {
            Debug.Log("[KQ] RefreshWalletAsync completed");

            var bs = Core.Bootstrap.BootstrapService.Instance;
            if (bs != null && bs.Wallet != null && manager != null)
            {
                manager.UpdateWallet(bs.Wallet.available_balance);
                Debug.Log($"[KQ] Wallet UI updated from Bootstrap: {bs.Wallet.available_balance}");
            }
            else
            {
                Debug.LogWarning("[KQ] Bootstrap wallet not available after refresh, cannot update game UI");
            }
        }
    }
    public void SendBetDataToServer(int betOn, float betAmount)
    {
        _ = SendBetAsync(betOn, betAmount);
    }

    async Task SendBetAsync(int betOn, float betAmount)
    {
        try
        {
            await CGSBetService.Instance.PlaceBetAsync(CGSGameKeys.KingQueen, betOn, (long)betAmount);
        }
        catch (ApiException ex)
        {
            Debug.LogWarning($"[SocketManagerKQ] Bet rejected: {ex.StatusCode} {ex.Message}");
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
