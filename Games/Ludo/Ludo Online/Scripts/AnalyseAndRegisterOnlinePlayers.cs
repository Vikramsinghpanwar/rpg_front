
using System;
using System.Collections.Generic;
using Features.Lobby.Integration;
using UnityEngine;

class AnalyseAndRegisterOnlinePlayers : MonoBehaviour
{

    public static AnalyseAndRegisterOnlinePlayers instance;
    private void Awake()
    {
        instance = this;
    }

    public void AnaliysisAndRegistration(List<string> playersId, Dictionary<string, int> playerProfilesIndex, Dictionary<string, string> playerNames)
    {


        int thisPlayerIdInTheListIndex = 0;
        if (playersId.Contains(BootstrapLobbyAdapter.GetUserId()))
        {
            thisPlayerIdInTheListIndex = playersId.FindIndex(x => x == BootstrapLobbyAdapter.GetUserId());
        }
        else
        {
            print("player id not in the list");
            print("something went wrong try again");
            return;
        }
        Registertion(thisPlayerIdInTheListIndex, playersId, playerProfilesIndex, playerNames);
    }


    private void Registertion(int thisPlayerListIndex, List<string> playersId, Dictionary<string, int> playerProfilesIndex, Dictionary<string, string> playerNames)
    {
        switch (playersId.Count)
        {
            case 2:
                TwoPlayers.instance.PawnTypeAssignerToPlayerId(playersId, playerProfilesIndex, playerNames);
                break;
            case 3:
                FourPlayers.instance.PawnTypeAssignerToPlayerId(playersId, playerProfilesIndex, playerNames);
                break;
            case 4:
                FourPlayers.instance.PawnTypeAssignerToPlayerId(playersId, playerProfilesIndex, playerNames);
                break;
            default:
                break;
        }
    }
}

