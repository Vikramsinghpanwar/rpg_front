using System;
using System.Collections;
using Features.Lobby.Integration;
using UnityEngine;
using UnityEngine.UI;
public class DiceController : DiceAI
{
    [SerializeField] private Sprite[] _diceValue;
    [SerializeField] private Sprite[] _diceAnimation;
    [SerializeField] private Button[] _rollDiceButton_Array;
    [SerializeField] private Color[] _diceColour;

    [SerializeField] public PawnType currentPawn;
    [SerializeField] public int currentDiceValue;

    public AudioSource diceAudioSource;
    public AudioClip diceRollAudioClip;
    public AudioClip pawnMoveAudioClip;
    public AudioClip pawnReachHomeAudioClip;
    public AudioClip pawnKillAudioClip;

    public static DiceController instance;
    public GameObject myChanceImage;
    private void Awake()
    {
        instance = this;
    }

    [SerializeField] public bool playerMovementIsFinished;
    [SerializeField] public bool playerCanMove;
    [SerializeField] public bool canRollDice;
    public bool movingAutomatically = false;




    private void Start()
    {
        playerMovementIsFinished = true;
    }

    public void UpdateValue()
    {
        playerMovementIsFinished = true;
    }

    public void OnClick()
    {
        if (canRollDice == false)
        {
            Debug.LogWarning("Player can't roll dice");
            return;
        }
        if (GameLiveData.instance.currentPlayersChance == BootstrapLobbyAdapter.GetUserId())
        {
            Debug.Log("Dice _ Rolled -------------------------------------------------");
            playerMovementIsFinished = false;
            canRollDice = false;
            ServerRequest.instance.RollDice_Request();
        }
    }
    #region OldCode
    public IEnumerator RollDice(string playerId, int diceValueFromServer = 0)
    {
        PlayDiceRollSound();
        Debug.Log("Server Dice Rolled by : " + playerId + " with value : " + diceValueFromServer);
        if (playerId == BootstrapLobbyAdapter.GetUserId())
        {
            myChanceImage.SetActive(false);
        }

        if (diceValueFromServer == 6)
            PawnTimer.instance.ResetTimer();


        // Determine dice value
        int diceVal = (diceValueFromServer != 0) ? diceValueFromServer - 1 : UnityEngine.Random.Range(0, _diceValue.Length);

        // Apply AI if it's our turn and local roll
        if (currentPawn == GameLocalData.pawnType && diceValueFromServer == 0)
        {
            int desirableDiceValue = base.RollDiceAl(diceVal + 1);
            currentDiceValue = desirableDiceValue == 0 ? diceVal + 1 : desirableDiceValue;
            if (desirableDiceValue != 0) diceVal = desirableDiceValue - 1;
        }
        else
        {
            currentDiceValue = diceVal + 1;
        }

        if (playerId == BootstrapLobbyAdapter.GetUserId())
        {
            currentPawn = GameLocalData.pawnType;
        }

        // // Send to server if local roll
        // if (diceValueFromServer == 0)
        // {
        //     ServerRequest.instance.RollDice(currentDiceValue, currentPawn);
        // }

        // Animation
        int index = CalculatePlayerIndex(playerId);
        Image _rollDiceImage = _rollDiceButton_Array[index].image;

        for (int i = 0; i < _diceAnimation.Length; i++)
        {
            _rollDiceImage.sprite = _diceAnimation[i];
            yield return new WaitForSeconds(0.03f);
        }
        _rollDiceImage.sprite = _diceValue[diceVal];

        // Unified turn handling
        //HandleTurnLogic(playerId, diceValueFromServer == 0);
    }

    private int CalculatePlayerIndex(string playerId)
    {
        int index = GameLiveData.instance.playersIdList.IndexOf(playerId) -
                    GameLiveData.instance.playersIdList.IndexOf(BootstrapLobbyAdapter.GetUserId());
        if (index < 0) index += 4;

        // Handle 2-player game cases
        if (GameLiveData.instance.roomLength == 2)
        {
            return playerId != BootstrapLobbyAdapter.GetUserId() ? 2 : 0;
        }
        return index;
    }


    #endregion
    public string GetNextPlayerId()
    {
        int currentIndex = GameLiveData.instance.playersIdList.IndexOf(GameLiveData.instance.currentPlayersChance);
        int nextIndex = (currentIndex + 1) % GameLiveData.instance.playersIdList.Count;
        return GameLiveData.instance.playersIdList[nextIndex];
    }


    public bool CanPlayerMovePawns(PawnType pawnType, int dicevalue)
    {
        foreach (var Player in PlayerInfo.instance.pawnInstances)
        {
            if (Player.pawnType != pawnType)
            {
                continue;
            }

            if (!Player.isLeftTheHouse)
            {
                if (dicevalue == 6)
                {
                    return true;
                }
            }
            else
            {
                //yaha logic lagega ki index kam hai
                if (Player.Lastspot - Player.spotIndexOnLudoBoard >= dicevalue)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public int MoveablePawnsCount(int currentDiceValue)
    {
        int count = 0;
        foreach (var Player in PlayerInfo.instance.pawnInstances)
        {
            if (Player.pawnType != currentPawn)
            {
                continue;
            }

            if (!Player.isLeftTheHouse)
            {
                if (currentDiceValue == 6)
                {
                    count++;
                }
            }
            else
            {
                //yaha logic lagega ki index kam hai
                if (Player.Lastspot - Player.spotIndexOnLudoBoard >= currentDiceValue)
                {
                    count++;
                }
            }
        }
        return count;

    }

    public void AutoMoveTheOnlyPawn(int diceValue)
    {
        instance.RemoveAllHighlights();

        foreach (var Player in PlayerInfo.instance.pawnInstances)
        {
            if (Player.pawnType != currentPawn)
            {
                continue;
            }

            if (!Player.isLeftTheHouse)
            {
                if (diceValue == 6)
                {
                    StartCoroutine(Player.MoveTo(diceValue));
                    playerCanMove = false;
                    return;
                }
            }
            else
            {
                //yaha logic lagega ki index kam hai
                if (Player.Lastspot - Player.spotIndexOnLudoBoard >= diceValue)
                {
                    Debug.Log("player can move his coins, going to auto move with dice value: " + diceValue);
                    StartCoroutine(Player.MoveTo(diceValue));
                    playerCanMove = false;
                    return;
                }
            }
        }
    }
    public void HighlightMovablePawns(PawnType pawnType, int dicevalue)
    {
        foreach (var Player in PlayerInfo.instance.pawnInstances)
        {
            if (Player.pawnType != pawnType)
            {
                continue;
            }
            if (Player.richedTheDestination) continue;

            if (!Player.isLeftTheHouse)
            {
                if (dicevalue == 6)
                {
                    Player.HighlightPawn(true);
                }
            }
            else
            {
                //yaha logic lagega ki index kam hai
                if (Player.Lastspot - Player.spotIndexOnLudoBoard >= dicevalue)
                {
                    Player.HighlightPawn(true);
                }
            }
        }
    }

    public void RemoveAllHighlights()
    {
        foreach (var Player in PlayerInfo.instance.pawnInstances)
        {
            Player.RemoveHighlight();
        }
    }

    //sounds


    public void PlayDiceRollSound()
    {
        if (diceAudioSource != null && diceRollAudioClip != null)
        {
            diceAudioSource.PlayOneShot(diceRollAudioClip);
        }
    }

    public void PlayPawnMoveSound()
    {
        if (diceAudioSource != null && pawnMoveAudioClip != null)
        {
            diceAudioSource.PlayOneShot(pawnMoveAudioClip);
        }
    }

    public void PlayPawnReachHomeSound()
    {
        if (diceAudioSource != null && pawnReachHomeAudioClip != null)
        {
            diceAudioSource.PlayOneShot(pawnReachHomeAudioClip);
        }
    }

    public void PlayPawnKillSound()
    {
        if (diceAudioSource != null && pawnKillAudioClip != null)
        {
            diceAudioSource.PlayOneShot(pawnKillAudioClip);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ServerRequest.instance.WinMove(0);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            ServerRequest.instance.WinMove(1);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ServerRequest.instance.WinMove(2);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            ServerRequest.instance.WinMove(3);
        }


    }
}
