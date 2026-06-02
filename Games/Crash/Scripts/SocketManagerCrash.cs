using System.Collections;
using UnityEngine;
using SocketIOClient;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient.Newtonsoft.Json;
using SocketIOClient.JsonSerializer;
using UnityEngine.SceneManagement;
using TMPro;
using Core.Config;
using Features.Lobby.Integration;

public class SocketManagerCrash : MonoBehaviour
{
    public static SocketManagerCrash Instance { get; private set; }
    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    public ControllerCrash controllerRef;
    float roundStartTime = 1f;
    float[] multiplierHistory;
    bool _isInitialFocus = true;
    double roundIdd = 0;
    public TextMeshProUGUI pingText;
    private float lastPingTime;

    public GameObject connectingPanel;
    private void OnApplicationPause(bool focus)
    {
        if (!focus)
        {
            if (!_isInitialFocus)
            {
                MainThreadDispatcher.DequeueAll();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                //socket.Emit("fetchLiveMultiplier");
            }
        }
        else
        {
            socket.Disconnect();
            MainThreadDispatcher.DequeueAll();
            _isInitialFocus = false;
        }
    }

    private void Awake()
    {
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
    void Start()
    {
        controllerRef = FindObjectOfType<ControllerCrash>();

        var uri = new Uri(ServerConfig.SocketUrl + "/crash");
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
            OnSocketConnected?.Invoke();
            MainThreadDispatcher.Enqueue(() =>
            {
                connectingPanel.SetActive(false);

            });

        };

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("Disconnected from server");
        };

        socket.Connect();


        // Initialize game status when connecting
        socket.On("initGameStatus", (response) =>
        {
            Debug.Log("initial game data : " + response);
            try
            {
                JArray jsonResponseArray = JArray.Parse(response.ToString());
                JObject firstObject = jsonResponseArray[0] as JObject;
                firstObject.TryGetValue("gamePhase", out JToken gamePhase);
                firstObject.TryGetValue("remainingBettingTime", out JToken remainingBettingTime);
                firstObject.TryGetValue("roundStartTime", out JToken roundStartTime);
                firstObject.TryGetValue("multiplierHistory", out JToken multiplierHistory);
                firstObject.TryGetValue("roundId", out JToken rId);
                firstObject.TryGetValue("roundId", out JToken seeed);


                string seed = seeed.ToObject<string>();
                string game_Phase = gamePhase.ToObject<string>();
                float remBettingTime = remainingBettingTime.ToObject<float>();
                double round_Start_Time = roundStartTime.ToObject<double>();
                long roundId = rId.ToObject<long>();
                float[] history = multiplierHistory.ToObject<float[]>();

                roundIdd = roundId;
                MainThreadDispatcher.Enqueue(() =>
                {
                    connectingPanel.SetActive(false);
                    controllerRef.History(history);

                    switch (game_Phase)
                    {
                        case "Betting":
                            if (remBettingTime > 1f)
                            {
                                controllerRef.ShowWaitingPanel(roundId, remBettingTime, seed);
                            }
                            else
                            {
                                controllerRef.roundId = roundId.ToString();
                            }
                            controllerRef.HistoryUpdated();
                            break;
                        case "Flying":
                            controllerRef.StartFlyingPhase(round_Start_Time, roundId);
                            controllerRef.xText.gameObject.SetActive(true);
                            controllerRef.HistoryUpdated();
                            break;
                        case "Ending":

                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError("Error parsing response: " + ex.Message);
            }
        });


        socket.On("startFlying", (response) =>
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                if (Application.isFocused)
                {
                    if (connectingPanel.activeInHierarchy)
                    {
                        connectingPanel.SetActive(false);
                    }
                    StartFlying(response);
                }
            });
        });

        socket.On("startBetting", (response) =>
        {

            MainThreadDispatcher.Enqueue(() =>
            {
                if (Application.isFocused)
                {
                    if (connectingPanel.activeInHierarchy)
                    {
                        connectingPanel.SetActive(false);
                    }
                    StartBetting(response);
                }
            });
        });


        socket.On("endRound", (response) =>
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                JArray jsonResponseArray = JArray.Parse(response.ToString());
                float gameEndMultiplier = jsonResponseArray[0].ToObject<float>();
                if (controllerRef == null)
                {
                    controllerRef = FindObjectOfType<ControllerCrash>();
                }
                controllerRef.EndGame(gameEndMultiplier);
            });
        });







        socket.On("CashOutSuccessful", (response) =>
        {
            Debug.Log("response from cashout Req : " + response);
            MainThreadDispatcher.Enqueue(() =>
            {
                JArray jsonResponseArray = JArray.Parse(response.ToString());

                JObject firstObject = jsonResponseArray[0] as JObject;

                firstObject.TryGetValue("multiplier", out JToken multiplier);
                float multiplierVal = multiplier.ToObject<float>();

                controllerRef.bet1.DelayedCashOut(multiplierVal);

            });
        });


        // Listen for the "pong" event from the server
        socket.On("pong_back", (response) =>
        {

            MainThreadDispatcher.Enqueue(() =>
            {
                float currentPingTime = Time.time - lastPingTime;
                pingText.text = (currentPingTime * 1000f).ToString("F0") + "ms";


                if (currentPingTime * 1000f <= 200)
                {
                    pingText.color = Color.green;

                }
                else if (currentPingTime * 1000f > 200)
                {
                    pingText.color = Color.white;
                }
                else if (currentPingTime * 1000f > 500)
                {
                    pingText.color = Color.yellow;
                }
                else
                {
                    pingText.color = Color.red;
                }


            });
        });

        // Start sending pings
        InvokeRepeating("SendPing", 1f, 5f); // Adjust interval as needed



    }



    void StartFlying(SocketIOResponse response)
    {
        try
        {
            JArray jsonResponseArray = JArray.Parse(response.ToString());
            double roundStartTime = jsonResponseArray[0].ToObject<double>();
            long roundIdd = jsonResponseArray[0].ToObject<long>();
            controllerRef.StartFlyingPhase(roundStartTime, roundIdd);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in StartFlying() : " + ex.Message);
        }
    }

    void StartBetting(SocketIOResponse response)
    {
        Debug.Log("start Betting : " + response);
        try
        {
            JArray jsonResponseArray = JArray.Parse(response.ToString());
            JObject firstObject = jsonResponseArray[0] as JObject;
            firstObject.TryGetValue("remainingTime", out JToken remainingBettingTime);
            firstObject.TryGetValue("roundId", out JToken Id);
            long roundId = Id.ToObject<long>();
            string seed = firstObject["seed"].ToString();

            float remBettingTime = remainingBettingTime.ToObject<float>();
            controllerRef.ShowWaitingPanel(roundId, remBettingTime, seed);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in StartFlying() : " + ex.Message);
        }
    }



    void HistoryUpdate(SocketIOResponse response)
    {
        Debug.Log(response);
        if (response.Count == 0)
        {
            return;
        }
        try
        {
            // Parse the response as an array
            JArray jsonResponseArray = JArray.Parse(response.ToString());
            if (jsonResponseArray.Count == 0)
            {
                return;
            }
            // Get the first object in the array and extract "multiplierHistory"
            var gameHistory = jsonResponseArray[0]["multiplierHistory"].ToObject<float[]>();

            // Assign to multiplierHistory
            multiplierHistory = gameHistory;

            // Debugging the data

            // Calling the controller reference
            if (controllerRef == null)
            {
                controllerRef = FindObjectOfType<ControllerCrash>();
            }
            controllerRef.History(multiplierHistory);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error parsing multiplier history: " + ex.Message);
        }
    }

    public void Cashout(float betAmount)
    {
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
            { "betAmount", betAmount }
        };
        Debug.Log("Emitted Cashout");
        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("cashOutData", jsonData);
    }

    void SendPing()
    {
        lastPingTime = Time.time;
        socket.Emit("ping_test"); // Send ping event to server
    }
    public void SendBetData(float betAmount)
    {
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
            { "betAmount", betAmount }
        };

        string jsonData = JsonConvert.SerializeObject(data);
        Debug.Log("json : " + jsonData);
        socket.Emit("sendBetData", jsonData);
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
    public void CancelBet(float betAmount)
    {
        var data = new Dictionary<string, object>
        {
            { "betAmount", betAmount }
        };

        string jsonData = JsonConvert.SerializeObject(data);
        Debug.Log("json : " + jsonData);
        socket.Emit("cancelBet", jsonData);
    }

    public class BetDataWrapper
    {
        public BetData betData;

        public BetDataWrapper(BetData data)
        {
            betData = data;
        }
    }


    public class BetData
    {
        public float amount;
    }


}

