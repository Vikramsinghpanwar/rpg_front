using System.Collections;
using UnityEngine;
using SocketIOClient;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using SocketIOClient.Newtonsoft.Json;
using System.Threading.Tasks;
using Core.API;
using Core.Config;
using Core.Services;
using Features.Lobby.Integration;
using Core.Bootstrap;

public class SocketManagerBac : MonoBehaviour
{
    public static SocketManagerBac Instance { get; private set; }

    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    ManagerBac manager;
    bool _isInitialFocus = true;
    bool _walletRefreshed;



    private void Awake()
    {
        manager = FindObjectOfType<ManagerBac>();

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
        var uri = new Uri(ServerConfig.SocketUrl + "/baccarat");
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
                ThrowItemAnim(response);
            });
        });


        socket.On("initGameStatus", (response) =>
        {
            Debug.Log("response : " + response);
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

        socket.On("stopBetting", (response) =>// listening for change in game status
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                manager.StopBetting();
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
        string gPhase = (string)gameStatus["gamePhase"];
        Debug.Log("game status: " + gPhase);

        int remainingTime = (int)gameStatus["timeLeft"];
        Debug.Log("remaining time: " + remainingTime);

        JArray charactersArray = (JArray)gameStatus["history"];
        char[] characters = charactersArray.ToObject<char[]>(); // Convert JArray to char[]

        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();

        long serverStartTime = (long)gameStatus["roundStartTime"];
        JArray botDataArray = (JArray)gameStatus["botsData"];

        if (characters.Length != 0)
        {


            if (gPhase != "Betting")
            {
                manager.EnableWaitingPanel();
                //historyRef.GenerateHistoryData(characters);
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
                //historyRef.GenerateHistoryData(characters);

            }
        }



        Debug.Log("deserialisation for : \n" + botDataArray.ToString());
        try
        {
            List<BotData> bots = JsonConvert.DeserializeObject<List<BotData>>(botDataArray.ToString());

            // Access the first list of bots
            if (bots.Count > 0)
            {


                if (manager == null)
                {
                    manager = FindObjectOfType<ManagerBac>();
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

    public void StartBetting(string response)
    {
        _walletRefreshed = false;

        Debug.Log("my : " + response);
        JArray jsonArray = JArray.Parse(response);

        JObject gameStatus = (JObject)jsonArray[0];
        int remainingTime = (int)gameStatus["timeLeft"];

        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();
        long serverStartTime = (long)gameStatus["roundStartTime"];


        JArray botDataArray = (JArray)gameStatus["botsData"];


        if (manager == null)
        {
            manager = FindObjectOfType<ManagerBac>();
        }

        manager.StartBetting(remainingTime, serverStartTime, seeds);


        Debug.Log("deserialisation for : \n" + botDataArray.ToString());
        try
        {
            List<BotData> bots = JsonConvert.DeserializeObject<List<BotData>>(botDataArray.ToString());

            // Access the first list of bots
            if (bots.Count > 0)
            {


                if (manager == null)
                {
                    manager = FindObjectOfType<ManagerBac>();
                }
                manager.InitializeBots(bots, serverStartTime);

                foreach (var bot in bots)
                {
                    Debug.Log($"Bot ID: {bot.botId}, Name: {bot.botName}, Wallet: {bot.botWallet}");
                    if (bot.bets == null)
                    {
                        Debug.LogError("Bets list is null for Bot ID: " + bot.botId);
                    }
                    else
                    {
                        Debug.Log("Bets count: " + bot.bets.Count);
                        foreach (var bet in bot.bets)
                        {
                            Debug.Log($"Bet -> Amount: {bet.amount}, Time: {bet.time}, BetOn: {bet.betOn}");
                        }
                    }
                }

            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
        }

    }

    public void Result(string response)
    {
        Debug.Log("[Bac] Result received");
        try
        {
            JArray jsonArray = JArray.Parse(response);
            JObject gameResult = (JObject)jsonArray[0];
            List<string> playerCards = gameResult["playerCards"].ToObject<List<string>>();
            List<string> bankerCards = gameResult["bankerCards"].ToObject<List<string>>();
            char winner = (char)gameResult["winner"];
            int playerPair = gameResult["playerPair"].ToObject<int>();
            int bankerPair = gameResult["bankerPair"].ToObject<int>();
            JArray botWinA = (JArray)gameResult["botWinnerArray"];
            int[] botWinArray = botWinA.ToObject<int[]>();

            Debug.Log("[Bac] Result animation starting");
            manager.BeginResultProcessing();
            manager.DisplayCards(playerCards, bankerCards, winner, playerPair, bankerPair, botWinArray);
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

        Debug.Log("[Bac] Result animation completed");
        Debug.Log("[Bac] RefreshWalletAsync starting");

        if (_walletRefreshed)
        {
            Debug.Log("[Bac] Wallet refresh already performed this round, skipping duplicate");
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
            Debug.LogWarning($"[Bac] RefreshWalletAsync failed: {refreshTask.Exception?.Message}");
        }
        else
        {
            Debug.Log("[Bac] RefreshWalletAsync completed");

            var bs = Core.Bootstrap.BootstrapService.Instance;
            if (bs != null && bs.Wallet != null && manager != null)
            {
                manager.UpdateWallet(bs.Wallet.available_balance);
                Debug.Log($"[Bac] Wallet UI updated from Bootstrap: {bs.Wallet.available_balance}");
            }
            else
            {
                Debug.LogWarning("[Bac] Bootstrap wallet not available after refresh, cannot update game UI");
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
            await CGSBetService.Instance.PlaceBetAsync(CGSGameKeys.Baccarat, betOn, (long)betAmount);
        }
        catch (ApiException ex)
        {
            Debug.LogWarning($"[SocketManagerBac] Bet rejected: {ex.StatusCode} {ex.Message}");
        }
    }

    public void ClearAllBets()
    {
        // Bets are HTTP-authoritative — no socket emit needed for clear
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
