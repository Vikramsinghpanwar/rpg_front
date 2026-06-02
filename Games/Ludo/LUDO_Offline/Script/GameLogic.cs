using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class GameLogic : MonoBehaviour
{
    public GameUI GameUIRef;
    public AudioFX AudioFXRef;

    PlayerColor currentUserColor;
    GameType currentGameType;
    PlayerCount currentPlayerCount;

    GameObject[][] currentPlayerPieces;
    MovablePositionsOnBoard playerMovablePosition;
    PlayerPath[] playerPath;
    List<GameObject> globalPieces = new List<GameObject>();
    GameObject[] currentGameDice;
    List<int> winnerIndexList = new List<int>();
    List<int> continuesSixDiceList = new List<int>();
    List<int> highlightedPieceGlobalIndexList = new List<int>();
    List<int> globalSafePositionindexlist = new List<int> { 0, 8, 13, 21, 26, 34, 39, 47 };

    int CURRENT_PLAYER_COUNT;
    int TURN_INDEX = 0;
    int CURRENT_DICE_COUNT = 0;
    bool isApplicableForSequentialTurn = false;

    #region SCALING_&_SORTING

    Vector2[] localPosOnScaleDown = { new Vector2(-0.07F, -0.08F), new Vector2(-0.07F, 0.075F), new Vector2(0.08F, 0.075F), new Vector2(0.08F, -0.08F), new Vector2(-0.14F, 0), new Vector2(0.14F, 0), new Vector2(0, 0.15F), new Vector2(0, -0.15F) };
    Vector2 downScale = new Vector2(0.5F, 0.5F);
    Vector2 noramlScale = Vector2.one;
    Vector2 normalPos = Vector2.zero;

    string[] scaledPieceSortingLayerNames = { "Player1", "Player2", "Player3", "Player4", "Player5", "Player6", "Player7", "Player8", "Player9" };
    string movingPieceSortingLayerName = "Moving";
    string normalPieceSortingLayerName = "PlayerNormal";
    string highlightedSortingLayerName = "Highlighted";

    #endregion

    #region TIME_CONSTANT

    float WAITTIME_FOR_TURNCHANGE = 0.5F;
    float WAITTIME_FOR_ROLLDICE = 1F;
    float WAITTIME_FOR_PIECEMOVE = 0.05F;
    float WAITTIME_FOR_PIECEMOVE_BACKWARD = 0.05F;
    float WAITTIME_FOR_SCALEUPDOWN = 0.1F;

    public float SPEED_FOR_PIECEMOVE = 0.01F;

    public float SPEED_FOR_PIECEMOVE_BACKWARD = 0.05F;

    public float SPEED_FOR_ROLLDICE = 0.05F;

    public float SPEED_FOR_SCALEUPDOWN = 0.05F;

    #endregion

    public void ClearGameData()
    {
        StopAllCoroutines();
        globalPieces.Clear();
        continuesSixDiceList.Clear();
        winnerIndexList.Clear();

        highlightedPieceGlobalIndexList.Clear();

        DisableClickOfAllDice();
        TURN_INDEX = 0;
    }

    public void SetupGameData(GameObject[][] _playerpieces, PlayerColor _playerColor, GameType _gameType, PlayerCount _playerCount, MovablePositionsOnBoard _movablePositionsOnBoard, PlayerPath[] _playerPath, GameObject[] _currentGameDice)
    {
        currentPlayerPieces = _playerpieces;
        currentUserColor = _playerColor;
        currentGameType = _gameType;
        currentPlayerCount = _playerCount;
        playerMovablePosition = _movablePositionsOnBoard;
        playerPath = _playerPath;
        currentGameDice = _currentGameDice;

        CURRENT_PLAYER_COUNT = (int)_playerCount;

        FillGlobalPieceList();

        DisableClickOfAllDice();
        EnableClickOfDice(TURN_INDEX);
        currentGameDice[TURN_INDEX].GetComponent<GameDice>().PlayTapDiceTween();
    }

    void FillGlobalPieceList()
    {
        for (int i = 0; i < CURRENT_PLAYER_COUNT; i++)
        {
            for (int j = 0; j < currentPlayerPieces[i].Length; j++)
            {
                globalPieces.Add(currentPlayerPieces[i][j]);
            }
        }
    }

    public void GenerateDiceCount(int diceindex)
    {
        LeanTween.cancelAll(true);
        DisableClickOfAllDice();

        CURRENT_DICE_COUNT = Random.Range(1, 7);

        if (IsAllPiecesAtHome(diceindex) && CURRENT_DICE_COUNT != 6)
        {
            CURRENT_DICE_COUNT = 6;
        }

        //Stopping sequence of 6 dice face
        if (CURRENT_DICE_COUNT == 6)
        {
            continuesSixDiceList.Add(diceindex);
            if (continuesSixDiceList.Count > 2)
            {
                CURRENT_DICE_COUNT = Random.Range(1, 6);
            }
            else
            {
                isApplicableForSequentialTurn = true;
            }
        }
        else
        {
            isApplicableForSequentialTurn = false;
        }

        StartCoroutine(RollDice(diceindex, CURRENT_DICE_COUNT));
    }

    IEnumerator RollDice(int _diceindex, int _rolleddicecount)
    {
        currentGameDice[_diceindex].GetComponent<GameDice>().PlayRollDiceTween(_rolleddicecount);
        yield return new WaitForSeconds(WAITTIME_FOR_ROLLDICE);

        if (_diceindex == 0)
        {
            FindMovablePiece(_diceindex, _rolleddicecount);
        }
        else
        {
            if (currentGameType == GameType.Local)
            {
                FindMovablePiece(_diceindex, _rolleddicecount);
            }
            else if (currentGameType == GameType.WithBot)
            {
                FindAndMoveBotPiece(_diceindex, _rolleddicecount);
            }
        }
    }

    public void MovePiece(int playerId, int pieceId)
    {
        DisableClickOfAllPiece();
        DisableIndicationOfAllPiece();

        StartCoroutine(Move(playerId, pieceId, CURRENT_DICE_COUNT));
    }

    IEnumerator Move(int player_id, int piece_id, int rolledDiceFaceCount)
    {
        if (currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentIndex != -1)
        {
            int currentMoveIndexStart = currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentIndex + 1;
            int currentMoveIndexStop = currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentIndex + rolledDiceFaceCount;
            int tempGlobalIndex = currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentGlobalIndex;

            PieceArrangmentOfHighlightedPieces(tempGlobalIndex);

            //Make piece position and scake normal before trigger move
            if (currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().isScaleDown)
            {
                LeanTween.moveLocal(currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().transformToScale.gameObject, normalPos, SPEED_FOR_SCALEUPDOWN).setEaseLinear();
                LeanTween.scale(currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().transformToScale.gameObject, noramlScale, SPEED_FOR_SCALEUPDOWN).setEaseLinear();
                currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().SetSortingLayerTo(movingPieceSortingLayerName);
                currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().isScaleDown = false;

                yield return new WaitForSeconds(WAITTIME_FOR_SCALEUPDOWN);
            }
            else
            {
                currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().SetSortingLayerTo(movingPieceSortingLayerName);
            }

            for (int step = currentMoveIndexStart; step <= currentMoveIndexStop; step++)
            {
                int toIndex = playerPath[player_id].movablePositionIndex[step];
                Vector2 toPos = playerMovablePosition.movablePositions[toIndex].Position();

                LeanTween.moveLocal(currentPlayerPieces[player_id][piece_id], toPos, SPEED_FOR_PIECEMOVE).setEaseLinear();

                currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentIndex += 1;
                currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentGlobalIndex = playerPath[player_id].movablePositionIndex[step];

                if (step == currentMoveIndexStart)
                {
                    PieceArrangmentOnGrid(tempGlobalIndex);
                }
                yield return new WaitForSeconds(WAITTIME_FOR_PIECEMOVE);
            }
            AudioFXRef.PieceMove();

            //Check if piece reached to the destination or not
            if (currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentIndex == 56)
            {
                AudioFXRef.PieceAtFinalHome();
                isApplicableForSequentialTurn = true;
            }
            //Kill Logic
            DiscoverKillPossibilities(player_id, currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentGlobalIndex);
        }
        else
        {
            int initialValues = 0;

            int toIndex = playerPath[player_id].movablePositionIndex[initialValues];
            Vector2 toPos = playerMovablePosition.movablePositions[toIndex].Position();
            LeanTween.moveLocal(currentPlayerPieces[player_id][piece_id], toPos, SPEED_FOR_PIECEMOVE).setEaseOutCirc();

            currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentIndex = initialValues;
            currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentGlobalIndex = playerPath[player_id].movablePositionIndex[initialValues];

            AudioFXRef.PieceMove();

            yield return new WaitForSeconds(WAITTIME_FOR_PIECEMOVE);

            PieceArrangmentOnGrid(currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentGlobalIndex);

            FinishPlayerTurn(player_id);
        }

    }

    void FinishPlayerTurn(int player_id)
    {
        if (IsUserWin(player_id) && !winnerIndexList.Contains(player_id))
        {
            Debug.Log("WINNER ADDED TO THE LIST =>" + player_id);
            winnerIndexList.Add(player_id);
            GameUIRef.EnableWinElementOf(player_id, winnerIndexList.Count - 1);
        }

        if (winnerIndexList.Count != CURRENT_PLAYER_COUNT - 1)
        {
            StartCoroutine(ChangeTurn());
        }
        else
        {
            StartCoroutine(GameOver());
        }
    }

    #region DISCOVER_KILL_POSSIBILITES

    void DiscoverKillPossibilities(int player_id, int global_index)
    {
        if (globalSafePositionindexlist.Contains(global_index))
        {
            PieceArrangmentOnGrid(global_index);
            FinishPlayerTurn(player_id);
        }
        else
        {
            GameObject[] gatheredPiece = globalPieces.Where(currentPiece => currentPiece.GetComponentInChildren<GamePiece>().currentGlobalIndex == global_index && currentPiece.GetComponentInChildren<GamePiece>().playerID == player_id).ToArray();

            if (gatheredPiece.Length == 0)
            {

            }
            else if (gatheredPiece.Length == 1)
            {
                PieceArrangmentPatternONE(gatheredPiece);
            }
            else if (gatheredPiece.Length == 2)
            {
                PieceArrangmentPatternTWO(gatheredPiece);
            }
            else if (gatheredPiece.Length >= 3 && gatheredPiece.Length <= 9)
            {
                PieceArrangmentPatternTHREE(gatheredPiece);
            }

            GameObject[] pieceToBeKilled = globalPieces.Where(currentPiece => currentPiece.GetComponentInChildren<GamePiece>().currentGlobalIndex == global_index && currentPiece.GetComponentInChildren<GamePiece>().playerID != player_id).ToArray();

            if (pieceToBeKilled.Length > 0)
            {
                //move for kill
                for (int i = 0; i < pieceToBeKilled.Length; i++)
                {
                    if (pieceToBeKilled[i].GetComponentInChildren<GamePiece>().isScaleDown)
                    {
                        LeanTween.moveLocal(pieceToBeKilled[i].GetComponentInChildren<GamePiece>().transformToScale.gameObject, normalPos, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
                        LeanTween.scale(pieceToBeKilled[i].GetComponentInChildren<GamePiece>().transformToScale.gameObject, noramlScale, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
                        pieceToBeKilled[i].GetComponentInChildren<GamePiece>().SetSortingLayerTo(movingPieceSortingLayerName);
                        pieceToBeKilled[i].GetComponentInChildren<GamePiece>().isScaleDown = false;
                    }
                    else
                    {
                        pieceToBeKilled[i].GetComponentInChildren<GamePiece>().SetSortingLayerTo(movingPieceSortingLayerName);
                    }

                    if (i == 0)
                    {
                        StartCoroutine(BackwardMove(pieceToBeKilled[i].GetComponentInChildren<GamePiece>().playerID, pieceToBeKilled[i].GetComponentInChildren<GamePiece>().pieceID));
                    }
                    else
                    {
                        StartCoroutine(BackwardMoveWhenKillCountMoreThenOne(pieceToBeKilled[i].GetComponentInChildren<GamePiece>().playerID, pieceToBeKilled[i].GetComponentInChildren<GamePiece>().pieceID));
                    }
                }
            }
            else
            {
                FinishPlayerTurn(player_id);
            }
        }
    }

    IEnumerator BackwardMove(int player_id, int piece_id)
    {
        AudioFXRef.PieceCaptured();
        int currentMoveIndexStart = currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentIndex - 1;
        int currentMoveIndexStop = 0;

        for (int step = currentMoveIndexStart; step >= currentMoveIndexStop; step--)
        {
            int toIndex = playerPath[player_id].movablePositionIndex[step];
            Vector2 toPos = playerMovablePosition.movablePositions[toIndex].Position();

            LeanTween.moveLocal(currentPlayerPieces[player_id][piece_id], toPos, SPEED_FOR_PIECEMOVE_BACKWARD).setEaseLinear();

            currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentIndex -= 1;
            currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentGlobalIndex = playerPath[player_id].movablePositionIndex[step];

            yield return new WaitForSeconds(WAITTIME_FOR_PIECEMOVE_BACKWARD);
        }

        currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().ResetPiecePropertiesOnKill();
        yield return new WaitForSeconds(WAITTIME_FOR_PIECEMOVE_BACKWARD);
        isApplicableForSequentialTurn = true;

        FinishPlayerTurn(player_id);
    }

    IEnumerator BackwardMoveWhenKillCountMoreThenOne(int player_id, int piece_id)
    {
        AudioFXRef.PieceCaptured();
        int currentMoveIndexStart = currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentIndex - 1;
        int currentMoveIndexStop = 0;

        for (int step = currentMoveIndexStart; step >= currentMoveIndexStop; step--)
        {
            int toIndex = playerPath[player_id].movablePositionIndex[step];
            Vector2 toPos = playerMovablePosition.movablePositions[toIndex].Position();

            LeanTween.moveLocal(currentPlayerPieces[player_id][piece_id], toPos, SPEED_FOR_PIECEMOVE_BACKWARD).setEaseLinear();

            currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentIndex -= 1;
            currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().currentGlobalIndex = playerPath[player_id].movablePositionIndex[step];

            yield return new WaitForSeconds(WAITTIME_FOR_PIECEMOVE_BACKWARD);
        }

        currentPlayerPieces[player_id][piece_id].GetComponentInChildren<GamePiece>().ResetPiecePropertiesOnKill();
        yield return new WaitForSeconds(WAITTIME_FOR_PIECEMOVE_BACKWARD);
        isApplicableForSequentialTurn = true;
    }

    #endregion

    #region PIECE_ARRANGEMENT
    void PieceArrangmentOnGrid(int movableGlobalPositionIndex)
    {
        GameObject[] gatheredPiece = globalPieces.Where(currentPiece => currentPiece.GetComponentInChildren<GamePiece>().currentGlobalIndex == movableGlobalPositionIndex).ToArray();

        if (gatheredPiece.Length == 0)
        {

        }
        else if (gatheredPiece.Length == 1)
        {
            PieceArrangmentPatternONE(gatheredPiece);
        }
        else if (gatheredPiece.Length == 2)
        {
            PieceArrangmentPatternTWO(gatheredPiece);
        }
        else if (gatheredPiece.Length >= 3 && gatheredPiece.Length <= 9)
        {
            PieceArrangmentPatternTHREE(gatheredPiece);
        }
    }

    void PieceArrangmentPatternONE(GameObject[] pieceToBeArranged)
    {
        LeanTween.moveLocal(pieceToBeArranged[0].GetComponentInChildren<GamePiece>().transformToScale.gameObject, normalPos, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
        LeanTween.scale(pieceToBeArranged[0].GetComponentInChildren<GamePiece>().transformToScale.gameObject, noramlScale, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
        pieceToBeArranged[0].GetComponentInChildren<GamePiece>().SetSortingLayerTo(normalPieceSortingLayerName);
        pieceToBeArranged[0].GetComponentInChildren<GamePiece>().isScaleDown = false;
    }

    void PieceArrangmentPatternTWO(GameObject[] pieceToBeArranged)
    {
        LeanTween.moveLocal(pieceToBeArranged[0].GetComponentInChildren<GamePiece>().transformToScale.gameObject, localPosOnScaleDown[0], SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
        LeanTween.scale(pieceToBeArranged[0].GetComponentInChildren<GamePiece>().transformToScale.gameObject, downScale, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
        pieceToBeArranged[0].GetComponentInChildren<GamePiece>().SetSortingLayerTo(scaledPieceSortingLayerNames[0]);
        pieceToBeArranged[0].GetComponentInChildren<GamePiece>().isScaleDown = true;

        LeanTween.moveLocal(pieceToBeArranged[1].GetComponentInChildren<GamePiece>().transformToScale.gameObject, localPosOnScaleDown[2], SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
        LeanTween.scale(pieceToBeArranged[1].GetComponentInChildren<GamePiece>().transformToScale.gameObject, downScale, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
        pieceToBeArranged[1].GetComponentInChildren<GamePiece>().SetSortingLayerTo(scaledPieceSortingLayerNames[2]);
        pieceToBeArranged[1].GetComponentInChildren<GamePiece>().isScaleDown = true;
    }

    void PieceArrangmentPatternTHREE(GameObject[] pieceToBeArranged)
    {
        if (pieceToBeArranged.Length == 5 || pieceToBeArranged.Length == 7 || pieceToBeArranged.Length == 9)
        {
            for (int i = 0; i < pieceToBeArranged.Length - 1; i++)
            {
                LeanTween.moveLocal(pieceToBeArranged[i].GetComponentInChildren<GamePiece>().transformToScale.gameObject, localPosOnScaleDown[i], SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
                LeanTween.scale(pieceToBeArranged[i].GetComponentInChildren<GamePiece>().transformToScale.gameObject, downScale, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
                pieceToBeArranged[i].GetComponentInChildren<GamePiece>().SetSortingLayerTo(scaledPieceSortingLayerNames[i]);
                pieceToBeArranged[i].GetComponentInChildren<GamePiece>().isScaleDown = true;
            }
            LeanTween.moveLocal(pieceToBeArranged[pieceToBeArranged.Length - 1].GetComponentInChildren<GamePiece>().transformToScale.gameObject, normalPos, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
            LeanTween.scale(pieceToBeArranged[pieceToBeArranged.Length - 1].GetComponentInChildren<GamePiece>().transformToScale.gameObject, downScale, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
            pieceToBeArranged[pieceToBeArranged.Length - 1].GetComponentInChildren<GamePiece>().SetSortingLayerTo(scaledPieceSortingLayerNames[pieceToBeArranged.Length - 1]);
            pieceToBeArranged[pieceToBeArranged.Length - 1].GetComponentInChildren<GamePiece>().isScaleDown = true;
        }
        else
        {
            for (int i = 0; i < pieceToBeArranged.Length; i++)
            {
                LeanTween.moveLocal(pieceToBeArranged[i].GetComponentInChildren<GamePiece>().transformToScale.gameObject, localPosOnScaleDown[i], SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
                LeanTween.scale(pieceToBeArranged[i].GetComponentInChildren<GamePiece>().transformToScale.gameObject, downScale, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
                pieceToBeArranged[i].GetComponentInChildren<GamePiece>().SetSortingLayerTo(scaledPieceSortingLayerNames[i]);
                pieceToBeArranged[i].GetComponentInChildren<GamePiece>().isScaleDown = true;
            }
        }
    }

    //To be called on every piece move start
    void PieceArrangmentOfHighlightedPieces(int globalIndexToRemove)
    {
        if (highlightedPieceGlobalIndexList.Contains(globalIndexToRemove))
        {
            highlightedPieceGlobalIndexList.Remove(globalIndexToRemove);
        }

        for (int i = 0; i < highlightedPieceGlobalIndexList.Count; i++)
        {
            PieceArrangmentOnGrid(highlightedPieceGlobalIndexList[i]);
        }
    }

    #endregion

    #region PIECE_INDICATION

    public void FindMovablePiece(int _playerID, int _currentRolledDice)
    {
        //Debug.Log("DICE =>" + _currentRolledDice);

        GameObject[] movablePieces = globalPieces.Where(currentPiece => currentPiece.GetComponentInChildren<GamePiece>().playerID == _playerID && currentPiece.GetComponentInChildren<GamePiece>().IsMovable(_currentRolledDice)).ToArray();

        if (movablePieces.Length == 0)
        {
            //Pass the turn to next player
            StartCoroutine(ChangeTurn());
        }
        else if (movablePieces.Length == 1)
        {
            //Move piece automattically
            MovePiece(_playerID, movablePieces[0].GetComponentInChildren<GamePiece>().pieceID);
        }
        else if (IsAllPiecesAtHome(_playerID))
        {
            MovePiece(_playerID, movablePieces[0].GetComponentInChildren<GamePiece>().pieceID);
        }
        else if (movablePieces.Length >= 2)
        {
            for (int i = 0; i < movablePieces.Length; i++)
            {
                movablePieces[i].GetComponentInChildren<GamePiece>().EnableMovableIndication();
                movablePieces[i].GetComponentInChildren<GamePiece>().EnableCollider();

                if (movablePieces[i].GetComponentInChildren<GamePiece>().isScaleDown)
                {
                    LeanTween.moveLocal(movablePieces[i].GetComponentInChildren<GamePiece>().transformToScale.gameObject, normalPos, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
                    LeanTween.scale(movablePieces[i].GetComponentInChildren<GamePiece>().transformToScale.gameObject, noramlScale, SPEED_FOR_SCALEUPDOWN).setEaseOutCirc();
                    movablePieces[i].GetComponentInChildren<GamePiece>().SetSortingLayerTo(highlightedSortingLayerName);
                    movablePieces[i].GetComponentInChildren<GamePiece>().isScaleDown = false;
                    highlightedPieceGlobalIndexList.Add(movablePieces[i].GetComponentInChildren<GamePiece>().currentGlobalIndex);
                }
            }
        }
    }

    void DisableIndicationOfAllPiece()
    {
        for (int i = 0; i < globalPieces.Count; i++)
        {
            globalPieces[i].GetComponentInChildren<GamePiece>().DisableMovableIndication();
        }
    }
    #endregion

    #region BOT_MOVE

    public void FindAndMoveBotPiece(int _playerID, int _currentRolledDice)
    {
        GameObject[] movablePieces = globalPieces.Where(currentPiece => currentPiece.GetComponentInChildren<GamePiece>().playerID == _playerID && currentPiece.GetComponentInChildren<GamePiece>().IsMovable(_currentRolledDice)).ToArray();

        if (movablePieces.Length == 0)
        {
            //Pass the turn to next player
            StartCoroutine(ChangeTurn());
        }
        else if (movablePieces.Length == 1)
        {
            //Move piece automattically
            MovePiece(_playerID, movablePieces[0].GetComponentInChildren<GamePiece>().pieceID);
        }
        else if (movablePieces.Length >= 2)
        {
            for (int i = 0; i < movablePieces.Length; i++)
            {
                if (IsAbleToKillOtherPlayerPiece(movablePieces[i], _currentRolledDice))
                {
                    MovePiece(_playerID, movablePieces[i].GetComponentInChildren<GamePiece>().pieceID);
                    return;
                }
                else if (IsAbleToMoveOutFromHome(movablePieces[i], _currentRolledDice))
                {
                    MovePiece(_playerID, movablePieces[i].GetComponentInChildren<GamePiece>().pieceID);
                    return;
                }
                else if (IsAbleToMoveToSafeHouse(movablePieces[i], _currentRolledDice))
                {
                    MovePiece(_playerID, movablePieces[i].GetComponentInChildren<GamePiece>().pieceID);
                    return;
                }
            }

            GameObject highPosPiece = GetHighPosPiece(movablePieces);
            MovePiece(_playerID, highPosPiece.GetComponentInChildren<GamePiece>().pieceID);
        }
    }

    bool IsAbleToMoveOutFromHome(GameObject current_piece, int current_rolledDice)
    {
        if (current_piece.GetComponentInChildren<GamePiece>().currentIndex == -1 && current_rolledDice == 6)
        {
            return true;
        }
        return false;
    }

    bool IsAbleToKillOtherPlayerPiece(GameObject current_piece, int current_rolledDice)
    {
        int globalPosIndexToCompare = LocalIndexToGlobalIndex(current_piece.GetComponentInChildren<GamePiece>().playerID, current_piece.GetComponentInChildren<GamePiece>().currentIndex + current_rolledDice);

        bool isAnyPieceAvailableToBeKilled = globalPieces.Where(currentPiece => currentPiece.GetComponentInChildren<GamePiece>().currentGlobalIndex == globalPosIndexToCompare && !globalSafePositionindexlist.Contains(currentPiece.GetComponentInChildren<GamePiece>().currentGlobalIndex) && currentPiece.GetComponentInChildren<GamePiece>().playerID != current_piece.GetComponentInChildren<GamePiece>().playerID).Any();

        if (isAnyPieceAvailableToBeKilled)
        {
            return true;
        }
        return false;
    }

    bool IsAbleToMoveToSafeHouse(GameObject current_piece, int current_rolledDice)
    {
        int globalPosIndexToCheck = LocalIndexToGlobalIndex(current_piece.GetComponentInChildren<GamePiece>().playerID, current_piece.GetComponentInChildren<GamePiece>().currentIndex + current_rolledDice);

        if (globalSafePositionindexlist.Contains(globalPosIndexToCheck))
        {
            return true;
        }
        return false;
    }

    GameObject GetHighPosPiece(GameObject[] current_pieces)
    {
        GameObject highPosPieces = current_pieces.OrderByDescending(currentPiece => currentPiece.GetComponentInChildren<GamePiece>().currentIndex).First();
        return highPosPieces;
    }

    GameObject GetRandomPiece(GameObject[] current_pieces)
    {
        return current_pieces[Random.Range(0, current_pieces.Length)];
    }

    int LocalIndexToGlobalIndex(int playerid, int localIndex)
    {
        return playerPath[playerid].movablePositionIndex[localIndex];
    }
    #endregion

    void DisableClickOfAllPiece()
    {
        for (int i = 0; i < globalPieces.Count; i++)
        {
            globalPieces[i].GetComponentInChildren<GamePiece>().DisableCollider();
        }
    }

    void DisableClickOfAllDice()
    {
        for (int i = 0; i < currentGameDice.Length; i++)
        {
            currentGameDice[i].GetComponent<GameDice>().DisableCollider();
        }
    }

    void EnableClickOfDice(int diceid)
    {
        for (int i = 0; i < currentGameDice.Length; i++)
        {
            if (currentGameDice[i].GetComponent<GameDice>().diceID == diceid)
            {
                if (diceid == 0)
                {
                    currentGameDice[i].GetComponent<GameDice>().EnableCollider();
                    currentGameDice[i].GetComponent<GameDice>().ShowDice();
                }
                else
                {
                    if (currentGameType == GameType.Local)
                    {
                        currentGameDice[i].GetComponent<GameDice>().EnableCollider();
                        currentGameDice[i].GetComponent<GameDice>().ShowDice();
                    }
                    else if (currentGameType == GameType.WithBot)
                    {
                        currentGameDice[i].GetComponent<GameDice>().ShowDice();
                    }
                }
            }
            else
            {
                currentGameDice[i].GetComponent<GameDice>().HideDice();
            }
        }
    }

    IEnumerator ChangeTurn()
    {
        yield return new WaitForSeconds(WAITTIME_FOR_TURNCHANGE);

        if (isApplicableForSequentialTurn && !winnerIndexList.Contains(TURN_INDEX))
        {
            isApplicableForSequentialTurn = false;
        }
        else
        {
            isApplicableForSequentialTurn = false;

            continuesSixDiceList.Clear();

            if (currentPlayerCount == PlayerCount.Two)
            {
                if (TURN_INDEX == 0)
                {
                    TURN_INDEX = 1;
                }
                else
                {
                    TURN_INDEX = 0;
                }
            }
            else
            {
                TURN_INDEX = GetNextNonWinnerIndex(TURN_INDEX);
            }
        }

        EnableClickOfDice(TURN_INDEX);

        //Automatic dice roll for bot
        if (TURN_INDEX != 0 && currentGameType == GameType.WithBot)
        {
            currentGameDice[TURN_INDEX].GetComponent<GameDice>().OnMouseDown();
        }
        else
        {
            currentGameDice[TURN_INDEX].GetComponent<GameDice>().PlayTapDiceTween();
        }
    }

    int GetNextNonWinnerIndex(int turn_index)
    {
        int nextTurnIndex = GetNextTurnIndex(turn_index);

        if (!IsUserWin(nextTurnIndex))
        {
            return nextTurnIndex;
        }
        else
        {
            return GetNextNonWinnerIndex(nextTurnIndex);
        }
    }

    int GetNextTurnIndex(int currentTurnIndex)
    {
        if (currentTurnIndex != CURRENT_PLAYER_COUNT - 1)
        {
            currentTurnIndex += 1;
        }
        else
        {
            currentTurnIndex = 0;
        }
        return currentTurnIndex;
    }

    bool IsUserWin(int playerid)
    {
        GameObject[] playerPieces = globalPieces.Where(currentPiece => currentPiece.GetComponentInChildren<GamePiece>().playerID == playerid && currentPiece.GetComponentInChildren<GamePiece>().IsReachedOnDestination()).ToArray();

        if (playerPieces.Length == 4)
        {
            return true;
        }
        return false;
    }

    bool IsAllPiecesAtHome(int playerid)
    {
        GameObject[] playerPieces = globalPieces.Where(currentPiece => currentPiece.GetComponentInChildren<GamePiece>().playerID == playerid && currentPiece.GetComponentInChildren<GamePiece>().IsOnHome()).ToArray();

        if (playerPieces.Length == 4)
        {
            return true;
        }
        return false;
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1F);
        GameUIRef.ShowGameOverScreen(winnerIndexList);
    }
}
