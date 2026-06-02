using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using TMPro;
using System.ComponentModel;
using Ludo;
using Features.Lobby.Integration;

namespace Ludo
{

    class GameManager : MonoBehaviour
    {
        public TextMeshProUGUI[] playerNameText_Array;
        public Image[] playerImage_Array;
        public Sprite[] playerImage_SpriteArray;
        public GameObject youWinPanel;
        public GameObject youLosePanel;
        public static GameManager instance;

        public void YouWin()
        {
            youWinPanel.SetActive(true);
        }
        public void YouLose()
        {
            youLosePanel.SetActive(true);
        }
        void Awake()
        {
            instance = this;
        }
        public GameObject[] playerObjects_Array;
        void Start()
        {
            if (GameLiveData.instance.playersIdList.Count == 2)
            {
                playerObjects_Array[1].SetActive(false);
                playerObjects_Array[3].SetActive(false);
            }
            else if (GameLiveData.instance.playersIdList.Count == 3)
            {
                playerObjects_Array[3].SetActive(false);
            }
            else if (GameLiveData.instance.playersIdList.Count == 4)
            {
                playerObjects_Array[0].SetActive(true);
                playerObjects_Array[1].SetActive(true);
                playerObjects_Array[2].SetActive(true);
                playerObjects_Array[3].SetActive(true);
            }
            PawnTimer.instance.StartTimer(GameLiveData.instance.GetPlayerPawnType(GameLiveData.instance.currentPlayersChance).pawnType);

            if (GameLiveData.instance.currentPlayersChance == BootstrapLobbyAdapter.GetUserId())
            {
                DiceController.instance.canRollDice = true;
                DiceController.instance.myChanceImage.SetActive(true);
            }
            playerNameText_Array[0].text = UserDetail.UserName;
            playerImage_Array[0].sprite = playerImage_SpriteArray[UserDetail.profileImageIndex];
            int myIndex = GameLiveData.instance.playersIdList.FindIndex(x => x == BootstrapLobbyAdapter.GetUserId());
            switch (GameLiveData.instance.playersIdList.Count)
            {
                case 2:
                    playerNameText_Array[2].text = GameLiveData.instance.playersNameList[myIndex == 0 ? 1 : 0];
                    playerImage_Array[2].sprite = playerImage_SpriteArray[GameLiveData.instance.GetPlayerPawnType(GameLiveData.instance.playersIdList[myIndex == 0 ? 1 : 0]).profileImageIndex];
                    break;
                case 3:
                    playerNameText_Array[1].text = GameLiveData.instance.playersNameList[1];
                    playerImage_Array[1].sprite = playerImage_SpriteArray[GameLiveData.instance.GetPlayerPawnType(GameLiveData.instance.playersIdList[1]).profileImageIndex];
                    playerNameText_Array[2].text = GameLiveData.instance.playersNameList[2];
                    playerImage_Array[2].sprite = playerImage_SpriteArray[GameLiveData.instance.GetPlayerPawnType(GameLiveData.instance.playersIdList[2]).profileImageIndex];
                    break;
                case 4:

                    playerNameText_Array[1].text = GameLiveData.instance.playersNameList[1];
                    playerImage_Array[1].sprite = playerImage_SpriteArray[GameLiveData.instance.GetPlayerPawnType(GameLiveData.instance.playersIdList[1]).profileImageIndex];
                    playerNameText_Array[2].text = GameLiveData.instance.playersNameList[2];
                    playerImage_Array[2].sprite = playerImage_SpriteArray[GameLiveData.instance.GetPlayerPawnType(GameLiveData.instance.playersIdList[2]).profileImageIndex];
                    playerNameText_Array[3].text = GameLiveData.instance.playersNameList[3];
                    playerImage_Array[3].sprite = playerImage_SpriteArray[GameLiveData.instance.GetPlayerPawnType(GameLiveData.instance.playersIdList[3]).profileImageIndex];
                    break;
                default:
                    break;
            }

        }
        public void ExitGame()
        {
            SocketManager.Instance.LeaveGame();
            SceneManager.LoadScene("Lobby");
        }
    }
}