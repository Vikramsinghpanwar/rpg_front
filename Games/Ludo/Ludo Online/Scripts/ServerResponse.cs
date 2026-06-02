using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft;
using Newtonsoft.Json;
using System;
using SocketIOClient;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using Ludo;
using Features.Lobby.Integration;

public class ServerResponse : MonoBehaviour
{
    public static SocketIOUnity socket;


    public void InitializeListners(SocketIOUnity socket)
    {
        socket.On("test", (r) => { Debug.Log("test data : " + r); });

        socket.On(ServerReponseApi.START_GAME.ToString(), (response) => MainThreadDispatcher.Enqueue(() => StartGame(response)));
        //socket.On(ServerReponseApi.ROLL_DICE.ToString(), (response) => MainThreadDispatcher.Enqueue(() => RollDice(response)));
        socket.On(ServerReponseApi.ON_PLAYER_WIN.ToString(), (response) => MainThreadDispatcher.Enqueue(() => OnPlayerWin(response)));
        socket.On(ServerReponseApi.YOU_WIN.ToString(), (response) => MainThreadDispatcher.Enqueue(() => YouWin(response)));
        socket.On(ServerReponseApi.PLAYER_REGISTRATION.ToString(), (response) => MainThreadDispatcher.Enqueue(() => RegisterPlayerId(response)));
        socket.On(ServerReponseApi.SWITCH_PLAYER.ToString(), (response) => MainThreadDispatcher.Enqueue(() => SwitchPlayers(response)));
        socket.On(ServerReponseApi.PLAYER_FINISHED_MOVING.ToString(), (response) => MainThreadDispatcher.Enqueue(() => PlayerFinishedMoving(response)));
        socket.On(ServerReponseApi.AVOID_SWITCH_PLAYER.ToString(), (response) => MainThreadDispatcher.Enqueue(() => AvoidSwitchingPlayer(response)));
        socket.On(ServerReponseApi.PLAYER_MOVED.ToString(), (response) => MainThreadDispatcher.Enqueue(() => PlayerMoved(response)));
        socket.On(ServerReponseApi.ONLINE_PLAYERS.ToString(), (response) => MainThreadDispatcher.Enqueue(() => OnlinePlayers(response)));
        socket.On(ServerReponseApi.EXIT_ROOM.ToString(), (response) => MainThreadDispatcher.Enqueue(() => ExitRoom(response)));
        socket.On(ServerReponseApi.GET_PLAYERS_INFO.ToString(), (response) => MainThreadDispatcher.Enqueue(() => GetPlayerInfo(response)));
        socket.On(ServerReponseApi.CUSTOM_ROOM_CREATED.ToString(), (response) => MainThreadDispatcher.Enqueue(() => OnCustomRoomCreated(response)));
        socket.On(ServerReponseApi.PLAYER_LEFT_ROOM.ToString(), (response) => MainThreadDispatcher.Enqueue(() => PlayerLeftRoom(response)));
        socket.On(ServerReponseApi.DICE_ROLLED.ToString(), (response) => MainThreadDispatcher.Enqueue(() => DiceRolled(response)));
        socket.On("playerJoined", (response) => MainThreadDispatcher.Enqueue(() => PlayerJoined(response)));
        socket.On("RoomJoined", (response) => MainThreadDispatcher.Enqueue(() => RoomJoined(response)));
        //socket.On("test", (response) => MainThreadDispatcher.Enqueue(() =>Debug.Log("test data : " + response)));    
        // Chat listeners - using the same pattern as your other listeners
        socket.On("new_chat_message", (response) => MainThreadDispatcher.Enqueue(() => OnNewChatMessage(response.ToString())));
        socket.On("chat_history", (response) => MainThreadDispatcher.Enqueue(() => OnChatHistory(response.ToString())));
        //socket.On("chat_assets", (response) => MainThreadDispatcher.Enqueue(() => OnChatAssets(response.ToString())));
        socket.On("player_joined_chat", (response) => MainThreadDispatcher.Enqueue(() => OnPlayerJoinedChat(response.ToString())));
        socket.On("player_left_chat", (response) => MainThreadDispatcher.Enqueue(() => OnPlayerLeftChat(response.ToString())));

    }


    private void GetPlayerInfo(SocketIOResponse response)
    {
        try
        {

            Debug.Log("Get player info : " + response.ToString());
        }
        catch (Exception e)
        {
            print(e);
        }
    }

    private void OnCustomRoomCreated(SocketIOResponse response)
    {
        Debug.Log("custom room created : " + response);
        string rawResponse = response.ToString();


        JArray jsonResponseArray = JArray.Parse(rawResponse);
        JObject firstObject = jsonResponseArray[0] as JObject;

        if (firstObject != null)
        {
            string roomId = firstObject["roomId"].ToString();

            LobbyUI.instance.OnRoomCreated(roomId);
        }
        else
        {
            Debug.LogError("Invalid response format: Missing 'somethingyyy'.");
        }

    }

    void PlayerLeftRoom(SocketIOResponse response)
    {
        Debug.Log("player left room : " + response);
        string rawResponse = response.ToString();
        Debug.Log("Received Game Result: " + rawResponse);


        JArray jsonResponseArray = JArray.Parse(rawResponse);
        JObject firstObject = jsonResponseArray[0] as JObject;

        if (firstObject != null)
        {
            string playerId = firstObject["playerId"].ToString();
            Debug.Log("player left room " + playerId);
            GameLiveData.instance.playersIdList.Remove(playerId);
            GameLiveData.instance.playersProfileIndexList.Remove(GameLiveData.instance.playersIdList.IndexOf(playerId));
            LobbyUI.instance.UpdatePlayers();
        }
        else
        {
            Debug.LogError("Invalid response format: Missing 'somethingyyy'.");
        }

    }

    void OnConnected(SocketIOResponse response)
    {
        print("connected");
#if UNITY_ANDROID

#endif
    }
    private void StartGame(SocketIOResponse response)
    {
        try
        {
            Debug.Log("StartGame response : " + response.ToString());

            JArray jsonArray = JArray.Parse(response.ToString());
            JObject gameStatus = (JObject)jsonArray[0];

            string currentPlayerId = (string)gameStatus["currentPlayerId"];
            string nextPlayerId = (string)gameStatus["nextPlayerId"];
            List<string> allPlayersId = gameStatus["allPlayersId"].ToObject<List<string>>();
            Dictionary<string, int> profileImagesIndex = gameStatus["allProfiles"].ToObject<Dictionary<string, int>>();
            Dictionary<string, string> playerNames = gameStatus["playerNames"].ToObject<Dictionary<string, string>>();
            Dictionary<string, string> playerWallets = gameStatus["playerWallets"].ToObject<Dictionary<string, string>>();
            AnalyseAndRegisterOnlinePlayers a = FindObjectOfType<AnalyseAndRegisterOnlinePlayers>();
            AnalyseAndRegisterOnlinePlayers.instance.AnaliysisAndRegistration(allPlayersId, profileImagesIndex, playerNames);
            ServerRequest.instance.OnGameStarted();
            GameLiveData g = GameLiveData.instance;
            g.playersIdList = new List<string>();
            foreach (string s in allPlayersId)
            {
                g.playersIdList.Add(s);
            }
            foreach (string n in playerNames.Values)
            {
                g.playersNameList.Add(n);
            }
            foreach (string w in playerWallets.Values)
            {
                g.playersWalletList.Add(w);
            }
            foreach (int i in profileImagesIndex.Values)
            {
                g.playersProfileIndexList.Add(i);
            }

            g.roomLength = allPlayersId.Count;
            g.currentPlayersChance = currentPlayerId;
            g.nextPlayerId = nextPlayerId;
            SceneManager.LoadScene("Ludo");
        }
        catch (Exception error)
        {
            print("exception " + error);
        }

    }

    void SwitchPlayers(SocketIOResponse response)
    {
        Debug.Log("SwitchPlayers response : " + response.ToString());
        DiceController.instance.RemoveAllHighlights();
        JArray jsonArray = JArray.Parse(response.ToString());
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        string currentPlayerId = (string)gameStatus["passedBy"];
        string nextPlayerId = (string)gameStatus["passedTo"];
        GameLiveData.instance.currentPlayersChance = nextPlayerId;
        UserSocketDetails currentPlayerData = GameLiveData.instance.GetPlayerPawnType(nextPlayerId);
        PawnType currentPawn = currentPlayerData.pawnType;
        print("#switchpawns " + currentPawn);

        PawnTimer.instance.StartTimer(currentPawn);

        if (GameLocalData.pawnType == currentPawn)
        {
            //Player ki chance hai!!
            DiceController.instance.playerMovementIsFinished = true;
            DiceController.instance.currentPawn = currentPawn;
        }
        else
        {
            DiceController.instance.playerCanMove = false;
        }

        if (nextPlayerId == BootstrapLobbyAdapter.GetUserId())
        {
            DiceController.instance.canRollDice = true;
            DiceController.instance.myChanceImage.SetActive(true);
        }
    }

    void DiceRolled(SocketIOResponse response)
    {
        Debug.Log("DiceRolled response : " + response.ToString());

        JArray jsonArray = JArray.Parse(response.ToString());
        JObject data = (JObject)jsonArray[0];

        int diceValue = data["diceValue"]?.Value<int>() ?? -1;
        string playerId = (string)data["playerId"]; /// He rolled the dice
        string nextPlayerId = (string)data["nextPlayerId"];


        StartCoroutine(DiceController.instance.RollDice(playerId, diceValue));

        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        if (playerId == BootstrapLobbyAdapter.GetUserId())
        {
            //mene chalaya hai
            DiceController.instance.currentDiceValue = diceValue;

            if (!DiceController.instance.CanPlayerMovePawns(GameLiveData.instance.GetPlayerPawnType(playerId).pawnType, diceValue))
            {
                Debug.Log("player can't move pawns");
                GameLiveData.instance.currentPlayersChance = nextPlayerId;
                ServerRequest.instance.SwitchPlayers();
            }
            else
            {
                DiceController.instance.HighlightMovablePawns(GameLiveData.instance.GetPlayerPawnType(playerId).pawnType, diceValue);
                GameLiveData.instance.nextPlayerId = nextPlayerId;
                if (playerId == BootstrapLobbyAdapter.GetUserId())
                {
                    DiceController.instance.playerCanMove = true;
                }
                Debug.Log("Player can move pawn/pawns " + DiceController.instance.MoveablePawnsCount(diceValue));
                if (DiceController.instance.MoveablePawnsCount(diceValue) == 1)
                {
                    Debug.Log("dice value is " + diceValue + " and only one pawn can move, auto moving the pawn");
                    DiceController.instance.AutoMoveTheOnlyPawn(diceValue);
                }

            }
        }


        if (playerId == BootstrapLobbyAdapter.GetUserId())
        {
            //mene chalaya hai
            if (!DiceController.instance.playerCanMove & diceValue != 6)
            {
                //DiceController.instance.canRollDice = true;
            }
        }
        else
        {
            //opponent ne chalaya hai
            if (!DiceController.instance.CanPlayerMovePawns(GameLiveData.instance.GetPlayerPawnType(playerId).pawnType, diceValue))
            {
                DiceController.instance.canRollDice = true;
            }
        }

    }


    // void RollDice(SocketIOResponse response)
    // {
    //     Debug.Log("RollDice response : " + response.ToString());

    //     JArray jsonArray = JArray.Parse(response.ToString());
    //     JObject data = (JObject)jsonArray[0];

    //     int diceValue = data["diceValue"]?.Value<int>() ?? -1;
    //     string nextPlayerId = (string)data["nextPlayerId"];
    //     string currentPlayerId = (string)data["currentPlayerId"];

    //     if (!DiceController.instance.CanPlayerMovePawns(GameLiveData.instance.GetPlayerPawnType(currentPlayerId).pawnType, diceValue))
    //     {
    //         Debug.Log("player can't move pawns");
    //         GameLiveData.instance.currentPlayersChance = nextPlayerId;
    //     }
    //     else GameLiveData.instance.nextPlayerId = nextPlayerId;

    //             StartCoroutine(DiceController.instance.RollDice(currentPlayerId, diceValue));

    //     if (currentPlayerId == BootstrapLobbyAdapter.GetUserId())
    //     {
    //         //mene chalaya hai
    //         if (nextPlayerId == BootstrapLobbyAdapter.GetUserId() && !DiceController.instance.playerCanMove & diceValue != 6)
    //         {
    //             DiceController.instance.canRollDice = true;
    //         }
    //     }
    //     else
    //     {
    //         //opponent ne chalaya hai
    //         if (!DiceController.instance.CanPlayerMovePawns(GameLiveData.instance.GetPlayerPawnType(currentPlayerId).pawnType, diceValue))
    //         {
    //             DiceController.instance.canRollDice = true;
    //         }
    //     }       
    // }

    void OnPlayerWin(SocketIOResponse response)
    {
        Debug.Log("OnPlayerWin response : " + response.ToString());

        JArray jsonArray = JArray.Parse(response.ToString());
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        string winnerId = (string)gameStatus["playerId"];

        if (winnerId == BootstrapLobbyAdapter.GetUserId())
        {
            Ludo.GameManager.instance.YouWin();
            return;
        }
        Ludo.GameManager.instance.YouLose();



    }

    void PlayerJoined(SocketIOResponse response)
    {
        JArray jsonArray = JArray.Parse(response.ToString());
        JObject data = (JObject)jsonArray[0];

        string playerId = (string)data["playerId"];
        string roomId = (string)data["roomId"];
        int profileImageIndex = (int)data["profileImageIndex"];
        Debug.Log("new player joined " + playerId + " with profile " + profileImageIndex);
        GameLiveData.instance.playersIdList.Add(playerId);
        GameLiveData.instance.playersProfileIndexList.Add(profileImageIndex);
        if (playerId != BootstrapLobbyAdapter.GetUserId())
            LobbyUI.instance.UpdatePlayers();
    }

    void RoomJoined(SocketIOResponse response)
    {
        JArray jsonArray = JArray.Parse(response.ToString());
        JObject data = (JObject)jsonArray[0];
        Dictionary<string, int> profiles = data["profiles"].ToObject<Dictionary<string, int>>();
        string roomId = (string)data["roomId"];
        Debug.Log("new player joined in  " + roomId);
        LobbyUI.instance.RoomJoined(roomId, profiles);

    }

    void AvoidSwitchingPlayer(SocketIOResponse response)
    {
        Debug.Log("AvoidSwitchingPlayer response : " + response.ToString());

        JArray jsonArray = JArray.Parse(response.ToString());
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        int diceValue = (int)gameStatus["diceValue"];
        string currentPlayerId = (string)gameStatus["currentPlayerId"];

        StartCoroutine(DiceController.instance.RollDice(currentPlayerId, diceValue));
    }

    void PlayerFinishedMoving(SocketIOResponse response)
    {
        Debug.Log("Player Finished Moving response : " + response.ToString());


        JArray jsonArray = JArray.Parse(response.ToString());
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        int diceValue = (int)gameStatus["diceValue"];
        string nextPlayerId = (string)gameStatus["nextPlayerId"];

        UserSocketDetails userSocketDetails = GameLiveData.instance.GetPlayerPawnType(nextPlayerId);
        PawnType currentPawn = userSocketDetails.pawnType;
        print("#PawnFinishedMoving " + currentPawn);

        if (GameLocalData.pawnType == currentPawn)
            DiceController.instance.playerMovementIsFinished = true;

        DiceController.instance.currentPawn = currentPawn;
        DiceController.instance.UpdateValue();
        if (diceValue != 6)
        {
            PawnTimer.stopTimer = true;
            PawnTimer.instance.StartTimer(currentPawn);

        }
        GameLiveData.instance.currentPlayersChance = nextPlayerId;
        if (nextPlayerId == BootstrapLobbyAdapter.GetUserId())
        {
            DiceController.instance.canRollDice = true;
            //DiceController.instance.myChanceImage.SetActive(true);
        }
    }



    private void RegisterPlayerId(SocketIOResponse response)
    {
        Debug.Log("RegisterPlayerId response : " + response.ToString());

        /*LocalPlayer.playerId = JsonConvert.DeserializeObject < string> ( e.data["id"].ToString());
        UiManager.instance.UpdateUi();
        LocalPlayer.SaveGame();
        print("playerId"+ LocalPlayer.playerId);*/
    }



    private void ExitRoom(SocketIOResponse response)
    {
        try
        {

            print("exit room triger");
            Debug.Log("ExitRoom response : " + response.ToString());

            /*  string playerId = JsonConvert.DeserializeObject<string>(e.data["playerId"].ToString());
              PawnType exitPawn = TempOnlinePlayersData.instance.GetPlayerPawnType(playerId);
              print("exit room triger by :"+playerId+"and pawntype:"+exitPawn);
              PlayerInfo.instance.RemovePawn(exitPawn);
              UiManager.instance.ExitPanel(exitPawn);
              print($"remove player {playerId} of pawntype {exitPawn}");
              TempOnlinePlayersData.instance.RemovePlayer(playerId);*/

        }
        catch (Exception error)
        {
            print(error.Message);
            throw;
        }
    }

    private void YouWin(SocketIOResponse response)
    {
        Debug.Log("YouWin response : " + response.ToString());
        JArray jsonArray = JArray.Parse(response.ToString());
        JObject gameStatus = (JObject)jsonArray[0];
        if (gameStatus.ContainsKey("playerId"))
        {
            string playerId = (string)gameStatus["playerId"];
            if (playerId == BootstrapLobbyAdapter.GetUserId())
            {
                Ludo.GameManager.instance.YouWin();
            }
            else
            {
                Ludo.GameManager.instance.YouLose();
            }
        }

        try
        {

            //string playerId = JsonConvert.DeserializeObject<string>(e.data["playerId"].ToString());
            //PawnType exitPawn = TempOnlinePlayersData.instance.GetPlayerPawnType(playerId);
            //_youWinPanel.OnPlayerWin(exitPawn);
            //PlayerInfo.instance.RemovePawn(exitPawn);
            //UiManager.instance.ExitPanel(exitPawn);
            //print($"remove player {playerId} of pawntype {exitPawn}");
            //TempOnlinePlayersData.instance.RemovePlayer(playerId);
        }
        catch (Exception error)
        {

            print(error.Message);
        }
    }
    private void OnDisconnected(SocketIOResponse response)
    {
        Debug.Log("Client disconnected: ");
    }

    private void PlayerMoved(SocketIOResponse response)
    {
        Debug.Log("PlayerMoved response : " + response.ToString());
        JArray jsonArray = JArray.Parse(response.ToString());
        JObject gameStatus = (JObject)jsonArray[0];
        // Parse the values from the JSON object
        int diceValue = (int)gameStatus["diceValue"];
        int pawnNo = (int)gameStatus["pawnNo"];
        string playerId = (string)gameStatus["playerId"];

        UserSocketDetails userSocketDetails = GameLiveData.instance.GetPlayerPawnType(playerId);
        PawnType pawnType = userSocketDetails.pawnType;
        print($"pawm movement dicevalue:{diceValue}  pawnNo:{pawnNo} pawnType:{pawnType} playerId:{playerId}");
        FindAndMoveThePawn(diceValue, pawnNo, pawnType);
    }


    private void FindAndMoveThePawn(int diceValue, int pawnNo, PawnType pawnType)
    {
        foreach (var pawn in PlayerInfo.instance.pawnInstances)
        {
            bool notSamePawn = pawn.pawnType != pawnType;
            if (notSamePawn)
                continue;
            if (pawn.pawnNumber == pawnNo)
            {
                if (pawn.isLeftTheHouse == false && diceValue != 6)
                {
                    Debug.Log("pawn not moved as its in home and dice value is not 6");
                    return;
                }
                else if (pawn.isLeftTheHouse == false && diceValue == 6)
                {
                    Debug.Log("pawn is moving out of home");
                    diceValue = 1;
                    pawn.isLeftTheHouse = true;
                }

                StartCoroutine(pawn.MoveTo(diceValue, pawnType, true));
                if (pawn.spotIndexOnLudoBoard == 0)
                    pawn.isLeftTheHouse = true;
            }
        }

        ////////////////////////////////Chats--------------------------
        /// 

        ////////////////////////////////Chats--------------------------
        /// 
        /// 
        /// 
    }
    private void OnNewChatMessage(string response)
    {
        Debug.Log("OnNewChatMessage response : " + response.ToString());
        if (ChatManager.Instance != null)
            ChatManager.Instance.OnNewChatMessage(response.ToString());
    }

    private void OnChatHistory(string response)
    {
        if (ChatManager.Instance != null)
            ChatManager.Instance.OnChatHistory(response.ToString());
    }

    private void OnChatAssets(string response)
    {
        if (ChatManager.Instance != null)
            ChatManager.Instance.OnChatAssets(response.ToString());
    }

    private void OnPlayerJoinedChat(string response)
    {
        if (ChatManager.Instance != null)
            ChatManager.Instance.OnPlayerJoinedChat(response.ToString());
    }

    private void OnPlayerLeftChat(string response)
    {
        if (ChatManager.Instance != null)
            ChatManager.Instance.OnPlayerLeftChat(response.ToString());
    }


    private void OnlinePlayers(SocketIOResponse response)
    {
        /* int players = int.Parse(e.data["onlinePlayers"].ToString());
         UiManager.instance.onlinePlayers.text = players.ToString();*/

    }




}

