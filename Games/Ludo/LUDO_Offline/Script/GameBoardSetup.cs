using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class CustomVector
{
    public float x;
    public float y;

    public CustomVector(Vector2 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
    }

    public Vector2 Position()
    {
        return new Vector2(this.x, this.y);
    }
}

[System.Serializable]
public class PlayerPositions
{
    public List<CustomVector> initialPositions;
    public List<CustomVector> pathPositions;
}

[System.Serializable]
public class MovablePositionsOnBoard
{
    public CustomVector[] movablePositions;
}

[System.Serializable]
public class HomePositionsOnBoard
{
    public CustomVector[] homePositions;
}

[System.Serializable]
public class PlayerPath
{
    public int[] movablePositionIndex;
}

public class GameBoardSetup : MonoBehaviour
{
    public PlayerNameManager PlayerNameManagerRef;
    public GameLogic GameLogicRef;
    public AudioFX AudioFXRef;
    public GameObject piecePrefab;
    public Transform boardTransform;
    public GameObject[] gameDice;

    [Header("Game Theme")]
    public Sprite[] pieceSprites;
    public Sprite boardSprite;

    PlayerColor currentUserColor;
    GameType currentGameType;
    PlayerCount currentPlayerCount;

    Vector2[][] currentPlayerHomePositions;
    GameObject[][] currentPlayerPieces;
    PlayerPath[] playerPath;
    GameObject[] currentGameDice;
    Sprite[] currentPieceColorSprites;

    [HideInInspector]
    public int[] pieceColorSpritesIndex = { 0, 1, 2, 3 };

    int CURRENT_PLAYER_COUNT;

    public void SetupGameBoard(PlayerColor _playerColor, GameType _gameType, PlayerCount _playerCount)
    {
        InitializeGameDice();

        currentUserColor = _playerColor;
        currentGameType = _gameType;
        currentPlayerCount = _playerCount;
        currentPieceColorSprites = pieceSprites;
        boardTransform.gameObject.GetComponent<SpriteRenderer>().sprite = boardSprite;

        PieceColorSetup();
    }

    void InitializeGameDice()
    {
        for (int i = 0; i < gameDice.Length; i++)
        {
            gameDice[i].GetComponent<GameDice>().InitializeScaleAndRotation();
        }
    }

    void PieceColorSetup()
    {
        //R-0
        //B-1
        //G-2
        //Y-3

        if (currentPlayerCount == PlayerCount.Two)
        {
            if (currentUserColor == PlayerColor.Red)
            {
                int[] temp = { 0, 2, 1, 3 };
                pieceColorSpritesIndex = temp;
                boardTransform.Rotate(new Vector3(0f, 0f, 0f));
            }
            else if (currentUserColor == PlayerColor.Blue)
            {
                int[] temp = { 1, 3, 2, 0 };
                pieceColorSpritesIndex = temp;
                boardTransform.Rotate(new Vector3(0f, 0f, 90f));

            }
            else if (currentUserColor == PlayerColor.Green)
            {
                int[] temp = { 2, 0, 3, 1 };
                pieceColorSpritesIndex = temp;
                boardTransform.Rotate(new Vector3(0f, 0f, 180f));

            }
            else if (currentUserColor == PlayerColor.Yellow)
            {
                int[] temp = { 3, 1, 0, 2 };
                pieceColorSpritesIndex = temp;
                boardTransform.Rotate(new Vector3(0f, 0f, 270f));
            }
        }
        else
        {
            if (currentUserColor == PlayerColor.Red)
            {
                int[] temp = { 0, 1, 2, 3 };
                pieceColorSpritesIndex = temp;
                boardTransform.Rotate(new Vector3(0f, 0f, 0f));
            }
            else if (currentUserColor == PlayerColor.Blue)
            {
                int[] temp = { 1, 2, 3, 0 };
                pieceColorSpritesIndex = temp;
                boardTransform.Rotate(new Vector3(0f, 0f, 90f));
            }
            else if (currentUserColor == PlayerColor.Green)
            {
                int[] temp = { 2, 3, 0, 1 };
                pieceColorSpritesIndex = temp;
                boardTransform.Rotate(new Vector3(0f, 0f, 180f));
            }
            else if (currentUserColor == PlayerColor.Yellow)
            {
                int[] temp = { 3, 0, 1, 2 };
                pieceColorSpritesIndex = temp;
                boardTransform.Rotate(new Vector3(0f, 0f, 270f));
            }
        }

        DisableAllDice();

        ApplyUserSelection();
    }

    private void ApplyUserSelection()
    {
        CURRENT_PLAYER_COUNT = (int)currentPlayerCount;

        HomePositionsOnBoard playerHomePosition;

        if (CURRENT_PLAYER_COUNT == 2)
        {
            currentGameDice = new GameObject[CURRENT_PLAYER_COUNT];

            currentPlayerHomePositions = new Vector2[CURRENT_PLAYER_COUNT][];

            playerPath = new PlayerPath[CURRENT_PLAYER_COUNT];

            for (int j = 0; j < CURRENT_PLAYER_COUNT; j++)
            {
                currentGameDice[j] = gameDice[j + j];

                playerHomePosition = JsonUtility.FromJson<HomePositionsOnBoard>(ReadJson("0" + (j + j + 1).ToString()));

                playerPath[j] = JsonUtility.FromJson<PlayerPath>(ReadJson((j + j + 1).ToString()));

                currentPlayerHomePositions[j] = new Vector2[playerHomePosition.homePositions.Length];

                for (int i = 0; i < playerHomePosition.homePositions.Length; i++)
                {
                    currentPlayerHomePositions[j][i] = playerHomePosition.homePositions[i].Position();
                }
            }
        }
        else
        {
            currentGameDice = new GameObject[CURRENT_PLAYER_COUNT];

            currentPlayerHomePositions = new Vector2[CURRENT_PLAYER_COUNT][];

            playerPath = new PlayerPath[CURRENT_PLAYER_COUNT];

            for (int j = 0; j < CURRENT_PLAYER_COUNT; j++)
            {
                currentGameDice[j] = gameDice[j];

                playerHomePosition = JsonUtility.FromJson<HomePositionsOnBoard>(ReadJson("0" + (j + 1).ToString()));

                playerPath[j] = JsonUtility.FromJson<PlayerPath>(ReadJson((j + 1).ToString()));

                currentPlayerHomePositions[j] = new Vector2[playerHomePosition.homePositions.Length];

                for (int i = 0; i < playerHomePosition.homePositions.Length; i++)
                {
                    currentPlayerHomePositions[j][i] = playerHomePosition.homePositions[i].Position();
                }
            }
        }

        PrepareGameBoard();
    }

    public string ReadJson(string filename)
    {
        string path = null;

        path = "PositionsData/" + filename;
        var jsonTextFile = Resources.Load<TextAsset>(path);
        return jsonTextFile.ToString();
    }

    private void PrepareGameBoard()
    {
        currentPlayerPieces = new GameObject[CURRENT_PLAYER_COUNT][];

        for (int i = 0; i < CURRENT_PLAYER_COUNT; i++)
        {
            currentGameDice[i].GetComponent<GameDice>().SetDiceProperties(i, GameLogicRef, AudioFXRef);

            currentPlayerPieces[i] = new GameObject[4];

            for (int j = 0; j < 4; j++)
            {
                currentPlayerPieces[i][j] = Instantiate(piecePrefab);

                currentPlayerPieces[i][j].GetComponentInChildren<GamePiece>().SetPieceProperties(currentPieceColorSprites[pieceColorSpritesIndex[i]], currentPlayerHomePositions[i][j], GameLogicRef, i, j);

                currentPlayerPieces[i][j].name = "Piece" + i.ToString() + j.ToString();

                currentPlayerPieces[i][j].transform.SetParent(this.transform, false);
            }
        }

        MovablePositionsOnBoard playerMovablePosition = JsonUtility.FromJson<MovablePositionsOnBoard>(ReadJson("MovableBoardPositions"));

        GameLogicRef.SetupGameData(currentPlayerPieces, currentUserColor, currentGameType, currentPlayerCount, playerMovablePosition, playerPath, currentGameDice);

        PlayerNameManagerRef.PrepareNameInputScreen(pieceColorSpritesIndex, CURRENT_PLAYER_COUNT, currentGameType);
        //pieceColorSpritesIndex[i]
    }

    public void ClearGameBoard()
    {
        StopAllCoroutines();
        for (int i = 0; i < currentPlayerPieces.Length; i++)
        {
            for (int j = 0; j < currentPlayerPieces[i].Length; j++)
            {
                Destroy(currentPlayerPieces[i][j]);
            }
        }

        DisableAllDice();
        boardTransform.localRotation = Quaternion.identity;
    }

    void DisableAllDice()
    {
        for (int i = 0; i < gameDice.Length; i++)
        {
            gameDice[i].SetActive(false);
        }
    }
}
