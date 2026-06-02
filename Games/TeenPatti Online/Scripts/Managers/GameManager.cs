using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Teenpatti;
using System;
using System.Linq;
using System.Threading.Tasks;
using Teenpatti;
using UnityEngine.Networking;
using Core.Config;
using Core.Bootstrap;
using Features.Lobby.Integration;

namespace Teenpatti
{

    public class GameManager : MonoBehaviour
    {

        public TextMeshProUGUI rechargeTimer_TXT;

        public TextMeshProUGUI roundNoTMP;
        public TextMeshProUGUI tableID_TMP;
        public GameObject StartTableBtn;
        public TextMeshProUGUI bootAmountTMP;
        public TextMeshProUGUI potLimitTMP;
        public TextMeshProUGUI chaalLimitTMP;
        public TextMeshProUGUI PrivateTableTMP;
        public TextMeshProUGUI commisionTMP;

        public GameObject showDownPanel;
        public Image showDown_player1_profile;
        public Image showDown_player2_profile;
        public TextMeshProUGUI showDown_player1_name;
        public TextMeshProUGUI showDown_player2_name;
        public static GameManager Instance { get; private set; }

        [Header("Player References")]
        public List<PlayerManager> playersList;
        public PlayerManager localPlayer;

        [Header("UI References")]
        public TextMeshProUGUI potAmountText;
        public TextMeshProUGUI roundText;
        public TextMeshProUGUI userWalletTxt;
        public GameObject actionButtonsPanel;
        public GameObject gameEndPanel;
        public TextMeshProUGUI winnerText;
        public GameObject sideShowRequestPanel;
        public TextMeshProUGUI sideShowRequestText;
        public AudioSource cardDistributionAudio;

        [Header("Game State")]
        public float lastBetAmount;
        public int currentTurnPlayerIndex = -1;
        public bool isGameActive;

        [Header("Card Assets")]
        public Sprite cardBackSprite;

        [Header("Timer")]
        public Coroutine timerCoroutine;
        private float turnTimeRemaining = 30f;
        private const float TURN_TIME_LIMIT = 30f;

        [Header("Sprites")]
        public List<Sprite> avatarList;
        public List<Sprite> deck;

        public CardsDeal cardsDeal;

        public GameObject playerRechargingPanel;
        public TextMeshProUGUI playerRechargingPanelText;
        public Sprite chaalSpr, chaal2xSpr, blindSpr, blind2xSpr, seenSpr, packSpr;
        public Sprite emptyProfileSpr;
        public Sprite declinedSpr;
        public Sprite spectatorProfileSpr;


        public AudioClip addMoneyAC;
        public AudioClip joinAC;
        public AudioClip clockTickAC;
        public AudioClip myTurnAC;
        public AudioClip flipCardAC;
        public AudioClip packAC;
        public AudioClip playerLoseAC;


        public GameObject gameStartCountDownPanel;
        public TextMeshProUGUI gameStartCountDown_TMP;

        public GameObject waitingPanel;
        public bool isRecharging = false;
        public string rechargingPlayerId = "";




        [Header("Spectator Assets")]
        public GameObject spectatorPanel; // Panel to show spectator view


        public Image[] showDownPlayer1Cards;
        public Image[] showDownPlayer2Cards;
        public TextMeshProUGUI player1CardType;
        public TextMeshProUGUI player2CardType;
        public GameObject sureToExitPanel;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Instance = this;
            }
            Instance = this;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Debug.Log("App has come to foreground - refreshing game state");
                if (isRecharging && !string.IsNullOrEmpty(rechargingPlayerId) && rechargingPlayerId == BootstrapLobbyAdapter.GetUserId())
                {
                    //FetchWallet();

                }
            }
        }


        void CheckIfRechargeSuccessful()
        {
            float wallet = GameLiveData.instance.isPrivateTable ? (BootstrapService.Instance.Wallet.deposit_balance / 100f + BootstrapService.Instance.Wallet.win_balance / 100f) : BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;
            if (wallet >= GameLiveData.instance.lastBetAmount + playersList[0].amountInvested)
            {
                isRecharging = false;
                rechargingPlayerId = "";
                playerRechargingPanel.SetActive(false);
                Logger.Instance.Log("Recharge successful, resuming game");
                ActionButtons.instance.addRechargePanel.SetActive(false);
                InGameRecharge.instance.ClosePanel();
                if (WebSocketServerRequest.Instance != null)
                {
                    int chips = GameLiveData.instance.isPrivateTable
                        ? Mathf.FloorToInt((float)(BootstrapService.Instance.Wallet.deposit_balance / 100f + BootstrapService.Instance.Wallet.win_balance / 100f))
                        : Mathf.FloorToInt((float)(BootstrapService.Instance.Wallet.deposit_balance / 100f + BootstrapService.Instance.Wallet.win_balance / 100f + BootstrapLobbyAdapter.GetBonusBalance() / 100f));

                    WebSocketServerRequest.Instance.SendAction("recharge_complete", chips);
                }
            }
        }


        public void OnLobbyBtnClicked()
        {
            if (GameLiveData.instance.isSpectator)
            {
                if (ConnectionManager.Instance != null)
                {
                    ConnectionManager.Instance.LeaveTable();
                }
                SceneManager.LoadScene(1);
                return;
            }
            else
            {
                sureToExitPanel.SetActive(true);
            }

        }

        public void ConfirmExitToLobby()
        {
            Loader.Instance.ShowLoading();
            _ = WebSocketServerRequest.Instance.LeaveTable();
            Invoke("Lobbby", 2f);
            // WebSocketServerRequest.Instance.SendAction("leave_table");
        }

        void Lobbby()
        {
            SceneManager.LoadScene(1);
        }

        public void OnLobby()
        {
            Loader.Instance.HideLoading();
            if (ConnectionManager.Instance)
            {
                Destroy(ConnectionManager.Instance.gameObject);
            }
            if (SceneManager.GetActiveScene().name == "Teenpatti")
            {
                CancelInvoke("Lobbby");
                Lobbby();

            }
        }

        void LoadAllAvatars()
        {
            System.Object[] loadedSprites = Resources.LoadAll("Avatar", typeof(Sprite));

            avatarList = new List<Sprite>();

            // Add each loaded sprite to the list
            foreach (System.Object sprite in loadedSprites)
            {
                avatarList.Add(sprite as Sprite);
            }

        }


        // Promote spectator to player
        public void PromoteSpectatorToPlayer(string userID)
        {
            foreach (var player in playersList)
            {
                if (player.myId == userID && player.isSpectator)
                {
                    player.PromoteToPlayer(player.nameTxt.text, 1000); // Default chips or get from server
                    break;
                }
            }
        }




        // Add this method to GameManager.cs
        public void UpdateChaalAmount()
        {
            lastBetAmount = GameLiveData.instance.lastBetAmount;

            if (ActionButtons.instance != null)
            {
                ActionButtons.instance.UpdateChaalAmountTxt();
            }
        }

        void DeactivateAllPlayers()
        {
            playersList[0].isLocalPlayer = true;
            localPlayer = playersList[0];
            playersList[0].CardObj.SetActive(false);
            for (int i = 1; i < playersList.Count; i++)
            {
                playersList[i].CardObj.SetActive(false);
            }
        }

        public async void StartPrivateTable()
        {
            await WebSocketServerRequest.Instance.StartPrivateTableGame();
        }

        public void InviteFriend()
        {
            string message = $"your friend {UserDetail.UserName} has invited you to join a private table on The Crown Empire. Click to join. {Const.landingPageURL + "?game=teenpatti&roomId=" + GameLiveData.instance.privateTableCode} \n\n or start game and go to private table and enter code '{GameLiveData.instance.privateTableCode}'. Have Fun!";
            ShareManager.instance.ShareText(message);
            Debug.Log("Invite message: " + message);
        }


        void Start()
        {
            //LoadAllAvatars();       
            tableID_TMP.text = GameLiveData.instance.tableId;
            if (GameMode.mode == GameMode.Modes.privateGame)
            {
                userWalletTxt.text = (BootstrapService.Instance.Wallet.deposit_balance / 100f + BootstrapService.Instance.Wallet.win_balance / 100f).ToString("F2");
                PrivateTableTMP.gameObject.SetActive(true);
                PrivateTableTMP.text = "Private Code : " + GameLiveData.instance.privateTableCode;
                // PrivateTableTMP.text = "Private Code : " + TeenpattiGameDataLobby.teenpattiTableID_for_JOIN;
                // if (TeenpattiGameDataLobby.createTable)
                //     {
                //         StartTableBtn.SetActive(true);
                //     }
            }
            else
            {
                userWalletTxt.text = (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
                PrivateTableTMP.gameObject.SetActive(false);
            }
            cardsDeal = FindObjectOfType<CardsDeal>();


            if (GameLiveData.instance.tableState == "waiting")
            {
                waitingPanel.SetActive(true);
            }

            if (GameLiveData.instance.rejoin && GameLiveData.instance.tableState != "waiting")
            {
                SyncGameState();
            }
            else if (GameLiveData.instance.isSpectator)
            {
                spectatorPanel.SetActive(true);
                SyncGameState();
            }
            else
            {
                // simple join data
                PopulatePlayersOnJoin(GameLiveData.instance.playerDetailsArray);
            }

            if (GameLiveData.instance.rechargingPlayer != null && GameLiveData.instance.rechargingPlayer != "" && GameLiveData.instance.rechargeTimeRemaining > 0 && !playerRechargingPanel.activeInHierarchy)
            {
                Debug.Log($"Player {GameLiveData.instance.rechargingPlayer} is recharging with {GameLiveData.instance.rechargeTimeRemaining} seconds left");
                Recharging(GameLiveData.instance.rechargingPlayer, GameLiveData.instance.rechargeTimeRemaining);
            }
        }

        public void ResetTableForNextRound()
        {
            if (ActionButtons.instance.ImBack_Panel.activeInHierarchy)
            {
                Lobby();
            }
            // foreach (var player in GameLiveData.instance.playerDetailsArray)
            // {
            //     if(player.userId == BootstrapLobbyAdapter.GetUserId() && !player.isActive)
            //     {
            //         Lobby();
            //     }
            // }


            ActionButtons.instance.ResetGUI();
            ActionButtons.instance.checkBtn.SetActive(true);

            potAmountText.text = "";
            foreach (var player in playersList)
            {
                if (!player.isVacant)
                {
                    player.ResetPlayer();
                }
                player.playerAnimator.ResetTrigger("win");
            }
        }


        public void Recharged()
        {
            isRecharging = false;
            rechargingPlayerId = "";
            playerRechargingPanel.SetActive(false);
            Logger.Instance.Log("Recharge successful, resuming game");
            ActionButtons.instance.addRechargePanel.SetActive(false);
        }
        public void Recharging(string UserId, float timeout)
        {
            Debug.Log($"Player {UserId} is recharging with timeout {timeout}");
            playerRechargingPanel.SetActive(true);
            string playerName = playersList.Find(p => p.myId == UserId)?.nameTxt.text ?? UserId;
            playerRechargingPanelText.text = $"Waiting for {playerName} to recharge...";
            foreach (PlayerManager player in playersList)
            {
                if (player.myId == UserId)
                {
                    isRecharging = true;
                    rechargingPlayerId = UserId;
                    player.StartRecharging(timeout);
                    if (playerRechargeCoroutine != null)
                    {
                        StopCoroutine(playerRechargeCoroutine);
                    }
                    playerRechargeCoroutine = StartCoroutine(RechargeTimer(timeout));
                }
            }
        }
        Coroutine playerRechargeCoroutine;

        IEnumerator RechargeTimer(float timer)
        {
            while (timer > 0)
            {
                TimeSpan time = TimeSpan.FromSeconds(timer);

                rechargeTimer_TXT.text = string.Format("{0:00}:{1:00}",
                    time.Minutes,
                    time.Seconds);

                yield return new WaitForSeconds(1f);
                timer--;
            }

            rechargeTimer_TXT.text = "00:00";
        }

        public void OnCardsDealComplete()
        {
            foreach (var player in playersList)
            {
                if (!player.isSpectator && !player.isVacant) player.CardObj.SetActive(true);
            }
        }

        public void UpdateRoundNumberTMP()
        {
            roundNoTMP.text = "Round No. : " + GameLiveData.instance.currentRound;
        }


        public void InitializeGame(PlayerDetails[] playerDetails, bool isSpectator = false)
        {
            UpdateRoundNumberTMP();
            bootAmountTMP.text = GameLiveData.instance.bootAmount + "";
            chaalLimitTMP.text = (GameLiveData.instance.bootAmount * 128) + "";
            potLimitTMP.text = (GameLiveData.instance.bootAmount * 2048) + "";
            commisionTMP.text = GameLiveData.instance.commision + "%";
            waitingPanel.SetActive(false);
            spectatorPanel.SetActive(false);
            DeactivateAllPlayers();
            if (!isSpectator)
            {
                UpdatePlayers(playerDetails);
            }
            lastBetAmount = GameLiveData.instance.lastBetAmount;

            if (ActionButtons.instance != null)
                ActionButtons.instance.UpdateChaalAmountTxt();


            StartCoroutine(GameStartCountdown());
        }


        IEnumerator GameStartCountdown()
        {
            gameStartCountDownPanel.SetActive(true);
            for (int i = 3; i > 0; i--)
            {
                gameStartCountDown_TMP.text = "Game starting in " + i;
                yield return new WaitForSeconds(1f);
            }
            gameStartCountDownPanel.SetActive(false);

        }

        public void SyncGameState()
        {
            PlayerDetails[] playerDetails = GameLiveData.instance.playerDetailsArray;
            DeactivateAllPlayers();
            if (GameLiveData.instance.isSpectator)
            {
                SyncPlayersForSpectator(playerDetails);
            }
            else
            {
                SyncPlayers(playerDetails);
            }

            lastBetAmount = GameLiveData.instance.lastBetAmount;
            if (ActionButtons.instance != null)
                ActionButtons.instance.UpdateChaalAmountTxt();

            UpdatePotAmount();

        }

        public void Showdown(string user1, string user2, CardObject[] player1Cards, CardObject[] player2Cards, string player1CType, string player2CType)
        {
            var player1 = playersList.Find(p => p.myId == user1);
            var player2 = playersList.Find(p => p.myId == user2);
            showDown_player1_name.text = player1.nameTxt.text;
            showDown_player2_name.text = player2.nameTxt.text;
            showDown_player1_profile.sprite = player1.profileImg.sprite;
            showDown_player2_profile.sprite = player2.profileImg.sprite;
            for (int i = 0; i < player1Cards.Length; i++)
            {
                showDownPlayer1Cards[i].sprite = GetSprite(player1Cards[i].code);
                showDownPlayer2Cards[i].sprite = GetSprite(player2Cards[i].code);
            }
            player1CardType.text = player1CType;
            player2CardType.text = player2CType;

            showDownPanel.SetActive(true);

        }

        public void ShowdownDe(string requesterID, float amount, string type)
        {
            if (type == "requested")
            {
                foreach (PlayerManager player in playersList)
                {
                    if (player.myId == requesterID)
                    {
                        player.HandleShow(amount);

                    }
                }

            }

        }


        void SyncPlayers(PlayerDetails[] playerArray)
        {
            int totalSeats = playersList.Count;
            string localUserId = BootstrapLobbyAdapter.GetUserId();

            int localSeat = -1;

            foreach (var data in playerArray)
            {
                if (data != null && data.userId == localUserId)
                {
                    localSeat = data.position;
                    break;
                }
            }

            if (localSeat == -1)
            {
                Debug.LogError("Local player not found in playerArray!");
                return;
            }
            ActionButtons.instance.checkBtn.SetActive(true);

            // Clear seats
            for (int i = 0; i < totalSeats; i++)
            {
                playersList[i].ResetPlayer();
                playersList[i].SetVacant(true, "");
                playersList[i].isLocalPlayer = false;
            }

            // Map players
            foreach (var data in playerArray)
            {
                if (data == null || !data.isActive)
                    continue;

                int uiIndex = (data.position - localSeat + totalSeats) % totalSeats;
                PlayerManager player = playersList[uiIndex];

                player.SetVacant(false);
                player.CardObj.SetActive(true);
                player.status = "active";
                player.myId = data.userId;
                player.amountInvested = data.betAmount;
                player.myAmountTxt.text = data.betAmount.ToString("F2");
                player.PopulateWithPlayer(
                    data.profileImageIndex,
                    data.username
                );

                player.Connected(data.username, data.chips);

                if (data.userId == localUserId)
                {
                    player.isLocalPlayer = true;
                    localPlayer = player;

                    if (data.cards != null && data.cards.Length > 0)
                    {
                        Debug.Log("" + data.cards.Length);
                        Sprite[] s = new Sprite[data.cards.Length];
                        for (int i = 0; i < data.cards.Length; i++)
                            s[i] = GetSprite(data.cards[i]);

                        playersList[0].cardsSpriteList = new Sprite[data.cards.Length];
                        playersList[0].myCardsType = data.cardsType;
                        for (int j = 0; j < data.cards.Length; j++)
                        {
                            playersList[0].cardsSpriteList[j] = GetSprite(data.cards[j]);
                        }
                        // player.ShowCards(s);
                        if (data.hasSeenCards)
                            ActionButtons.instance.MarkLocalPlayerAsSeenCards();
                    }
                }

                if (data.position == GameLiveData.instance.dealerPosition)
                    player.MarkAsDealer();

                if (data.hasFolded)
                    player.Pack();

                if (data.hasSeenCards)
                    player.SetAsSeen();
            }

            // Sync turn correctly
            SyncTurnSeatBased(localSeat);
        }

        void SyncTurnSeatBased(int currentPlayerTurn)
        {
            int totalSeats = playersList.Count;
            int currentTurnSeat = GameLiveData.instance.currentTurn;

            int uiIndex = (currentTurnSeat - currentPlayerTurn + totalSeats) % totalSeats;

            PlayerManager turnPlayer = playersList[uiIndex];

            if (GameLiveData.instance.rejoin)
            {
                turnPlayer.TimerStart(GameLiveData.instance.timeRemainingForTurn);
            }
            else
            {
                turnPlayer.TimerStart();
            }


            foreach (var player in GameLiveData.instance.playerDetailsArray)
            {
                if (player.id == BootstrapLobbyAdapter.GetUserId() && turnPlayer.isLocalPlayer)//if (player.position == currentPlayerTurn && player.id == BootstrapLobbyAdapter.GetUserId())
                {
                    if (ActionButtons.instance != null)
                    {
                        float wallet = GameLiveData.instance.isPrivateTable ? (BootstrapService.Instance.Wallet.deposit_balance / 100f + BootstrapService.Instance.Wallet.win_balance / 100f) : BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;
                        if (wallet < GameLiveData.instance.lastBetAmount)
                        {
                            ActionButtons.instance.InsufficientFunds();
                        }
                        ActionButtons.instance.ShowButtonsPanel();
                        ActionButtons.instance.ShowBtn.gameObject.SetActive(player.canShow);
                        ActionButtons.instance.SideBetBtn.interactable = player.canSideShow;
                    }
                }
            }
        }


        void PopulatePlayersOnJoin(PlayerDetails[] playerArray)
        {
            int totalSeats = playersList.Count;
            string localUserId = BootstrapLobbyAdapter.GetUserId();

            int localSeat = -1;

            foreach (var data in playerArray)
            {
                if (data != null && data.userId == localUserId)
                {
                    localSeat = data.position;
                    break;
                }
            }
            ActionButtons.instance.checkBtn.SetActive(true);


            // Clear seats
            for (int i = 0; i < totalSeats; i++)
            {
                playersList[i].ResetPlayer();
                playersList[i].SetVacant(true, "");
                playersList[i].isLocalPlayer = false;
            }

            // Map players
            foreach (var data in playerArray)
            {
                if (data == null || data.userId == "")
                    continue;

                Debug.Log("pLAYER : " + data.userId);
                Debug.Log("pLAYER : " + data.username);
                int uiIndex = (data.position - localSeat + totalSeats) % totalSeats;
                PlayerManager player = playersList[uiIndex];

                player.SetVacant(false);
                player.status = "active";
                player.myId = data.userId;

                player.PopulateWithPlayer(
                    data.profileImageIndex,
                    data.username
                );

                player.Connected(data.username, data.chips);

                if (data.userId == localUserId)
                {
                    player.isLocalPlayer = true;
                    localPlayer = player;
                }
            }

        }

        public void UnregisterPlayer(string userId)
        {
            foreach (var player in playersList)
            {
                if (player.myId == userId)
                {
                    player.ResetPlayer();
                    player.SetVacant(true, "Empty");
                    break;
                }
            }
        }

        void SyncPlayersForSpectator(PlayerDetails[] playerDetailArray)
        {
            int totalSeats = playersList.Count;
            int spectatorSeat = GameLiveData.instance.spectatorSeatPosition;
            ActionButtons.instance.checkBtn.SetActive(true);
            // Clear all seats first
            for (int i = 0; i < totalSeats; i++)
            {
                playersList[i].ResetPlayer();
                playersList[i].SetVacant(true, "");
            }

            foreach (var data in playerDetailArray)
            {
                if (data == null || !data.isActive)
                    continue;

                // Convert server seat → UI seat
                int uiIndex = (data.position - spectatorSeat + totalSeats) % totalSeats;

                PlayerManager player = playersList[uiIndex];

                player.SetVacant(false);
                player.status = "active";
                player.CardObj.SetActive(true);

                player.Connected(data.username, data.chips);
                player.profileImg.sprite = GetAvatarByIndex(data.profileImageIndex);
                player.myId = data.id;
                player.amountInvested = data.betAmount;
                player.myAmountTxt.text = data.betAmount.ToString("F2");

                if (data.position == GameLiveData.instance.dealerPosition)
                {
                    player.MarkAsDealer();
                }

                if (data.hasFolded)
                {
                    player.Pack();
                }
            }
        }


        public void DealCards()
        {
            if (waitingPanel.activeInHierarchy)
            {
                waitingPanel.SetActive(false);
            }
            // cardShuffleAnimator.SetBool("_shuffle", true);
            cardDistributionAudio.Play();
            if (cardsDeal == null)
            {
                Debug.Log("radhey");
            }
            StartCoroutine(cardsDeal.DealCards());
            var playerDetailArray = GameLiveData.instance.playerDetailsArray;
            for (int i = 0; i < playerDetailArray.Length; i++)
            {
                Debug.Log("i: " + i);
                if (playerDetailArray[i] == null) continue;
                if (playerDetailArray[i].cards != null && playerDetailArray[i].cards.Length > 0)
                {
                    playersList[0].cardsSpriteList = new Sprite[3];

                    Debug.Log("cards length: " + playersList[0].cardsSpriteList.Length);

                    playersList[0].myCardsType = GameLiveData.instance.playerDetailsArray[i].cardsType;
                    for (int j = 0; j < playerDetailArray[i].cards.Length; j++)
                    {
                        playersList[0].cardsSpriteList[j] = GetSprite(playerDetailArray[i].cards[j]);
                    }
                }
            }
        }

        public PlayerManager GetPlayerByID(string id)
        {
            foreach (var player in playersList)
            {
                if (player.myId == id)
                {
                    return player;
                }
            }
            return null;
        }

        public void UpdatePlayers(PlayerDetails[] playerDetailArray)
        {
            Debug.Log("Updating players");
            string localPlayerID = BootstrapLobbyAdapter.GetUserId();
            ActionButtons.instance.checkBtn.SetActive(true);

            int pivotIndex = 0;
            for (int i = 0; i < playerDetailArray.Length; i++)
            {
                if (playerDetailArray[i] == null || !playerDetailArray[i].isActive || playerDetailArray[i].userId == "")
                {
                    // Reset player details
                    if (playersList[i] != null)
                    {
                        playersList[i].ResetPlayer();
                        playersList[i].SetVacant(true, "Empty");
                        playersList[i].isLocalPlayer = false;
                        playersList[i].myId = "";
                    }
                    continue;
                }
                if (playerDetailArray[i].userId == localPlayerID)
                {
                    pivotIndex = i;
                    break;
                }
            }
            Debug.Log("" + pivotIndex);
            playerDetailArray = playerDetailArray.Skip(pivotIndex).Concat(playerDetailArray.Take(pivotIndex)).ToArray();
            GameLiveData.instance.localPlayerPivotIndex = pivotIndex;

            // Clear all positions first
            for (int i = 0; i < playersList.Count; i++)
            {
                playersList[i].ResetPlayer();
                playersList[i].SetVacant(true, "");
            }



            for (int i = 0; i < playerDetailArray.Length; i++)
            {
                PlayerManager player = playersList[i];
                if (playerDetailArray[i] == null || !playerDetailArray[i].isActive || playerDetailArray[i].userId == "")
                {
                    continue;
                }
                Debug.Log("filling player with: " + playerDetailArray[i].username);


                if (playerDetailArray[i].position == GameLiveData.instance.dealerPosition)
                {
                    player.MarkAsDealer();
                }
                player.profileImg.sprite = GetAvatarByIndex(playerDetailArray[i].profileImageIndex);
                player.CardObj.SetActive(false);
                player.status = "active";
                player.SetVacant(false);
                playersList[i].Connected(playerDetailArray[i].username, playerDetailArray[i].amount);
                playersList[i].myId = playerDetailArray[i].id;

                if (playerDetailArray[i].userId == localPlayerID)
                {
                    player.isLocalPlayer = true;
                    localPlayer = player;
                }


            }

        }




        public void UpdatePlayersForSpectator(PlayerDetails[] playerDetailArray)
        {
            Debug.Log("Updating players");
            string playerId = BootstrapLobbyAdapter.GetUserId();

            int pivotIndex = 0;
            for (int i = 0; i < playerDetailArray.Length; i++)
            {
                if (playerDetailArray[i].userId == playerId)
                {
                    pivotIndex = i;
                    break;
                }
            }
            playerDetailArray = playerDetailArray.Skip(pivotIndex).Concat(playerDetailArray.Take(pivotIndex)).ToArray();


            // Clear all positions first
            for (int i = 0; i < playersList.Count; i++)
            {
                playersList[i].ResetPlayer();
                playersList[i].SetVacant(true, "Empty");
            }



            int k = 0;
            for (int i = 0; i < playerDetailArray.Length; i++)
            {
                PlayerManager player = playersList[i];
                if (!playerDetailArray[i].isActive) continue;

                if (playerDetailArray[i].position == GameLiveData.instance.dealerPosition)
                {
                    player.MarkAsDealer();
                }
                player.profileImg.sprite = GetAvatarByIndex(playerDetailArray[i].profileImageIndex);
                player.CardObj.SetActive(false);
                player.status = "active";
                player.SetVacant(false);
                if (playerDetailArray[i].userId == playerId)
                {
                    playersList[0].myId = playerDetailArray[i].id;
                    playersList[0].Connected(playerDetailArray[i].username, 0);
                    player.isLocalPlayer = true;
                    localPlayer = player;
                    continue;
                }
                else
                {
                    playersList[k + 1].Connected(playerDetailArray[i].username, playerDetailArray[i].amount);
                    playersList[k + 1].myId = playerDetailArray[i].id;
                    k++;
                }

            }

        }



        public void PreBets()
        {
            foreach (PlayerManager player in playersList)
            {
                if (!player.isVacant && !player.isSpectator)
                {
                    player.FirstBet();
                }
            }
            UpdatePotAmount();
        }

        public Sprite GetAvatarByIndex(int index)
        {
            return avatarList[index];
        }

        public Sprite GetSprite(string name)
        {
            int index = deck.FindIndex(p => p.name == name);
            if (index < 0) return null;
            else return deck[index];
        }


        public void PlayerAction(string playerId, string action, int amount)
        {
            if (playerRechargingPanel.activeInHierarchy && (action == "chaal" || action == "chaal2x" || action == "blind" || action == "blind2x" || action == "show" || action == "side_show" || action == "pack" || action == "fold")) playerRechargingPanel.SetActive(false);
            if (playersList == null) return;
            if (ActionButtons.instance.addRechargePanel.activeInHierarchy) ActionButtons.instance.addRechargePanel.SetActive(false);
            // UpdateRoundNumberTMP();

            // Find the player in our arranged list
            foreach (var player in playersList)
            {

                if (player.myId == playerId && player.gameObject.activeSelf)
                {
                    // Debug.Log("action taken by : " + player.myId + "" + action);
                    player.ActionTaken(action, amount);
                    break;
                }

            }

        }

        public void UpdatePotAmount(bool isDouble = false)
        {
            Debug.Log("Updating pot amount: " + GameLiveData.instance.currentPot);
            potAmountText.text = GameLiveData.instance.currentPot.ToString("F2");
        }

        public void UpdateWalletTxt()
        {
            if (GameMode.mode == GameMode.Modes.privateGame)
            {
                userWalletTxt.text = (BootstrapService.Instance.Wallet.win_balance / 100f + BootstrapService.Instance.Wallet.deposit_balance / 100f).ToString("F2");
                return;
            }
            userWalletTxt.text = (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
        }

        public void ShowPackedPlayers(PackedPlayerData[] packedPlayers)
        {
            if (packedPlayers == null || packedPlayers.Length == 0)
                return;

            foreach (var packed in packedPlayers)
            {
                PlayerManager player = GetPlayerByID(packed.userID);

                if (player == null)
                    continue;

                // Convert card codes to sprites
                Sprite[] sprites = new Sprite[packed.cards.Length];

                for (int i = 0; i < packed.cards.Length; i++)
                {
                    sprites[i] = GetSprite(packed.cards[i]);
                }

                // Reveal cards
                player.PopulateCards(packed.cards);
                player.ShowCards(sprites);

                // Optional: show hand type
                player.ShowHandRank(packed.handType);

                // // Play lose animation (if not winner)
                // player.Lose();

                Debug.Log($"Packed Player Revealed: {packed.userID} - {packed.handType}");
            }
        }

        public void ShowWinner(string winnerId, string[] cards)
        {
            Debug.Log("cards: " + cards[0]);
            Debug.Log("Showing winner: " + winnerId);
            // Stop all timers
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
            }

            // Stop all player timers
            foreach (var player in playersList)
            {
                player.StopAllCoroutines();
                player.timerImg.fillAmount = 0;
                player.timerImg.gameObject.SetActive(false);
            }

            // Deactivate action buttons
            if (ActionButtons.instance != null)
            {
                ActionButtons.instance.HideButtonsPanel();
            }

            // Find and highlight winner
            for (int i = 0; i < playersList.Count; i++)
            {
                if (playersList[i].myId == winnerId)
                {
                    playersList[i].PopulateCards(cards);
                    playersList[i].Win();

                    // Show winner's cards
                    //playersList[i].CheckCards();

                    // Show winner text
                    winnerText.text = $"{playersList[i].nameTxt.text} WINS!";
                    break;
                }
            }
            Invoke("ResetTableForNextRound", 4);

        }



        // Update the UpdateTurn method
        public void UpdateTurn(string userID, bool _canShow, bool _canSideShow)
        {
            if (GameLiveData.instance.currentRound >= 5)
            {
                if (!localPlayer.hasSeenCards)
                    ActionButtons.instance.OnCheckCards();
            }
            PlayerManager currentPlayer = null;
            foreach (var player in playersList)
            {
                if (player.myId == userID)
                {
                    currentPlayer = player;
                    break;
                }
            }

            if (GameLiveData.instance.currentTurn >= 0 &&
                GameLiveData.instance.currentTurn < playersList.Count)
            {
                // Stop previous player's timer
                foreach (var player in playersList)
                {
                    player.SetTurn(false);
                    player.StopAllCoroutines();
                }

                // Start current player's timer
                currentPlayer.SetTurn(true);
                currentPlayer.TimerStart();
                // If it's local player's turn, show buttons
                if (currentPlayer.isLocalPlayer)
                {
                    if (ActionButtons.instance != null)
                    {
                        float wallet = GameLiveData.instance.isPrivateTable ? (BootstrapService.Instance.Wallet.deposit_balance / 100f + BootstrapService.Instance.Wallet.win_balance / 100f) : BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;
                        if (wallet < GameLiveData.instance.lastBetAmount)
                        {

                            ActionButtons.instance.InsufficientFunds();
                        }
                        ActionButtons.instance.ShowBtn.gameObject.SetActive(_canShow);
                        ActionButtons.instance.SideBetBtn.interactable = _canSideShow;
                        ActionButtons.instance.UpdateChaalAmountTxt();
                        ActionButtons.instance.ShowButtonsPanel();

                    }

                }
                else
                {
                    if (ActionButtons.instance != null)
                    {
                        ActionButtons.instance.HideButtonsPanel();
                    }
                }

            }
        }



        public void Lobby()
        {
            ConnectionManager.Instance.Disconnect();
            MainThreadDispatcher.Reset();
            StartCoroutine(DelayedLobby(1f));

        }
        public IEnumerator DelayedLobby(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(GameLiveData.instance.gameObject);
            SceneManager.LoadScene(1);
        }

        public void PlayAgain()
        {
            gameEndPanel.SetActive(false);
            SceneManager.LoadScene("Lobby");
        }

        public void LeaveGame()
        {
            ConnectionManager.Instance.LeaveTable();
            SceneManager.LoadScene("Lobby");
        }
    }
}