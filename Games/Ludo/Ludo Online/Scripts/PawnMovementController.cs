using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PawnMovementController : MonoBehaviour
{

    [SerializeField] public int spotIndexOnLudoBoard;
    [SerializeField] private GameObject[] commonMovingSpot;

    public int pawnNumber = 0;
    public int Lastspot = 0;
    public float speed = 20;
    public Spot currentSpot;
    public GameObject home;
    public PawnType pawnType;
    public bool richedTheDestination;

    private Spot destinationSpot;
    private bool _onSafeSpot;

    public GameObject highlight_Object;

    private void Start()
    {
        highlight_Object.SetActive(false);
        Lastspot = commonMovingSpot.Length - 1;
        spotIndexOnLudoBoard = -1;
        destinationSpot = commonMovingSpot[commonMovingSpot.Length - 1].GetComponent<Spot>();
    }
    public IEnumerator MoveTo(int moveToIndex, PawnType server_pawnType = PawnType.yellow, bool fromServer = false)
    {
        Debug.Log("Moving pawn: " + pawnType.ToString() + " by " + moveToIndex + " steps. From server: " + fromServer);
        CheckAndGetOutOfSafeSpot();
        if(DiceController.instance.currentDiceValue != 6)
        PawnTimer.stopTimer = true;
        Vector3 lastPostion = Vector3.zero;
        float distance = 0;
        int moveIndex = 0;

        int steps = moveToIndex;

        if (!fromServer)
        {

            ServerRequest.instance.MovePlayer(moveToIndex, pawnNumber, (int)pawnType);

            steps = stepOutFromHome == 1 ? stepOutFromHome : DiceController.instance.currentDiceValue;
            if (stepOutFromHome == 1) stepOutFromHome = 0;


        }

        for (int i = 1; i <= steps; i++)
        {
            // if(i + spotIndexOnLudoBoard >= commonMovingSpot.Length)
            // {
            //     Debug.LogError("abhi tak spot index: " + spotIndexOnLudoBoard);
            //     Debug.LogError("ab isme add kr rahe hai " + (steps - 1));
            //     Debug.LogError("Index out of bounds: " + (i + spotIndexOnLudoBoard) + " >= " + commonMovingSpot.Length);
            //     break;
            // }
            DiceController.instance.PlayPawnMoveSound();
            if(spotIndexOnLudoBoard != -1)
            moveIndex = i + spotIndexOnLudoBoard;
            Debug.Log("move indes : " + moveIndex);
            distance = (commonMovingSpot[moveIndex].transform.position - this.transform.position).sqrMagnitude;

            while (distance > .1f)
            {
                distance = (commonMovingSpot[moveIndex].transform.position - this.transform.position).sqrMagnitude;
                this.transform.position =
                    Vector3.MoveTowards(this.transform.position,
                    commonMovingSpot[moveIndex].transform.position,
                    speed);

                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(0.15f);
            lastPostion = commonMovingSpot[moveIndex].transform.position;
            currentSpot = commonMovingSpot[moveIndex].GetComponent<Spot>();
        }
        this.transform.position = lastPostion;
        FinishMovement(steps);
        CheckForPawnKill(fromServer, server_pawnType);
    }

    private void FinishMovement(int lastIndex)
    {
        spotIndexOnLudoBoard += lastIndex;
        DiceController.instance.playerMovementIsFinished = true;

        bool richedAtTheEnd = destinationSpot.spotNo == currentSpot.spotNo;
        if (richedAtTheEnd)
        {
            DiceController.instance.PlayPawnReachHomeSound();
            richedTheDestination = true;
            isLeftTheHouse = true;
            WinnerChecker.instance.CheckForWinner();
            ServerRequest.instance.AvoidSwitchingPlayer();
        }
    }
    private void CheckForPawnKill(bool fromServer, PawnType server_pawnType)
    {
        if (CanKillEnemyPawn())
        {
            DiceController.instance.PlayPawnKillSound();
            Debug.Log("Killing enemy pawn: " + pawnType.ToString());
            if (!fromServer)
            {
                PawnTimer.instance.StartTimer(pawnType);
            }
            else
            {
                //from server
                PawnTimer.instance.StartTimer(server_pawnType);
            }

            DiceController.instance.canRollDice = true;
            DiceController.instance.playerMovementIsFinished = true;
        }
        else
        {
            if (!fromServer)
            {
                ServerRequest.instance.PlayerFinishedMoving(richedTheDestination);
            }
        }
    }

    public void HighlightPawn(bool highlight)
    {
        highlight_Object.SetActive(highlight);
    }
    public void RemoveHighlight()
    {
        highlight_Object.SetActive(false);
    }
   
   
    public bool CanMoveAhead(int moveTo)
    {
        if (moveTo != 6)
            return false;
        if (!isLeftTheHouse)
            return false;
        bool canMoveForward = spotIndexOnLudoBoard + moveTo <= Lastspot;

        return (spotIndexOnLudoBoard + moveTo <= Lastspot);
    }

    private bool IsOnSafeSpot()
    {
        if (currentSpot.GetComponent<SafeSpot>() != null)
        {
            currentSpot.gameObject.GetComponent<PawnAjusterOnSafeSpots>().AddPawn(this.gameObject);
            _onSafeSpot = true;
            return true;
        }

        return false;
    }

    private bool CanKillEnemyPawn()
    {
        Debug.Log("Checking if pawn: " + pawnType.ToString() + " can kill any enemy pawn.");
        if (IsOnSafeSpot())
        {
            Debug.Log("Pawn is on safe spot");
            return false;
        }

        bool canKillOtherPlayerPawn = false;
        GameObject pawnsOnSafeSpot = new GameObject();
        pawnsOnSafeSpot = null;
        foreach (var otherPlayersPawn in PlayerInfo.instance.pawnInstances)
        {
            Debug.Log("round check");
            bool stillAtHome = otherPlayersPawn.spotIndexOnLudoBoard == -1;
            if (stillAtHome)
            {
                Debug.Log("Still at home");
                continue;
            }

            bool samePawnType = otherPlayersPawn.pawnType == this.pawnType;
            if (samePawnType)
            {
                Debug.Log("same pawn type");
                Debug.LogWarning("yaha dono ko minimize krna hai mittar same detact ho gae hai");
                continue;
            }
            if(otherPlayersPawn.currentSpot == null) continue;
            canKillOtherPlayerPawn = otherPlayersPawn.currentSpot.Equals(this.currentSpot);
            Debug.Log("Checking kill possibility against pawn: " + otherPlayersPawn.pawnType.ToString() + ". Can kill: " + canKillOtherPlayerPawn);
            Debug.Log(otherPlayersPawn.currentSpot + " " + this.currentSpot);
            if (canKillOtherPlayerPawn)
            {
                ServerRequest.instance.PawnKilled(otherPlayersPawn.pawnType, otherPlayersPawn.pawnNumber);
                StartCoroutine(SendBackToHome(otherPlayersPawn, otherPlayersPawn.home));
                break;
            }
        }


        return canKillOtherPlayerPawn;
    }

    private IEnumerator SendBackToHome(PawnMovementController pawn, GameObject home)
    {
        ResetPawn(pawn);
        float diffrentBetweenHouseAndPawn = (home.transform.position - pawn.gameObject.transform.position).sqrMagnitude;
        while (diffrentBetweenHouseAndPawn > .1f)
        {
            diffrentBetweenHouseAndPawn = (home.transform.position - pawn.gameObject.transform.position).sqrMagnitude;
            pawn.gameObject.transform.position = Vector3.MoveTowards(pawn.gameObject.transform.position,
                home.transform.position,
                speed);
            yield return new WaitForEndOfFrame();
        }
        pawn.gameObject.transform.position = home.transform.position;

    }


    void ResetPawn(PawnMovementController otherPlayer)
    {
        otherPlayer.isLeftTheHouse = false;
        otherPlayer.spotIndexOnLudoBoard = -1;
        otherPlayer.stepOutFromHome = 0;
        otherPlayer.currentSpot = null;
    }


    public bool isLeftTheHouse = false;
    protected int stepOutFromHome = 0;
    protected void CheckHome()
    {

        if (isLeftTheHouse)
        {
            return;
        }
        //very first time leaving the home
        if (!isLeftTheHouse && DiceController.instance.currentDiceValue == 6)
        {
            stepOutFromHome = 1;//value is going to use only once
            isLeftTheHouse = true;
            return;
        }
    }
    protected bool EnoughSpotsLeft(int moveToIndex)
    {
        bool enoughSpotsLeft = Lastspot >= moveToIndex + spotIndexOnLudoBoard;
        return enoughSpotsLeft;
    }
    protected void CheckAndGetOutOfSafeSpot()
    {
        if (!_onSafeSpot)
            return;

        this.transform.SetParent(PawnSpawner.instance.spotPanel.transform);
        RearrangePawns.instance.Rearrange();
        currentSpot.gameObject.GetComponent<PawnAjusterOnSafeSpots>().RemovePawn(this.gameObject);

        _onSafeSpot = false;
    }
}
