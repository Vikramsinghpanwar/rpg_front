using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TeenpattiGameDataLobby : MonoBehaviour
{
    public static string teenpattiTableID_for_JOIN;
    public static TeenpattiGameDataLobby Instance;
    public static bool createTable;
    public static bool isRejoin;
    public TMP_InputField tableID_IF;

    void Awake()
    {
        if(Instance != this)
        {
            Instance = this;
        }
    }
    public void Teenpatti_GameMode(int mode)
    {
        if(mode == 0)
        GameMode.mode = GameMode.Modes.publicGame;
        else if(mode == 1)
        GameMode.mode = GameMode.Modes.privateGame;
    }

    public void JoinPrivateTable(string tableID = ""){
        string tableIDToUse = tableID != "" ? tableID : tableID_IF.text;
        if(tableIDToUse == "" || tableIDToUse.Length < 5) {
            Logger.Instance.Error("Invalid table ID (at least 5 characters).");
            return;
        }
        else
        {
            createTable = false;
            teenpattiTableID_for_JOIN = tableIDToUse;
            GameMode.mode = GameMode.Modes.privateGame;
            SceneManager.LoadScene("LobbyTeenpatti");
        }
    }

    public void CreatePrivateTable(){
        createTable = true;
    }
}
