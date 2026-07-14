// WebSocketMessageHandler.cs
using System;
using System.Collections;
using System.Collections.Generic;
using Features.Lobby.Integration;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Core.Utils;
using Core.Bootstrap;

namespace Teenpatti
{
    [System.Serializable]
    public class WSMessage
    {
        public string type;
        public string data; // JSON string
    }

    public class WebSocketMessageHandler : MonoBehaviour
    {
        public static WebSocketMessageHandler Instance { get; private set; }
        private Dictionary<string, Action<string>> messageHandlers;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeMessageHandlers();


        }

        private void Start()
        {
            StartCoroutine(WaitForWebSocket());
        }

        private IEnumerator WaitForWebSocket()
        {
            while (WebSocketClient.Instance == null)
            {
                yield return null;
            }

            WebSocketClient.Instance.OnMessageReceived += HandleMessage;
        }

        private void InitializeMessageHandlers()
        {
            messageHandlers = new Dictionary<string, Action<string>>
            {
                ["joined"] = HandleJoined<JoinedResponse>,
                ["roomCreated"] = HandlePrivateRoomCreated<JoinedResponse>,
                ["rejoined"] = HandleRejoined<RejoinedResponse>,
                ["spectator_state"] = HandleSpectatorState<SpectatorStateResponse>,
                ["spectator_joined"] = HandleSpectatorJoined<SpectatorJoinedData>,
                ["player_joined"] = HandlePlayerJoined<PlayerJoinedData>,
                ["spectator_left"] = HandleSpectatorLeft<SpectatorLeftData>,
                ["game_started"] = HandleGameStarted<GameStartedResponse>,
                ["cards_dealt"] = HandleCardsDealt<CardsDealtResponse>,
                ["action_update"] = HandleActionUpdate<ActionUpdateResponse>,
                ["gift_update"] = HandleGiftUpdate<GiftUpdateResponse>,
                ["player_update"] = HandlePlayerUpdate<PlayerState>,
                ["imBackSuccessful"] = HandlePlayerBack<PlayerState>,
                ["turn_update"] = HandleTurnUpdate<TurnUpdateResponse>,
                ["player_recharging"] = HandleRechargeUpdate<PlayerRechargingResponse>,
                ["player_recharged"] = HandlePlayerRecharged<PlayerRechargedResponse>,
                ["recharge_success"] = HandleRechargeSuccess<RechargeSuccessResponse>,
                ["player_recharged"] = HandlePlayerRecharged<RechargeSuccessResponse>,
                ["table_state"] = HandleTableState<TableStateResponse>,
                ["bet_update"] = HandleBetUpdate,
                ["show_down"] = HandleShowdownResult<ShowDownResult>,
                ["side_show_request"] = HandleSideShowRequest<SideShowRequestData>,
                ["side_show_result"] = HandleSideShowResult<SideShowResultData>,
                ["game_ended"] = HandleGameEnded<GameEndedData>,
                ["action_accepted"] = HandleActionAccepted<ActionAcceptedData>,
                ["left"] = HandleLeft<LeftResponse>,
                ["pot_limit_reached"] = HandlePotLimitReached<PotLimitReachedResponse>,
                ["player_disconnected"] = HandlePlayerDisconnected<PlayerDisconnectedResponse>,
                ["player_left"] = HandleLeft<LeftResponse>,
                ["error"] = HandleError<ErrorResponse>,
                ["pong"] = HandlePong
            };
        }


        [Serializable]
        public class WSBaseMessage
        {
            public string type;
            public WSBaseMSGData data;
        }

        [Serializable]
        public class WSBaseMSGData
        {
            public string message;
        }


        [Serializable]
        public class WSMessage<T>
        {
            public string type;
            public T data;
        }

        private void HandleMessage(string json)
        {
            Debug.Log("json: " + json);
            WSBaseMessage baseMsg;
            try
            {
                baseMsg = JsonUtility.FromJson<WSBaseMessage>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Base parse failed: " + e.Message);
                return;
            }

            if (baseMsg == null || string.IsNullOrEmpty(baseMsg.type))
            {
                Debug.LogError("Invalid WS message (missing type)");
                return;
            }


            if (messageHandlers.TryGetValue(baseMsg.type, out var handler))
            {
                Debug.Log("raw msg: " + json);
                if (baseMsg.type == "error")
                {
                    Logger.Instance.Log(baseMsg.data.message);
                    if (baseMsg.data.message.Contains("Insufficient balance"))
                    {
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            if (ActionButtons.instance != null)
                                ActionButtons.instance.InsufficientFunds();
                        });
                    }
                }
                handler(json);
            }
            else
            {
                Debug.LogWarning("Unhandled WS type: " + baseMsg.type);
            }
        }




        #region Message Handlers

        private void HandlePrivateRoomCreated<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Typed parse failed: {e.Message}");
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                JoinedResponse roomData = JsonUtility.FromJson<JoinedResponse>(data);
                if (roomData == null)
                {
                    Debug.LogError("Failed to parse roomCreated response as JoinedResponse");
                    return;
                }

                MainThreadDispatcher.Enqueue(() =>
                {
                    ConnectionManager.Instance.GetCurrentPrivateCode();
                    GameLiveData.instance.isGameActive = true;
                    GameLiveData.instance.privateTableCode = roomData.privateCode;
                    GameLiveData.instance.tableId = roomData.tableID;
                    GameLiveData.instance.tableState = roomData.status;

                    if (TeenpattiLobby.instance != null)
                    {
                        if (!string.IsNullOrEmpty(roomData.privateCode))
                        {
                            TeenpattiLobby.instance.OnRoomCreated(roomData.privateCode);
                        }
                    }

                    if (roomData.status == "waiting")
                    {
                        SceneManager.LoadSceneAsync("Teenpatti");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling roomCreated: {ex.Message}");
            }
        }

        private void HandleJoined<T>(string json)
        {

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Typed parse failed: {e.Message}");
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var joinData = JsonUtility.FromJson<JoinedResponse>(data);
                if (joinData != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        GameLiveData.instance.isGameActive = true;
                        GameLiveData.instance.tableId = joinData.tableID;

                        if (!string.IsNullOrEmpty(joinData.privateCode))
                        {
                            GameLiveData.instance.privateTableCode = joinData.privateCode;
                        }

                        if (joinData.status == "waiting" || joinData.status == "starting")
                        {
                            if (TeenpattiLobby.instance != null)
                            {
                                TeenpattiLobby.instance.ShowRoomPanel(joinData.tableID, 1);
                            }
                            SceneManager.LoadSceneAsync("Teenpatti");
                        }
                        else if (joinData.status == "playing")
                        {
                            if (SceneManager.GetActiveScene().name != "Teenpatti")
                            {
                                SceneManager.LoadSceneAsync("Teenpatti");
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling joined: {ex.Message}");
            }
        }

        private void HandleRejoined<T>(string json)
        {

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);


            try
            {
                var rejoinData = JsonUtility.FromJson<RejoinedResponse>(data);
                if (rejoinData != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        GameLiveData.instance.tableId = rejoinData.tableID;

                        // Restore complete game state
                        if (rejoinData.gameState != null)
                        {
                            RestoreGameState(rejoinData.gameState);
                        }

                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling rejoined: {ex.Message}");
            }
        }

        // In WebSocketMessageHandler.cs, add these methods:

        // private void HandleSpectatorState<T>(string json)
        // {
        //     WSMessage<T> msg;

        //     try
        //     {
        //         msg = JsonUtility.FromJson<WSMessage<T>>(json);
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError("Typed parse failed: " + e.Message);
        //         return;
        //     }

        //     if (msg == null || msg.data == null)
        //     {
        //         Debug.LogError("Message data is null");
        //         return;
        //     }

        //     string data = JsonUtility.ToJson(msg.data);

        //     try
        //     {
        //         var spectatorState = JsonUtility.FromJson<SpectatorStateResponse>(data);
        //         if (spectatorState != null)
        //         {
        //             MainThreadDispatcher.Enqueue(() =>
        //             {
        //                 // Update spectator count display
        //                 if (GameManager.Instance != null && GameManager.Instance.spectatorCountText != null)
        //                 {
        //                     GameManager.Instance.spectatorCountText.text = 
        //                         $"Spectators: {spectatorState.spectators}";
        //                 }

        //                 // Mark players as spectators in the UI
        //                 if (spectatorState.players != null && GameManager.Instance != null)
        //                 {
        //                     foreach (var playerState in spectatorState.players)
        //                     {
        //                         var player = GameManager.Instance.GetPlayerByID(playerState.UserID);
        //                         if (player != null)
        //                         {
        //                             player.MarkAsSpectator(playerState.IsSpectator);
        //                         }
        //                     }
        //                 }
        //             });
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogError($"Error handling spectator_state: {ex.Message}");
        //     }
        // }

        private void HandlePlayerJoined<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var PlayerJoined = JsonUtility.FromJson<PlayerJoinedData>(data);
                if (PlayerJoined != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        if (GameManager.Instance != null)
                        {
                            PlayerManager player = GameManager.Instance.playersList[PlayerJoined.position];
                            player.PopulateWithPlayer(
                                0,
                                PlayerJoined.username,
                                PlayerJoined.profileImageUrl
                            );
                            player.myId = PlayerJoined.userID;
                            player.nameTxt.text = PlayerJoined.username;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling spectator_joined: {ex.Message}");
            }
        }
        private void HandleSpectatorJoined<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var spectatorJoined = JsonUtility.FromJson<SpectatorJoinedData>(data);
                if (spectatorJoined == null) return;

                MainThreadDispatcher.Enqueue(() =>
                {
                    if (GameManager.Instance == null) return;

                    int serverPosition = spectatorJoined.position;
                    int totalPlayers = GameManager.Instance.playersList.Count;

                    // Replicate the same pivot logic used in UpdatePlayers
                    int localPivot = GameLiveData.instance.localPlayerPivotIndex;
                    int rotatedIndex = (serverPosition - localPivot + totalPlayers) % totalPlayers;
                    if (localPivot < 0)
                    {
                        Debug.LogError("Could not find local player pivot for spectator mapping");
                        return;
                    }

                    // Map server position → rotated UI index

                    // Safety: never allow overwriting index 0 (local player's slot)
                    if (rotatedIndex == 0)
                    {
                        Debug.LogWarning($"Spectator at server pos {serverPosition} mapped to index 0 — skipping to avoid overwriting local player.");
                        return;
                    }

                    if (rotatedIndex < 0 || rotatedIndex >= totalPlayers)
                    {
                        Debug.LogError($"Spectator rotated index {rotatedIndex} out of range");
                        return;
                    }

                    GameManager.Instance.playersList[rotatedIndex].MarkAsSpectator(true);
                    GameManager.Instance.playersList[rotatedIndex].myId = spectatorJoined.userID;
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling spectator_joined: {ex.Message}");
            }
        }

        private void HandleSpectatorLeft<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var spectatorLeft = JsonUtility.FromJson<SpectatorLeftData>(data);
                if (spectatorLeft != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // Update spectator count
                        if (GameManager.Instance != null)
                        {
                            foreach (var player in GameManager.Instance.playersList)
                            {
                                if (player.myId == spectatorLeft.userID)
                                {
                                    player.MarkAsSpectator(false);
                                    player.SetVacant(true);
                                }
                            }
                        }


                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling spectator_left: {ex.Message}");
            }
        }

        private void HandleGameStarted<T>(string json)
        {

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var gameStartData = JsonUtility.FromJson<GameStartedResponse>(data);
                if (gameStartData != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        GameLiveData.instance.ResetGameData();
                        // Convert to PlayerDetails array
                        PlayerDetails[] playerDetails = new PlayerDetails[gameStartData.players.Length];
                        for (int i = 0; i < gameStartData.players.Length; i++)
                        {
                            var playerState = gameStartData.players[i];
                            if (playerState == null || !playerState.isActive) continue;
                            playerDetails[i] = new PlayerDetails
                            {
                                id = playerState.publicId,
                                publicId = playerState.publicId,
                                username = playerState.username,
                                chips = playerState.chips,
                                amount = playerState.chips,
                                position = playerState.position,
                                isActive = playerState.isActive,
                                hasFolded = playerState.hasFolded,
                                hasSeenCards = playerState.isSeen,
                                betAmount = playerState.betAmount,
                                totalBetAmount = playerState.totalBet,
                                action = playerState.lastAction,
                                isMyTurn = playerState.publicId == gameStartData.currentTurnPublicId,
                                isBot = playerState.isBot,
                                profileImageUrl = playerState.profileImageUrl,
                            };

                        }

                        // Update GameLiveData
                        GameLiveData.instance.playerDetailsArray = playerDetails;
                        GameLiveData.instance.tableId = gameStartData.tableID;
                        GameLiveData.instance.currentPot = gameStartData.pot;
                        GameLiveData.instance.lastBetAmount = gameStartData.bootAmount;
                        GameLiveData.instance.lastPlayerSeen = false;
                        GameLiveData.instance.bootAmount = gameStartData.bootAmount;
                        GameLiveData.instance.dealerPosition = gameStartData.dealerPosition;
                        GameLiveData.instance.isSpectator = false;
                        GameLiveData.instance.isGameActive = true;
                        GameLiveData.instance.commision = gameStartData.commision;

                        GameManager.Instance.InitializeGame(playerDetails);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling game_started: {ex.Message}");
            }
        }

        private void HandleCardsDealt<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var cardsData = JsonUtility.FromJson<CardsDealtResponse>(data);
                if (cardsData != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // Convert cards to string format
                        string[] cardCodes = new string[cardsData.cards.Length];
                        for (int i = 0; i < cardsData.cards.Length; i++)
                        {
                            cardCodes[i] = cardsData.cards[i].code;
                        }


                        // Find player by position and set cards
                        if (GameLiveData.instance.playerDetailsArray != null)
                        {
                            for (int i = 0; i < GameLiveData.instance.playerDetailsArray.Length; i++)
                            {
                                var player = GameLiveData.instance.playerDetailsArray[i];
                                if (player == null) continue;
                                if (player.position == cardsData.position)
                                {
                                    player.cards = cardCodes;
                                    player.cardsType = cardsData.cards_type;

                                    break;
                                }
                            }
                        }

                        GameManager.Instance.DealCards();

                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling cards_dealt: {ex.Message}");
            }
        }
        private void HandlePlayerBack<T>(string json)
        {

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var playerUpdate = JsonUtility.FromJson<PlayerState>(data);
                if (playerUpdate != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // Update player in GameLiveData
                        if (GameLiveData.instance.playerDetailsArray != null)
                        {
                            for (int i = 0; i < GameLiveData.instance.playerDetailsArray.Length; i++)
                            {
                                var player = GameLiveData.instance.playerDetailsArray[i];
                                if (player == null) continue;
                                if (player.publicId == playerUpdate.publicId)
                                {
                                    // Update player details
                                    player.username = playerUpdate.username;
                                    player.chips = playerUpdate.chips;
                                    player.position = playerUpdate.position;
                                    player.isActive = playerUpdate.isActive;
                                    player.hasFolded = playerUpdate.hasFolded;
                                    player.hasSeenCards = playerUpdate.isSeen;
                                    player.betAmount = playerUpdate.betAmount;
                                    player.totalBetAmount = playerUpdate.totalBet;
                                    player.action = playerUpdate.lastAction;
                                    player.isBot = playerUpdate.isBot;

                                    // Update UI
                                    ActionButtons.instance.ImBack_Panel.SetActive(false);

                                    break;
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling player_update: {ex.Message}");
            }
        }
        private void HandlePlayerUpdate<T>(string json)
        {

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var playerUpdate = JsonUtility.FromJson<PlayerState>(data);
                if (playerUpdate != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // Update player in GameLiveData
                        if (GameLiveData.instance.playerDetailsArray != null)
                        {
                            for (int i = 0; i < GameLiveData.instance.playerDetailsArray.Length; i++)
                            {
                                var player = GameLiveData.instance.playerDetailsArray[i];
                                if (player == null) continue;
                                if (player.publicId == playerUpdate.publicId)
                                {
                                    // Update player details
                                    player.username = playerUpdate.username;
                                    player.chips = playerUpdate.chips;
                                    player.position = playerUpdate.position;
                                    player.isActive = playerUpdate.isActive;
                                    player.hasFolded = playerUpdate.hasFolded;
                                    player.hasSeenCards = playerUpdate.isSeen;
                                    player.betAmount = playerUpdate.betAmount;
                                    player.totalBetAmount = playerUpdate.totalBet;
                                    player.action = playerUpdate.lastAction;
                                    player.isBot = playerUpdate.isBot;

                                    // try
                                    // {
                                    //     string localId = BootstrapLobbyAdapter.GetUserId();
                                    //     if (!string.IsNullOrEmpty(localId) && playerUpdate.publicId == localId && BootstrapService.Instance != null && BootstrapService.Instance.Wallet != null)
                                    //     {
                                    //         BootstrapService.Instance.Wallet.available_balance = playerUpdate.chips * 100;
                                    //         BootstrapService.Instance.Wallet.withdrawable_amount = playerUpdate.chips * 100;
                                    //     }
                                    // }
                                    // catch { }

                                    // Update UI
                                    if (GameManager.Instance != null && i < GameManager.Instance.playersList.Count)
                                    {
                                        foreach (var p in GameManager.Instance.playersList)
                                        {
                                            if (p.myId == playerUpdate.publicId)
                                            {
                                                var playerManager = p;
                                                // playerManager.myAmountTxt.text = playerUpdate.chips.ToString();  
                                                if (playerUpdate.isSeen)
                                                {
                                                    playerManager.MarkSeen();
                                                }
                                                if (playerUpdate.hasFolded)
                                                {
                                                    playerManager.Pack();
                                                }
                                                break;
                                            }
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling player_update: {ex.Message}");
            }
        }

        private void HandleSpectatorState<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var spectatorState = JsonUtility.FromJson<SpectatorStateResponse>(data);

                MainThreadDispatcher.Enqueue(() =>
                {
                    GameLiveData.instance.ResetGameData();

                    // 👇 IMPORTANT FLAG
                    GameLiveData.instance.isSpectator = true;

                    // Convert players
                    PlayerDetails[] playerDetails =
                        new PlayerDetails[spectatorState.players.Length];

                    for (int i = 0; i < spectatorState.players.Length; i++)
                    {
                        var p = spectatorState.players[i];
                        Debug.Log("adding: " + p.username + " at " + p.position);
                        playerDetails[i] = new PlayerDetails
                        {
                            id = p.publicId,
                            publicId = p.publicId,
                            username = p.username,
                            chips = p.chips,
                            position = p.position,

                            isActive = p.isActive,
                            hasFolded = p.hasFolded,
                            hasSeenCards = p.isSeen,

                            betAmount = p.betAmount,
                            //totalBetAmount = p.betAmount,
                            //action = p.lastAction,

                            //isBot = p.isBot,

                            // ❗spectator NEVER has turn
                            isMyTurn = false,

                            profileImageUrl = p.profileImageUrl
                        };
                    }

                    GameLiveData.instance.playerDetailsArray = playerDetails;

                    GameLiveData.instance.tableId = spectatorState.tableID;
                    GameLiveData.instance.currentPot = spectatorState.pot;
                    GameLiveData.instance.lastBetAmount = spectatorState.bootAmount;
                    GameLiveData.instance.lastPlayerSeen = spectatorState.lastPlayerSeen;
                    GameLiveData.instance.spectatorSeatPosition = spectatorState.position;
                    GameLiveData.instance.privateTableCode = spectatorState.privateCode;

                    GameLiveData.instance.dealerPosition =
                        spectatorState.dealerPosition;

                    GameLiveData.instance.currentRound =
                        spectatorState.roundNumber;

                    GameLiveData.instance.isGameActive =
                        spectatorState.state == "playing";

                    Debug.Log("✅ Spectator Initialized");

                    // Load scene if needed
                    if (SceneManager.GetActiveScene().name != "Teenpatti")
                    {
                        SceneManager.LoadScene("Teenpatti");
                    }
                    else
                    {
                        // StartCoroutine(InitializeGame(playerDetails));
                        GameManager.Instance?.InitializeGame(playerDetails, true);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError("spectator_state error: " + ex.Message);
            }
        }

        // IEnumerator InitializeGame(PlayerDetails[] playerDetails)
        // {
        //     yield return new WaitForSeconds(2f);
        //     GameManager.Instance?.InitializeGame(playerDetails);
        // }

        private void HandleTableState<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var tableState = JsonUtility.FromJson<TableStateResponse>(data);
                if (tableState != null && tableState.state == "playing")
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {


                        GameLiveData.instance.ResetGameData();
                        // Convert to PlayerDetails array
                        PlayerDetails[] playerDetails = new PlayerDetails[tableState.players.Length];
                        for (int i = 0; i < tableState.players.Length; i++)
                        {
                            var playerState = tableState.players[i];
                            playerDetails[i] = new PlayerDetails
                            {
                                id = playerState.publicId,
                                publicId = playerState.publicId,
                                username = playerState.username,
                                chips = playerState.chips,
                                amount = playerState.chips,
                                position = playerState.position,
                                isActive = playerState.isActive,
                                hasFolded = playerState.hasFolded,
                                hasSeenCards = playerState.isSeen,
                                betAmount = playerState.betAmount,
                                totalBetAmount = playerState.totalBet,
                                cards = playerState.cards,
                                cardsType = playerState.cardsType,
                                action = playerState.lastAction,
                                isMyTurn = playerState.position == tableState.currentTurn,
                                isBot = playerState.isBot,
                                profileImageUrl = playerState.profileImageUrl,
                                canShow = playerState.canShow,
                                canSideShow = playerState.canSideShow,
                            };
                        }

                        // Update GameLiveData
                        GameLiveData.instance.playerDetailsArray = playerDetails;
                        GameLiveData.instance.tableId = tableState.tableID;
                        GameLiveData.instance.currentPot = tableState.pot;
                        GameLiveData.instance.lastBetAmount = tableState.lastBetAmount;
                        GameLiveData.instance.lastPlayerSeen = tableState.lastPlayerSeen;
                        GameLiveData.instance.bootAmount = tableState.bootAmount;
                        GameLiveData.instance.dealerPosition = tableState.dealerPosition;
                        GameLiveData.instance.currentTurn = tableState.currentTurn;
                        GameLiveData.instance.timeRemainingForTurn = tableState.turnTimeRemaining;
                        GameLiveData.instance.rechargingPlayer = tableState.rechargingPlayer;
                        GameLiveData.instance.rechargeTimeRemaining = tableState.rechargeTimeRemaining;
                        GameLiveData.instance.tableState = tableState.state;


                        // Find current turn index
                        for (int i = 0; i < playerDetails.Length; i++)
                        {
                            if (playerDetails[i].position == tableState.currentTurn)
                            {
                                GameLiveData.instance.currentTurn = i;
                                break;
                            }
                        }

                        GameLiveData.instance.isGameActive = true;
                        if (GameManager.Instance)
                        {
                            GameManager.Instance.SyncGameState();
                            if (tableState.rechargingPlayer != null && tableState.rechargingPlayer != "" && tableState.rechargeTimeRemaining > 0)
                            {
                                Debug.Log($"Player {tableState.rechargingPlayer} is recharging with {tableState.rechargeTimeRemaining} seconds left");
                                GameManager.Instance.Recharging(tableState.rechargingPlayer, "", tableState.rechargeTimeRemaining);
                            }
                        }


                        Debug.Log($"Game started on table: {tableState.tableID}, pot: {tableState.pot}");
                        // GameManager.Instance.SyncGameState(playerDetails);
                        GameLiveData.instance.rejoin = true;
                    });
                }
                else if (tableState != null && tableState.state == "waiting" || tableState.state == "starting")
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        GameLiveData.instance.ResetGameData();
                        GameLiveData.instance.isGameActive = false;
                        GameLiveData.instance.rejoin = false;
                        GameLiveData.instance.tableState = tableState.state;
                        if (GameManager.Instance)
                        {
                            GameManager.Instance.waitingPanel.SetActive(true);
                            //GameManager.Instance.SyncGameState();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling turn_update: {ex.Message}");
            }
        }

        private void HandleRechargeUpdate<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var rechargeUpdate = JsonUtility.FromJson<PlayerRechargingResponse>(data);
                if (rechargeUpdate != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.Recharging(rechargeUpdate.publicId, rechargeUpdate.username, rechargeUpdate.timeout);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling turn_update: {ex.Message}");
            }
        }

        private void HandlePlayerRecharged<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var rechargeSuccess = JsonUtility.FromJson<RechargeSuccessResponse>(data);
                if (rechargeSuccess != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        if (GameManager.Instance == null) return;

                        string localUserId = BootstrapLobbyAdapter.GetUserId();
                        bool isLocal = false;

                        if (!string.IsNullOrEmpty(localUserId))
                        {
                            isLocal = rechargeSuccess.publicId == localUserId || rechargeSuccess.userID == localUserId;
                        }
                        if (isLocal) return;

                        string targetId = string.IsNullOrEmpty(rechargeSuccess.publicId) ? rechargeSuccess.userID : rechargeSuccess.publicId;

                        if (!string.IsNullOrEmpty(targetId) && GameLiveData.instance.playerDetailsArray != null)
                        {
                            foreach (var player in GameLiveData.instance.playerDetailsArray)
                            {
                                if (player == null) continue;
                                if (player.publicId == targetId)
                                {
                                    player.chips = rechargeSuccess.chips;
                                    player.amount = rechargeSuccess.chips;
                                    break;
                                }
                            }
                        }

                        if (isLocal)
                        {
                            GameData.Instance.playerChips = rechargeSuccess.chips;
                            PlayerPrefs.SetInt("TP_Chips", rechargeSuccess.chips);
                            PlayerPrefs.Save();

                            try
                            {
                                if (BootstrapService.Instance != null && BootstrapService.Instance.Wallet != null)
                                {
                                    BootstrapService.Instance.Wallet.available_balance += rechargeSuccess.amount * 100;
                                    BootstrapService.Instance.Wallet.withdrawable_amount += rechargeSuccess.amount * 100;
                                }
                            }
                            catch { }

                            GameManager.Instance.UpdateWalletTxt();

                            // if (GameManager.Instance.localPlayer != null)
                            // {
                            //     GameManager.Instance.localPlayer.myAmountTxt.text = MoneyFormatter.FormatPaisa((long)(rechargeSuccess.chips * 100));
                            // }

                            if (ActionButtons.instance != null)
                            {
                                ActionButtons.instance.addRechargePanel.SetActive(false);
                            }

                            if (InGameRecharge.instance != null)
                            {
                                InGameRecharge.instance.ClosePanel();
                            }

                        }
                        GameManager.Instance.Recharged();

                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling recharge_success: {ex.Message}");
            }
        }


        private void HandleRechargeSuccess<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var rechargeSuccess = JsonUtility.FromJson<RechargeSuccessResponse>(data);
                if (rechargeSuccess != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        if (GameManager.Instance == null) return;

                        string localUserId = BootstrapLobbyAdapter.GetUserId();
                        bool isLocal = false;

                        if (!string.IsNullOrEmpty(localUserId))
                        {
                            isLocal = rechargeSuccess.publicId == localUserId || rechargeSuccess.userID == localUserId;
                        }

                        string targetId = string.IsNullOrEmpty(rechargeSuccess.publicId) ? rechargeSuccess.userID : rechargeSuccess.publicId;

                        if (!string.IsNullOrEmpty(targetId) && GameLiveData.instance.playerDetailsArray != null)
                        {
                            foreach (var player in GameLiveData.instance.playerDetailsArray)
                            {
                                if (player == null) continue;
                                if (player.publicId == targetId)
                                {
                                    player.chips = rechargeSuccess.chips;
                                    player.amount = rechargeSuccess.chips;
                                    break;
                                }
                            }
                        }

                        if (isLocal)
                        {
                            GameData.Instance.playerChips = rechargeSuccess.chips;
                            PlayerPrefs.SetInt("TP_Chips", rechargeSuccess.chips);
                            PlayerPrefs.Save();

                            try
                            {
                                if (BootstrapService.Instance != null && BootstrapService.Instance.Wallet != null)
                                {
                                    BootstrapService.Instance.Wallet.available_balance = rechargeSuccess.chips * 100 + (GameMode.mode == GameMode.Modes.privateGame ? BootstrapService.Instance.Wallet.bonus_balance : 0);
                                }
                            }
                            catch { }

                            GameManager.Instance.UpdateWalletTxt();

                            // if (GameManager.Instance.localPlayer != null)
                            // {
                            //     GameManager.Instance.localPlayer.myAmountTxt.text = MoneyFormatter.FormatPaisa((long)(rechargeSuccess.chips * 100));
                            // }

                            if (ActionButtons.instance != null)
                            {
                                ActionButtons.instance.addRechargePanel.SetActive(false);
                            }

                            if (InGameRecharge.instance != null)
                            {
                                InGameRecharge.instance.ClosePanel();
                            }

                        }
                        GameManager.Instance.Recharged();

                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling recharge_success: {ex.Message}");
            }
        }

        private void HandleTurnUpdate<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var turnUpdate = JsonUtility.FromJson<TurnUpdateResponse>(data);
                if (turnUpdate != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // Find player index by userID
                        int turnIndex = -1;
                        if (GameLiveData.instance.playerDetailsArray != null)
                        {
                            for (int i = 0; i < GameLiveData.instance.playerDetailsArray.Length; i++)
                            {
                                if (GameLiveData.instance.playerDetailsArray[i] == null) continue;
                                if (GameLiveData.instance.playerDetailsArray[i].publicId == turnUpdate.currentTurnPublicId)
                                {
                                    turnIndex = i;
                                    GameLiveData.instance.currentTurn = i;
                                    break;
                                }
                            }
                        }

                        GameLiveData.instance.currentRound = turnUpdate.round;

                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.UpdateRoundNumberTMP();
                            GameManager.Instance.UpdateTurn(turnUpdate.currentTurnPublicId, turnUpdate.canShow, turnUpdate.canSideShow);

                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling turn_update: {ex.Message}");
            }
        }

        private void HandleActionUpdate<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var actionUpdate = JsonUtility.FromJson<ActionUpdateResponse>(data);
                if (actionUpdate != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameLiveData.instance.currentPot = actionUpdate.pot;
                            GameLiveData.instance.lastBetAmount = actionUpdate.lastBetAmount;
                            GameLiveData.instance.lastPlayerSeen = actionUpdate.lastPlayerSeen;

                            try
                            {
                                string localId = BootstrapLobbyAdapter.GetUserId();
                                if (!string.IsNullOrEmpty(localId) && actionUpdate.publicId == localId && BootstrapService.Instance != null && BootstrapService.Instance.Wallet != null)
                                {
                                    long deduction = (long)(actionUpdate.amount * 100);
                                    if (deduction > 0)
                                    {
                                        // BootstrapService.Instance.Wallet.available_balance -= deduction;
                                        // BootstrapService.Instance.Wallet.withdrawable_amount -= deduction;
                                    }
                                }
                            }
                            catch { }
                            GameManager.Instance.PlayerAction(actionUpdate.publicId, actionUpdate.action, actionUpdate.amount);

                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling turn_update: {ex.Message}");
            }
        }

        private void HandleGiftUpdate<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var actionUpdate = JsonUtility.FromJson<GiftUpdateResponse>(data);
                if (actionUpdate != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        Debug.Log($"Gift received from: {actionUpdate.fromPublicId}, to: {actionUpdate.to}, gift: {actionUpdate.gift}");
                        GiftSystem.instance.OnGiftReceived(actionUpdate.fromPublicId, actionUpdate.to, actionUpdate.gift);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling turn_update: {ex.Message}");
            }
        }

        private void HandleBetUpdate(string data)
        {
            try
            {
                var betUpdate = JsonUtility.FromJson<BetUpdateData>(data);
                if (betUpdate != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        GameLiveData.instance.currentPot = betUpdate.pot;
                        GameLiveData.instance.lastBetAmount = betUpdate.lastBetAmount;
                        GameLiveData.instance.lastPlayerSeen = betUpdate.lastPlayerSeen;

                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.UpdatePotAmount();
                            GameManager.Instance.UpdateChaalAmount();

                            Debug.Log($"Bet updated: current bet = {betUpdate.lastBetAmount}, pot = {betUpdate.pot}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling bet_update: {ex.Message}");
            }
        }

        private void HandleSideShowRequest<T>(string json)
        {

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var sideShowRequest = JsonUtility.FromJson<SideShowRequestData>(data);
                if (sideShowRequest != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // Find requester and target player names

                        // Show side show request UI
                        if (SideShowHandler.Instance != null)
                        {

                            SideShowHandler.Instance.ShowSideShowRequest(
                                sideShowRequest.requesterPublicId,
                                sideShowRequest.targetPublicId,
                                sideShowRequest.timeout
                            );

                            foreach (PlayerManager player in GameManager.Instance.playersList)
                            {
                                if (player.myId == sideShowRequest.requesterPublicId)
                                {
                                    player.ShowMsg("SideShow");
                                    break;
                                }
                            }

                            foreach (PlayerManager player in GameManager.Instance.playersList)
                            {
                                if (player.myId == sideShowRequest.requesterPublicId && player.myId == GameManager.Instance.localPlayer.myId)
                                {
                                    player.StopTimer();
                                    break;
                                }
                            }
                        }

                        Debug.Log($"Side show requested by: {sideShowRequest.requesterPublicId}, timeout: {sideShowRequest.timeout}s");
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling side_show_request: {ex.Message}");
            }
        }

        private void HandleSideShowResult<T>(string json)
        {

            WSMessage<T> msg;
            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var sideShowResult = JsonUtility.FromJson<SideShowResultData>(data);
                if (sideShowResult != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // Hide side show panel if visible
                        if (SideShowHandler.Instance != null)
                        {
                            SideShowHandler.Instance.HideSideShowPanel();
                        }

                        // Update UI based on result
                        if (sideShowResult.winnerFound)
                        {
                            Debug.Log($"Number of players: {GameManager.Instance.playersList.Count}");
                            foreach (PlayerManager player in GameManager.Instance.playersList)
                            {
                                if (player == null) continue;
                                Debug.Log($"Player: {player.myId}, Requester: {sideShowResult.requesterPublicId}, Responder: {sideShowResult.responderPublicId}");
                                if (player.myId == sideShowResult.requesterPublicId)
                                {
                                    player.AddMoneyToPool(sideShowResult.amount);
                                }
                                if (player.myId == GameManager.Instance.localPlayer.myId && !player.hasSeenCards)
                                {
                                    ActionButtons.instance.OnCheckCards();
                                }
                            }

                            foreach (PlayerManager player in GameManager.Instance.playersList)
                            {
                                if (player == null) continue;
                                if (player.myId == sideShowResult.responderPublicId)
                                {
                                    player.ShowMsg("Accepted");
                                    break;
                                }
                            }

                            // Mark loser as folded
                            if (GameLiveData.instance.playerDetailsArray != null)
                            {
                                foreach (var player in GameLiveData.instance.playerDetailsArray)
                                {
                                    if (player.publicId == sideShowResult.loserPublicId)
                                    {
                                        player.hasFolded = true;

                                        // Update UI if game is active
                                        if (GameManager.Instance != null)
                                        {
                                            StartCoroutine(SideShowAnim(sideShowResult));
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (PlayerManager player in GameManager.Instance.playersList)
                            {
                                if (player.myId == sideShowResult.responderPublicId)
                                {
                                    player.DeclinedSideShow();
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling side_show_result: {ex.Message}");
            }
        }

        IEnumerator SideShowAnim(SideShowResultData sideShowResult)
        {
            yield return new WaitForSeconds(1f);
            foreach (var playerManager in GameManager.Instance.playersList)
            {

                if (sideShowResult.winnerFound)
                {
                    var from = GameManager.Instance.playersList
                        .Find(p => p.myId == sideShowResult.winnerPublicId);

                    var to = GameManager.Instance.playersList
                        .Find(p => p.myId == sideShowResult.loserPublicId);

                    ThunderUIManager.Instance.PlayThunder(from, to);
                }


                if (playerManager.myId == sideShowResult.loserPublicId)
                {
                    playerManager.Lose();
                    break;
                }
            }
        }


        private void HandleShowdownResult<T>(string json)
        {
            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Showdown data null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var showdown = JsonUtility.FromJson<ShowDownResult>(data);
                if (showdown == null) return;

                MainThreadDispatcher.Enqueue(() =>
                {
                    StartCoroutine(ShowDownAnim(showdown));

                });
            }
            catch (Exception ex)
            {
                Debug.LogError("Showdown handling failed: " + ex.Message);
            }
        }

        IEnumerator ShowDownAnim(ShowDownResult showdown)
        {
            GameLiveData.instance.currentPot = showdown.pot;
            GameManager.Instance.ShowdownDe(showdown.requester, showdown.amount, showdown.type);
            yield return new WaitForSeconds(1.5f);

            var winnerPM = GameManager.Instance.playersList.Find(p => p.myId == showdown.winnerPublicId);
            var loserPM = GameManager.Instance.playersList.Find(p => p.myId == showdown.loserPublicId);

            var requesterPM = GameManager.Instance.playersList.Find(p => p.myId == showdown.requester);

            if (winnerPM != null && requesterPM != null)
            {
                ThunderUIManager.Instance.PlayThunder(winnerPM, loserPM);
                yield return new WaitForSeconds(1.5f);
            }
            GameManager.Instance.Showdown(showdown.players[0].publicId, showdown.players[1].publicId, showdown.players[0].handRank.Cards, showdown.players[1].handRank.Cards, showdown.players[0].handRank.Name, showdown.players[1].handRank.Name);

            yield return new WaitForSeconds(2f);

            GameManager.Instance.showDownPanel.SetActive(false);

            foreach (var sdPlayer in showdown.players)
            {
                var playerManager = GameManager.Instance.playersList
                    .Find(p => p.myId == sdPlayer.publicId);

                if (playerManager == null) continue;

                // 🔥 Reveal cards
                Sprite[] cardSprites = new Sprite[sdPlayer.handRank.Cards.Length];
                for (int i = 0; i < sdPlayer.handRank.Cards.Length; i++)
                {
                    cardSprites[i] =
                        GameManager.Instance.GetSprite(sdPlayer.handRank.Cards[i].code);
                }

                playerManager.ShowCards(cardSprites);
                if (sdPlayer.publicId == GameManager.Instance.localPlayer.myId && ActionButtons.instance != null)
                    ActionButtons.instance.checkBtn.SetActive(false);


                // 🏷 Show hand rank
                playerManager.ShowHandRank(sdPlayer.handRank.Name);

                // 🏆 Winner vs Loser
                if (sdPlayer.publicId == showdown.winnerPublicId)
                {
                    //playerManager.Win();
                }
                else
                {

                    playerManager.Lose(); // ❗ LOSER PACK
                    if (sdPlayer.publicId == GameManager.Instance.localPlayer.myId)
                    {
                        playerManager.myAudioSouce.PlayOneShot(GameManager.Instance.playerLoseAC);
                    }
                }
            }

            // 💰 Pot update
            GameLiveData.instance.currentPot = showdown.pot;
            GameManager.Instance.UpdatePotAmount();
        }

        private void HandleGameEnded<T>(string json)
        {

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var gameEndData = JsonUtility.FromJson<GameEndedData>(data);
                if (gameEndData != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        GameLiveData.instance.currentPot = gameEndData.pot;

                        if (GameManager.Instance != null)
                        {
                            // Update pot display
                            GameManager.Instance.potAmountText.text = gameEndData.pot.ToString();

                            // Show winner
                            string winnerId = "";
                            CardObject[] cards = null;
                            if (gameEndData.result == "winner" && gameEndData.winner != null)
                            {
                                winnerId = gameEndData.winner.publicId;
                                cards = gameEndData.cards;
                            }

                            if (!string.IsNullOrEmpty(winnerId))
                            {
                                string[] myCards = new string[] { "", "", "", };
                                for (int i = 0; i < cards.Length; i++)
                                {
                                    myCards[i] = cards[i].code.ToString();
                                }
                                GameManager.Instance.ShowWinner(winnerId, myCards);
                                if (gameEndData.ssPacked != null && gameEndData.ssPacked.Length > 0)
                                {
                                    GameManager.Instance.ShowPackedPlayers(gameEndData.ssPacked);
                                }


                                // Update chips if local player won
                                if (gameEndData.winner != null && gameEndData.winner.publicId == BootstrapLobbyAdapter.GetUserId())
                                {
                                    GameData.Instance.UpdateChips(gameEndData.pot);
                                    try
                                    {
                                        if (BootstrapService.Instance != null && BootstrapService.Instance.Wallet != null)
                                        {
                                            BootstrapService.Instance.Wallet.available_balance += gameEndData.prizePaisa;
                                            BootstrapService.Instance.Wallet.withdrawable_amount += gameEndData.prizePaisa;
                                        }
                                    }
                                    catch { }
                                    GameManager.Instance.UpdateWalletTxt();
                                }
                            }

                        }

                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling game_ended: {ex.Message}");
            }
        }

        private System.Collections.IEnumerator ResetGameAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            GameLiveData.instance.ResetGameData();
        }

        private void HandleActionAccepted<T>(string json)
        {

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var actionAccepted = JsonUtility.FromJson<ActionAcceptedData>(data);
                if (actionAccepted != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // Hide action buttons after action is accepted
                        if (ActionButtons.instance != null)
                        {
                            ActionButtons.instance.HideButtonsPanel();
                        }

                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling action_accepted: {ex.Message}");
            }
        }

        private void HandlePlayerDisconnected<T>(string json)
        {
            Debug.Log("Handling player disconnected message" + json);

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var playerDisconnectedData = JsonUtility.FromJson<PlayerDisconnectedResponse>(data);
                if (playerDisconnectedData != null)// && leftData.success)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // Debug.Log("Unregistering player: " + playerDisconnectedData.publicId);
                        // GameManager.Instance.UnregisterPlayer(playerDisconnectedData.publicId);
                        Loader.Instance.HideLoading();
                        Debug.Log("Player disconnected: " + playerDisconnectedData.publicId);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling player disconnected: {ex.Message}");
            }
        }

        private void HandlePotLimitReached<T>(string json)
        {
            Debug.Log("Handling pot limit reached message" + json);

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var potLimitReachedData = JsonUtility.FromJson<PotLimitReachedResponse>(data);
                if (potLimitReachedData != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        Logger.Instance.Log($"Pot limit of {potLimitReachedData.potLimit} reached.");
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling pot limit reached: {ex.Message}");
            }
        }

        private void HandleLeft<T>(string json)
        {
            Debug.Log("Handling player left message" + json);

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var leftData = JsonUtility.FromJson<LeftResponse>(data);
                if (leftData != null)// && leftData.success)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // Return to lobby
                        //SceneManager.LoadScene("Lobby");
                        if (leftData.publicId == GameManager.Instance.localPlayer.myId)
                        {
                            Debug.Log("Local player left the game. Returning to lobby.");
                            GameLiveData.instance.ResetGameData();
                            GameManager.Instance.OnLobby();
                        }
                        else
                        {
                            Debug.Log($"Player {leftData.publicId} left the game.");
                            GameManager.Instance.UnregisterPlayer(leftData.publicId);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling left: {ex.Message}");
            }
        }

        private void HandleError<T>(string json)
        {

            WSMessage<T> msg;

            try
            {
                msg = JsonUtility.FromJson<WSMessage<T>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Typed parse failed: " + e.Message);
                return;
            }

            if (msg == null || msg.data == null)
            {
                Debug.LogError("Message data is null");
                return;
            }

            string data = JsonUtility.ToJson(msg.data);

            try
            {
                var errorData = JsonUtility.FromJson<ErrorResponse>(data);
                if (errorData != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // Show error message to user
                        Debug.LogError($"Server error [{errorData.code}]: {errorData.message}");

                        // You might want to show this in a UI popup
                        // ErrorPopup.Show(errorData.message);

                        // Handle specific error codes
                        Debug.Log(errorData.code);
                        switch (errorData.code)
                        {
                            case "INVALID_ACTION":
                                // Enable action buttons again
                                if (ActionButtons.instance != null)
                                {
                                    ActionButtons.instance.ShowButtonsPanel();
                                }
                                break;
                            case "NOT_YOUR_TURN":
                                // Hide action buttons
                                if (ActionButtons.instance != null)
                                {
                                    ActionButtons.instance.HideButtonsPanel();
                                }
                                break;
                            case "ROOM_NOT_FOUND":
                            case "CREATE_FAILED":
                            case "JOIN_FAILED":
                                MainThreadDispatcher.Enqueue(() =>
                                {
                                    Debug.LogError($"Critical error: {errorData.message}. Returning to lobby.");
                                    ConnectionManager.Instance.Disconnect();
                                    SceneManager.LoadScene("Lobby");
                                });
                                break;

                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling error message: {ex.Message}");
            }
        }

        private void HandlePong(string data)
        {
            // Simple pong response - connection is alive
            //Debug.Log("Received pong from server");
        }

        #endregion

        #region Helper Methods

        private void RequestGameState()
        {
            // Send rejoin request to get current game state
            if (WebSocketServerRequest.Instance != null)
            {
                ///_ = WebSocketServerRequest.Instance.RejoinGame();
            }
        }

        private void RestoreGameState(GameState gameState)
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                // Convert GameState to PlayerDetails array
                PlayerDetails[] playerDetails = new PlayerDetails[gameState.players.Length];
                for (int i = 0; i < gameState.players.Length; i++)
                {
                    var playerState = gameState.players[i];
                    playerDetails[i] = new PlayerDetails
                    {
                        id = playerState.publicId,
                        publicId = playerState.publicId,
                        username = playerState.username,
                        chips = playerState.chips,
                        amount = playerState.chips,
                        position = playerState.position,
                        isActive = playerState.isActive,
                        hasFolded = playerState.hasFolded,
                        hasSeenCards = playerState.isSeen,
                        betAmount = playerState.betAmount,
                        totalBetAmount = playerState.totalBet,
                        action = playerState.lastAction,
                        isMyTurn = playerState.publicId == gameState.currentTurn,
                        isBot = playerState.isBot,
                        profileImageUrl = "",
                        cards = gameState.yourCards != null && playerState.publicId == GameData.Instance.userId
                            ? Array.ConvertAll(gameState.yourCards, card => card.code)
                            : null
                    };
                }

                // Update GameLiveData
                GameLiveData.instance.playerDetailsArray = playerDetails;
                GameLiveData.instance.tableId = gameState.tableID;
                GameLiveData.instance.currentPot = gameState.pot;
                GameLiveData.instance.dealerPosition = gameState.dealerPosition;
                GameLiveData.instance.currentRound = gameState.round;
                GameLiveData.instance.isGameActive = gameState.state == "playing";

                // Find current turn index
                for (int i = 0; i < playerDetails.Length; i++)
                {
                    if (playerDetails[i].publicId == gameState.currentTurn)
                    {
                        GameLiveData.instance.currentTurn = i;
                        break;
                    }
                }

                // If we're in game scene, update UI
                if (SceneManager.GetActiveScene().name == "Teenpatti")
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.InitializeGame(playerDetails);

                        // If it's our turn, show buttons
                        if (gameState.isYourTurn)
                        {
                            if (ActionButtons.instance != null)
                            {
                                ActionButtons.instance.ShowButtonsPanel();
                            }
                        }
                    }
                }
                else
                {
                    // Load game scene
                    SceneManager.LoadScene("Teenpatti");
                }

            });
        }

        #endregion

        #region Data Classes

        [System.Serializable]
        public class JoinedResponse
        {
            public string tableID;
            public string status; // waiting or playing
            public int position;
            public string privateCode;
            public PlayerOnJoin[] players;
        }



        [System.Serializable]
        public class TableStateResponse
        {
            public string tableID;
            public int bootAmount; // waiting or playing
            public int dealerPosition;
            public string state;
            public int pot;
            public int lastBetAmount;
            public bool lastPlayerSeen;
            public int currentTurn;
            public int round;
            public PlayerStateOnRejoin[] players;
            public bool yourTurn;
            public float turnTimeRemaining;
            public string rechargingPlayer;
            public int rechargeTimeRemaining;

        }

        [Serializable]
        public class SpectatorStateResponse
        {
            public string tableID;
            public int bootAmount;
            public int lastBetAmount;
            public int position;
            public bool lastPlayerSeen;

            public int currentTurn;      // <-- NUMBER from server
            public int dealerPosition;

            public string state;         // <-- "playing"

            public bool isSpectator;
            public int spectators;

            public int pot;
            public int roundNumber;
            public string privateCode;

            public SpectatorPlayer[] players;
        }




        [System.Serializable]
        public class RejoinedResponse
        {
            public string tableID;
            public GameState gameState;
        }

        [System.Serializable]
        public class GameStartedResponse
        {
            public string tableID;
            public int bootAmount;
            public int dealerPosition;
            public int pot;
            public PlayerStateOnGS[] players;
            public string currentTurn;
            public string currentTurnPublicId;
            public int commision;
        }

        [System.Serializable]
        public class CardsDealtResponse
        {
            public CardObject[] cards;
            public string cards_type;
            public int position;
        }



        [System.Serializable]
        public class YourTurnResponse
        {
            public int timeRemaining;
            public int minBet;
            public int maxBet;
            public bool canShow;
            public bool canSideShow;
            public int lastBetAmount;
            public bool lastPlayerSeen;
            public int pot;
        }

        [System.Serializable]
        public class PlayerState
        {
            public string publicId;
            public string username;
            public int chips;
            public CardObject[] cards;
            public int position;
            public bool isActive;
            public bool hasFolded;
            public bool isBot;
            public bool isBlind;
            public bool isSeen;
            public int betAmount;
            public int totalBet;
            public string lastAction;
        }


        [System.Serializable]
        public class PlayerStateOnGS
        {
            // public string userID;
            public string publicId;
            public string username;
            public int chips;
            public int position;
            public bool isActive;
            public bool hasFolded;
            public bool isBot;
            public bool isBlind;
            public bool isSeen;
            public int betAmount;
            public int totalBet;
            public string lastAction;
            public bool canSideShow;
            public bool canShow;

            public string profileImageUrl;


        }



        [System.Serializable]
        public class PlayerOnJoin
        {
            public string userID;
            public string username;
            public int position;
            public String profileImageUrl;

        }

        [System.Serializable]
        public class PlayerStateOnRejoin
        {
            public string publicId;
            public string username;
            public int chips;
            public int position;
            public bool isActive;
            public bool hasFolded;
            public bool isBot;
            public bool isBlind;
            public bool isSeen;
            public int betAmount;
            public int totalBet;
            public string lastAction;
            public bool canSideShow;
            public bool canShow;
            public string[] cards;
            public string cardsType;

            public string profileImageUrl;


        }

        [Serializable]
        public class SpectatorPlayer
        {
            public int chips;
            public bool hasFolded;
            public bool isActive;
            public bool isBlind;
            public bool isSeen;
            public int position;
            public string publicId;
            public string username;
            public int betAmount;
            public string profileImageUrl;
        }


        [Serializable]
        public class HandRank
        {
            public int Rank;
            public string Name;
            public int HighCard;
            public int PairValue;
            public CardObject[] Cards;
        }

        [Serializable]
        public class ShowDownPlayer
        {
            public string publicId;
            public HandRank handRank;
        }


        [Serializable]
        public class GiftUpdateResponse
        {
            public string fromPublicId;
            public string to;
            public string toPublicId;
            public int gift;
        }

        [Serializable]
        public class ActionUpdateResponse
        {
            public string userID;
            public string publicId;
            public string action;
            public int amount;
            public int pot;
            public int lastBetAmount;
            public bool lastPlayerSeen;
        }

        [System.Serializable]
        public class TurnUpdateResponse
        {
            public string currentTurn;
            public string currentTurnPublicId;
            public int position;
            public int round;
            public int timeRemaining;
            public bool canShow;
            public bool canSideShow;
        }
        [System.Serializable]
        public class PlayerRechargingResponse
        {
            public string publicId;
            public string username;
            public float timeout;
        }

        [System.Serializable]
        public class PlayerRechargedResponse
        {
            public string userID;
        }

        [System.Serializable]
        public class RechargeSuccessResponse
        {
            public string userID;
            public string publicId;
            public int amount;
            public int chips;
        }

        [Serializable]
        public class BetUpdateData
        {
            public int lastBetAmount;
            public bool lastPlayerSeen;
            public int pot;
        }

        [System.Serializable]
        public class RoundUpdateData
        {
            public string round; // preflop, flop, turn, river, showdown
            public int roundNumber;
            public bool sideShowEnabled;
        }

        [System.Serializable]
        public class SideShowRequestData
        {
            // public string requesterID;
            public string requesterPublicId;
            // public string targetID;
            public string targetPublicId;
            public int timeout;
        }

        [System.Serializable]
        public class SideShowResultData
        {
            public bool winnerFound;
            public string responderPublicId;
            public string requesterPublicId;
            public string winnerPublicId;
            public string loserPublicId;
            public int amount;
        }


        [System.Serializable]
        public class GameEndedData
        {
            public string tableID;
            public int pot;
            public long prizePaisa;
            public string result; // winner or tie
            public PlayerState winner;
            public CardObject[] cards;
            public PackedPlayerData[] ssPacked;


        }



        [Serializable]
        public class ShowDownResult
        {
            public string tableID;
            public int pot;
            public ShowDownPlayer[] players;
            public string loserPublicId;
            public string requester;
            public string winnerPublicId;
            public string type;
            public float amount;
        }


        [System.Serializable]
        public class ActionAcceptedData
        {
            public string action;
            public int amount;
        }

        [System.Serializable]
        public class LeftResponse
        {
            public string publicId;
        }

        [System.Serializable]
        public class PotLimitReachedResponse
        {
            public int potLimit;
        }

        [System.Serializable]
        public class PlayerDisconnectedResponse
        {
            public string publicId;
        }

        [System.Serializable]
        public class ErrorResponse
        {
            public string code;
            public string message;
        }

        [System.Serializable]
        public class GameState
        {
            public string tableID;
            public string state; // waiting/starting/playing/ended
            public int round;
            public string currentTurn;
            public int pot;
            public PlayerState[] players;
            public CardObject[] yourCards;
            public int yourPosition;
            public bool isYourTurn;
            public int timeRemaining;
            public int dealerPosition;
            public int roundNumber;
            public SideShowRequest sideShowRequest;
        }

        [System.Serializable]
        public class SideShowRequest
        {
            public string requesterID;
            public string targetID;
            public string timestamp;
        }

        [System.Serializable]
        public class SpectatorJoinedData
        {
            public string userID;
            public string username;
            public int position;
            public int totalSpectators;
        }

        [System.Serializable]
        public class PlayerJoinedData
        {
            public string userID;
            public string username;
            public int position;
            public string profileImageUrl;
        }

        [System.Serializable]
        public class SpectatorLeftData
        {
            public string userID;
            public int totalSpectators;
        }

        #endregion

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (WebSocketClient.Instance != null)
            {
                WebSocketClient.Instance.OnMessageReceived -= HandleMessage;
            }
        }
    }

    [System.Serializable]
    public class PackedPlayerData
    {
        public string publicId;
        public string[] cards;
        public string handType;
    }

    [System.Serializable]
    public class CardObject
    {
        public string rank;
        public string suit;
        public int value;
        public string code;
    }

}