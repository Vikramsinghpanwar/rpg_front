using System.Security.Cryptography.X509Certificates;
using System.Collections;
using UnityEngine;
using SocketIOClient;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Core.API;
using Core.Config;
using Core.Services;
using Features.Lobby.Integration;
using Core.Bootstrap;


public class SocketManagerAB : MonoBehaviour
{
    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    public AndarBaharGame manager;
    public static SocketManagerAB Instance { get; private set; }

    HistoryAB historyAB;
    bool _isInitialFocus = true;
    bool _walletRefreshed;
    long remainingTime;
    string jokerCard;
    GameObject currentlyOpenPanel;
    RandomHistoryAB randomHistoryAB;




    private void Awake()
    {
        historyAB = FindObjectOfType<HistoryAB>();
        randomHistoryAB = FindObjectOfType<RandomHistoryAB>();
        manager = FindObjectOfType<AndarBaharGame>();
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
        // if (!focus)
        // {
        //     if (!_isInitialFocus)
        //     {
        //         Debug.Log("App is going to background");
        //         MainThreadDispatcher.DequeueAll(); // Clear the queue to avoid lingering tasks

        //     }
        // }
        // else // App comes to foreground
        // {
        //     if (!_isInitialFocus)
        //     {
        //         Debug.Log("App is coming back to foreground");
        //        socket.On("initGameStatus", (response) =>
        // {

        //     MainThreadDispatcher.Enqueue(() =>
        //     {
        //         InitGameStatusUpdate(response.ToString());

        //     });


        // });

        //     }
        //     _isInitialFocus = false;
        // }
    }

    void Start()
    {
        var uri = new Uri(ServerConfig.SocketUrl + "/andarBahar");
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




        socket.On("initGameStatus", (response) =>
        {

            MainThreadDispatcher.Enqueue(() =>
            {
                InitGameStatusUpdate(response.ToString());
            });


        });

        socket.On("startBetting", (response) =>
        {
            Debug.Log("startBetting : " + response);
            MainThreadDispatcher.Enqueue(() =>
            {
                JArray jsonArray = JArray.Parse(response.ToString());
                JObject gamePhase = (JObject)jsonArray[0];

                jokerCard = gamePhase["jokerCard"].ToString();
                Debug.Log("Joker card from response: " + jokerCard);
                JArray seedsArray = (JArray)gamePhase["seeds"];
                string[] seeds = seedsArray.ToObject<string[]>();
                long roundStartTime = (long)gamePhase["roundStartTime"];
                JArray botDataArray = (JArray)gamePhase["botsData"];

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Populate
                };
                if (botDataArray != null)
                {
                    List<BotData> bots = JsonConvert.DeserializeObject<List<BotData>>(botDataArray.ToString(), settings);
                    if (bots.Count > 0)
                    {
                        manager.InitializeBots(bots, roundStartTime);
                    }
                }


                manager.StartBetting(15f, roundStartTime, seeds);
                manager.SetJokerCard(jokerCard);
                _walletRefreshed = false;

            });


        });

        socket.On("gameResult", (response) =>// listening for change in game status
        {
            Debug.Log("gameResult : " + response);
            //is in Resting period = gameStatus;
            MainThreadDispatcher.Enqueue(() =>
            {
                HandleGameResult(response);
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
    }


    public void InitGameStatusUpdate(string response)
    {

        Debug.Log("response : " + response);


        JArray jsonArray = JArray.Parse(response);
        JObject gamePhase = (JObject)jsonArray[0];


        string gameInProgress = (string)gamePhase["gamePhase"];
        Debug.Log("Game phase: " + gameInProgress);

        remainingTime = (long)gamePhase["timeLeft"];

        JArray botDataArray = (JArray)gamePhase["botsData"];
        Debug.Log("bot data array : " + botDataArray);


        jokerCard = gamePhase["jokerCard"].ToString();
        Debug.Log("Joker card from response: " + jokerCard);
        manager.SetJokerCard(jokerCard);


        JArray seedsArray = (JArray)gamePhase["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();
        long roundStartTime = (long)gamePhase["roundStartTime"];

        JArray historyArray = (JArray)gamePhase["history"];
        Debug.Log("Ladfgja");

        Debug.Log("Hist array" + historyArray[0]);
        HisObj[] history = historyArray.ToObject<HisObj[]>();

        Debug.Log(history.Length);
        if (history.Length != 0)
        {
            //Debug.Log("history: " + string.Join(",", history)+ string.Join(",", history));



            randomHistoryAB.GenerateHistoryData(history);
        }


        try
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Populate
            };
            List<BotData> bots = JsonConvert.DeserializeObject<List<BotData>>(botDataArray.ToString(), settings);
            Debug.Log("hhhh " + bots[1].botName);
            // Access the first list of bots
            if (bots.Count > 0)
            {


                if (manager == null)
                {
                    manager = FindObjectOfType<AndarBaharGame>();
                }
                manager.InitializeBots(bots, remainingTime);


            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
        }


        if (gameInProgress == "Result")
        {
            manager.EnableWaitingPanel();
            manager.SetJokerCard(jokerCard);
        }
        else if (gameInProgress == "Betting")
        {
            manager.StartBetting(remainingTime, roundStartTime, seeds);
        }
        else
        {
            Debug.LogError("Unknown game phase: " + gameInProgress);
        }
    }


    public void HandleGameResult(SocketIOResponse response)
    {
        Debug.Log("[AB] Result received");
        string rawResponse = response.ToString();
        Debug.Log("Received Game Result: " + rawResponse);

        JArray jsonResponseArray = JArray.Parse(rawResponse);
        JObject firstObject = jsonResponseArray[0] as JObject;

        if (firstObject != null)
        {
            JArray andarCards = (JArray)firstObject["andarCards"];
            JArray baharCards = (JArray)firstObject["baharCards"];
            int winner = (int)firstObject["winner"];
            int[] botsWinArray = firstObject["botWinnerArray"].ToObject<int[]>();
            int cardsDealt = (int)firstObject["cardsDealt"];

            Debug.Log("[AB] Result animation starting");
            manager.BeginResultProcessing();
            randomHistoryAB.AddHistoryData(winner, cardsDealt);
            manager.UpdateGameCards(andarCards.ToObject<List<string>>(), baharCards.ToObject<List<string>>(), cardsDealt, botsWinArray);
            StartCoroutine(WaitForResultThenRefreshWallet());
        }
        else
        {
            Debug.LogError("Invalid response format: Missing 'gameResult'.");
        }
    }

    private IEnumerator WaitForResultThenRefreshWallet()
    {
        yield return new WaitUntil(() => manager.IsResultAnimationComplete);

        Debug.Log("[AB] Result animation completed");
        Debug.Log("[AB] RefreshWalletAsync starting");

        if (_walletRefreshed)
        {
            Debug.Log("[AB] Wallet refresh already performed this round, skipping duplicate");
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
            Debug.LogWarning($"[AB] RefreshWalletAsync failed: {refreshTask.Exception?.Message}");
        }
        else
        {
            Debug.Log("[AB] RefreshWalletAsync completed");

            var bs = Core.Bootstrap.BootstrapService.Instance;
            if (bs != null && bs.Wallet != null && manager != null)
            {
                manager.UpdateWallet(bs.Wallet.available_balance);
                Debug.Log($"[AB] Wallet UI updated from Bootstrap: {bs.Wallet.available_balance}");
            }
            else
            {
                Debug.LogWarning("[AB] Bootstrap wallet not available after refresh, cannot update game UI");
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
            await CGSBetService.Instance.PlaceBetAsync(CGSGameKeys.AndarBahar, betOn, (long)betAmount);
        }
        catch (ApiException ex)
        {
            Debug.LogWarning($"[SocketManagerAB] Bet rejected: {ex.StatusCode} {ex.Message}");
        }
    }

    public void ClearAllBets()
    {
        // Bets are HTTP-authoritative — no socket emit needed for clear
    }

    public void ThrowItemAnim(SocketIOResponse response)
    {
        Debug.Log("throw Item : " + response);
        try
        {
            JArray jsonResponseArray = JArray.Parse(response.ToString());
            JObject firstObject = jsonResponseArray[0] as JObject;
            firstObject.TryGetValue("player", out JToken playerNum);
            firstObject.TryGetValue("item", out JToken itemNum);

            int player = playerNum.ToObject<int>();
            int item = itemNum.ToObject<int>();

            Debug.Log("playerNum......" + player + "item" + item);

            manager.ThrowItemAnim("abcd123", player, item);
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

        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("throwItem", jsonData);
    }
}


public class HisObj
{
    public int winner;
    public int cardsDealt;

}