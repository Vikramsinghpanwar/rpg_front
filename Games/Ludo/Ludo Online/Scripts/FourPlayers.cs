using System.Collections.Generic;
using Features.Lobby.Integration;

public class FourPlayers : OnlinePlayers
{
    public static FourPlayers instance;

    private void Awake()
    {
        instance = this;
    }


    public override void PawnTypeAssignerToPlayerId(List<string> playersId, Dictionary<string, int> playerProfilesIndex, Dictionary<string, string> playerNames)
    {
        UserSocketDetails myUserSocketDetails = new UserSocketDetails
        {
            pawnType = GameLocalData.pawnType,
            playerName = UserDetail.UserName,
            profileImageIndex = UserDetail.profileImageIndex
        };
        GameLiveData.instance.AddPlayerPawnType(BootstrapLobbyAdapter.GetUserId(), myUserSocketDetails);
        int thisPlayerIdIndex = playersId.FindIndex(x => x == BootstrapLobbyAdapter.GetUserId());
        int pawnNo = (int)GameLocalData.pawnType;

        //this will assign pawnColour to otherplayerIds
        for (int i = thisPlayerIdIndex + 1; i < playersId.Count; i++)
        {
            pawnNo = GetOpponentPawnColour(pawnNo);
            UserSocketDetails userSocketDetails = new UserSocketDetails
            {
                pawnType = (PawnType)pawnNo,
                playerName = playerNames[playersId[i]],
                profileImageIndex = playerProfilesIndex[playersId[i]]
            };
            GameLiveData.instance.AddPlayerPawnType(playersId[i], userSocketDetails);
            //onlinePlayersProfileManager.SetPlayersProfile((PawnType)pawnNo, profiles[playersId[i]], playersId[i]);
        }

        for (int i = 0; i < thisPlayerIdIndex; i++)
        {
            pawnNo = GetOpponentPawnColour(pawnNo);
            UserSocketDetails userSocketDetails = new UserSocketDetails
            {
                pawnType = (PawnType)pawnNo,
                playerName = playerNames[playersId[i]],
                profileImageIndex = playerProfilesIndex[playersId[i]]
            };
            GameLiveData.instance.AddPlayerPawnType(playersId[i], userSocketDetails);
            //onlinePlayersProfileManager.SetPlayersProfile((PawnType)pawnNo, profiles[playersId[i]], playersId[i]);
        }
    }

    private int GetOpponentPawnColour(int currentPawnNo)
    {
        int[] pawns = { 1, 2, 3, 4 };
        int nextPawn = 0;

        for (int i = 0; i < pawns.Length; i++)
        {
            if (currentPawnNo == 4)
            {
                nextPawn = 1;
                break;
            }
            if (pawns[i] == currentPawnNo)
            {
                nextPawn = pawns[i + 1];
                break;
            }
        }

        return nextPawn;
    }
}
