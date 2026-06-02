using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootValueDecider : MonoBehaviour
{
    public static float teenPattiBootValue;
    public static float ak47BootValue;
    public static float JokerBootValue;
    public static float potBlindBootValue;
    public static float fourXCardBootValue;
    public static float muflisBootValue;
    public static float rummyBootValue;
    public static int ludoPlayerCount;
    public static int ludoEntryFee;
    public static int rummyPlayerCount;
    public static string teenpattiVariant = "";

    public GameObject addCashPanel;
    PlayGame playGameRef;
    private void Start()
    {
        playGameRef = FindObjectOfType<PlayGame>();
    }

    public void OpenTeenpatti(float bootVal)
    {
        teenPattiBootValue = bootVal;
        teenpattiVariant = "";
        playGameRef.LoadScene("LobbyTeenPatti");
    }


    public void OpenRummy(float bootVal)
    {
        rummyBootValue = bootVal;
        playGameRef.LoadScene("Rummy");
    }
    public void RummyPlayerCount(int val)
    {
        rummyPlayerCount = val;
    }
    
    public void LudoEntryFee(int ludoBetVal)
    {
        ludoEntryFee  = ludoBetVal;
        playGameRef.LoadScene("Ludo_Online");

    }
    public void LudoPlayer(int playerNum)
    {
        ludoPlayerCount = playerNum;
    }
}
