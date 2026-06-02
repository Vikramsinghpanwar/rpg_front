using System.Collections;
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


public class SocketManagerHR : MonoBehaviour
{
    public SocketIOUnity socket;
    public static event Action OnSocketConnected;
    HorseRacingGame manager;
    public static SocketManagerHR Instance { get; private set; }
    GameObject currentlyOpenPanel;

    bool _isInitialFocus = true;

    private void Awake()
    {
        manager = FindObjectOfType<HorseRacingGame>();
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
        var uri = new Uri(ServerConfig.SocketUrl + "/horseRacing");
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
            Debug.Log("startBetting : " + response);
            //is in Resting period = gameStatus;
            MainThreadDispatcher.Enqueue(() =>
            {

                JArray jsonArray = JArray.Parse(response.ToString());
                JObject gamePhase = (JObject)jsonArray[0];

                JArray horseMultipliersArray = (JArray)gamePhase["horseMultiplier"];
                float[] horseMultipliers = horseMultipliersArray.ToObject<float[]>();
                JArray botDataArray = (JArray)gamePhase["botsData"];

                long jackpotVal = (long)gamePhase["jackpotVal"];

                long serverStartTime = (long)gamePhase["roundStartTime"];

                JArray seedsArray = (JArray)gamePhase["seeds"];
                string[] seeds = seedsArray.ToObject<string[]>();

                manager.UpdateHorseOdds(horseMultipliers);
                manager.StartBetting(14f, serverStartTime, seeds, jackpotVal);
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
                            manager = FindObjectOfType<HorseRacingGame>();
                        }
                        manager.InitializeBots(bots, 15);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
                }
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
    }


    public void InitGameStatusUpdate(string response)
    {
        Debug.Log("Response: " + response);

        JArray jsonArray = JArray.Parse(response);
        JObject gamePhase = (JObject)jsonArray[0];

        // Parse the values from the JSON object
        string gameInProgress = (string)gamePhase["gamePhase"];
        long remainingTime = (long)gamePhase["timeLeft"];

        long jackpotVal = (long)gamePhase["jackpotVal"];

        JArray seedsArray = (JArray)gamePhase["seeds"];
        string[] seeds = seedsArray.ToObject<string[]>();

        // Get the history
        JArray historyArray = (JArray)gamePhase["history"];
        int[] history = historyArray.ToObject<int[]>();

        JArray botDataArray = (JArray)gamePhase["botsData"];
        long serverStartTime = (long)gamePhase["roundStartTime"];


        // Log history for debugging
        if (history.Length != 0)
        {
            manager.UpdateHistory(history);
        }

        // Handle the game phases (Racing / Betting)
        if (gameInProgress == "Racing" || remainingTime < 4f)
        {
            manager.EnableWaitingPanel();
        }
        else if (gameInProgress == "Betting")
        {
            manager.StartBetting(remainingTime - 2, serverStartTime, seeds, jackpotVal);
        }
        else
        {
            Debug.LogError("Unknown game phase: " + gameInProgress);
        }

        if (gamePhase["horseMultiplier"] != null)
        {

            JArray horseMultipliersArray = (JArray)gamePhase["horseMultiplier"];
            float[] horseMultipliers = horseMultipliersArray.ToObject<float[]>();
            manager.UpdateHorseOdds(horseMultipliers);
        }
        else
        {
            Debug.LogError("Invalid response format: Missing 'horseMultiplier'.");
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
                    manager = FindObjectOfType<HorseRacingGame>();
                }
                manager.InitializeBots(bots, remainingTime);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Deserialization failed: {ex.Message}\n{ex.StackTrace}");
        }


    }



    public void HandleGameResult(SocketIOResponse response)
    {
        try
        {
            string rawResponse = response.ToString();
            Debug.Log("Received Game Result: " + rawResponse);  // Log the raw response

            JArray jsonResponseArray = JArray.Parse(rawResponse);
            JObject firstObject = jsonResponseArray[0] as JObject;

            JArray botWinA = (JArray)firstObject["botWinnerArray"];

            int[] botWinArray = botWinA.ToObject<int[]>();

            // Check if 'gameResult' exists in the response
            if (firstObject != null && firstObject["winIndex"] != null)
            {
                int winIndex = (int)firstObject["winIndex"];  // Extract winIndex
                Debug.Log("Winning Horse Index from server: " + winIndex);  // Log the winning horse index

                // Pass the winning horse index to the HorseRacingGame manager
                manager.GameResult(winIndex, botWinArray);
            }
            else
            {
                Debug.LogError("Invalid response format: Missing 'winIndex'.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error processing game result: " + ex.Message);
        }
    }




    public void SendBetDataToServer(int betOn, float betAmount)
    {
        var data = new Dictionary<string, object>
        {
            { "betOn", betOn },
            { "betAmount", betAmount },
            {"userId", BootstrapLobbyAdapter.GetUserId()},
        };

        string jsonData = JsonConvert.SerializeObject(data);
        Debug.Log("JSON: " + jsonData);
        socket.Emit("sendData", jsonData);
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
        Debug.Log("throw");
        string jsonData = JsonConvert.SerializeObject(data);
        socket.Emit("throwItem", jsonData);
    }

}
