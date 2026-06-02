using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using Teenpatti;
using System.Threading.Tasks;
using System;

namespace Teenpatti
{

    public class TeenpattiLobby : MonoBehaviour
    {
        public static TeenpattiLobby instance;

        [Header("UI Panels")]
        public GameObject matchmakingPanel;
        public GameObject roomPanel;
        public GameObject startGameBtn;


        [Header("Text Displays")]
        public TextMeshProUGUI tableIdTxt;
        public TextMeshProUGUI playersCountTxt;
        public GameObject matchFoundPanel;
        public GameObject matchmakingStatusPanel;
        public TextMeshProUGUI matchFoundText;
        public TextMeshProUGUI matchmakingStatusText;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            OnStart();
        }

        async Task OnStart()
        {
            if (GameMode.mode == GameMode.Modes.publicGame)
            {
                matchmakingPanel.SetActive(true);
                bool joined = await ConnectionManager.Instance.JoinViaGateway(GameMode.tableEntryFee);
                if (!joined)
                {
                    // Handle join failure
                    Debug.LogError("Failed to join public game");
                }
            }
            else if (GameMode.mode == GameMode.Modes.privateGame)
            {
                Debug.Log("Private table type");
                bool joined = await ConnectionManager.Instance.JoinViaGateway(
                    GameMode.tableEntryFee,
                    TeenpattiGameDataLobby.teenpattiTableID_for_JOIN
                );
                // On success, the joined handler in WebSocketMessageHandler will handle room state
            }
        }

        public void CancelMatchMaking()
        {
            ConnectionManager.Instance.LeaveTable();
        }

        public void JoinPrivateRoom(string tableID)
        {
            TeenpattiGameDataLobby.isRejoin = false;
            if (!string.IsNullOrEmpty(tableID))
            {
                ConnectionManager.Instance.JoinPrivateTable(tableID);
            }
        }

        public void CreatePrivateRoom()
        {
            TeenpattiGameDataLobby.isRejoin = false;
            Debug.Log("Creating private room");
            // Get boot amount from UI or settings
            int bootAmount = GameMode.tableEntryFee; // Or from UI input
            ConnectionManager.Instance.CreatePrivateRoom(bootAmount, $"{UserDetail.UserName}'s Room");
        }

        public void StartPrivateGame()
        {
            ConnectionManager.Instance.StartPrivateGame();
        }

        public void OnRoomCreated(string tableId)
        {
            roomPanel.SetActive(true);
            matchmakingPanel.SetActive(false);

            tableIdTxt.text = $"{tableId}";
            playersCountTxt.text = "1/6";
            startGameBtn.SetActive(true);
        }


        public void OnPlayerJoined(int playerCount)
        {
            playersCountTxt.text = $"{playerCount}/6";

            if (playerCount >= 2)
            {
                startGameBtn.SetActive(true);
            }
        }

        public void CopyToClipboard(TextMeshProUGUI t)
        {
            GUIUtility.systemCopyBuffer = t.text;
        }



        public void ShowRoomPanel(string tableId, int playerCount)
        {
            roomPanel.SetActive(true);
            matchmakingPanel.SetActive(false);

            tableIdTxt.text = $"{tableId}";
            playersCountTxt.text = $"{playerCount}/6";
            startGameBtn.SetActive(playerCount >= 2);
        }

        public void HideRoomPanel()
        {
            roomPanel.SetActive(false);
            if (GameMode.mode == GameMode.Modes.publicGame)
            {
                matchmakingPanel.SetActive(true);
            }

        }

        public void CopyPrivateCode()
        {
            ConnectionManager.Instance.CopyPrivateCodeToClipboard();
        }


        public void StartGame()
        {
            ConnectionManager.Instance.StartPrivateGame();
        }

        public void ShowMatchmakingStatus(string message)
        {
            if (matchmakingStatusPanel != null)
            {
                matchmakingStatusText.text = message;
                matchmakingStatusPanel.SetActive(true);
            }
        }

        public void HideMatchmakingStatus()
        {
            if (matchmakingStatusPanel != null)
                matchmakingStatusPanel.SetActive(false);
        }

        public void ShowMatchFoundStatus()
        {
            if (matchFoundPanel != null)
            {
                matchFoundText.text = "Match found! Joining table...";
                matchFoundPanel.SetActive(true);

                // Hide after 3 seconds
                StartCoroutine(HideMatchFoundAfterDelay(3f));
            }
        }

        IEnumerator HideMatchFoundAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (matchFoundPanel != null)
                matchFoundPanel.SetActive(false);
        }

        public void OnPlayerJoined(int playerCount, bool hasBots = false)
        {
            playersCountTxt.text = $"{playerCount}/6";

            if (hasBots)
            {
                playersCountTxt.text += " (with bots)";
            }

            if (playerCount >= 2)
            {
                startGameBtn.SetActive(true);
            }
        }

        internal void ShowReconnectOverlay(bool v)
        {
            Debug.Log($"ShowReconnectOverlay called with: {v}");
            throw new NotImplementedException();
        }
    }
}