using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludo;

public class OnlinePlayersProfileManager : MonoBehaviour
{
    [SerializeField] private Ludo.Player[] players;

    public void SetPlayersProfile(PawnType pawn,string profilePic,string playerId)
    {
        foreach (var player in players)
        {
            if (player.playerPawn == pawn)
            {
                player.SetProfile(profilePic, playerId);
            }
        }
    }
}
