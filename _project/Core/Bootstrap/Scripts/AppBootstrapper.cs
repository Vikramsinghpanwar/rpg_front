using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Core.Auth;
using Core.Bootstrap;
using Core.Managers;
using Core.Notifications;
using Core.Session;
using Core.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Core.Models;

namespace Core.Bootstrap
{
    /// <summary>
    /// Milestone 5: Bootstrap is the sole startup orchestrator.
    /// AuthScene is reduced to a login UI only.
    /// </summary>
    public class AppBootstrapper : MonoBehaviour
    {
        [Header("Routing")]
        [SerializeField] private string authScene = ScenePaths.Auth;
        [SerializeField] private string lobbyScene = ScenePaths.Lobby;

        [Header("Loading UI")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TMP_Text statusText;

        [Header("Retry UI (placeholder)")]
        [SerializeField] private GameObject retryPanel;
        [SerializeField] private Button retryButton;
        [SerializeField] private TMP_Text retryStatusText;

        [Header("Version Gate UI (placeholder)")]
        [SerializeField] private GameObject versionGatePanel;

        [Header("Maintenance UI (placeholder)")]
        [SerializeField] private GameObject maintenancePanel;

        [Header("Diagnostics")]
        [SerializeField] private bool enableDiagnostics = true;
        [SerializeField] private bool logSceneTransitions = true;

        static Stopwatch bootTimer;
        string bootId;
        bool sessionRouted;
        bool _sessionRunning;

        void Awake()
        {
            bootId = Guid.NewGuid().ToString("N").Substring(0, 8);
            bootTimer = new Stopwatch();
            bootTimer.Start();

            Log("[BOOT] Bootstrap Scene Loaded");
            SetStatusText("Starting...");

            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryBindSessionEvents();
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnbindSessionEvents();
        }

        void TryBindSessionEvents()
        {
            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.OnSessionReady -= OnSessionReady;
                SessionManager.Instance.OnSessionReady += OnSessionReady;

                SessionManager.Instance.OnSessionFailed -= OnSessionFailed;
                SessionManager.Instance.OnSessionFailed += OnSessionFailed;

                SessionManager.Instance.OnUnauthenticated -= OnUnauthenticated;
                SessionManager.Instance.OnUnauthenticated += OnUnauthenticated;
            }
        }

        void UnbindSessionEvents()
        {
            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.OnSessionReady -= OnSessionReady;
                SessionManager.Instance.OnSessionFailed -= OnSessionFailed;
                SessionManager.Instance.OnUnauthenticated -= OnUnauthenticated;
            }
        }

        void OnSessionReady(BootstrapResponse response)
        {
            if (sessionRouted) return;
            sessionRouted = true;
            Log("[BOOT] Session Ready");
            SetStatusText("Loading lobby...");
            _ = RegisterFcmIfAvailableAsync();
            SceneManager.LoadScene(lobbyScene);
        }

        void OnSessionFailed(string reason)
        {
            if (sessionRouted) return;
            LogError($"[BOOT] Session Failed: {reason}");
            SetStatusText("Session failed. Tap retry.");
            ShowRetryPanel();
        }

        void OnUnauthenticated()
        {
            if (sessionRouted) return;
            sessionRouted = true;
            Log("[BOOT] Unauthenticated, routing to Auth");
            SceneManager.LoadScene(authScene);
        }

        async void Start()
        {
            Log("[BOOT] Initializing...");

            try
            {
                await InitializeTokenProviderAsync();
                await InitializeSettingsManagerAsync();
                await InitializeAuthManagerAsync();
                await InitializeBootstrapServiceAsync();
                await InitializeInfrastructureAsync();
                await InitializePushNotificationsAsync();

                await RunSessionAsync();
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] Startup failed: {ex.GetType().Name}: {ex.Message}");
                SetStatusText("Startup failed. Tap retry.");
                ShowRetryPanel();
            }
        }

        async Task RunSessionAsync()
        {
            if (_sessionRunning) return;
            _sessionRunning = true;

            try
            {
                Log("[BOOT] Starting SessionManager");
                SetStatusText("Checking session...");

                await SessionManager.Instance.Initialize();

                if (sessionRouted) return;

                if (SessionManager.Instance.IsReady)
                {
                    Log("[BOOT] Session Ready");
                    Log($"[BOOT] Loading Lobby elapsedMs={bootTimer.ElapsedMilliseconds}");
                    SetStatusText("Loading lobby...");
                    _ = RegisterFcmIfAvailableAsync();
                    SceneManager.LoadScene(lobbyScene);
                }
                else
                {
                    var auth = AuthManager.Instance;
                    if (auth == null || !auth.IsAuthenticated)
                    {
                        Log("[BOOT] Not authenticated, routing to Auth");
                        SceneManager.LoadScene(authScene);
                    }
                    else
                    {
                        Log("[BOOT] Version gate blocked, staying in Bootstrap");
                        SetStatusText("Update required or maintenance in progress.");
                        ShowVersionGatePanel();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] SessionManager Failed: {ex.GetType().Name}: {ex.Message}");
                SetStatusText("Session failed. Tap retry.");
                ShowRetryPanel();
            }
            finally
            {
                _sessionRunning = false;
            }
        }

        async Task RegisterFcmIfAvailableAsync()
        {
            string fcmToken = PlayerPrefs.GetString("fcm_token", null);
            if (!string.IsNullOrEmpty(fcmToken))
            {
                await DeviceRegistrationService.RegisterDeviceAsync(fcmToken);
            }
        }

        // ── Infrastructure init (unchanged) ────────────────────────────────────

        async Task InitializeTokenProviderAsync()
        {
            try
            {
                Log("[BOOT] Initializing TokenProvider");
                SetStatusText("Loading session...");

                await Task.Yield();

                TokenProvider.Initialize();
                Log("[BOOT] TokenProvider Ready");
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] TokenProvider Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        async Task InitializeSettingsManagerAsync()
        {
            try
            {
                Log("[BOOT] Initializing SettingsManager");
                SetStatusText("Loading settings...");

                await Task.Yield();

                SettingsManager.Initialize();
                Log("[BOOT] SettingsManager Ready");
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] SettingsManager Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        async Task InitializeAuthManagerAsync()
        {
            try
            {
                Log("[BOOT] Initializing AuthManager");
                SetStatusText("Preparing authentication...");

                await Task.Yield();

                AuthManager.EnsureInitialized();
                Log("[BOOT] AuthManager Ready");
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] AuthManager Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        async Task InitializeBootstrapServiceAsync()
        {
            try
            {
                Log("[BOOT] Initializing BootstrapService");
                SetStatusText("Loading configuration...");

                await Task.Yield();

                BootstrapService.Initialize();
                Log("[BOOT] BootstrapService Ready");
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] BootstrapService Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        async Task InitializeInfrastructureAsync()
        {
            try
            {
                Log("[BOOT] Initializing infrastructure managers");

                await InitializeLoggerAsync();
                await InitializeToastAsync();
                await InitializePopupManagerAsync();
                await InitializeLoadingManagerAsync();
                await InitializeCGSBetServiceAsync();

                Log("[BOOT] Infrastructure managers Ready");
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] Infrastructure initialization Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        async Task InitializeLoggerAsync()
        {
            try
            {
                Log("[BOOT] Initializing Logger");
                SetStatusText("Initializing logger...");

                await Task.Yield();

                Logger.EnsureInitialized();
                Log("[BOOT] Logger Ready");
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] Logger Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        async Task InitializeToastAsync()
        {
            try
            {
                Log("[BOOT] Initializing Toast");
                SetStatusText("Initializing toast...");

                await Task.Yield();

                Toast.EnsureInitialized();
                Log("[BOOT] Toast Ready");
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] Toast Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        async Task InitializePopupManagerAsync()
        {
            try
            {
                Log("[BOOT] Initializing PopupManager");
                SetStatusText("Initializing dialogs...");

                await Task.Yield();

                Core.Managers.PopupManager.EnsureInitialized();
                Log("[BOOT] PopupManager Ready");
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] PopupManager Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        async Task InitializeLoadingManagerAsync()
        {
            try
            {
                Log("[BOOT] Initializing LoadingManager");
                SetStatusText("Initializing loading...");

                await Task.Yield();

                Core.Managers.LoadingManager.EnsureInitialized();
                Log("[BOOT] LoadingManager Ready");
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] LoadingManager Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        async Task InitializeCGSBetServiceAsync()
        {
            try
            {
                Log("[BOOT] Initializing CGSBetService");
                SetStatusText("Initializing game services...");

                await Task.Yield();

                CGSBetService.EnsureInitialized();
                Log("[BOOT] CGSBetService Ready");
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] CGSBetService Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        async Task InitializePushNotificationsAsync()
        {
            try
            {
                Log("[BOOT] Initializing PushNotificationService");
                SetStatusText("Initializing notifications...");

                await Task.Yield();

                Core.Notifications.PushNotificationService.EnsureInitialized();
                Core.Notifications.PushNotificationService.Instance.Initialize();
                Log("[BOOT] PushNotificationService Ready");
            }
            catch (Exception ex)
            {
                LogError($"[BOOT] PushNotificationService Failed: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // ── Scene transition logging ──────────────────────────────────────────

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (logSceneTransitions)
            {
                Log($"[BOOT] {scene.name} Scene Loaded");
            }

            if (scene.name == authScene)
            {
                sessionRouted = false;
            }
        }

        // ── UI helpers ────────────────────────────────────────────────────────

        void SetStatusText(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        public void ShowRetryPanel()
        {
            if (retryPanel != null)
                retryPanel.SetActive(true);
        }

        public void HideRetryPanel()
        {
            if (retryPanel != null)
                retryPanel.SetActive(false);
        }

        public void ShowVersionGatePanel()
        {
            if (versionGatePanel != null)
                versionGatePanel.SetActive(true);
        }

        public void HideVersionGatePanel()
        {
            if (versionGatePanel != null)
                versionGatePanel.SetActive(false);
        }

        public void ShowMaintenancePanel()
        {
            if (maintenancePanel != null)
                maintenancePanel.SetActive(true);
        }

        public void HideMaintenancePanel()
        {
            if (maintenancePanel != null)
                maintenancePanel.SetActive(false);
        }

        public void HideLoadingPanel()
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }

        // ── Diagnostics ───────────────────────────────────────────────────────

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        void Log(string msg)
        {
            if (enableDiagnostics)
                UnityEngine.Debug.Log(msg);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        void LogError(string msg)
        {
            if (enableDiagnostics)
                UnityEngine.Debug.LogError(msg);
        }
    }
}
