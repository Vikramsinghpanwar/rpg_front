using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Lobby.Integration;

namespace Teenpatti
{

    public class ActionButtons : MonoBehaviour
    {

        [System.Serializable]
        public class WSMessage<T>
        {
            public string type;   // "join"
            public T data;
        }


        [System.Serializable]
        public class RechargeRequestData
        {
            public string userID;   // "join"
        }
        public static ActionButtons instance { get; private set; }

        [Header("Buttons")]
        public Button ChaalBtn;
        public Button Chaal2XBtn;
        public Button PackBtn;
        public Button ShowBtn;
        public Button SideBetBtn;
        public GameObject checkBtn;
        public Animator buttonsPanelAnimator;
        [Header("Text")]
        public TextMeshProUGUI chaalAmountTxt;
        public TextMeshProUGUI chaal2xAmountTxt;
        public GameObject addRechargePanel;
        public GameObject ImBack_Panel;

        private bool _isChaal2x = false;

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

        public void InsufficientFunds()
        {
            if (addRechargePanel.activeInHierarchy) return;
            addRechargePanel.SetActive(true);
        }

        public void UIAfterSeen()
        {
            ChaalBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Chaal";
            Chaal2XBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Chaal 2X";
        }

        public void ShowButtonsPanel()
        {
            ActivateActionButtons();
            if (GameManager.Instance.localPlayer.hasSeenCards) UIAfterSeen();
            buttonsPanelAnimator.SetBool("_show", true);
            UpdateChaalAmountTxt();
        }

        public void HideButtonsPanel()
        {
            DeactivateActionButtons();
            buttonsPanelAnimator.SetBool("_show", false);
        }

        void Start()
        {
            _isChaal2x = false;
            ShowBtn.gameObject.SetActive(false);
            ChaalBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Blind";
            Chaal2XBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Blind 2X";

        }

        public async void AddRecharge()
        {
            //addRechargePanel.SetActive(false);
            InGameRecharge.instance.OpenPanel();
            if (GameManager.Instance.isRecharging && GameManager.Instance.rechargingPlayerId == GameManager.Instance.localPlayer.myId) return;
            // GameManager.Instance.isRecharging = true;
            // GameManager.Instance.rechargingPlayerId = GameManager.Instance.localPlayer.myId;

            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.SendAction("request_recharge");
            }

        }

        public async void PlayerBack()
        {

            if (WebSocketServerRequest.Instance != null)
            {
                //ImBack_Panel.SetActive(false);
                await WebSocketServerRequest.Instance.SendAction("playerBack", 0);
            }
        }

        public async void OnChaal()
        {
            int amount = (int)GameLiveData.instance.lastBetAmount;
            string action = GameManager.Instance.localPlayer.hasSeenCards ? "chaal" : "blind";
            if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < amount)
            {
                InsufficientFunds();
                return;
            }

            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.SendAction(action, amount);
            }
        }

        public async void OnChaal2X()
        {
            int amount = 2 * (int)GameLiveData.instance.lastBetAmount;
            string action = GameManager.Instance.localPlayer.hasSeenCards ? "chaal2x" : "blind2x";
            if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < amount)
            {
                InsufficientFunds();
                return;
            }

            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.SendAction(action, amount);
            }
        }

        public async void OnPack()
        {
            if (addRechargePanel.activeInHierarchy)
            {
                addRechargePanel.SetActive(false);
            }
            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.SendAction("pack");
            }
        }

        public async void OnShowClicked()
        {
            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.SendAction("show");
            }
        }

        public async void OnBecomeSeen()
        {
            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.SendAction("become_seen");
            }
        }

        // Add side show methods
        public async void RequestSideShow()
        {
            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.SendAction("side_show");
            }
        }

        public async void RespondToSideShow(bool accept, string requesterID)
        {
            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.SendAction("side_show_response", 0, requesterID, accept);
            }
        }

        public void MarkLocalPlayerAsSeenCards()
        {
            if (checkBtn != null)
                checkBtn.SetActive(false);

            UIAfterSeen();

            GameManager.Instance.localPlayer.hasSeenCards = true;
            UpdateChaalAmountTxt();

            if (GameManager.Instance != null && GameManager.Instance.playersList.Count > 0)
            {
                GameManager.Instance.playersList[0].CheckCards();
            }

        }
        public async void OnCheckCards()
        {
            if (checkBtn != null)
                checkBtn.SetActive(false);

            UIAfterSeen();

            GameManager.Instance.localPlayer.hasSeenCards = true;
            UpdateChaalAmountTxt();

            if (GameManager.Instance != null && GameManager.Instance.playersList.Count > 0)
            {
                GameManager.Instance.playersList[0].CheckCards();
            }
            if (WebSocketServerRequest.Instance != null)
            {
                await WebSocketServerRequest.Instance.SendAction("seen");
            }
        }


        public void UpdateChaalAmountTxt()
        {
            float P = GameLiveData.instance.lastBetAmount;

            // Safety for round start
            if (P <= 0)
                P = GameLiveData.instance.bootAmount;

            bool youSeen = GameManager.Instance.localPlayer.hasSeenCards;
            bool prevSeen = GameLiveData.instance.lastPlayerSeen;

            float normal, twoX;

            // YOU ARE SEEN
            if (youSeen)
            {
                if (prevSeen)
                {
                    // seen after seen
                    normal = P;
                    twoX = P * 2;
                }
                else
                {
                    // seen after blind
                    normal = P * 2;
                    twoX = P * 4;
                }
            }
            // YOU ARE BLIND
            else
            {
                if (prevSeen)
                {
                    // blind after seen
                    normal = P / 2f;
                    twoX = P;
                }
                else
                {
                    // blind after blind
                    normal = P;
                    twoX = P * 2;
                }
            }

            chaalAmountTxt.text = normal.ToString("F2");
            chaal2xAmountTxt.text = twoX.ToString("F2");
        }


        void ActivateActionButtons()
        {
            ChaalBtn.interactable = true;
            Chaal2XBtn.interactable = true;

            PackBtn.interactable = true;

            ShowBtn.interactable = true;

        }

        void DeactivateActionButtons()
        {
            ChaalBtn.interactable = false;
            Chaal2XBtn.interactable = false;


            PackBtn.interactable = false;
            ShowBtn.interactable = false;
        }

        public void ResetGUI()
        {
            ChaalBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Blind";
            Chaal2XBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Blind 2X";

        }


    }
}