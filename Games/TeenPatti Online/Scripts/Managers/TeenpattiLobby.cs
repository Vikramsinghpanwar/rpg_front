using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
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
            TeenpattiGameDataLobby.isRejoin = false;
            if (GameMode.mode == GameMode.Modes.publicGame)
            {
                matchmakingPanel.SetActive(true);
                bool joined = await ConnectionManager.Instance.JoinViaGateway(GameMode.tableEntryFee);
                if (!joined)
                {
                    Debug.LogError("Failed to join public game");
                }
            }
            else if (GameMode.mode == GameMode.Modes.privateGame)
            {
                Debug.Log("Private table type");
                string code = TeenpattiGameDataLobby.teenpattiTableID_for_JOIN;
                bool joined = await ConnectionManager.Instance.JoinViaGateway(1, code);
                if (!joined)
                {
                    Debug.LogError("Failed to join private room");
                    SceneManager.LoadScene("Lobby");
                }
            }
        }

        public void CancelMatchMaking()
        {
            ConnectionManager.Instance.LeaveTable();
        }

        public void JoinPrivateRoom(string tableID)
        {
            TeenpattiGameDataLobby.isRejoin = false;
            TeenpattiGameDataLobby.createTable = false;
            if (!string.IsNullOrEmpty(tableID))
            {
                ConnectionManager.Instance.JoinPrivateTable(tableID);
            }
        }

        public void JoinPrivateRoomByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                Debug.LogError("Private code is empty");
                return;
            }
            TeenpattiGameDataLobby.isRejoin = false;
            TeenpattiGameDataLobby.createTable = false;
            TeenpattiGameDataLobby.teenpattiTableID_for_JOIN = code.Trim();
            GameMode.mode = GameMode.Modes.privateGame;
            SceneManager.LoadScene("LobbyTeenpatti");
        }

        public void CreatePrivateRoom()
        {
            TeenpattiGameDataLobby.isRejoin = false;
            TeenpattiGameDataLobby.createTable = true;
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
            Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] [ReconnectOverlay] Show={v}");
            if (matchmakingStatusPanel != null)
            {
                matchmakingStatusPanel.SetActive(v);
                if (v && matchmakingStatusText != null)
                {
                    matchmakingStatusText.text = "Reconnecting...";
                }
            }
        }
    }
}