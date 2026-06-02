using UnityEngine;
using SocketIOClient;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient.Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;
using Core.Config;
using Features.Lobby.Integration;

public class SocketManagerAviator : MonoBehaviour
{

    public static SocketManagerAviator Instance { get; private set; }



    public SocketIOUnity socket;

    bool _isInitialFocus = true;
    public Controller controllerRef;
    double roundIdd = 0;
    System.DateTime offTime;
    public long timeDifference;

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

    private void OnApplicationPause(bool pause)
    {
        Debug.Log("Focus Changed : " + pause);
        if (!pause)
        {
            if (!_isInitialFocus)
            {
                MainThreadDispatcher.DequeueAll();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else
        {
            controllerRef.StopAllCoroutines();
            Disconnect();
            _isInitialFocus = false;
        }
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


    private void SyncClock()
    {
        Debug.Log("Sending CS req");
        long sendTime = GetCurrentUnixTimeSeconds(); // Record send time

        // Send sync request to server
        socket.Emit("clockSyncRequest");

        // Listen for the server's response
        socket.On("clockSyncResponse", (data) =>
        {
            Debug.Log("Recciving CS req : " + data);

            long receiveTime = GetCurrentUnixTimeSeconds(); // Record receive time

            JArray jsonResponseArray = JArray.Parse(data.ToString());
            JObject firstObject = jsonResponseArray[0] as JObject;
            firstObject.TryGetValue("currentTime", out JToken sTime);

            long serverTime = (long)sTime.ToObject<long>();
            Debug.Log("s time: " + serverTime);
            // Calculate latency
            long latency = (receiveTime - sendTime) / 2;

            // Adjust server time
            long adjustedServerTime = serverTime + latency;
            Debug.Log("a s time: " + adjustedServerTime);

            // Get current client time (UTC)
            long clientTime = GetCurrentUnixTimeSeconds();

            // Calculate the time difference between client and server time


            /* MainThreadDispatcher.Enqueue(() =>
             {
                 timeDifference = adjustedServerTime - clientTime;

                 controllerRef.d1.text = "l: " + latency;
                 controllerRef.d2.text = "t.d: " + timeDifference;

                 Debug.Log("Adjusted Server Time: " + adjustedServerTime);
                 Debug.Log("Client Time Difference: " + timeDifference);
                 Debug.Log("latency : " + latency);
             });*/


        });
    }

    // Get current UTC time in Unix timestamp (seconds)
    private long GetCurrentUnixTimeSeconds()
    {
        return (long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    void Start()
    {



        if (socket != null)
        {
            socket.OnConnected -= OnSocketConnected;
            socket.OnDisconnected -= OnSocketDisconnected;
            socket.Off("initGameData");
            socket.Off("startFlying");
            socket.Off("startBetting");
            socket.Off("endRound");
            socket.Off("CashOutSuccessful");
            socket.Off("pong_back");
        }


        controllerRef = FindObjectOfType<Controller>();
        var uri = new Uri(ServerConfig.SocketUrl + "/aviator");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", "UNITY" }
            },
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();
        socket.Connect();

        socket.OnConnected += OnSocketConnected;
        socket.OnDisconnected += OnSocketDisconnected;





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
                firstObject.TryGetValue("totalBetsThisRound", out JToken totalBets);
                firstObject.TryGetValue("multiplierHistory", out JToken multiplierHistory);
                firstObject.TryGetValue("roundId", out JToken rId);

                string game_Phase = gamePhase.ToObject<string>();
                float remBettingTime = remainingBettingTime.ToObject<float>();
                int totalBetsThisRound = totalBets.ToObject<int>();
                double round_Start_Time = roundStartTime.ToObject<double>();
                double roundId = rId.ToObject<double>();
                float[] history = multiplierHistory.ToObject<float[]>();
                JArray botDataArray = (JArray)firstObject["botsData"];

                roundIdd = roundId;




                MainThreadDispatcher.Enqueue(() =>
                {
                    controllerRef.History(history);
                    controllerRef.CheckForPendingBets(roundId);


                    try
                    {
                        var settings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Populate
                        };
                        List<botDataAviator> bots = JsonConvert.DeserializeObject<List<botDataAviator>>(botDataArray.ToString(), settings);

                        // Access the first list of bots
                        if (bots.Count > 0)
                        {
                            controllerRef.InitializeBots(bots, roundId, totalBetsThisRound);

                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
                    }

                    switch (game_Phase)
                    {
                        case "Betting":
                            if (remBettingTime > 0.2f)
                            {
                                controllerRef.StartBetting(roundId, remBettingTime - 0.19f);
                            }
                            else
                            {
                                controllerRef.roundId = roundId.ToString();
                            }
                            break;
                        case "Flying":
                            controllerRef.StartFlyingPhase(round_Start_Time, roundId);
                            controllerRef.xText.gameObject.SetActive(true);
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
                StartFlying(response);

            });
        });

        socket.On("startBetting", (response) =>
        {

            MainThreadDispatcher.Enqueue(() =>
            {
                StartBetting(response);

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
                    controllerRef = FindObjectOfType<Controller>();
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

                firstObject.TryGetValue("sysNum", out JToken sysNo);
                int sysNum = sysNo.ToObject<int>();

                if (sysNum == 0)
                {
                    controllerRef.bet1.DelayedCashOut(multiplierVal);
                }
                else if (sysNum == 1)
                {
                    controllerRef.bet2.DelayedCashOut(multiplierVal);
                }
            });
        });


        // Listen for the "pong" event from the server
        socket.On("pong_back", (response) =>
        {

            MainThreadDispatcher.Enqueue(() =>
            {
                controllerRef.PingUpdate();
            });
        });

        // Start sending pings
        InvokeRepeating("SendPing", 1f, 5f); // Adjust interval as needed


    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////


    void OnSocketConnected(object sender, EventArgs e)
    {
        SyncClock();
        Debug.Log("Connected to server");
    }

    void OnSocketDisconnected(object sender, string e)
    {
        Debug.Log("Disconnected from server");
    }


    void StartFlying(SocketIOResponse response)
    {
        Debug.Log("start Flying : " + response);
        try
        {
            JArray jsonResponseArray = JArray.Parse(response.ToString());
            double roundStartTime = jsonResponseArray[0].ToObject<double>();
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
            double roundId = Id.ToObject<double>();
            float remBettingTime = remainingBettingTime.ToObject<float>();
            controllerRef.StartBetting(roundId, remBettingTime);
            JArray botDataArray = (JArray)firstObject["botsData"];
            firstObject.TryGetValue("totalBetsThisRound", out JToken totalBets);
            int totalBetsThisRound = totalBets.ToObject<int>();

            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Populate
                };
                List<botDataAviator> bots = JsonConvert.DeserializeObject<List<botDataAviator>>(botDataArray.ToString(), settings);

                // Access the first list of bots
                if (bots.Count > 0)
                {
                    controllerRef.InitializeBots(bots, roundId, totalBetsThisRound);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in StartFlying() : " + ex.Message);
        }


    }



    void SendPing()
    {
        controllerRef.lastPingTime = Time.time;
        socket.Emit("ping_test"); // Send ping event to server
    }

    public void Cashout(float betAmount, int betSysNum, float multiplier, float autoMultiplier = -1f)
    {
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
            { "betAmount", betAmount },
            { "betSysNum", betSysNum },
            { "cOmultiplier" , multiplier },
            { "autoCOMultiplier", autoMultiplier }
        };

        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("cashOutData", jsonData);
    }

    public void SendBetData(float betAmount, int betSysNum, float autoCashOut = -1f)
    {
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
            { "betAmount", betAmount },
            { "betSysNum", betSysNum },
            { "autoCashOutVal", autoCashOut }
        };

        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("sendBetData", jsonData);
    }
    public void CancelBet(float betAmount, int betSysNum)
    {
        var data = new Dictionary<string, object>
        {
            {"userId", BootstrapLobbyAdapter.GetUserId()},
            { "betSysNum", betSysNum },
            { "betAmount", betAmount }
        };

        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("cancelBet", jsonData);
    }



    public void Connected()
    {
        controllerRef.xText.gameObject.SetActive(true);

    }
}

public class botDataAviator
{
    public int botId;
    public string botName;
    public int botBetAmount;
    public float botCashOutMultiplier;
}

