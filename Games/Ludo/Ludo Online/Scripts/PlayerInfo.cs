using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    public List<PawnMovementController> pawnInstances=new List<PawnMovementController>();
    public GameObject[] safeSpots;
    public GameObject pawnPanel;
    public int players = 2;
    public static PlayerInfo instance;
    public PawnType selectedPawn;

    [SerializeField] private LudoBoard _board;
    [SerializeField] private ProfleImagePawnTypeMapper _profleImagePawnType;

    private void Awake()
    {
        instance = this;
    }

    public void Start(){
        selectedPawn = GameLocalData.pawnType;
        UpdateGameData(selectedPawn);
        DiceController.instance.currentPawn = selectedPawn;
    }

    void UpdateGameData(PawnType pawnType){
        GameLocalData.pawnType = pawnType;
        DiceController.instance.currentPawn = pawnType;
        DiceController.instance.UpdateValue();
        _board.Rotate(pawnType);
        _profleImagePawnType.ArangePawnType();
        PawnSpawner.instance.ArrangePawns(pawnType);
    }
    public void RemoveAllPawns()
    {
        pawnInstances.Clear();
        foreach (Transform pawn in pawnPanel.transform)
        {
            Destroy(pawn.gameObject);
        }
    }
    public void RemovePawn(PawnType removePawn)
    {
        //remove from list and game
        foreach (var pawn in pawnInstances)
        {
            if (pawn.pawnType == removePawn)
            {
                Destroy(pawn.gameObject);
            }
        }
        pawnInstances.RemoveAll(item => item == null);
        print("player left" + pawnInstances.Count);
    }
}
