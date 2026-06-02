using UnityEngine;
using Teenpatti;

namespace Teenpatti{

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }
    
    // User Data
    public string userId = "";
    public string username = "";
    public int playerChips = 1000;
    public int totalGames = 0;
    public int wins = 0;
    public int losses = 0;
    public string avatar = "default";
    public int level = 1;
    public int experience = 0;
    public string authToken = "";
    
    // Current Game
    public string currentTableId = "";
    public bool isInGame = false;
    public bool isTableOwner = false;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Load saved data
        LoadPlayerData();
    }
    
    public void SavePlayerData(string token, string id, string name, int chips)
    {
        authToken = token;
        userId = id;
        username = name;
        playerChips = chips;
        
        PlayerPrefs.SetString("TP_Token", token);
        PlayerPrefs.SetString("TP_UserId", id);
        PlayerPrefs.SetString("TP_Username", name);
        PlayerPrefs.SetInt("TP_Chips", chips);
        PlayerPrefs.Save();
    }
    
    private void LoadPlayerData()
    {
        authToken = PlayerPrefs.GetString("TP_Token", "");
        userId = PlayerPrefs.GetString("TP_UserId", "");
        username = PlayerPrefs.GetString("TP_Username", "");
        playerChips = PlayerPrefs.GetInt("TP_Chips", 1000);
    }
    
    public void ClearData()
    {
        authToken = "";
        userId = "";
        username = "";
        playerChips = 1000;
        currentTableId = "";
        isInGame = false;
        isTableOwner = false;
        
        PlayerPrefs.DeleteKey("TP_Token");
        PlayerPrefs.DeleteKey("TP_UserId");
        PlayerPrefs.DeleteKey("TP_Username");
        PlayerPrefs.DeleteKey("TP_Chips");
        PlayerPrefs.Save();
    }
    
    public void UpdateChips(int amount)
    {
        playerChips += amount;
        PlayerPrefs.SetInt("TP_Chips", playerChips);
        PlayerPrefs.Save();
    }
}
}