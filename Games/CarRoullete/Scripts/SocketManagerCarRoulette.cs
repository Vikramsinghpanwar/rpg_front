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
using System.Threading.Tasks;
using Core.API;
using Core.Config;
using Core.Services;
using Features.Lobby.Integration;
using Core.Bootstrap;

public class SocketManagerCarRoulette : MonoBehaviour
{
    public static SocketManagerCarRoulette Instance { get; private set; }

    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    CarRoulleteManager manager;
    bool _isInitialFocus = true;
    public string currentRoundId;
    bool _walletRefreshed;

    private void Awake()
    {
        manager = FindObjectOfType<CarRoulleteManager>();

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

        var uri = new Uri(ServerConfig.SocketUrl + "/carRoulette");

        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", TokenProvider.Instance?.AccessToken ?? string.Empty}
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
        Debug.Log("game status: " + gamePhase);

        int remainingTime = (int)gameStatus["timeLeft"];
        Debug.Log("remaining time: " + remainingTime);

        JArray stringArray = (JArray)gameStatus["history"];

        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();

        long serverStartTime = (long)gameStatus["roundStartTime"];



        JArray botDataArray = (JArray)gameStatus["botsData"];
        Debug.Log("bot data array : " + botDataArray);

        string[] characters = stringArray.ToObject<string[]>(); // Convert JArray to char[]
        manager.HistoryUpdate(characters);

        if (characters.Length != 0)
        {


            if (gamePhase == "Betting")
            {
                if (remainingTime > 2)
                {
                    manager.StartBetting(remainingTime, serverStartTime, seeds);

                }
                else
                {
                    manager.EnableWaitingPanel();
                }

                //historyRef.GenerateHistoryData(characters);
            }
            else
            {
                manager.EnableWaitingPanel();

                //historyRef.GenerateHistoryData(characters);

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
                    manager = FindObjectOfType<CarRoulleteManager>();
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
        _walletRefreshed = false;

        Debug.Log("my : " + response);
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


        manager.StartBetting(remainingTime, serverStartTime, seeds);


        JArray botDataArray = (JArray)gameStatus["botsData"];
        Debug.Log("bot data array : " + botDataArray);


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
                    manager = FindObjectOfType<CarRoulleteManager>();
                }
                manager.InitializeBots(bots, serverStartTime);


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
        Debug.Log("[CR] Result received");
        try
        {
            JArray jsonArray = JArray.Parse(response);
            Debug.Log("idhar : " + response);
            JObject gameResult = (JObject)jsonArray[0];
            JArray botWinA = (JArray)gameResult["botWinnerArray"];
            int[] botWinArray = botWinA.ToObject<int[]>();
            int winIndex = (int)gameResult["winIndex"];

            Debug.Log("[CR] Result animation starting");
            manager.BeginResultProcessing();
            manager.ShowResult(winIndex, botWinArray);
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

        Debug.Log("[CR] Result animation completed");
        Debug.Log("[CR] RefreshWalletAsync starting");

        if (_walletRefreshed)
        {
            Debug.Log("[CR] Wallet refresh already performed this round, skipping duplicate");
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
            Debug.LogWarning($"[CR] RefreshWalletAsync failed: {refreshTask.Exception?.Message}");
        }
        else
        {
            Debug.Log("[CR] RefreshWalletAsync completed");

            var bs = Core.Bootstrap.BootstrapService.Instance;
            if (bs != null && bs.Wallet != null && manager != null)
            {
                manager.UpdateWallet(bs.Wallet.available_balance);
                Debug.Log($"[CR] Wallet UI updated from Bootstrap: {bs.Wallet.available_balance}");
            }
            else
            {
                Debug.LogWarning("[CR] Bootstrap wallet not available after refresh, cannot update game UI");
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
            await CGSBetService.Instance.PlaceBetAsync(CGSGameKeys.RoundRoulette, betOn, (long)betAmount);
        }
        catch (ApiException ex)
        {
            Debug.LogWarning($"[SocketManagerCarRoulette] Bet rejected: {ex.StatusCode} {ex.Message}");
        }
    }

    public void ClearlAllBets()
    {
        // Bets are HTTP-authoritative — no socket emit needed for clear
    }

    public void SendGameStatusRequest()
    {
        Debug.Log("Asking for game Status");
        socket.Emit("GameStatusRequest");
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

