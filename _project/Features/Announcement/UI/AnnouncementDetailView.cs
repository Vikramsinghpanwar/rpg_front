using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Announcement.Models;

namespace Features.Announcement.UI
{
    // Text-only — the contract's PublicAnnouncementDto does not include any image field.
    public class AnnouncementDetailView : MonoBehaviour
    {
        [Header("Text Mode")]
        [SerializeField] private GameObject textModeContainer;
        [SerializeField] private TextMeshProUGUI contentTitleText;
        [SerializeField] private TextMeshProUGUI contentBodyText;

        [Header("Close Button")]
        [SerializeField] private Button closeButton;

        public event System.Action OnClose;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(() => OnClose?.Invoke());
        }

        public void ShowDetail(AnnouncementDisplay display)
        {
            if (display == null || !display.HasContent)
            {
                gameObject.SetActive(false);
                return;
            }

            textModeContainer?.SetActive(true);
            if (contentTitleText != null) contentTitleText.text = display.Title;
            if (contentBodyText != null) contentBodyText.text = display.Content;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();

            OnClose = null;
        }
    }
}