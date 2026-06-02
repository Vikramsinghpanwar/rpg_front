using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Teenpatti;

namespace Teenpatti
{


    public class GameLiveData : MonoBehaviour
    {
        public static GameLiveData instance { get; private set; }
        public static bool wasReconnectFromGateway = false;
        public int localPlayerPivotIndex = 0;

        public int commision;
        [Header("Game Settings")]
        public float lastBetAmount;

        public float bootAmount;
        public float potLimit;
        public float chaalLimit;
        public int totalPlayers;
        public int spectatorSeatPosition;
        public PlayerDetails[] playerDetailsArray;

        [Header("Game State")]
        public string tableId;
        public int currentRound;
        public int currentPot;
        public int dealerPosition;
        public int currentTurn;
        public bool isGameActive = false;
        public bool rejoin;
        public bool isSpectator;
        public string tableState;
        public bool lastPlayerSeen;
        public bool isPrivateTable;
        public string privateTableCode;
        public float timeRemainingForTurn;
        public string rechargingPlayer;
        public float rechargeTimeRemaining;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ResetGameData()
        {
            lastBetAmount = 0;
            potLimit = 0;
            chaalLimit = 0;
            tableId = "";
            currentTurn = 0;
            totalPlayers = 0;
            playerDetailsArray = null;
            lastPlayerSeen = false;
            currentRound = 1;
            currentPot = 0;
            dealerPosition = -1;
            currentTurn = -1;
            isGameActive = false;
        }
    }
}