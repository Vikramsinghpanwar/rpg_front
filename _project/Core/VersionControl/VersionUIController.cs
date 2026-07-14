using System;
using System.Threading.Tasks;
using Core.Managers;
using Core.VersionControl;
using Core.VersionControl.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.VersionControl
{
    public class VersionUIController : MonoBehaviour
    {
        public static VersionUIController Instance { get; private set; }

        [Header("View References")]
        [SerializeField] private UpdateAvailablePopup updateAvailablePopup;
        [SerializeField] private ForceUpdatePopup forceUpdatePopup;
        [SerializeField] private MaintenanceScreen maintenanceScreen;
        [SerializeField] private LoadingManager loadingManager;

        [Header("Network Failure Overlay")]
        [SerializeField] private GameObject networkFailureRoot;
        [SerializeField] private TMP_Text networkFailureText;
        [SerializeField] private Button networkRetryButton;
        [SerializeField] private TMP_Text networkRetryButtonLabel;

        bool pendingRetry;
        bool inFlight;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            // if (Instance != null) return;
            // var go = new GameObject("[VersionUIController]");
            // DontDestroyOnLoad(go);
            // Instance = go.AddComponent<VersionUIController>();
        }

        void Awake()
        {
            // if (Instance != null && Instance != this)
            // {
            //     Destroy(gameObject);
            //     return;
            // }
            Instance = this;
            // DontDestroyOnLoad(gameObject);

            if (VersionManager.Instance != null)
            {
                VersionManager.Instance.OnOutcomeChanged += OnOutcomeChanged;
                VersionManager.Instance.OnCheckFailed += OnCheckFailed;
            }
        }

        void Start()
        {
            if (loadingManager == null)
                loadingManager = Core.Managers.LoadingManager.Instance;
        }

        void OnDestroy()
        {
            if (VersionManager.Instance != null)
            {
                VersionManager.Instance.OnOutcomeChanged -= OnOutcomeChanged;
                VersionManager.Instance.OnCheckFailed -= OnCheckFailed;
            }
            HideAll();
        }

        void OnOutcomeChanged(VersionInfo info)
        {
            if (info == null) return;
            HideAll();
            if (networkFailureRoot != null) networkFailureRoot.SetActive(false);
            pendingRetry = false;

            if (updateAvailablePopup == null) updateAvailablePopup = FindObjectOfType<UpdateAvailablePopup>(true);
            if (forceUpdatePopup == null) forceUpdatePopup = FindObjectOfType<ForceUpdatePopup>(true);
            if (maintenanceScreen == null) maintenanceScreen = FindObjectOfType<MaintenanceScreen>(true);

            switch (info.Status)
            {
                case VersionStatus.UpToDate:
                    CloseVersionGate();
                    break;
                case VersionStatus.SoftUpdate:
                    ShowSoftUpdate(info);
                    break;
                case VersionStatus.ForceUpdate:
                case VersionStatus.Unsupported:
                    ShowForceUpdate(info);
                    break;
                case VersionStatus.Maintenance:
                    ShowMaintenance(info);
                    break;
            }
        }

        void OnCheckFailed(Exception ex, string checkId)
        {
            HideAll();
            ShowNetworkFailure(ex?.Message ?? "Network error");
        }

        public void CloseVersionGate()
        {
            VersionLog.OutcomeHandled(Guid.NewGuid().ToString("N"), "gate_closed");
        }

        void ShowSoftUpdate(VersionInfo info)
        {
            if (updateAvailablePopup != null) updateAvailablePopup.Show(info);
        }

        void ShowForceUpdate(VersionInfo info)
        {
            if (forceUpdatePopup != null) forceUpdatePopup.Show(info);
        }

        void ShowMaintenance(VersionInfo info)
        {
            if (maintenanceScreen != null) maintenanceScreen.Show(info);
        }

        void ShowNetworkFailure(string message)
        {
            if (networkFailureRoot == null)
            {
                Debug.LogWarning("[VC] Network failure but no UI assigned: " + message);
                return;
            }
            pendingRetry = true;
            if (networkFailureText != null)
            {
                networkFailureText.text = "Unable to reach server. Please check your connection.";
            }
            if (networkRetryButtonLabel != null) networkRetryButtonLabel.text = "Retry";
            WireRetryButton();
            networkFailureRoot.SetActive(true);
        }

        void WireRetryButton()
        {
            if (networkRetryButton != null) networkRetryButton.onClick.RemoveListener(OnNetworkRetry);
            if (networkRetryButton != null) networkRetryButton.onClick.AddListener(OnNetworkRetry);
        }

        async void OnNetworkRetry()
        {
            if (inFlight) return;
            inFlight = true;
            if (networkRetryButton != null) networkRetryButton.interactable = false;
            if (networkFailureRoot != null) networkFailureRoot.SetActive(false);

            await Task.Yield();

            if (Core.Session.SessionManager.Instance != null)
            {
                Core.Session.SessionManager.Instance.ForceVersionRecheck();
            }
            else if (Core.VersionControl.VersionManager.Instance != null)
            {
                await Core.VersionControl.VersionManager.Instance.PerformVersionCheckAsync();
            }

            inFlight = false;
            if (networkRetryButton != null) networkRetryButton.interactable = true;
        }

        public void HideAll()
        {
            if (updateAvailablePopup != null) updateAvailablePopup.Hide();
            if (forceUpdatePopup != null) forceUpdatePopup.Hide();
            if (maintenanceScreen != null) maintenanceScreen.Hide();
        }

        public void SetUpdatePopup(UpdateAvailablePopup popup)
        {
            updateAvailablePopup = popup;
        }

        public void SetForceUpdatePopup(ForceUpdatePopup popup)
        {
            forceUpdatePopup = popup;
        }

        public void SetMaintenanceScreen(MaintenanceScreen screen)
        {
            maintenanceScreen = screen;
        }

        public void BindNetworkFailure(GameObject root, TMP_Text label, Button retry, TMP_Text retryLabel)
        {
            networkFailureRoot = root;
            networkFailureText = label;
            networkRetryButton = retry;
            networkRetryButtonLabel = retryLabel;
            WireRetryButton();
        }
    }
}
