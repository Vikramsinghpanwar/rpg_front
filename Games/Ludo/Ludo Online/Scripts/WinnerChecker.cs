using UnityEngine;
using UnityEngine.UI;

class WinnerChecker : MonoBehaviour
{
    private int noOfPawnRequireToWin = 4;
    public static WinnerChecker instance;

    private void Awake()=> instance = this;

    public void CheckForWinner()
    {
        Debug.Log("Checking for winner...");
        int noOfPawnsRichedTheDestination = 0;

        foreach (var pawn in PlayerInfo.instance.pawnInstances)
        {
            if (pawn.pawnType == GameLocalData.pawnType)
            {

                if(pawn.GetComponent<PawnMovementController>().richedTheDestination)
                    noOfPawnsRichedTheDestination++;
            }
        }
        if (noOfPawnsRichedTheDestination == noOfPawnRequireToWin)
        {
            ServerRequest.instance.ThisPlayerWins();
        }
    }

}

