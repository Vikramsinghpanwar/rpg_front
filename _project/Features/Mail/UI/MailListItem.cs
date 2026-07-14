using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Mail.Models;
using System;

namespace Features.Mail.UI
{
    public class MailListItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI subjectText;
        [SerializeField] private TextMeshProUGUI bodyPreviewText;
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI unreadBadgeText;
        [SerializeField] private GameObject unreadIndicator;
        [SerializeField] private GameObject attachmentIndicator;
        [SerializeField] private GameObject rewardIndicator;
        [SerializeField] private Button clickButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color unreadBackgroundColor;
        [SerializeField] private Color readBackgroundColor;

        private MailItem currentItem;
        private Action<MailItem> onClickCallback;

        public string MailId => currentItem?.id;
        public bool IsUnread => currentItem != null && !currentItem.isRead;

        private void Awake()
        {
            if (clickButton != null)
            {
                clickButton.onClick.AddListener(OnClick);
            }
        }

        public void Setup(MailItem item, Action<MailItem> callback)
        {
            currentItem = item;
            onClickCallback = callback;

            UpdateUI(item);
        }

        public void UpdateItem(MailItem updatedItem)
        {
            currentItem = updatedItem;
            UpdateUI(updatedItem);
        }

        private void UpdateUI(MailItem item)
        {
            if (item == null) return;

            if (subjectText != null)
            {
                subjectText.text = item.subject ?? string.Empty;
            }

            if (bodyPreviewText != null)
            {
                bodyPreviewText.text = GetPreviewText(item.body);
            }

            if (dateText != null)
            {
                dateText.text = FormatDateTime(item.createdAt);
            }

            bool isUnread = !item.isRead;

            if (unreadIndicator != null)
            {
                unreadIndicator.SetActive(isUnread);
            }

            if (unreadBadgeText != null)
            {
                unreadBadgeText.gameObject.SetActive(false);
            }

            if (backgroundImage != null && isUnread)
            {
                backgroundImage.color = unreadBackgroundColor;
            }
            else if (backgroundImage != null)
            {
                backgroundImage.color = readBackgroundColor;
            }

            bool hasAttachment = HasAttachment(item.attachments);
            if (attachmentIndicator != null)
            {
                attachmentIndicator.SetActive(hasAttachment);
            }

            bool hasReward = HasAttachment(item.rewardAttachments);
            if (rewardIndicator != null)
            {
                rewardIndicator.SetActive(hasReward);
            }
        }

        private void OnClick()
        {
            onClickCallback?.Invoke(currentItem);
        }

        private static string GetPreviewText(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return string.Empty;
            }

            if (body.Length > 80)
            {
                return body.Substring(0, 80) + "...";
            }

            return body;
        }

        private static string FormatDateTime(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return string.Empty;

            if (System.DateTime.TryParse(dateString, out var date))
            {
                var utc = date.ToUniversalTime();
                return utc.ToString("HH:mm\nMMM dd, yyyy");
            }

            return dateString;
        }

        private static bool HasAttachment(object attachment)
        {
            if (attachment == null) return false;

            if (attachment is Newtonsoft.Json.Linq.JArray array && array.Count > 0)
            {
                return true;
            }

            if (attachment is System.Collections.IEnumerable enumerable)
            {
                foreach (var _ in enumerable)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDestroy()
        {
            if (clickButton != null)
            {
                clickButton.onClick.RemoveListener(OnClick);
            }

            onClickCallback = null;
        }
    }
}
