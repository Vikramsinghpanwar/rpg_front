// ConnectionManager.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Threading;
using Core.API.Endpoints;
using Core.API;
using static Teenpatti.WebSocketServerRequest;
using Features.Lobby.Integration;

namespace Teenpatti
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance { get; private set; }

        private const float PING_INTERVAL = 30f;
        private float pingTimer = 0f;
        private bool isConnected = false;
        private bool isInPrivateRoom = false;
        private string currentPrivateCode = "";

        // Reconnection settings
        private const int MAX_RECONNECT_ATTEMPTS = 5;
        private const float RECONNECT_DELAY = 2f;
        private int reconnectAttempts = 0;
        private bool isReconnecting = false;

        // Game state tracking
        private string lastTableId = "";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetupWebSocketListeners();
        }

        private void Update()
        {
            // if (Input.GetKeyDown(KeyCode.W))
            // {
            //     OnApplicationPause(true);
            //     Disconnect();
            // }else if (Input.GetKeyDown(KeyCode.S))
            // {
            //     OnApplicationPause(false);
            // }
            // Handle ping/pong keep-alive
            if (isConnected)
            {
                pingTimer += Time.deltaTime;
                if (pingTimer >= PING_INTERVAL)
                {
                    pingTimer = 0f;
                    SendPing();
                }
            }
        }



        public async Task<bool> JoinViaGateway(int bootAmount, string privateCode = "")
        {
            var joinRequest = new TeenPattiJoinRequest
            {
                // userID = BootstrapLobbyAdapter.GetUserId(),
                // username = UserDetail.UserName,

                boot_amount = bootAmount,
                privateCode = privateCode,
                buyin = 50,//(int)Wallet.GetTotalWallet(),
                gameType = GameMode.mode == GameMode.Modes.publicGame ? "public" : "private",
                attemptID = Guid.NewGuid().ToString() // Unique ID for this join attempt
            };

            try
            {
                var response = await ApiClient.Instance.Post<JoinApiResponse>(TeenPattiRoutes.JoinGame, joinRequest);
                GameLiveData.wasReconnectFromGateway = response.was_reconnect;
                GameLiveData.instance.tableId = response.table_id;// ?? response.tableID;

                if (response == null)
                {
                    Debug.LogError("Join API returned null");
                    return false;
                }

                switch (response.status)
                {
                    case "ACTIVE":
                    case "RECONNECTABLE":
                        Debug.Log($"Join successful. Connecting to WS: {response.ws_url}");
                        await ConnectToServer(response.ws_url, response.ws_token);
                        return true;

                    case "SETTLING":
                        Debug.Log("Game is settling, please wait...");
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            TeenpattiLobby.instance?.ShowMatchmakingStatus("Round in progress, please wait...");
                        });
                        // Implement polling or wait for SETTLING to complete
                        return await PollForReconnect(bootAmount, privateCode);

                    case "NONE":
                        Debug.Log("No active game session found");
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            // Show "session ended" UI for rejoin scenarios
                            if (TeenpattiGameDataLobby.isRejoin)
                            {
                                // User can re-join from lobby
                            }
                        });
                        return false;

                    default:
                        Debug.LogError($"Unknown join status: {response.status}");
                        return false;
                }
            }
            catch (ApiException ex)
            {
                Debug.LogError($"Join API failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> PollForReconnect(int bootAmount, string privateCode)
        {
            int attempts = 0;
            const int maxAttempts = 30;
            const float delayMs = 500f;

            while (attempts < maxAttempts)
            {
                await Task.Delay((int)delayMs);

                var rejoinResponse = await ApiClient.Instance.Get<RejoinApiResponse>(
                    $"{TeenPattiRoutes.RejoinGame}?tableID={PlayerPrefs.GetString("savedTableID")}");

                if (rejoinResponse?.status == "ACTIVE" || rejoinResponse?.status == "RECONNECTABLE")
                {
                    await ConnectToServer(rejoinResponse.ws_url, rejoinResponse.ws_token);
                    return true;
                }

                attempts++;
            }

            return false; // Timeout
        }


        public async Task ConnectToServer(string wsUrl = null, string wsToken = null)
        {
            string serverUrl = wsUrl ?? GetServerUrl();

            // If token is provided, append as query parameter
            if (!string.IsNullOrEmpty(wsToken))
            {
                serverUrl = $"{serverUrl}?token={wsToken}";
            }

            if (WebSocketClient.Instance != null)
            {
                await WebSocketClient.Instance.Connect(serverUrl);
            }
        }

        private void SetupWebSocketListeners()
        {
            if (WebSocketClient.Instance != null)
            {
                WebSocketClient.Instance.OnConnected += OnWebSocketConnected;
                WebSocketClient.Instance.OnDisconnected += OnWebSocketDisconnected;
                WebSocketClient.Instance.OnMessageReceived += OnWebSocketMessage;
            }
        }

        private void OnWebSocketConnected()
        {
            isConnected = true;
            pingTimer = 0f;
            reconnectAttempts = 0;
            isReconnecting = false;

            Debug.Log("Successfully connected to WebSocket server");

            MainThreadDispatcher.Enqueue(async () =>
            {
                if (TeenpattiGameDataLobby.isRejoin || GameLiveData.wasReconnectFromGateway)
                {
                    // This is a reconnection - request table state directly
                    Debug.Log("Reconnect detected - requesting table state");
                    if (WebSocketServerRequest.Instance != null)
                    {
                        //join/rejoin was handled by http, just need to sync state
                        //await WebSocketServerRequest.Instance.RejoinGame(GameLiveData.instance.tableId);
                    }
                }
                else if (GameMode.mode == GameMode.Modes.publicGame)
                {
                    JoinMatchmaking();
                }
                else if (GameMode.mode == GameMode.Modes.privateGame)
                {
                    Debug.Log("about private table");
                    if (TeenpattiGameDataLobby.isRejoin)
                    {
                        // Rejoin private room if we were in one
                        if (TeenpattiGameDataLobby.createTable)
                        {
                            Debug.Log("crrr");
                            //CreatePrivateRoom(GameMode.tableEntryFee, TeenpattiGameDataLobby.teenpattiTableID_for_JOIN);
                        }
                        else
                        {
                            JoinPrivateTable(TeenpattiGameDataLobby.teenpattiTableID_for_JOIN ?? GameLiveData.instance.privateTableCode ?? PlayerPrefs.GetString("savedPrivateCode"));
                        }

                    }
                    else
                    {
                        // Rejoin private room if we were in one
                        if (TeenpattiGameDataLobby.createTable)
                        {
                            Debug.Log("crrr");
                            CreatePrivateRoom(GameMode.tableEntryFee, TeenpattiGameDataLobby.teenpattiTableID_for_JOIN);
                        }
                        else
                        {
                            JoinPrivateTable(TeenpattiGameDataLobby.teenpattiTableID_for_JOIN);
                        }

                    }
                }
            });
        }

        // private void OnWebSocketDisconnected(string reason)
        // {
        //     isConnected = false;
        //     Debug.Log($"Disconnected from server: {reason}");
        //     if (WebSocketClient.Instance.isConnecting) return; // Don't attempt reconnection if we were still trying to connect
        //     if (WebSocketClient.Instance.connected) return;
        //     // return;
        //     // Don't attempt reconnection if it was intentional or we're in lobby
        //     bool shouldReconnect = !reason.Contains("Intentional") &&
        //                           (GameLiveData.instance.isGameActive ||
        //                            SceneManager.GetActiveScene().name != "Lobby");

        //     if (shouldReconnect && reconnectAttempts < MAX_RECONNECT_ATTEMPTS)
        //     {
        //         reconnectAttempts++;
        //         isReconnecting = true;

        //         MainThreadDispatcher.Enqueue(async () =>
        //         {
        //             Debug.Log($"Attempting reconnection ({reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS})...");
        //             await Task.Delay((int)(RECONNECT_DELAY * 1000));
        //             await ConnectToServer();
        //         });
        //     }
        //     else if (isReconnecting)
        //     {
        //         // Failed to reconnect
        //         isReconnecting = false;
        //         MainThreadDispatcher.Enqueue(() =>
        //         {
        //             // Show connection lost message
        //             ShowConnectionError("Connection lost. Returning to lobby.");
        //             SceneManager.LoadScene("Lobby");
        //         });
        //     }
        // }


        private void OnWebSocketDisconnected(string reason)
        {
            isConnected = false;
            Debug.Log($"Disconnected from server: {reason}");

            // Trigger reconnect via gateway
            MainThreadDispatcher.Enqueue(async () =>
            {
                await HandleReconnection();
            });
        }

        private async Task HandleReconnection()
        {
            if (isReconnecting) return;
            isReconnecting = true;

            // Show reconnecting UI
            TeenpattiLobby.instance?.ShowReconnectOverlay(true);

            float backoff = 0.5f;
            int maxAttempts = 60; // ~5 minutes with max backoff

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    var savedTableID = PlayerPrefs.GetString("savedTableID", "");
                    var rejoinResponse = await ApiClient.Instance.Post<RejoinApiResponse>(
                        TeenPattiRoutes.RejoinGame,
                        new { tableID = savedTableID, userID = BootstrapLobbyAdapter.GetUserId() }
                    );

                    if (rejoinResponse?.status == "ACTIVE" || rejoinResponse?.status == "RECONNECTABLE")
                    {
                        await ConnectToServer(rejoinResponse.ws_url, rejoinResponse.ws_token);
                        TeenpattiLobby.instance?.ShowReconnectOverlay(false);
                        isReconnecting = false;
                        return;
                    }
                    else if (rejoinResponse?.status == "NONE")
                    {
                        // Session expired
                        TeenpattiLobby.instance?.ShowReconnectOverlay(false);
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            // Show "session ended" UI, navigate to lobby
                            SceneManager.LoadScene("Lobby");
                        });
                        isReconnecting = false;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Reconnect attempt {attempt + 1} failed: {ex.Message}");
                }

                await Task.Delay((int)(backoff * 1000));
                backoff = Mathf.Min(backoff * 2, 8f);
            }

            // Max attempts reached
            TeenpattiLobby.instance?.ShowReconnectOverlay(false);
            MainThreadDispatcher.Enqueue(() =>
            {
                // Show connection failed UI
            });
            isReconnecting = false;
        }
        private void OnWebSocketMessage(string message)
        {
            // Messages are handled by WebSocketMessageHandler
        }

        private string GetServerUrl()
        {
            // Configure based on your environment
            string protocol = Const.isSecure ? "wss://" : "ws://";
            string host = Const.teenpattiHost;
            string port = Const.teenpattiPort;

            return $"{protocol}{host}:{port}/ws";
        }

        #region Public Game Methods

        public async void JoinMatchmaking(int bootAmount = -1)
        {
            if (bootAmount == -1)
            {
                bootAmount = GameMode.tableEntryFee;
            }

            Debug.Log($"Joining matchmaking with boot amount: {bootAmount}");

            if (WebSocketServerRequest.Instance != null)
            {
                //join was handled by http
                //await WebSocketServerRequest.Instance.JoinGame(bootAmount);
            }
        }

        #endregion

        #region Private Room Methods

        public async void CreatePrivateRoom(int bootAmount, string roomName = "")
        {
            Debug.Log($"Creating private room with boot amount: {bootAmount}");

            TeenpattiGameDataLobby.createTable = true;
            isInPrivateRoom = true;

            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.CreatePrivateRoom(bootAmount);

                // // Update UI
                // MainThreadDispatcher.Enqueue(() =>
                // {
                //     if (TeenpattiLobby.instance != null)
                //     {
                //         TeenpattiLobby.instance.OnRoomCreated(privateCode);
                //     }
                // });
            }
        }

        private void OnRoomCreated(string privateCode)
        {
            currentPrivateCode = privateCode;
            TeenpattiGameDataLobby.teenpattiTableID_for_JOIN = privateCode;

            MainThreadDispatcher.Enqueue(() =>
            {
                if (TeenpattiLobby.instance != null)
                    TeenpattiLobby.instance.OnRoomCreated(privateCode);
            });
        }


        public async void JoinPrivateTable(string privateCode)
        {
            if (string.IsNullOrEmpty(privateCode))
            {
                Debug.LogError("Private code cannot be empty");
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (TeenpattiLobby.instance != null)
                    {
                        TeenpattiLobby.instance.HideRoomPanel();
                    }
                    SceneManager.LoadScene("Lobby");
                });
                return;
            }

            Debug.Log($"Joining private table with code: {privateCode}");
            currentPrivateCode = privateCode;
            isInPrivateRoom = true;

            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.JoinPrivateRoom(privateCode);
            }
        }

        public async void StartPrivateGame()
        {
            if (!isInPrivateRoom || string.IsNullOrEmpty(currentPrivateCode))
            {
                Debug.LogError("Not in a private room");
                return;
            }

            Debug.Log($"Starting private game in room: {currentPrivateCode}");

            // For the new backend, starting a game might just require reaching min players
            // or could be handled automatically. If you need to explicitly start:
            // await WebSocketServerRequest.Instance.SendAction("start_game");

            // For now, we'll just log it since the backend might auto-start
            Debug.Log("Game start initiated. Waiting for players...");

            MainThreadDispatcher.Enqueue(() =>
            {
                // Update UI
                if (TeenpattiLobby.instance != null)
                {
                    TeenpattiLobby.instance.ShowMatchmakingStatus("Waiting for more players to start...");
                }
            });
        }

        public async void LeavePrivateRoom()
        {
            if (!isInPrivateRoom)
            {
                Debug.LogError("Not in a private room");
                return;
            }

            Debug.Log($"Leaving private room: {currentPrivateCode}");

            // Send leave message
            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.LeaveTable();
            }

            // Clean up
            isInPrivateRoom = false;
            currentPrivateCode = "";

            MainThreadDispatcher.Enqueue(() =>
            {
                // Return to lobby
                if (TeenpattiLobby.instance != null)
                {
                    TeenpattiLobby.instance.HideRoomPanel();
                }
                SceneManager.LoadScene("Lobby");
            });
        }

        private string GeneratePrivateCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            System.Random random = new System.Random();
            char[] code = new char[6];

            for (int i = 0; i < 6; i++)
            {
                code[i] = chars[random.Next(chars.Length)];
            }

            return new string(code);
        }

        #endregion

        #region Game Action Methods

        // public async void RejoinGame()
        // {
        //     Debug.Log("Attempting to rejoin game...");

        //     if (WebSocketServerRequest.Instance != null)
        //     {
        //         await WebSocketServerRequest.Instance.RejoinGame();
        //     }
        // }

        public async void SendGameAction(string action, int amount = 0, string targetId = null, bool accept = false)
        {
            if (!isConnected)
            {
                Debug.LogError("Cannot send action, not connected");
                return;
            }

            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.SendAction(action, amount, targetId, accept);
            }
        }

        public async void LeaveTable()
        {
            Debug.Log("Leaving current table...");

            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.LeaveTable();
            }

            // Clean up
            GameLiveData.instance.ResetGameData();
            isInPrivateRoom = false;
            currentPrivateCode = "";
            lastTableId = "";

            // Return to lobby
            MainThreadDispatcher.Enqueue(() =>
            {
                SceneManager.LoadScene("Lobby");
            });
        }

        #endregion

        #region Utility Methods

        private async void SendPing()
        {
            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.SendPing();
            }
        }

        public bool IsConnected()
        {
            return isConnected;
        }

        public bool IsInPrivateRoom()
        {
            return isInPrivateRoom;
        }

        public string GetCurrentPrivateCode()
        {
            return currentPrivateCode;
        }

        private void ShowConnectionError(string message)
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                // You can create a proper error popup UI
                Debug.LogError(message);

                // For now, just log to console
                if (TeenpattiLobby.instance != null && TeenpattiLobby.instance.matchmakingStatusText != null)
                {
                    TeenpattiLobby.instance.matchmakingStatusText.text = message;
                    TeenpattiLobby.instance.matchmakingStatusPanel.SetActive(true);
                }
            });
        }

        public void UpdateRoomPlayerCount(int playerCount)
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                if (TeenpattiLobby.instance != null)
                {
                    TeenpattiLobby.instance.OnPlayerJoined(playerCount);
                }
            });
        }

        public void HandlePlayerJoinedRoom(string playerName, int playerCount)
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                Debug.Log($"{playerName} joined the room. Total players: {playerCount}");

                if (TeenpattiLobby.instance != null)
                {
                    TeenpattiLobby.instance.OnPlayerJoined(playerCount);

                    // Show notification
                    if (playerCount >= 2 && isInPrivateRoom)
                    {
                        TeenpattiLobby.instance.startGameBtn.SetActive(true);
                    }
                }
            });
        }

        public void HandlePlayerLeftRoom(string playerName, int playerCount)
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                Debug.Log($"{playerName} left the room. Total players: {playerCount}");

                if (TeenpattiLobby.instance != null)
                {
                    TeenpattiLobby.instance.OnPlayerJoined(playerCount);

                    // Hide start button if not enough players
                    if (playerCount < 2 && isInPrivateRoom)
                    {
                        TeenpattiLobby.instance.startGameBtn.SetActive(false);
                    }
                }
            });
        }

        #endregion

        #region Application Lifecycle

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        // private void OnApplicationPause(bool pauseStatus)
        // {
        //     if (pauseStatus)
        //     {
        //         // Handle app going to background
        //         Debug.Log("Application paused - handling disconnection");
        //         Disconnect();
        //     }
        //     else
        //     {
        //         // Handle app coming back to foreground
        //         Debug.Log("Application resumed - attempting reconnection");

        //         // Wait a bit before reconnecting
        //         MainThreadDispatcher.Enqueue(async () =>
        //         {
        //             await Task.Delay(1000);

        //             if (!isConnected)
        //             {
        //                 await ConnectToServer();
        //             }
        //         });
        //     }
        // }

        private CancellationTokenSource _pauseCancelToken;

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Debug.Log("Application paused - starting grace period");

                // Cancel any previous pending disconnect
                _pauseCancelToken?.Cancel();
                _pauseCancelToken = new CancellationTokenSource();

                // Run async logic via your existing dispatcher pattern
                MainThreadDispatcher.Enqueue(async () =>
                {
                    try
                    {
                        if (GameLiveData.instance.isPrivateTable)
                        {
                            PlayerPrefs.GetString("savedPrivateCode", GameLiveData.instance.privateTableCode);
                        }
                        await Task.Delay(600000, _pauseCancelToken.Token);

                        // Still paused after 10s — now disconnect
                        Debug.Log("Grace period expired, disconnecting");
                        //Disconnect();
                    }
                    catch (TaskCanceledException)
                    {
                        // Resumed within grace period — stay connected, do nothing
                        Debug.Log("Resumed within grace period, staying connected");
                    }
                });
            }
            else
            {
                Debug.Log("Application resumed - cancelling pending disconnect");

                // Cancel the pending disconnect timer
                _pauseCancelToken?.Cancel();
                _pauseCancelToken = null;

                MainThreadDispatcher.Enqueue(async () =>
                {
                    await Task.Delay(500); // small settle delay

                    if (!isConnected)
                    {
                        Debug.Log("Was disconnected, rejoining...");
                        //GameManager.Instance.OnLobby();
                        //SceneManager.LoadScene(1);
                        TeenpattiGameDataLobby.createTable = false;
                        TeenpattiGameDataLobby.isRejoin = true;
                        await ReconnectAndRejoin();
                    }
                    else
                    {
                        Debug.Log("Still connected, no action needed");
                    }
                });
            }
        }

        private async Task ReconnectAndRejoin()
        {
            // Load saved state
            string savedTableID = PlayerPrefs.GetString("savedTableID", "");
            string savedPrivateCode = PlayerPrefs.GetString("savedPrivateCode", "");


            await ConnectToServer();

            if (!string.IsNullOrEmpty(savedTableID))
            {
                Debug.Log($"Rejoining saved table: {savedTableID}");
                //await RejoinGame(savedTableID, userID);
            }
            else
            {
                Debug.Log("No saved table, going to lobby");
                // navigate to lobby or whatever your normal flow is
            }
        }

        public async void Disconnect()
        {
            if (WebSocketClient.Instance != null)
            {
                await WebSocketClient.Instance.Disconnect();
            }
            isConnected = false;
            isInPrivateRoom = false;
            currentPrivateCode = "";
        }

        #endregion

        #region UI Integration Helpers

        // These methods help integrate with your existing UI

        public void CopyPrivateCodeToClipboard()
        {
            if (!string.IsNullOrEmpty(currentPrivateCode))
            {
                GUIUtility.systemCopyBuffer = currentPrivateCode;
                Debug.Log($"Copied private code to clipboard: {currentPrivateCode}");

                MainThreadDispatcher.Enqueue(() =>
                {
                    // Show confirmation
                    if (TeenpattiLobby.instance != null)
                    {
                        // You might want to add a "Copied!" notification
                    }
                });
            }
        }

        public void SetTableEntryFee(float amount)
        {
            GameMode.tableEntryFee = (int)amount;
        }

        public void SetGameMode(GameMode.Modes mode)
        {
            GameMode.mode = mode;
        }



        #endregion
    }

    // internal class TeenPattiJoinRequest
    // {
    //     public string userID { get; set; }
    //     public string username { get; set; }
    //     public int bootAmount { get; set; }
    //     public string privateCode { get; set; }
    //     public int chips { get; set; }
    // }

    class TeenPattiJoinRequest
    {
        public int buyin { get; set; }
        public int boot_amount { get; set; }
        public string gameType { get; set; }
        public string privateCode { get; set; }
        public string attemptID { get; set; }
    }
}