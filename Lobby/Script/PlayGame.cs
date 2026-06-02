using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Features.Lobby.Integration;

public class PlayGame : MonoBehaviour
{
    private float Balance;
    GameActive gameActiveRef;
    public Error errorRef;

    void Start()
    {
        errorRef = FindObjectOfType<Error>();
        gameActiveRef = FindObjectOfType<GameActive>();
        Balance = BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;

    }

    public void Ludo_GameMode(int mode)
    {
        if (mode == 0)
            GameMode.mode_ludo = GameMode.Modes.publicGame;
        else if (mode == 1)
            GameMode.mode_ludo = GameMode.Modes.privateGame;

    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }


}
