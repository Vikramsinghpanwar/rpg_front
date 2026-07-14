using System;
using System.Threading.Tasks;
using Core.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.VersionControl.UI
{
    public class MaintenanceScreen : MonoBehaviour
    {
        [Header("View Roots")]
        [SerializeField] private GameObject root;

        [Header("Text")]
        [SerializeField] private TMP_Text maintenanceHeaderText;
        [SerializeField] private TMP_Text maintenanceMessageText;
        [SerializeField] private TMP_Text expectedCompletionText;
        [SerializeField] private Button retryButton;
        [SerializeField] private TMP_Text retryButtonLabel;

        VersionInfo currentInfo;
        bool visible;
        bool autoRetrying;

        public bool Visible => visible;

        void Reset()
        {
            root = gameObject;
        }

        void Awake()
        {
            if (root != null) root.SetActive(false);
        }

        public void Show(VersionInfo info)
        {
            if (info == null || root == null) return;
            currentInfo = info;
            visible = true;
            autoRetrying = false;

            if (maintenanceHeaderText != null) maintenanceHeaderText.text = "Server Maintenance";
            if (maintenanceMessageText != null) maintenanceMessageText.text = info.MaintenanceMessage ?? "We're performing scheduled maintenance.";
            if (expectedCompletionText != null)
            {
                if (!string.IsNullOrEmpty(info.MaintenanceUntil))
                {
                    expectedCompletionText.text = $"Expected Completion\n{info.MaintenanceUntil}";
                }
                else
                {
                    expectedCompletionText.text = "Please wait…";
                }
            }
            if (retryButtonLabel != null) retryButtonLabel.text = "Retry";

            WireButtons();
            root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
            visible = false;
            currentInfo = null;
            UnwireButtons();
        }

        void WireButtons()
        {
            if (retryButton != null) retryButton.onClick.RemoveListener(OnRetryClicked);
            if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
        }

        void UnwireButtons()
        {
            if (retryButton != null) retryButton.onClick.RemoveListener(OnRetryClicked);
        }

        async void OnRetryClicked()
        {
            if (autoRetrying) return;

            VersionLog.OutcomeHandled(Guid.NewGuid().ToString("N"), "maintenance_retry_clicked");
            autoRetrying = true;
            if (retryButton != null) retryButton.interactable = false;

            bool ok = false;
            if (VersionManager.Instance != null)
            {
                ok = await VersionManager.Instance.PerformVersionCheckAsync();
            }

            autoRetrying = false;
            if (retryButton != null) retryButton.interactable = true;

            if (ok && VersionManager.Instance.CurrentStatus == VersionStatus.UpToDate)
            {
                Hide();
                if (Core.Session.SessionManager.Instance != null)
                {
                    Core.Session.SessionManager.Instance.ForceVersionRecheck();
                }
            }
        }
    }
}
