using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Mail.Models;

namespace Features.Mail.UI
{
    public class MailDetailView : MonoBehaviour
    {
        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI subjectText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private TextMeshProUGUI createdAtText;
        [SerializeField] private TextMeshProUGUI attachmentsLabelText;
        [SerializeField] private TextMeshProUGUI rewardsLabelText;
        [SerializeField] private GameObject attachmentsContainer;
        [SerializeField] private GameObject rewardsContainer;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;

        public event System.Action OnClose;

        public void ShowDetail(MailItem item)
        {
            if (item == null)
            {
                Hide();
                return;
            }

            if (subjectText != null)
            {
                subjectText.text = item.subject ?? string.Empty;
            }

            if (bodyText != null)
            {
                bodyText.text = item.body ?? string.Empty;
            }

            if (createdAtText != null)
            {
                createdAtText.text = FormatCreatedAt(item.createdAt);
            }

            bool hasAttachment = MailUtils.HasAttachment(item.attachments);
            if (attachmentsContainer != null)
            {
                attachmentsContainer.SetActive(hasAttachment);
            }

            if (attachmentsLabelText != null && hasAttachment)
            {
                attachmentsLabelText.text = "Attachments: " + MailUtils.FormatAttachmentCount(item.attachments);
            }

            bool hasReward = MailUtils.HasAttachment(item.rewardAttachments);
            if (rewardsContainer != null)
            {
                rewardsContainer.SetActive(hasReward);
            }

            if (rewardsLabelText != null && hasReward)
            {
                rewardsLabelText.text = "Rewards: " + MailUtils.FormatAttachmentCount(item.rewardAttachments);
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static string FormatCreatedAt(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return string.Empty;

            if (System.DateTime.TryParse(dateString, out var date))
            {
                var utc = date.ToUniversalTime();
                return utc.ToString("MMM dd, yyyy");
            }

            return dateString;
        }

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => OnClose?.Invoke());
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }

            OnClose = null;
        }
    }
}
