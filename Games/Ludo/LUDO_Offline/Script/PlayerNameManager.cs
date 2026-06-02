using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameManager : MonoBehaviour
{
    public GameUI GameUIRef;
    public GameObject nameInputScreen;
    public GameObject[] inputObjectList;
    public GameObject gameplayNamesParent;
    public GameObject gameBoard;
    public GameObject gamePlayScreen;

    int[] currentIndex;
    int currentPlayerCount;
    GameType currentGameType;

    string[] player_name_prefkeys = new string[] { "playeronename", "playertwoname", "playerthreename", "playerfourname" };
    string[] default_player_name_prefkeys = new string[] { "player 1", "player 2", "player 3", "player 4" };

    public void HideNameInputScreen()
    {
        nameInputScreen.SetActive(false);

        for (int i = 0; i < gameplayNamesParent.transform.childCount; i++)
        {
            gameplayNamesParent.transform.GetChild(i).gameObject.SetActive(false);
        }

        Transform nameParent = gameplayNamesParent.transform.GetChild(currentPlayerCount - 2);
        nameParent.gameObject.SetActive(true);

        for (int i = 0; i < currentPlayerCount; i++)
        {
            if (currentGameType == GameType.Local)
            {
                nameParent.GetChild(i).transform.GetChild(0).GetComponent<TextMeshPro>().text = PlayerPrefs.GetString(player_name_prefkeys[i], default_player_name_prefkeys[i]);
            }
            else
            {
                if (i == 0)
                {
                    nameParent.GetChild(i).transform.GetChild(0).GetComponent<TextMeshPro>().text = PlayerPrefs.GetString(player_name_prefkeys[i], default_player_name_prefkeys[i]);
                }
                else
                {
                    nameParent.GetChild(i).transform.GetChild(0).GetComponent<TextMeshPro>().text = "BOT" + i;
                }

            }
            nameParent.GetChild(i).transform.GetChild(0).GetComponent<TextMeshPro>().color = GameUIRef.gameColors[currentIndex[i]];
        }

        gameBoard.SetActive(true);
        gamePlayScreen.SetActive(true);
    }

    public void PrepareNameInputScreen(int[] _playerColorIndex, int playerCount, GameType gameType)
    {
        currentIndex = _playerColorIndex;
        currentPlayerCount = playerCount;
        currentGameType = gameType;

        for (int i = 0; i < inputObjectList.Length; i++)
        {
            inputObjectList[i].SetActive(false);
        }
        nameInputScreen.SetActive(true);

        for (int i = 0; i < playerCount; i++)
        {
            inputObjectList[i].SetActive(true);

            inputObjectList[i].transform.GetChild(0).GetComponent<Image>().color = GameUIRef.gameColors[_playerColorIndex[i]];
            inputObjectList[i].transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Image>().color = GameUIRef.gameColors[_playerColorIndex[i]];

            TMP_InputField temp = inputObjectList[i].transform.GetChild(2).GetComponent<TMP_InputField>();
            temp.textComponent.color = GameUIRef.gameColors[_playerColorIndex[i]];
            int valueToBePassed = i;
            //Debug.Log("VALUE TO BE PASSED =>" + valueToBePassed);
            temp.onEndEdit.AddListener(delegate { LockInput(temp, valueToBePassed); });
            if (currentGameType == GameType.Local)
            {
                temp.text = PlayerPrefs.GetString(player_name_prefkeys[i], default_player_name_prefkeys[i]);
                temp.interactable = true;
            }
            else
            {
                if (i == 0)
                {
                    temp.text = PlayerPrefs.GetString(player_name_prefkeys[i], default_player_name_prefkeys[i]);
                }
                else
                {
                    temp.text = "BOT" + i;
                    temp.interactable = false;
                }
            }
        }
    }
    // Checks if there is anything entered into the input field.
    void LockInput(TMP_InputField input, int _id)
    {
        Debug.Log("INPUT CLICKED ID =>" + _id);

        if (input.text.Length > 0)
        {
            Debug.Log("Text has been entered" + _id + "=>" + input.text);
            PlayerPrefs.SetString(player_name_prefkeys[_id], input.text);
        }
        else if (input.text.Length == 0)
        {
            Debug.Log("Main Input Empty");
            input.text = PlayerPrefs.GetString(player_name_prefkeys[_id], default_player_name_prefkeys[_id]);
        }
    }
}
