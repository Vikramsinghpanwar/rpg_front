using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Features.Lobby.Models;
using Core.Bootstrap;
using Core.Utils;
using Core.Models;
using Core.API.Endpoints;
using Features.Profile.Services;

namespace Features.Lobby.UI
{
    public class LobbyScreenUI : MonoBehaviour
    {
        [Header("Header Section")]
        [SerializeField] private TMP_Text usernameTMP;
        [SerializeField] private TMP_Text userIdTMP;
        [SerializeField] private TMP_Text walletBalanceTMP;
        [SerializeField] private TMP_Text bonusBalanceTMP;
        [SerializeField] private Image profileIcon;

        [Header("Game Grid")]
        [SerializeField] private Transform gameGridParent;
        [SerializeField] private GameObject gameItemPrefab;

        [Header("Filter Buttons")]
        [SerializeField] private Button allButton;
        [SerializeField] private Button multiplayerButton;
        [SerializeField] private Button skillButton;
        [SerializeField] private Button sportsButton;

        [Header("Mail")]
        [SerializeField] private Button mailButton;
        [SerializeField] private GameObject mailUnreadBadgeRoot;
        [SerializeField] private TextMeshProUGUI mailUnreadBadgeText;

        [Header("Notice")]
        [SerializeField] private Button noticeButton;

        [Header("Game History")]
        [SerializeField] private Button historyButton;

        [Header("Reward History")]
        [SerializeField] private Button rewardButton;

        [Header("Loading")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private Image loadingSpinner;

        [Header("Wallet Refresh")]
        [SerializeField] private Button refreshWalletButton;
        [SerializeField] private float refreshCooldownSeconds = 5f;
        [SerializeField] private RefreshButtonAnimator refreshButtonAnimator;

        Action<LobbyGame> onGameSelected;
        Action<GameFilterType> onFilterRequested;
        Action onMailClicked;
        Action onNoticeClicked;
        Action onHistoryClicked;
        Action onRewardClicked;

        float _lastWalletRefreshTime;

        void Awake()
        {
            SetupFilterButtons();
            SetupMailButton();
            SetupNoticeButton();
            SetupHistoryButton();
            SetupRewardButton();
            SetupRefreshWalletButton();
            RefreshHeader();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (refreshButtonAnimator != null)
                refreshButtonAnimator.ForceStop();

            if (rewardButton != null)
                rewardButton.onClick.RemoveListener(OnRewardClicked);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.Equals("Lobby", StringComparison.OrdinalIgnoreCase))
            {
                _ = RefreshWalletFromApi();
                _ = RefreshPaymentGatewaysOnLobbyEntry();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) return;
            if (Time.time - _lastWalletRefreshTime < refreshCooldownSeconds) return;
            if (!SceneManager.GetActiveScene().name.Equals("Lobby", StringComparison.OrdinalIgnoreCase)) return;

            _lastWalletRefreshTime = Time.time;
            _ = RefreshWalletFromApi();
        }

        private async Task RefreshPaymentGatewaysOnLobbyEntry()
        {
            Debug.Log("[PaymentGateway] Entered Lobby - Refreshing available gateways");

            if (BootstrapService.Instance != null)
            {
                await BootstrapService.Instance.Refresh();
            }

            var rechargeCtrl = FindObjectOfType<Features.Recharge.Controllers.RechargeController>();
            if (rechargeCtrl == null)
            {
                var go = new GameObject("RechargeController");
                rechargeCtrl = go.AddComponent<Features.Recharge.Controllers.RechargeController>();
            }

            await rechargeCtrl.LoadGateways("lobby", silent: true);
        }

        void OnEnable()
        {
            if (BootstrapService.Instance != null)
                BootstrapService.Instance.OnBootstrapUpdated += OnBootstrapUpdated;
        }

        void OnDisable()
        {
            if (BootstrapService.Instance != null)
                BootstrapService.Instance.OnBootstrapUpdated -= OnBootstrapUpdated;
        }

        void OnBootstrapUpdated(Core.Models.BootstrapResponse response)
        {
            RefreshHeader();

            if (profileIcon != null && response?.profile != null)
            {
                if (!string.IsNullOrEmpty(response.profile.avatar))
                {
                    var fallback = Resources.Load<Sprite>(response.profile.avatar);
                    if (fallback != null)
                    {
                        profileIcon.sprite = fallback;
                    }
                    else
                    {
                        UpdateRemoteAvatarAsync(response.profile.avatar);
                    }
                }
                else
                {
                    profileIcon.sprite = null;
                }
            }
        }

        void SetupFilterButtons()
        {
            if (allButton != null) allButton.onClick.AddListener(() => RequestFilter(GameFilterType.All));
            if (multiplayerButton != null) multiplayerButton.onClick.AddListener(() => RequestFilter(GameFilterType.Multiplayer));
            if (skillButton != null) skillButton.onClick.AddListener(() => RequestFilter(GameFilterType.Skill));
            if (sportsButton != null) sportsButton.onClick.AddListener(() => RequestFilter(GameFilterType.Sports));
        }

        void SetupMailButton()
        {
            if (mailButton != null) mailButton.onClick.AddListener(() => onMailClicked?.Invoke());
        }

        void SetupNoticeButton()
        {
            if (noticeButton != null) noticeButton.onClick.AddListener(() => onNoticeClicked?.Invoke());
        }

        void SetupHistoryButton()
        {
            if (historyButton != null) historyButton.onClick.AddListener(() => onHistoryClicked?.Invoke());
        }

        void SetupRewardButton()
        {
            if (rewardButton != null) rewardButton.onClick.AddListener(OnRewardClicked);
        }

        void OnRewardClicked()
        {
            onRewardClicked?.Invoke();
        }

        void SetupRefreshWalletButton()
        {
            if (refreshWalletButton != null)
            {
                refreshWalletButton.onClick.AddListener(OnRefreshWalletButtonClicked);
            }
        }

        void OnRefreshWalletButtonClicked()
        {
            if (refreshWalletButton == null) return;

            if (Time.time - _lastWalletRefreshTime < refreshCooldownSeconds)
            {
                // Debug.Log("[LobbyScreenUI] Refresh wallet button tapped before cooldown expired");
                return;
            }

            _lastWalletRefreshTime = Time.time;
            refreshButtonAnimator?.StartSpin();
            _ = RefreshWalletFromApi();
        }

        public async Task RefreshWalletFromApi()
        {
            try
            {
                string userId = Features.Profile.Services.ProfileService.GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    Debug.LogWarning("[LobbyScreenUI] RefreshWalletFromApi skipped: userId is empty");
                    refreshButtonAnimator?.StopSpin();
                    return;
                }

                await Core.Bootstrap.BootstrapService.Instance.RefreshWallet(userId);
                RefreshHeader();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LobbyScreenUI] RefreshWalletFromApi failed: " + ex.Message);
            }
            finally
            {
                refreshButtonAnimator?.StopSpin();
            }
        }

        public void SetOnMailClicked(Action callback)
        {
            onMailClicked = callback;
        }

        public void SetOnNoticeClicked(Action callback)
        {
            onNoticeClicked = callback;
        }

        public void SetOnHistoryClicked(Action callback)
        {
            onHistoryClicked = callback;
        }

        public void SetOnRewardClicked(Action callback)
        {
            onRewardClicked = callback;
        }

        public void UpdateMailUnreadBadge(int count)
        {
            bool showBadge = count > 0;

            if (mailUnreadBadgeRoot != null)
            {
                mailUnreadBadgeRoot.SetActive(showBadge);
            }

            if (mailUnreadBadgeText != null)
            {
                if (count > 99)
                {
                    mailUnreadBadgeText.text = "99+";
                }
                else if (count > 0)
                {
                    mailUnreadBadgeText.text = count.ToString();
                }
                else
                {
                    mailUnreadBadgeText.text = string.Empty;
                }

                // Debug.Log($"[LobbyScreenUI] Mail unread badge updated: count={count}, text={mailUnreadBadgeText.text}, rootActive={mailUnreadBadgeRoot != null && mailUnreadBadgeRoot.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("[LobbyScreenUI] mailUnreadBadgeText is null — badge will not be visible");
            }
        }

        public void SetOnFilterRequested(Action<GameFilterType> callback)
        {
            onFilterRequested = callback;
        }

        void RequestFilter(GameFilterType filter)
        {
            onFilterRequested?.Invoke(filter);
        }

        public void RefreshHeader()
        {
            // Debug.Log("[LobbyScreenUI] Refreshing header with latest profile and wallet data");
            if (BootstrapService.Instance?.HasData == true)
            {
                // Debug.Log("[LobbyScreenUI] Bootstrap data is available. Updating header.");
                var profile = BootstrapService.Instance.Profile;
                var wallet = BootstrapService.Instance.Wallet;

                if (profile != null)
                {
                    if (usernameTMP != null) usernameTMP.SetText(profile.username);
                    if (userIdTMP != null) userIdTMP.SetText(profile.public_id);
                }

                if (wallet != null)
                {
                    if (walletBalanceTMP != null) walletBalanceTMP.SetText(MoneyFormatter.FormatPaisa(wallet.available_balance));
                    if (bonusBalanceTMP != null) bonusBalanceTMP.SetText(MoneyFormatter.FormatPaisa(wallet.bonus_balance));
                }
            }
        }

        async void UpdateRemoteAvatarAsync(string avatarUrl)
        {
            try
            {
                var sprite = await AvatarLoader.LoadRemoteAsync(avatarUrl);
                if (profileIcon != null)
                {
                    profileIcon.sprite = sprite;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LobbyScreenUI] Remote avatar update failed: {ex.Message}");
            }
        }

        public void SetGames(List<LobbyGame> games)
        {
            PopulateGameGrid(games);
        }

        public void SetOnGameSelected(Action<LobbyGame> callback)
        {
            onGameSelected = callback;
        }

        void PopulateGameGrid(List<LobbyGame> games)
        {
            if (gameGridParent == null || gameItemPrefab == null) return;

            foreach (Transform child in gameGridParent)
            {
                Destroy(child.gameObject);
            }

            if (games == null) return;

            foreach (var game in games)
            {
                var instance = Instantiate(gameItemPrefab, gameGridParent);
                var itemUI = instance.GetComponent<LobbyGameItemUI>();
                if (itemUI != null)
                {
                    itemUI.Initialize(game, onGameSelected);
                }
            }
        }

        public void ShowLoading()
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(true);
        }

        public void HideLoading()
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(false);
        }

        public void Clear()
        {
            foreach (Transform child in gameGridParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
}