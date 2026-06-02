using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Config;

namespace Game.ChickenRoad
{
    public class SocketManager : MonoBehaviour
    {
        Jugad dbRef;
        public SocketIOUnity socket;
        public static event Action OnSocketConnected;
        public static SocketManager Instance { get; private set; }

        bool _isInitialFocus = true;
        long remainingTime;
        string jokerCard;
        GameObject currentlyOpenPanel;




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
            dbRef = FindObjectOfType<Jugad>();
            //var uri = new Uri(ServerConfig.SocketUrl + "/andarBahar");
            //var uri = new Uri("http://localhost:3000");
            var uri = new Uri("https://chickenroad.demotele.online");
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
                MainThreadDispatcher.Enqueue(() =>
                {
                    Debug.Log("Socket connected, starting game session");
                    StartGameSession(BetManager.instance.betAmount);

                });
                OnSocketConnected.Invoke();


            };

            socket.OnDisconnected += (sender, e) =>
            {
                Debug.Log("Disconnected from server");
            };





            socket.On("gameOver", (response) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    Debug.Log("Game Over: " + response.ToString());
                    GameManager.instance.EndGame();
                });
            });

            socket.On("gameStarted", (response) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    Debug.Log("Game Started: " + response.ToString());
                    dbRef.SaveGameHistoryDatabase(GameManager.instance.periodId, "Chicken Road", BetManager.instance.betAmount);
                    GameManager.instance.MoveHenToNextTile();
                });
            });

            socket.On("stepResult", (response) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    Debug.Log("Step Result: " + response.ToString());
                    GameManager.instance.MoveHenToNextTile();
                });
            });

            socket.On("gameCompleted", (response) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    NextStep();
                    ScrollSnapController.instance.ScrollNext();
                });
            });

            socket.On("cashoutComplete", (response) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    Debug.Log("Cashout Complete: " + response.ToString());
                    float amount = BetManager.instance.betAmount * BetManager.instance.GetCurrentMultiplier();
                    dbRef.UpdateGameHistory(GameManager.instance.periodId, amount);
                    Wallet.AddToWinWallet(amount);
                    GameManager.instance.CashOutSuccessfull();
                });
            });
        }

        public void ConnectToServer()
        {
            socket.Connect();
        }

        public void StartGameSession(float betAmount)
        {
            if (socket == null)
            {
                Debug.LogError("Socket is not initialized.");
                return;
            }

            var data = new Dictionary<string, object>
            {
                { "userId", "BootstrapLobbyAdapter.GetUserId()" },
                { "difficulty", "easy"},
                { "betAmount", betAmount },
            };

            string jsonData = JsonConvert.SerializeObject(data);
            Debug.Log("JSON: " + jsonData);
            socket.Emit("startGame", data);
        }

        public void NextStep()
        {
            if (socket == null)
            {
                Debug.LogError("Socket is not initialized.");
                return;
            }

            var data = new Dictionary<string, object>
            {
                { "userId", "BootstrapLobbyAdapter.GetUserId()" },
                { "difficulty", "easy"},
            };

            string jsonData = JsonConvert.SerializeObject(data);
            Debug.Log("JSON: " + jsonData);
            socket.Emit("nextStep", jsonData);
        }

        public void CashOut()
        {
            if (socket == null)
            {
                Debug.LogError("Socket is not initialized.");
                return;
            }

            var data = new Dictionary<string, object>
            {
                { "userId", "BootstrapLobbyAdapter.GetUserId()" },
                { "difficulty", "easy"},
            };

            string jsonData = JsonConvert.SerializeObject(data);
            Debug.Log("JSON: " + jsonData);
            socket.Emit("cashOut", jsonData);
        }



    }
}