using System;
using System.Threading.Tasks;
using Core;
using Core.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.VersionControl.UI
{
    public class UpdateAvailablePopup : MonoBehaviour
    {
        [Header("View Roots")]
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject cardBody;

        [Header("Text")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text versionText;
        [SerializeField] private TMP_Text releaseNotesText;
        [SerializeField] private Button updateButton;
        [SerializeField] private TMP_Text updateButtonLabel;
        [SerializeField] private Button laterButton;
        [SerializeField] private TMP_Text laterButtonLabel;
        [SerializeField] private ScrollRect releaseNotesScroll;

        VersionInfo currentInfo;
        bool visible;

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

            if (titleText != null) titleText.text = "New Update Available";
            if (versionText != null) versionText.text = $"Version {info.LatestVersion}";
            if (releaseNotesText != null) releaseNotesText.text = info.ReleaseNotes ?? "";
            if (releaseNotesScroll != null) releaseNotesScroll.verticalNormalizedPosition = 1f;
            if (updateButtonLabel != null) updateButtonLabel.text = "Update";
            if (laterButtonLabel != null) laterButtonLabel.text = "Later";

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
            if (updateButton != null) updateButton.onClick.RemoveListener(OnUpdateClicked);
            if (laterButton != null) laterButton.onClick.RemoveListener(OnLaterClicked);
            if (updateButton != null) updateButton.onClick.AddListener(OnUpdateClicked);
            if (laterButton != null) laterButton.onClick.AddListener(OnLaterClicked);
        }

        void UnwireButtons()
        {
            if (updateButton != null) updateButton.onClick.RemoveListener(OnUpdateClicked);
            if (laterButton != null) laterButton.onClick.RemoveListener(OnLaterClicked);
        }

        void OnUpdateClicked()
        {
            if (currentInfo == null) return;

            VersionLog.OutcomeHandled(Guid.NewGuid().ToString("N"), "update_clicked_soft");

            string targetUrl = !string.IsNullOrEmpty(currentInfo.UpdateUrl)
                ? currentInfo.UpdateUrl
                : Constants.APK_LANDING_PAGE_URL;

            try { Application.OpenURL(targetUrl); }
            catch (Exception ex) { Debug.LogWarning("[VC] OpenURL failed: " + ex.Message); }
        }

        void OnLaterClicked()
        {
            VersionLog.OutcomeHandled(Guid.NewGuid().ToString("N"), "later_clicked");
            Hide();
            if (VersionUIController.Instance != null)
                VersionUIController.Instance.CloseVersionGate();
        }
    }
}
