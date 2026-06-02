using System.Collections.Generic;
using UnityEngine;

 public class UserSocketDetails
    {
        public PawnType pawnType;
        public string playerName;
        public int profileImageIndex;
    }

public class GameLiveData : MonoBehaviour
{
   
    public static GameLiveData instance;

    private void Awake()
    {
        instance = this;
    }


    [SerializeField] public string mySocketId;
    [SerializeField] public string currentPlayersChance;
    [SerializeField] public string nextPlayerId;
    [SerializeField] public int roomLength;
    [SerializeField] public List<string> playersIdList;
    [SerializeField] public List<int> playersProfileIndexList;
    [SerializeField] public List<string> playersNameList;
    [SerializeField] public List<string> playersWalletList;
    [SerializeField] public int roomAmount;

    // NEW: Dictionary to track PawnType per playerId
    private Dictionary<string, UserSocketDetails> _playerPawnTypes = new Dictionary<string, UserSocketDetails>();

    // Method to Add a Player and their PawnType
    public void AddPlayerPawnType(string playerId, UserSocketDetails UserSocketDetails)
    {
        if (!_playerPawnTypes.ContainsKey(playerId))
        {
           
            _playerPawnTypes.Add(playerId, UserSocketDetails);
        }
        else
        {
            Debug.LogWarning($"Player {playerId} already exists in pawn type dictionary.");
        }
    }

    // Method to Remove a Player's PawnType
    public void RemovePlayerPawnType(string playerId)
    {
        if (_playerPawnTypes.ContainsKey(playerId))
        {
            _playerPawnTypes.Remove(playerId);
        }
        else
        {
            Debug.LogWarning($"Tried to remove non-existing player {playerId} from pawn type dictionary.");
        }
    }

    // Method to Get a Player's PawnType
    public UserSocketDetails GetPlayerPawnType(string playerId)
    {
        if (_playerPawnTypes.TryGetValue(playerId, out UserSocketDetails UserSocketDetails))
        {
            return UserSocketDetails;
        }
        else
        {
            Debug.LogWarning($"PawnType not found for playerId {playerId}. Returning default PawnType.");
            return null;
        }
    }

    // Method to Clear all Player PawnTypes
    public void ClearAllPlayerPawnTypes()
    {
        _playerPawnTypes.Clear();
    }
}
