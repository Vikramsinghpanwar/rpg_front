using System;
using System.Collections.Generic;
using Teenpatti;

namespace Teenpatti{

[System.Serializable]
public class PlayerDetails
{
    public string id;
    public string userId;
    public string username;
    public string token;
    public float amount;
    public float chips;
    public string status;
    public string[] cards;
    public string cardsType;
    public bool isMyTurn;
    public string action;
    public float totalBetAmount;
    public float betAmount;
    public bool participant;
    public int profileImageIndex;
    public bool hasFolded = false;
    public bool hasSeenCards = false;
    public int position = 0;
    public bool isActive = true;
    public bool isShowCards = false;
    public bool sideShowRequested = false;
    public bool isBot = false;  
    public bool canShow;  
    public bool canSideShow;  
}

 

[System.Serializable]
public class CardData
{
    public string rank;
    public string suit;
    public int value;
    public string code;
}

[System.Serializable]
public class TableData
{
    public string tableId;
    public string tableType;
    public string tableName;
    public int maxPlayers;
    public int minBet;
    public int maxBet;
    public int bootAmount;
    public int currentPlayers;
    public List<PlayerGameData> players;
    public string gameState;
    public string currentRound;
    public int dealerPosition;
    public int currentTurn;
    public int pot;
    public List<CardData> communityCards;
    public DateTime createdAt;
}

[System.Serializable]
public class PlayerGameData
{
    public string token;
    public string userId;
    public string username;
    public int chips;
    public int position;
    public bool isActive;
    public bool isTurn;
    public string lastAction;
    public List<CardData> cards;
    public int betAmount;
    public int totalBet;
    public bool hasFolded;
    public bool hasSeenCards;
    public bool isShowCards;
}

[System.Serializable]
public class GameStartData
{
    public int dealerPosition;
    public int currentTurn;
    public int pot;
    public int currentBet;
    public int currentRound;
    public List<PlayerGameData> players;
}

[System.Serializable]
public class PlayerActionData
{
    public string playerId;
    public string username;
    public string action;
    public int amount;
    public int? nextTurn;
    public int pot;
    public int currentBet;
    public PlayerGameData updatedPlayer;
}

[System.Serializable]
public class WinnerData
{
    public string userId;
    public string username;
    public string handRank;
    public int prize;
    public List<CardData> cards;
}

[System.Serializable]
public class GameEndData
{
    public WinnerData winner;
    public List<WinnerData> winners;
    public int pot;
    public int finalPot;
}

[System.Serializable]
public class TableCreatedData
{
    public bool success;
    public string tableId;
    public TableData table;
}

[System.Serializable]
public class TableUpdateData
{
    public string type;
    public string tableId;
    public string tableName;
    public string tableType;
    public int currentPlayers;
    public int maxPlayers;
}
}