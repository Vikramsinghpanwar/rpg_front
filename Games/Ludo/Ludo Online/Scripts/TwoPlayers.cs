
using System;
using System.Collections.Generic;
using Features.Lobby.Integration;
using UnityEngine;

public class TwoPlayers : OnlinePlayers
{
    public static TwoPlayers instance;

    private void Awake()
    {
        instance = this;
    }
    public override void PawnTypeAssignerToPlayerId(List<string> playersId, Dictionary<string, int> playerProfilesIndex, Dictionary<string, string> playerNames)
    {
        foreach (var id in playersId)
        {
            if (id == BootstrapLobbyAdapter.GetUserId())//LocalPlayer.playerId)
            {
                UserSocketDetails userSocketDetails = new UserSocketDetails
                {
                    pawnType = GameLocalData.pawnType,
                    playerName = UserDetail.UserName,
                    profileImageIndex = UserDetail.profileImageIndex
                };
                GameLiveData.instance.AddPlayerPawnType(id, userSocketDetails);
            }
            else
            {
                PawnType opponentPawnType = (PawnType)GetOpponentPawnColour();
                UserSocketDetails opponentUserSocketDetails = new UserSocketDetails
                {
                    pawnType = opponentPawnType,
                    playerName = playerNames[id],
                    profileImageIndex = playerProfilesIndex[id]
                };
                GameLiveData.instance.AddPlayerPawnType(id, opponentUserSocketDetails);
            }
        }
    }


    private int GetOpponentPawnColour()
    {
        int[] pawns = { 1, 2, 3, 4 };
        for (int i = 0; i < pawns.Length; i++)
        {
            bool isSameColour = pawns[i] == (int)GameLocalData.pawnType;
            if (isSameColour)
            {
                int opponentPawnColour = 2 + pawns[i] <= pawns.Length ? pawns[2 + i] : pawns[Math.Abs(pawns.Length - (2 + i))];
                return opponentPawnColour;
            }
        }
        return 0;
    }
}
