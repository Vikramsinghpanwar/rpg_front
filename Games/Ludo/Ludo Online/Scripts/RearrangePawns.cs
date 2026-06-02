using UnityEngine;


public class RearrangePawns : MonoBehaviour
{
    public static RearrangePawns instance;

    private void Awake()
    {
        instance = this;
    }


    /// <summary>
    /// this will move the all this player pawns at the bottom of hierarchy
    /// which will eventually put selected pawn on top of other players pawns
    /// </summary>
    public void Rearrange()
    {
        Debug.Log("Rearrangin pawans");
        foreach  (Transform pawn in PawnSpawner.instance.spotPanel.transform)
        {
            bool alreadyOnTheEdge = PawnSpawner.instance.spotPanel.transform.childCount - pawn.GetSiblingIndex() > 3;

            if (alreadyOnTheEdge)
                break;


            bool selectedPawn = pawn.GetComponent<PawnMovementController>().pawnType == GameLocalData.pawnType;

            if (selectedPawn)
            {
                pawn.SetSiblingIndex(PawnSpawner.instance.spotPanel.transform.childCount);
            }
        }
    }

}
