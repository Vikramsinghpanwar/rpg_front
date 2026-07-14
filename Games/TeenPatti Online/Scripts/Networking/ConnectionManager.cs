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
using Core.Bootstrap;
using Newtonsoft.Json;

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
        private CancellationTokenSource _reconnectCts;

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
            if (Input.GetKeyDown(KeyCode.W))
            {
                OnApplicationPause(true);
                Disconnect();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                OnApplicationPause(false);
            }

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
            Debug.Log($"Attempting to join via gateway: bootAmount={bootAmount}, privateCode={(string.IsNullOrEmpty(privateCode) ? "none" : privateCode)}");
            if (GameMode.mode == GameMode.Modes.privateGame)
            {
                if (TeenpattiGameDataLobby.createTable)
                {
                    return await CreatePrivateRoomViaGateway(bootAmount);
                }
                else if (!string.IsNullOrEmpty(privateCode))
                {
                    return await JoinPrivateRoomViaGateway(bootAmount, privateCode);
                }
            }
            Debug.Log("Joining public game via gateway");

            var request = new TeenPattiJoinRequest
            {
                boot_amount = bootAmount,
                buyin = BootstrapService.Instance.Wallet.available_balance / 100,
                gameType = "public",
                attemptID = Guid.NewGuid().ToString()
            };

            try
            {
                var response = await ApiClient.Instance.Post<JoinApiResponse>(TeenPattiRoutes.JoinGame, request);
                return await ProcessJoinResponse(response);
            }
            catch (ApiException ex)
            {
                Debug.LogError($"Join API failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> CreatePrivateRoomViaGateway(int bootAmount)
        {
            Debug.Log($"Creating private room via gateway: bootAmount={bootAmount}");
            var request = new TeenPattiJoinRequest
            {
                boot_amount = bootAmount,
                buyin = (BootstrapService.Instance.Wallet.available_balance - BootstrapService.Instance.Wallet.bonus_balance) / 100,
                gameType = "private",
                attemptID = Guid.NewGuid().ToString()
            };

            try
            {
                var response = await ApiClient.Instance.Post<PrivateCreateApiResponse>(TeenPattiRoutes.PrivateRoomCreate, request);
                if (response == null)
                {
                    Debug.LogError("Private room create API returned null");
                    return false;
                }

                isInPrivateRoom = true;
                currentPrivateCode = response.private_code;
                TeenpattiGameDataLobby.teenpattiTableID_for_JOIN = response.private_code;
                PlayerPrefs.SetString("savedPrivateCode", response.private_code);

                MainThreadDispatcher.Enqueue(() =>
                {
                    if (TeenpattiLobby.instance != null)
                        TeenpattiLobby.instance.OnRoomCreated(response.private_code);
                });

                GameLiveData.instance.tableId = response.session.table_id;
                Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Backend] Private room created: tableID={response.session.table_id}, wsUrl={response.session.ws_url}");
                await ConnectToServer(response.session.ws_url, response.session.ws_token);
                return true;
            }
            catch (ApiException ex)
            {
                Debug.LogError($"Private room create API failed: {ex.Message}");
                Toast.Instance.ShowError("Failed to create private room : " + ex.Message);
                return false;
            }
        }

        private async Task<bool> JoinPrivateRoomViaGateway(int bootAmount, string privateCode)
        {
            var request = new TeenPattiJoinRequest
            {
                boot_amount = bootAmount,
                buyin = (BootstrapService.Instance.Wallet.available_balance - BootstrapService.Instance.Wallet.bonus_balance) / 100,
                gameType = "private",
                private_code = privateCode,
                attemptID = Guid.NewGuid().ToString()
            };

            try
            {
                var response = await ApiClient.Instance.Post<JoinApiResponse>(TeenPattiRoutes.PrivateRoomJoin, request);
                isInPrivateRoom = true;
                currentPrivateCode = privateCode;
                PlayerPrefs.SetString("savedPrivateCode", privateCode);
                return await ProcessJoinResponse(response);
            }
            catch (ApiException ex)
            {
                Debug.LogError($"Private room join API failed: {ex.Message}");
                Toast.Instance.ShowError("Failed to join private room : " + ex.Message);
                return false;
            }
        }

        private async Task<bool> ProcessJoinResponse(JoinApiResponse response)
        {
            if (response == null)
            {
                Debug.LogError("Join API returned null");
                return false;
            }

            GameLiveData.wasReconnectFromGateway = response.was_reconnect;
            GameLiveData.instance.tableId = response.table_id;

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
                    return await PollForReconnect(GameMode.tableEntryFee, currentPrivateCode);
                case "NONE":
                    Debug.Log("No active game session found");
                    return false;

                default:
                    Debug.LogError($"Unknown join status: {response.status}");
                    return false;
            }
        }

        private async Task<bool> PollForReconnect(int bootAmount, string privateCode)
        {
            int attempts = 0;
            const int maxAttempts = 3;
            const float delayMs = 500f;

            while (attempts < maxAttempts)
            {
                await Task.Delay((int)delayMs);

                string url = $"{TeenPattiRoutes.RejoinGame}";
                Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Backend] PollForReconnect GET: {url}");

                var rejoinResponse = await ApiClient.Instance.Get<RejoinApiResponse>(url);

                if (rejoinResponse?.status == "ACTIVE" || rejoinResponse?.status == "RECONNECTABLE")
                {
                    await ConnectToServer(rejoinResponse.ws_url, rejoinResponse.ws_token);
                    return true;
                }

                attempts++;
            }

            Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Backend] PollForReconnect timed out after {maxAttempts} attempts");
            return false; // Timeout
        }


        public async Task ConnectToServer(string wsUrl = null, string wsToken = null)
        {
            string ts = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.Log($"[{ts}] [WS] ConnectToServer called: wsUrl={(string.IsNullOrEmpty(wsUrl) ? "null" : wsUrl)}, wsToken={(string.IsNullOrEmpty(wsToken) ? "null/empty" : "present")}");

            string serverUrl = wsUrl;

            if (!string.IsNullOrEmpty(wsToken))
            {
                serverUrl = $"{serverUrl}?token={wsToken}";
            }

            if (WebSocketClient.Instance != null)
            {
                await WebSocketClient.Instance.Connect(serverUrl);
            }
            else
            {
                Debug.LogError($"[{ts}] [WS] ConnectToServer failed: WebSocketClient.Instance is null");
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
            string ts = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.Log($"[{ts}] [WS] OnWebSocketConnected");
            isConnected = true;
            pingTimer = 0f;
            reconnectAttempts = 0;
            isReconnecting = false;

            bool wasRejoin = TeenpattiGameDataLobby.isRejoin;
            TeenpattiGameDataLobby.isRejoin = false;
            GameLiveData.wasReconnectFromGateway = false;

            Debug.Log("Successfully connected to WebSocket server");

            MainThreadDispatcher.Enqueue(async () =>
            {
                GameManager.Instance?.ShowReconnectOverlay(false);
                if (wasRejoin)
                {
                    Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [WS] Reconnect completed; table state will arrive from server");
                    if (WebSocketServerRequest.Instance != null)
                    {
                    }
                }
                else if (GameMode.mode == GameMode.Modes.publicGame)
                {
                }
                else if (GameMode.mode == GameMode.Modes.privateGame)
                {
                    Debug.Log("about private table");
                    // if (TeenpattiGameDataLobby.isRejoin)
                    // {
                    //     if (TeenpattiGameDataLobby.createTable)
                    //     {
                    //         Debug.Log("crrr");
                    //     }
                    //     else
                    //     {
                    //         JoinPrivateTable(TeenpattiGameDataLobby.teenpattiTableID_for_JOIN ?? GameLiveData.instance.privateTableCode ?? PlayerPrefs.GetString("savedPrivateCode"));
                    //     }

                    // }
                    // else
                    // {
                    //     if (TeenpattiGameDataLobby.createTable)
                    //     {
                    //         Debug.Log("crrr");
                    //         CreatePrivateRoom(GameMode.tableEntryFee, TeenpattiGameDataLobby.teenpattiTableID_for_JOIN);
                    //     }
                    //     else
                    //     {
                    //         JoinPrivateTable(TeenpattiGameDataLobby.teenpattiTableID_for_JOIN);
                    //     }

                    // }
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
            string ts = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.Log($"[{ts}] [WS] OnWebSocketDisconnected: {reason}");
            isConnected = false;
            if (reason.Contains("Intentional"))
            {
                Debug.Log($"[{ts}] [WS] Disconnection was intentional, no reconnection will be attempted");
                return;
            }
            if (SceneManager.GetActiveScene().name == "Lobby")
            {
                Debug.Log($"[{ts}] [WS] Disconnection occurred in Lobby scene, no reconnection will be attempted");
                return;
            }
            MainThreadDispatcher.Enqueue(async () =>
            {
                if (WebSocketClient.Instance.isConnecting) return; // Don't attempt reconnection if we were still trying to connect

                Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Reconnect] OnWebSocketDisconnected → calling HandleReconnection()");
                await HandleReconnection();
            });
        }

        private void OnDestroy()
        {
            if (WebSocketClient.Instance != null)
            {
                WebSocketClient.Instance.OnConnected -= OnWebSocketConnected;
                WebSocketClient.Instance.OnDisconnected -= OnWebSocketDisconnected;
                WebSocketClient.Instance.OnMessageReceived -= OnWebSocketMessage;
            }
            _pauseCancelToken?.Cancel();
            _pauseCancelToken = null;
            _reconnectCts?.Cancel();
            isReconnecting = false;
            MainThreadDispatcher.Reset();
        }

        private async Task HandleReconnection()
        {
            _reconnectCts?.Cancel();
            _reconnectCts = new CancellationTokenSource();
            var token = _reconnectCts.Token;

            string ts = DateTime.Now.ToString("HH:mm:ss.fff");
            if (isReconnecting)
            {
                Debug.Log($"[{ts}] [Reconnect] HandleReconnection skipped - already reconnecting");
                return;
            }
            isReconnecting = true;
            Debug.Log($"[{ts}] [Reconnect] HandleReconnection started");

            GameManager.Instance?.ShowReconnectOverlay(true);

            float backoff = 0.5f;
            int maxAttempts = 60;

            try
            {
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Backend] POST /tgs/rejoin attempt {attempt + 1}");

                        var rejoinResponse = await ApiClient.Instance.Post<RejoinApiResponse>(
                            TeenPattiRoutes.RejoinGame
                        );

                        Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Backend] Rejoin response: status={rejoinResponse?.status ?? "null"}, wsUrl={rejoinResponse?.ws_url ?? "null"}, wsToken={(string.IsNullOrEmpty(rejoinResponse?.ws_token) ? "MISSING" : "present")}, table_id={rejoinResponse?.table_id ?? "null"}");

                        if (rejoinResponse?.status == "ACTIVE" || rejoinResponse?.status == "RECONNECTABLE")
                        {
                            await ConnectToServer(rejoinResponse.ws_url, rejoinResponse.ws_token);
                            GameManager.Instance?.ShowReconnectOverlay(false);
                            isReconnecting = false;
                            Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Reconnect] Reconnect completed successfully");
                            return;
                        }
                        else if (rejoinResponse?.status == "NONE")
                        {
                            Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Reconnect] Backend returned NONE - session expired");
                            GameManager.Instance?.ShowReconnectOverlay(false);
                            MainThreadDispatcher.Enqueue(() =>
                            {
                                SceneManager.LoadScene("Lobby");
                            });
                            isReconnecting = false;
                            return;
                        }
                    }
                    catch (ApiException ex)
                    {
                        Debug.LogError($"[{DateTime.Now:HH:mm:ss.fff}] [Reconnect] HTTP error on attempt {attempt + 1}: status={ex.StatusCode}, code={ex.ErrorCode}, body={ex.RawBody}");
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Reconnect] Reconnect cancelled during attempt {attempt + 1}");
                        isReconnecting = false;
                        return;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[{DateTime.Now:HH:mm:ss.fff}] [Reconnect] Attempt {attempt + 1} failed: {ex.Message}");
                    }

                    await Task.Delay((int)(backoff * 1000), token);
                    token.ThrowIfCancellationRequested();
                    backoff = Mathf.Min(backoff * 2, 8f);
                }

                Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Reconnect] HandleReconnection failed - max attempts reached");
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Reconnect] HandleReconnection cancelled");
            }
            finally
            {
                GameManager.Instance?.ShowReconnectOverlay(false);
                isReconnecting = false;
            }
        }
        private void OnWebSocketMessage(string message)
        {
            // Messages are handled by WebSocketMessageHandler
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

            Debug.Log("Leaving private room");

            bool left = await LeaveTableViaGateway();
            if (!left)
            {
                Debug.LogWarning("Leave endpoint failed, disconnecting anyway");
            }

            CleanupAfterLeave();
        }

        private async Task<bool> LeaveTableViaGateway()
        {
            var tableId = GameLiveData.instance?.tableId ?? "";
            var userId = BootstrapLobbyAdapter.GetUserId();

            var payload = new
            {
                session_id = tableId,
                user_id = userId
            };

            Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Backend] POST /tgs/leave with payload: {JsonConvert.SerializeObject(payload)}");

            try
            {
                Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Backend] Sending leave request to {TeenPattiRoutes.LeaveTable}");
                var response = await ApiClient.Instance.Post<LeaveApiResponse>(TeenPattiRoutes.LeaveTable, payload);
                if (response == null)
                {
                    Debug.LogError("Leave API returned null");
                    return false;
                }

                string status = response.status;
                Debug.Log($"Leave API response status: {status}");
                return status == "cancelled" || status == "no_active_session";
            }
            catch (ApiException ex)
            {
                Debug.LogError($"Leave API failed: {ex.Message}");
                return false;
            }
        }

        private void CleanupAfterLeave()
        {
            if (WebSocketClient.Instance != null)
            {
                _ = WebSocketClient.Instance.Disconnect();
            }

            isConnected = false;
            isInPrivateRoom = false;
            currentPrivateCode = "";
            lastTableId = "";

            if (GameLiveData.instance != null)
            {
                GameLiveData.instance.ResetGameData();
                GameLiveData.instance.isSpectator = false;
                GameLiveData.instance.rejoin = false;
                GameLiveData.instance.isPrivateTable = false;
                GameLiveData.instance.privateTableCode = "";
            }

            PlayerPrefs.DeleteKey("savedPrivateCode");

            MainThreadDispatcher.Enqueue(() =>
            {
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
            Debug.Log("Leaving current table via gateway...");

            bool left = await LeaveTableViaGateway();
            Debug.Log($"Leave table response: {(left ? "success" : "failed")}");
            if (!left)
            {
                Debug.LogWarning("Leave endpoint failed, disconnecting anyway");
            }
            Debug.Log("Disconnecting from WebSocket server...");
            Loader.Instance.HideLoading();

            CleanupAfterLeave();
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
            Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Lifecycle] OnApplicationQuit");
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
            string ts = DateTime.Now.ToString("HH:mm:ss.fff");
            if (pauseStatus)
            {
                Debug.Log($"[{ts}] [Lifecycle] OnApplicationPause(true) - App backgrounded");

                _pauseCancelToken?.Cancel();
                _pauseCancelToken = new CancellationTokenSource();

                MainThreadDispatcher.Enqueue(async () =>
                {
                    try
                    {
                        if (GameLiveData.instance.isPrivateTable)
                        {
                            PlayerPrefs.SetString("savedPrivateCode", GameLiveData.instance.privateTableCode);
                        }
                        await Task.Delay(600000, _pauseCancelToken.Token);

                        Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Lifecycle] Grace period expired, disconnecting");
                        //Disconnect();
                    }
                    catch (TaskCanceledException)
                    {
                        Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Lifecycle] Resumed within grace period, staying connected");
                    }
                });
            }
            else
            {
                Debug.Log($"[{ts}] [Lifecycle] OnApplicationPause(false) - App resumed");
                _pauseCancelToken?.Cancel();
                _pauseCancelToken = null;

                MainThreadDispatcher.Enqueue(async () =>
                {
                    await Task.Delay(500);

                    string wsStateStr = WebSocketClient.Instance?.websocket != null
                        ? WebSocketClient.Instance.websocket.State.ToString()
                        : "null";
                    Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [Resume] isConnected={isConnected}, WebSocketState={wsStateStr}");

                    if (!isConnected)
                    {
                        Debug.Log($"[{ts}] [Resume] Reconnect path: ReconnectAndRejoin()");
                        TeenpattiGameDataLobby.createTable = false;
                        TeenpattiGameDataLobby.isRejoin = true;
                        GameLiveData.wasReconnectFromGateway = false;
                        await ReconnectAndRejoin();
                    }
                    else
                    {
                        Debug.Log($"[{ts}] [Resume] No reconnect attempted (isConnected=true)");
                    }
                });
            }
        }

        private async Task ReconnectAndRejoin()
        {
            await HandleReconnection();
        }

        public async void Disconnect()
        {
            _reconnectCts?.Cancel();
            _pauseCancelToken?.Cancel();
            _pauseCancelToken = null;

            if (WebSocketClient.Instance != null)
            {
                await WebSocketClient.Instance.Disconnect();
            }
            isConnected = false;
            isReconnecting = false;
            reconnectAttempts = 0;
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
        public long buyin { get; set; }
        public int boot_amount { get; set; }
        public string gameType { get; set; }
        public string private_code { get; set; }
        public string attemptID { get; set; }
    }

    class PrivateCreateApiResponse
    {
        public string private_code { get; set; }
        public SessionData session { get; set; }
    }

    class SessionData
    {
        public string session_id { get; set; }
        public string table_id { get; set; }
        public string ws_url { get; set; }
        public string ws_token { get; set; }
        public string status { get; set; }
    }

    class LeaveApiResponse
    {
        public string status;
    }
}