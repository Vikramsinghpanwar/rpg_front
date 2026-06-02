using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class GameUI : MonoBehaviour
{
    [Header("Mono-Reference")]
    public GameBoardSetup GameBoardSetupRef;
    public GameLogic GameLogicRef;

    [Header("Main Selection Screen")]
    public GameObject mainSelectionScreenPanel;
    public Image[] colorSelectionImages;

    [Header("Gameover Screen")]
    public GameObject gameOverScreenPanel;
    public GameObject[] winnerListArray;
    public RectTransform gameOverTitielImageRect;

    [Header("Gameplay Screen")]
    public GameObject gameplayBoard;
    public GameObject gameplayScreen;
    public Image confirmationUIBK;
    public RectTransform confirmationUI;
    public RectTransform confirmationUI_RefStart;
    public RectTransform confirmationUI_RefStop;

    [Header("Colors")]
    public Color[] gameColors;

    [Header("Win element")]
    public GameObject[] winElements;
    public Sprite[] winRankSprites;

    PlayerColor selectedUserColor;
    GameType selectedGameType;
    PlayerCount selectedPlayerCount;

    string[] player_name_prefkeys = new string[] { "playeronename", "playertwoname", "playerthreename", "playerfourname" };
    string[] default_player_name_prefkeys = new string[] { "player 1", "player 2", "player 3", "player 4" };

    public GameObject playerCountSelector;
    void Awake()
    {
        HideScreen(gameplayBoard);
        HideScreen(gameplayScreen);
        HideScreen(gameOverScreenPanel);

        InitializeUserSelection();
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        selectedGameType = OfflineLudoData._botMode ? GameType.WithBot : GameType.Local;
        if(OfflineLudoData._botMode) playerCountSelector.SetActive(false);
        else playerCountSelector.SetActive(true);
    }

    private void InitializeUserSelection()
    {
        //Initialize default selection modes
        selectedGameType = GameType.Local;
        selectedUserColor = PlayerColor.Red;
        selectedPlayerCount = PlayerCount.Two;
    }

    void DisableAllWinElements()
    {
        for (int i = 0; i < winElements.Length; i++)
        {
            winElements[i].SetActive(false);
        }
    }

    public void EnableWinElementOf(int winIndex, int winRank)
    {
        if (selectedPlayerCount == PlayerCount.Two && winIndex == 1)
        {
            winElements[2].SetActive(true);

            int calculatedIndex = GameBoardSetupRef.pieceColorSpritesIndex[2];

            winElements[2].GetComponent<GenericWinElement>().SetColorAndSprite(gameColors[calculatedIndex], winRankSprites[winRank]);
        }
        else
        {
            winElements[winIndex].SetActive(true);

            int calculatedIndex = GameBoardSetupRef.pieceColorSpritesIndex[winIndex];

            winElements[winIndex].GetComponent<GenericWinElement>().SetColorAndSprite(gameColors[calculatedIndex], winRankSprites[winRank]);
        }
    }

    #region ASSIGNABLE_EVENTS

    // public void OnValueChangedGameModeToggle(Toggle currentToggle)
    // {
    //     if (currentToggle.isOn)
    //     {
    //         selectedGameType = currentToggle.GetComponent<GenericGameModeToggle>().gameType;
    //     }
    // }

    public void OnValueChangedPlayerColorToggle(Toggle currentToggle)
    {
        if (currentToggle.isOn)
        {
            selectedUserColor = currentToggle.GetComponent<GenericPlayerColorToggle>().playerColor;
        }
    }

    public void OnValueChangedPlayerCountToggle(Toggle currentToggle)
    {
        if (currentToggle.isOn)
        {
            selectedPlayerCount = currentToggle.GetComponent<GenericPlayerCountToggle>().playerCount;
        }
    }

    public void OnClickPlay()
    {
        HideScreen(mainSelectionScreenPanel);
        HideScreen(gameOverScreenPanel);
        DisableAllWinElements();

        GameBoardSetupRef.SetupGameBoard(selectedUserColor, selectedGameType, selectedPlayerCount);
        //Show fullscreen ad here
        //GoogleAdMobController.Instance.ShowInterstitialAd();
    }

    public void OnClickHomeFromGameplay()
    {
        confirmationUIBK.gameObject.SetActive(true);
        LeanTween.move(confirmationUI, confirmationUI_RefStop.anchoredPosition, 0.5F).setEaseOutCirc();
    }

    public void OnClickHomeFromGameover()
    {
        ResetGameBoard();
        NavigateToMainScreenFromHome();
    }

    public void ConfirmationYes()
    {
        LeanTween.move(confirmationUI, confirmationUI_RefStart.anchoredPosition, 0.5F).setEaseOutCirc().setOnComplete(() =>
        {
            confirmationUIBK.gameObject.SetActive(false);
            ResetGameBoard();
            NavigateToMainScreenFromHome();
        });
    }

    public void ConfirmationNo()
    {
        LeanTween.move(confirmationUI, confirmationUI_RefStart.anchoredPosition, 0.5F).setEaseOutCirc();
        confirmationUIBK.gameObject.SetActive(false);
    }

    public void NavigateToMainScreenFromHome()
    {
        LeanTween.cancelAll(false);
        HideScreen(gameplayBoard);
        HideScreen(gameplayScreen);
        HideScreen(gameOverScreenPanel);

        ShowScreen(mainSelectionScreenPanel);
        //Show fullscreen ad here
        //GoogleAdMobController.Instance.ShowInterstitialAd();
    }

    public void ResetGameBoard()
    {
        StopAllCoroutines();
        GameBoardSetupRef.ClearGameBoard();
        GameLogicRef.ClearGameData();
        StopGameOverTitileImageTween();
    }

    public void OnClickExit()
    {
        SceneManager.LoadScene("Lobby");
    }

    #endregion

    public void ShowScreen(GameObject genericScreen)
    {
        genericScreen.SetActive(true);
    }

    public void HideScreen(GameObject genericScreen)
    {
        genericScreen.SetActive(false);
    }

    public void ShowGameOverScreen(List<int> winnerList)
    {
        HideScreen(gameplayBoard);
        HideScreen(gameplayScreen);

        string winnerString = "";

        for (int i = 0; i < winnerListArray.Length; i++)
        {
            winnerListArray[i].SetActive(false);
        }

        for (int i = 0; i < winnerList.Count; i++)
        {
            winnerString += winnerList[i].ToString();

            winnerListArray[i].SetActive(true);

            winnerListArray[i].transform.GetChild(0).GetComponent<Image>().color = gameColors[GameBoardSetupRef.pieceColorSpritesIndex[winnerList[i]]];
            winnerListArray[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = gameColors[GameBoardSetupRef.pieceColorSpritesIndex[winnerList[i]]];
            winnerListArray[i].transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Image>().color = gameColors[GameBoardSetupRef.pieceColorSpritesIndex[winnerList[i]]];


            if (selectedGameType == GameType.Local)
            {
                winnerListArray[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString(player_name_prefkeys[winnerList[i]], default_player_name_prefkeys[winnerList[i]]);
            }
            else
            {
                if (winnerList[i] == 0)
                {
                    winnerListArray[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString(player_name_prefkeys[winnerList[i]], default_player_name_prefkeys[winnerList[i]]);
                }
                else
                {
                    winnerListArray[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "BOT";
                }
            }
        }
        //winnerListText.text = winnerString;

        ShowScreen(gameOverScreenPanel);
        PlayGameOverTitleImageTween();
    }

    #region GAME_OVER_SCREEN_UI_TWEEN
    void PlayGameOverTitleImageTween()
    {
        LeanTween.rotateZ(gameOverTitielImageRect.gameObject, -720F, 3F).setEaseLinear().setLoopCount(-1);
    }

    void StopGameOverTitileImageTween()
    {
        if (LeanTween.isTweening(gameOverTitielImageRect.gameObject))
        {
            LeanTween.cancel(gameOverTitielImageRect.gameObject);
            LeanTween.rotateZ(gameOverTitielImageRect.gameObject, 0F, 0.5F).setEaseOutCubic();
        }
    }
    #endregion
}
