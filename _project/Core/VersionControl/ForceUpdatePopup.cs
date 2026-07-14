using System;
using System.Threading.Tasks;
using Core;
using Core.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.VersionControl.UI
{
    public class ForceUpdatePopup : MonoBehaviour
    {
        [Header("View Roots")]
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject cardBody;

        [Header("Text")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text versionText;
        [SerializeField] private Button updateButton;
        [SerializeField] private TMP_Text updateButtonLabel;

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

            if (titleText != null) titleText.text = "Update Required";
            if (messageText != null) messageText.text = "A newer version of the game is required.\nPlease update to continue.";
            if (versionText != null)
            {
                versionText.text = !string.IsNullOrEmpty(info.LatestVersion)
                    ? $"Update to {info.LatestVersion}"
                    : string.Empty;
                versionText.gameObject.SetActive(!string.IsNullOrEmpty(info.LatestVersion));
            }
            if (updateButtonLabel != null) updateButtonLabel.text = "Update";

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
            if (updateButton != null) updateButton.onClick.AddListener(OnUpdateClicked);
        }

        void UnwireButtons()
        {
            if (updateButton != null) updateButton.onClick.RemoveListener(OnUpdateClicked);
        }

        void OnUpdateClicked()
        {
            if (currentInfo == null) return;

            VersionLog.OutcomeHandled(Guid.NewGuid().ToString("N"), "update_clicked_force");

            string targetUrl = !string.IsNullOrEmpty(currentInfo.UpdateUrl)
                ? currentInfo.UpdateUrl
                : Constants.APK_LANDING_PAGE_URL;

            try { Application.OpenURL(targetUrl); }
            catch (Exception ex) { Debug.LogWarning("[VC] OpenURL failed: " + ex.Message); }
        }
    }
}
