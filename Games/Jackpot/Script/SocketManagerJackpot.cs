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

public class SocketManagerJackpot : MonoBehaviour
{
    public static SocketManagerJackpot Instance { get; private set; }

    public SocketIOUnity socket;
    public static event Action OnSocketConnected;

    public BettingSystem manager;

    HistoryJackpot historyJackpot;

    bool _isInitialFocus = true;



    private void Awake()
    {
        manager = FindObjectOfType<BettingSystem>();
        historyJackpot = FindObjectOfType<HistoryJackpot>();
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
        var uri = new Uri(ServerConfig.SocketUrl + "/jackpot");
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


        socket.On("startBetting", (response) =>// listening for change in game status
       {
           Debug.Log("startBetting : " + response);

           MainThreadDispatcher.Enqueue(() =>
           {

               StartBetting(response.ToString());

           });
       });

        socket.On("gameResult", (response) =>
        {
            Debug.Log("Result: " + response);
            MainThreadDispatcher.Enqueue(() =>
            {
                Result(response);
            });
        });
    }

    public void StartBetting(string response)
    {
        JArray jsonArray = JArray.Parse(response);

        JObject gameStatus = (JObject)jsonArray[0];

        int remainingTime = (int)gameStatus["timeLeft"];
        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();
        long roundStartTime = (long)gameStatus["roundStartTime"];


        manager.StartBetting(remainingTime, roundStartTime, seeds);
    }


    public void InitGameStatusUpdate(string response)
    {
        Debug.Log("Response: " + response);

        JArray jsonArray = JArray.Parse(response);
        JObject gameStatus = (JObject)jsonArray[0];

        string gameInProgress = (string)gameStatus["gamePhase"];
        float remainingTime = (float)gameStatus["timeLeft"];

        JArray seedsArray = (JArray)gameStatus["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();
        long roundStartTime = (long)gameStatus["roundStartTime"];

        JArray historyArray = (JArray)gameStatus["history"];
        int[] history = historyArray.ToObject<int[]>();
        if (history.Length != 0)
        {
            Debug.Log("History: " + string.Join(",", history));
            historyJackpot.AddServerHistoryResults(history);
        }


        if (gameInProgress == "Result")
        {
            manager.EnableWaitingPanel();
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



    public void Result(SocketIOResponse response)
    {
        try
        {
            JArray jsonResponseArray = JArray.Parse(response.ToString());
            JObject resultObject = (JObject)jsonResponseArray[0];

            int winner = resultObject["winner"].ToObject<int>();
            JArray cardsArray = (JArray)resultObject["cards"];

            Debug.Log("Winner: " + winner);
            Debug.Log("Cards: " + string.Join(", ", cardsArray.ToObject<List<string>>()));

            // Show winner and cards

            //  manager.ShowCards(cardsArray.ToObject<List<string>>()); 
            StartCoroutine(CallShowCardsWithDelay(cardsArray.ToObject<List<string>>(), winner));
            historyJackpot.AddResultToHistory(winner);
            _ = CGSBetService.Instance.RefreshWalletAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing game result: " + ex.Message);
        }
    }

    IEnumerator CallShowCardsWithDelay(List<string> cards, int winner)
    {
        // Wait for 1 second
        yield return new WaitForSeconds(1f);

        // Call the ShowCards method
        manager.ShowCards(cards);
        manager.ShowWinner(winner);
    }


    public void ClearAllBets()
    {
        // Bets are HTTP-authoritative — no socket emit needed for clear
    }

    public void SendBetDataToServer(int betOn, long betAmount, int userId)
    {
        _ = SendBetAsync(betOn, betAmount);
    }

    async Task SendBetAsync(int betOn, long betAmount)
    {
        try
        {
            await CGSBetService.Instance.PlaceBetAsync(CGSGameKeys.Jackpot, betOn, (long)betAmount);
        }
        catch (ApiException ex)
        {
            Debug.LogWarning($"[SocketManagerJackpot] Bet rejected: {ex.StatusCode} {ex.Message}");
        }
    }
}






