using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.SocialPlatforms;
using System.Collections.Generic;
using System;
using UnityEngine.PlayerLoop;
using Features.Lobby.Integration;
using Core.Bootstrap;

class ServerRequest : MonoBehaviour
{


    public static ServerRequest instance;
    public bool serverConnection = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            ThisPlayerWins();
        }

    }

    public void StartGameInRoom(string roomId)
    {
        if (serverConnection)
            return;

        var obj = new Dictionary<string, object>
        {
            { "roomName", roomId},
        };
        SocketManager.socket.Emit(ServerRequestApi.START_GAME_IN_ROOM.ToString(), obj);
    }

    public void OnGameStarted()
    {
        if (serverConnection)
            return;

        object playerId = new { BootstrapService.Instance?.Profile?.public_id };
        Debug.Log("player id : " + playerId);
        var obj = new Dictionary<string, object>
        {
            { "playerId", BootstrapLobbyAdapter.GetUserId()},
        };
        string data = JsonConvert.SerializeObject(obj);
        SocketManager.socket.Emit(ServerRequestApi.ON_GAME_STARTED.ToString(), obj);
    }

    public void RollDice_Request()
    {
        Debug.Log("sending Dice Roll Request");
        if (serverConnection)
            return;
        var obj = new Dictionary<string, object>
        {
            { "playerId", BootstrapLobbyAdapter.GetUserId()},
        };
        SocketManager.socket.Emit(ServerRequestApi.ROLL_DICE_REQUEST.ToString(), obj);
    }

    public void SwitchPlayers()
    {
        if (serverConnection)
            return;
        var obj = new Dictionary<string, object>
        {
            { "playerId", BootstrapLobbyAdapter.GetUserId()},
        };
        string data = JsonConvert.SerializeObject(obj);
        SocketManager.socket.Emit(ServerRequestApi.SWITCH_PLAYER.ToString(), data);
    }

    public void AvoidSwitchingPlayer()
    {
        if (serverConnection)
            return;
        var obj = new Dictionary<string, object>
        {
            { "playerId", BootstrapLobbyAdapter.GetUserId()},
            { "diceValue", DiceController.instance.currentDiceValue},
        };
        string data = JsonConvert.SerializeObject(obj);
        SocketManager.socket.Emit(ServerRequestApi.AVOID_SWITCHING_PLAYER.ToString(), data);
    }
    public void PlayerFinishedMoving(bool richedTheDestination)
    {
        if (serverConnection)
            return;
        int diceValue = DiceController.instance.currentDiceValue;
        var obj = new Dictionary<string, object>
        {
            { "diceValue", diceValue},
            { "playerId", BootstrapLobbyAdapter.GetUserId()},
            { "richedTheDestination", richedTheDestination}
        };
        string data = JsonConvert.SerializeObject(obj);
        Debug.Log("sending " + data.ToString());
        SocketManager.socket.Emit(ServerRequestApi.PLAYER_FINISHED_MOVING.ToString(), obj);
    }


    public void ExitRoom()
    {
        if (serverConnection)
            return;
        var obj = new Dictionary<string, object>
        {
            { "roomId", LobbyUI.instance.roomId_TMP.text},
            { "playerId", BootstrapLobbyAdapter.GetUserId()},
        };
        SocketManager.socket.Emit(ServerRequestApi.EXIT_ROOM.ToString(), obj);
    }

    public void QuitGame()
    {
        if (serverConnection)
            return;
        //var quit = new { quit = true, PlayerInfo.instance.players, LocalPlayer.playerId };
        //SocketManager.socket.Emit(ServerRequestApi.ON_QUIT.ToString(), new JSONObject(JsonConvert.SerializeObject(quit)));
    }
    public void ThisPlayerWins()
    {
        Debug.Log("This player wins called");
        if (serverConnection)
            return;
        var obj = new Dictionary<string, object>
        {
            { "playerId", BootstrapLobbyAdapter.GetUserId()}
        };
        SocketManager.socket.Emit(ServerRequestApi.ON_PLAYER_WIN.ToString(), obj);

    }

    public void MovePlayer(int diceValue, int pawnNo, int pawnType)
    {
        if (serverConnection)
            return;
        var obj = new Dictionary<string, object>
        {
            { "diceValue", diceValue},
            { "pawnNo", pawnNo},
            { "playerId", BootstrapLobbyAdapter.GetUserId()}
        };
        SocketManager.socket.Emit(ServerRequestApi.MOVE_PLAYER.ToString(), obj);
    }

    public void WinMove(int val)
    {
        var obj = new Dictionary<string, object>
        {
            { "diceValue", 56},
            { "pawnNo", val},
            { "playerId", BootstrapLobbyAdapter.GetUserId()}
        };
    }

    public void OnlinePlayers()
    {
        if (serverConnection)
            return;
        SocketManager.socket.Emit(ServerReponseApi.ONLINE_PLAYERS.ToString());
    }



    // public void OnPlayerWins(PawnType winnerPawn)
    // {
    //     if (serverConnection)
    //         return;
    //     object winner = new { winnerPawn };

    //     SocketManager.socket.Emit(ServerReponseApi.ON_PLAYER_WIN.ToString(),
    //         new JSONObject(JsonConvert.SerializeObject(winner)));

    // }

    public void PawnKilled(PawnType pawnType, int pawnNumber)
    {
        return;
        Debug.Log("sending pawn killed for pawn type: " + pawnType.ToString() + " pawn number: " + pawnNumber);
        Debug.Log("GameLocalData pawn type: " + GameLocalData.pawnType.ToString());
        if (pawnType != GameLocalData.pawnType) return;
        if (serverConnection)
            return;
        var obj = new Dictionary<string, object>
        {
            { "pawnType", pawnType.ToString()},
            { "pawnNumber", pawnNumber},
            { "playerId", BootstrapLobbyAdapter.GetUserId()}
        };
        Debug.Log("sending data");
        SocketManager.socket.Emit(ServerRequestApi.PAWN_KILLED.ToString(), obj);
        Debug.Log("sending pawn killed : " + JsonConvert.SerializeObject(obj));

    }

}

