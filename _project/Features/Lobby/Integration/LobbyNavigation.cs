using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Features.DailyBonus.Controllers;
using Features.DailyBonus.UI;
using Features.Lobby.Controllers;
using Features.Lobby.Models;
using Features.Lobby.UI;
using Features.Mail.Controllers;
using Features.Mail.UI;
using Features.Notice.Controllers;
using Features.Notice.Models;
using Features.Notice.UI;
using Features.Share.UI;
using Features.GameHistory.UI;
using Core.Bootstrap;
using Core.Managers;
using Core.Session;
using Features.Share.Controllers;
using Features.Rewards.UI;
using Features.Rewards.Controllers;

namespace Features.Lobby.Integration
{
    public class LobbyNavigation : MonoBehaviour
    {
        const string FirstLobbyOpenPref = "LobbyNavigation.FirstLobbyOpen";
        static bool _panelsShownThisLaunch;

        [Header("References")]
        [SerializeField] private LobbyController lobbyController;
        [SerializeField] private LobbyScreenUI lobbyScreenUI;

        [Header("Lobby GameObject")]
        [SerializeField] private GameObject lobbyRoot;

        [Header("Daily Bonus")]
        [SerializeField] private DailyBonus.Controllers.DailyBonusController dailyBonusController;
        [SerializeField] private Features.DailyBonus.UI.DailyBonusScreen dailyBonusScreen;

        [Header("Share")]
        [SerializeField] private Share.UI.ShareScreen shareScreen;
        [SerializeField] private GameObject sharePanel;

        [Header("Mail")]
        [SerializeField] private MailController mailController;
        [SerializeField] private MailInboxScreen mailInboxScreen;

        [Header("Notice")]
        [SerializeField] private NoticeController noticeController;
        [SerializeField] private NoticePanelView noticePanel;

        [Header("Game History")]
        [SerializeField] private GameHistoryScreen gameHistoryScreen;

        [Header("Reward History")]
        [SerializeField] private RewardHistoryScreen rewardHistoryScreen;

        void Start()
        {
            if (lobbyRoot != null)
            {
                lobbyRoot.SetActive(SessionManager.Instance.IsReady);
            }

            if (lobbyController == null)
                lobbyController = GetComponent<LobbyController>();
            if (lobbyScreenUI == null)
                lobbyScreenUI = GetComponent<LobbyScreenUI>();

            if (noticeController == null)
                noticeController = GetComponent<NoticeController>();
            if (noticePanel == null && noticeController != null)
            {
                noticePanel = noticeController.GetComponent<NoticePanelView>();
                if (noticePanel == null)
                    noticePanel = noticeController.GetComponentInChildren<NoticePanelView>(true);
            }

            if (noticeController == null)
                noticeController = FindObjectOfType<NoticeController>();
            if (noticePanel == null && noticeController != null)
            {
                noticePanel = noticeController.GetComponent<NoticePanelView>();
                if (noticePanel == null)
                    noticePanel = noticeController.GetComponentInChildren<NoticePanelView>(true);
            }

            if (noticePanel == null)
                noticePanel = FindObjectOfType<NoticePanelView>();

            // Debug.Log("[LobbyNavigation] Start: session-ready listener wired");
            SessionManager.Instance.OnSessionReady += OnSessionReady;
            SessionManager.Instance.OnUnauthenticated += HideLobby;

            if (SessionManager.Instance.IsReady)
            {
                // Debug.Log("[LobbyNavigation] Start: SessionManager already ready, calling InitializeLobby directly");
                _ = InitializeLobby();
            }

            lobbyController.OnGamesLoaded += lobbyScreenUI.SetGames;
            lobbyController.OnGamesLoadFailed += reason => Debug.LogWarning("[LobbyNavigation] Games load failed: " + reason);
            lobbyScreenUI.SetOnFilterRequested(lobbyController.FilterGames);

            if (mailController != null)
            {
                lobbyScreenUI.SetOnMailClicked(OpenMailScreen);
                mailController.OnUnreadCountUpdated += OnUnreadCountUpdated;
            }

            if (noticeController != null)
            {
                lobbyScreenUI.SetOnNoticeClicked(OpenNoticePanel);
            }

            if (gameHistoryScreen != null)
            {
                lobbyScreenUI.SetOnHistoryClicked(OpenGameHistoryScreen);
            }

            if (rewardHistoryScreen != null)
            {
                lobbyScreenUI.SetOnRewardClicked(OpenRewardHistoryScreen);
            }

            _ = RefreshMailUnreadCount();
        }

        private void OnSessionReady(Core.Models.BootstrapResponse response)
        {
            Debug.Log("[LobbyNavigation] OnSessionReady received -> InitializeLobby");
            _ = InitializeLobby();

            Core.Notifications.NotificationRouter.TryFlush();
        }

        private async Task InitializeLobby()
        {
            Debug.Log("[LobbyNavigation] InitializeLobby begin");
            var bootstrap = BootstrapService.Instance;
            if (bootstrap != null && bootstrap.HasData != true)
            {
                Debug.Log("[LobbyNavigation] Bootstrap missing -> refreshing");
                await bootstrap.Refresh();
                Debug.Log("[LobbyNavigation] Refresh done, HasData=" + bootstrap.HasData);
            }

            lobbyScreenUI.SetOnGameSelected(HandleGameSelected);
            lobbyScreenUI.ShowLoading();

            try
            {
                // Debug.Log("[LobbyNavigation] Loading games from bootstrap...");
                await lobbyController.InitializeFromBootstrap();
                lobbyScreenUI.RefreshHeader();
                lobbyScreenUI.HideLoading();

                if (lobbyRoot != null) lobbyRoot.SetActive(true);

                // Debug.Log("[LobbyNavigation] Games loaded, about to auto-open first-run panels");
                await AutoOpenFirstSessionPanels();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LobbyNavigation] Bootstrap failed: " + e.Message);
                lobbyScreenUI.HideLoading();
                LoadingManager.Instance?.Show("Failed to load lobby. Please try again.");
            }

            _ = RefreshMailUnreadCount();
        }

        void HandleGameSelected(LobbyGame game)
        {
            if (game == null) return;

            // Debug.Log("[LobbyNavigation] Selected game: " + game.name + " (scene: " + game.scene_name + ")");

            if (!string.IsNullOrEmpty(game.scene_name))
            {
                SceneManager.LoadScene(game.scene_name);
            }
            else
            {
                Debug.LogWarning("[LobbyNavigation] No scene_name defined for game: " + game.name);
            }
        }

        void HideLobby()
        {
            if (lobbyRoot != null) lobbyRoot.SetActive(false);
        }

        private async Task AutoOpenFirstSessionPanels()
        {
            // Debug.Log("[LobbyNavigation] AutoOpenFirstSessionPanels invoked");
            if (_panelsShownThisLaunch)
            {
                // Debug.Log("[LobbyNavigation] AutoOpenFirstSessionPanels SKIP: panels already shown in this cold-app session");
                return;
            }
            if (BootstrapService.Instance == null || !BootstrapService.Instance.HasData)
            {
                Debug.LogWarning("[LobbyNavigation] AutoOpenFirstSessionPanels SKIP: bootstrap still not ready");
                return;
            }

            try
            {
                // Debug.Log("[LobbyNavigation] AutoOpenFirstSessionPanels: entering Notice step");
                await OpenFirstOpenNotice();
                // Debug.Log("[LobbyNavigation] AutoOpenFirstSessionPanels: Notice step ended");
                // Debug.Log("[LobbyNavigation] AutoOpenFirstSessionPanels: entering DailyBonus step");
                await OpenDailyBonusOnce();
                // Debug.Log("[LobbyNavigation] AutoOpenFirstSessionPanels: DailyBonus step ended");
                // Debug.Log("[LobbyNavigation] AutoOpenFirstSessionPanels: entering Share step");
                await OpenShareOnce();
                // Debug.Log("[LobbyNavigation] AutoOpenFirstSessionPanels: Share step ended");
            }
            catch (Exception ex)
            {
                Debug.LogError("[LobbyNavigation] First-session panel sequence failed: " + ex);
            }

            _panelsShownThisLaunch = true;
            // Debug.Log("[LobbyNavigation] AutoOpenFirstSessionPanels: cold-launch panels marked as shown");
        }

        async Task OpenFirstOpenNotice()
        {
            // Debug.Log("[LobbyNavigation] OpenFirstOpenNotice start");
            // Debug.Log("[LobbyNavigation] OpenFirstOpenNotice state: noticeController=" + (noticeController != null) + ", noticePanel=" + (noticePanel != null) + ", panelGO=" + (noticePanel != null ? noticePanel.gameObject.name : "N/A") + ", activeInHierarchy=" + (noticePanel != null && noticePanel.gameObject != null && noticePanel.gameObject.activeInHierarchy));
            if (noticeController == null || noticePanel == null)
            {
                Debug.LogWarning("[LobbyNavigation] OpenFirstOpenNotice aborted: refs missing");
                return;
            }

            if (!noticeController.HasNotices)
            {
                // Debug.Log("[LobbyNavigation] OpenFirstOpenNotice: initializing from bootstrap");
                await noticeController.InitializeFromBootstrap();
                // Debug.Log("[LobbyNavigation] OpenFirstOpenNotice: init result HasNotices=" + noticeController.HasNotices);
                //Debug.Log($"[LobbyNavigation] OpenFirstOpenNotice: allNotices count={noticeController.AllNotices?.Count ?? 0}");
            }
            else
            {
                // Debug.Log("[LobbyNavigation] OpenFirstOpenNotice: notices already cached, count=" + (noticeController.AllNotices != null ? noticeController.AllNotices.Count : 0));
            }

            var wasActive = noticePanel.gameObject.activeInHierarchy;
            //Debug.Log($"[LobbyNavigation] OpenFirstOpenNotice: panel.activeInHierarchy BEFORE setActive={wasActive}");
            if (!wasActive)
            {
                // Debug.Log("[notice-debug] OpenFirstOpenNotice: activating panel");
                noticePanel.gameObject.SetActive(true);
                if (noticeController != null)
                {
                    // Debug.Log("[notice-debug] OpenFirstOpenNotice: calling noticeController.Render()");
                    noticeController.Render();
                }
            }
            else
            {
                // Debug.Log("[LobbyNavigation] OpenFirstOpenNotice: panel already active, skipping setActive");
            }
            // Debug.Log("[LobbyNavigation] OpenFirstOpenNotice: waiting for panel close (Will jump to next panel once noticePanel.SetActive(false) is called)");
            await WaitForPanelClose(noticePanel);
            // Debug.Log("[LobbyNavigation] OpenFirstOpenNotice end; panel.gameObject.activeInHierarchy=" + (noticePanel != null && noticePanel.gameObject != null ? noticePanel.gameObject.activeInHierarchy.ToString() : "N/A"));
        }

        async Task OpenDailyBonusOnce()
        {
            // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce start");
            var screen = dailyBonusScreen;
            // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: configuredScreen=" + (screen != null) +", screenName=" + (screen != null ? screen.gameObject.name : "null"));
            if (screen == null)
            {
                // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: fallback FindObjectOfType");
                screen = FindObjectOfType<Features.DailyBonus.UI.DailyBonusScreen>();
                // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: fallback result screen=" + (screen != null));
            }
            // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: resolvedScreen=" + (screen != null) +", screenGO=" + (screen != null ? screen.gameObject.name : "null") + ", activeInHierarchy=" + (screen != null ? screen.gameObject.activeInHierarchy.ToString() : "null"));
            if (screen != null && !screen.gameObject.activeInHierarchy)
            {
                // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: activating screen");
                screen.gameObject.SetActive(true);
                // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: after setActive activeInHierarchy=" + screen.gameObject.activeInHierarchy);
                if (!screen.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning("[LobbyNavigation] OpenDailyBonusOnce: setActive(true) did not activate the DailyBonusScreen!");
                }
            }
            await Task.Delay(100);
            // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: after 100ms delay, activeInHierarchy=" +(screen != null ? screen.gameObject.activeInHierarchy.ToString() : "null"));
            if (dailyBonusController == null && screen != null)
            {
                // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: resolving controller");
                var ctrl = screen.GetComponent<Features.DailyBonus.Controllers.DailyBonusController>();
                // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: screen ctrl=" + (ctrl != null));
                if (ctrl == null) ctrl = FindObjectOfType<Features.DailyBonus.Controllers.DailyBonusController>();
                // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: global ctrl=" + (ctrl != null));
                if (ctrl != null && ctrl.GetCurrentStatus() == null)
                {
                    // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: loading bonus status");
                    await ctrl.LoadBonusStatus();
                    // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: status loaded, canClaim after load=" +(ctrl.GetCurrentStatus() != null ? ctrl.GetCurrentStatus().can_claim.ToString() : "null status"));
                }
            }
            else if (dailyBonusController != null && dailyBonusController.GetCurrentStatus() == null)
            {
                // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: cached controller but status is null, loading status");
                await dailyBonusController.LoadBonusStatus();
            }
            // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce: waiting for close");
            await WaitForPanelClose(screen);
            // Debug.Log("[LobbyNavigation] OpenDailyBonusOnce end; activeInHierarchy=" +(screen != null ? screen.gameObject.activeInHierarchy.ToString() : "null"));
        }

        async Task OpenShareOnce()
        {
            // Debug.Log("[LobbyNavigation] OpenShareOnce start");

            Share.UI.ShareScreen screen = shareScreen;
            if (screen == null)
            {
                // Debug.Log("[LobbyNavigation] OpenShareOnce: shareScreen not assigned, trying FindObjectOfType");
                screen = FindObjectOfType<Features.Share.UI.ShareScreen>();
                // Debug.Log("[LobbyNavigation] OpenShareOnce: ShareScreen found=" + (screen != null));
            }

            if (screen == null)
            {
                // Debug.Log("[LobbyNavigation] OpenShareOnce: ShareScreen not found, falling back to legacy Share panel");
                if (sharePanel == null)
                {
                    var share = FindObjectOfType<global::Features.Share.UI.ShareScreen>();
                    sharePanel = share?.gameObject;
                }
                if (sharePanel != null && !sharePanel.activeInHierarchy)
                {
                    sharePanel.SetActive(true);
                }
                await WaitForPanelClose(sharePanel);
                return;
            }

            // Debug.Log("[LobbyNavigation] OpenShareOnce: activating ShareScreen");
            if (!screen.gameObject.activeInHierarchy)
            {
                screen.gameObject.SetActive(true);
            }

            var controller = screen.GetComponent<ShareController>();
            if (controller != null)
            {
                await controller.RefreshShareDataAsync();
            }

            await WaitForPanelClose(screen);
            // Debug.Log("[LobbyNavigation] OpenShareOnce end");
        }

        async Task WaitForPanelClose(Component panel)
        {
            string name = panel != null && panel.gameObject != null ? panel.gameObject.name : "null";
            //Debug.Log($"[LobbyNavigation] WaitForPanelClose START for panel={name}");
            int ticks = 0;
            while (panel != null && panel.gameObject != null && panel.gameObject.activeInHierarchy)
            {
                if (ticks % 60 == 0)
                {
                    //Debug.Log($"[LobbyNavigation] WaitForPanelClose still waiting for panel={name}, ticks={ticks}");
                }
                ticks++;
                await Task.Yield();
            }
            string reason = panel == null ? "panel=null"
                : panel.gameObject == null ? "gameObject=null"
                : !panel.gameObject.activeInHierarchy ? "deactivated"
                : "unknown";
            //Debug.Log($"[LobbyNavigation] WaitForPanelClose END for panel={name}, reason={reason}");
        }

        async Task WaitForPanelClose(GameObject go)
        {
            string name = go != null ? go.name : "null";
            //Debug.Log($"[LobbyNavigation] WaitForPanelClose(GameObject) START for go={name}");
            int ticks = 0;
            while (go != null && go.activeInHierarchy)
            {
                if (ticks % 60 == 0)
                {
                    //Debug.Log($"[LobbyNavigation] WaitForPanelClose(GameObject) still waiting for go={name}, ticks={ticks}");
                }
                ticks++;
                await Task.Yield();
            }
            string reason = go == null ? "go=null" : !go.activeInHierarchy ? "deactivated" : "unknown";
            //Debug.Log($"[LobbyNavigation] WaitForPanelClose(GameObject) END for go={name}, reason={reason}");
        }

        private async void OpenMailScreen()
        {
            // Debug.Log("[LobbyNavigation] OpenMailScreen called");
            if (mailController == null)
            {
                Debug.LogWarning("[LobbyNavigation] mailController is null");
                return;
            }

            if (mailInboxScreen == null)
            {
                Debug.LogError("[LobbyNavigation] mailInboxScreen is null in Inspector. Assign MailInboxScreen component to LobbyNavigation.mailInboxScreen.");
                return;
            }

            await RefreshMailUnreadCount();

            mailInboxScreen.mailController = mailController;
            mailController.inboxScreenUI = mailInboxScreen;

            // Debug.Log("[LobbyNavigation] Activating mailInboxScreen");
            mailInboxScreen.gameObject.SetActive(true);

            mailInboxScreen.ShowLoading();

            if (BootstrapService.Instance != null && !BootstrapService.Instance.HasData)
            {
                // Debug.Log("[LobbyNavigation] Refreshing bootstrap before loading inbox");
                await BootstrapService.Instance.Refresh();
            }

            // Debug.Log("[LobbyNavigation] Calling LoadInbox(true)");
            await mailController.LoadInbox(true);
            // Debug.Log("[LobbyNavigation] LoadInbox completed");

            mailInboxScreen.HideLoading();
        }

        private void CloseMailScreen()
        {
            if (mailInboxScreen != null)
            {
                mailInboxScreen.gameObject.SetActive(false);
            }

            _ = RefreshMailUnreadCount();
        }

        private async void OpenNoticePanel()
        {
            // Debug.Log("[LobbyNavigation] OpenNoticePanel called");

            //Debug.Log($"[LobbyNavigation] OpenNoticePanel refs: noticeController={noticeController != null}, noticePanel={noticePanel != null}");

            if (noticeController == null)
            {
                Debug.LogError("[LobbyNavigation] OpenNoticePanel aborted: noticeController is null");
                return;
            }
            if (noticePanel == null)
            {
                Debug.LogError("[LobbyNavigation] OpenNoticePanel aborted: noticePanel is null");
                return;
            }

            if (BootstrapService.Instance != null && !BootstrapService.Instance.HasData)
            {
                // Debug.Log("[LobbyNavigation] Refreshing bootstrap before loading notices");
                await BootstrapService.Instance.Refresh();
            }

            // Debug.Log("[notice-debug] OpenNoticePanel: forcing InitializeFromBootstrap to sync UI");
            await noticeController.InitializeFromBootstrap();
            // Debug.Log("[notice-debug] OpenNoticePanel: after init HasNotices=" + noticeController.HasNotices);

            // Debug.Log("[LobbyNavigation] Activating noticePanel");
            noticePanel.gameObject.SetActive(true);

            var closeBtn = FindCloseButton(noticePanel.gameObject);
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveListener(CloseNoticePanel);
                closeBtn.onClick.AddListener(CloseNoticePanel);
                // Debug.Log("[LobbyNavigation] Close button found and wired");
            }
            else
            {
                Debug.LogWarning("[LobbyNavigation] No close button found under noticePanel");
            }
        }

        private void CloseNoticePanel()
        {
            // Debug.Log("[LobbyNavigation] CloseNoticePanel invoked");
            if (noticePanel != null && noticePanel.gameObject != null)
            {
                // Debug.Log("[LobbyNavigation] CloseNoticePanel SETTING inactive: was active=" + noticePanel.gameObject.activeInHierarchy + ", path=" + noticePanel.gameObject.name + " (root=" + (noticePanel.gameObject.transform.root != null ? noticePanel.gameObject.transform.root.gameObject.name : "null") + ")");
                noticePanel.gameObject.SetActive(false);
                //Debug.Log($"[LobbyNavigation] CloseNoticePanel DONE: now active={noticePanel.gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("[LobbyNavigation] CloseNoticePanel skipped: noticePanel or its gameObject is null");
            }
        }

        private void OpenGameHistoryScreen()
        {
            if (gameHistoryScreen == null) return;
            gameHistoryScreen.gameObject.SetActive(true);
        }

        private void OpenRewardHistoryScreen()
        {
            if (rewardHistoryScreen == null) return;
            rewardHistoryScreen.gameObject.SetActive(true);
        }

        private void OnUnreadCountUpdated(int count)
        {
            lobbyScreenUI.UpdateMailUnreadBadge(count);
        }

        private async Task RefreshMailUnreadCount()
        {
            if (mailController != null)
            {
                int count = await mailController.LoadUnreadCount();
                lobbyScreenUI.UpdateMailUnreadBadge(count);
            }
        }

        static Button FindCloseButton(GameObject root)
        {
            var buttons = root.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var b in buttons)
            {
                if (b != null && b.name != null && b.name.ToLower().Contains("close"))
                {
                    return b;
                }
            }

            return null;
        }

        void OnDestroy()
        {
            if (lobbyController != null)
            {
                lobbyController.OnGamesLoaded -= lobbyScreenUI.SetGames;
                lobbyController.OnGamesLoadFailed -= HandleGameLoadFailed;
            }

            if (mailController != null)
            {
                mailController.OnUnreadCountUpdated -= OnUnreadCountUpdated;
            }

            SessionManager.Instance.OnSessionReady -= OnSessionReady;
            SessionManager.Instance.OnUnauthenticated -= HideLobby;
        }

        private void HandleGameLoadFailed(string reason)
        {
            Debug.LogWarning("[LobbyNavigation] Games load failed: " + reason);
        }
    }
}
