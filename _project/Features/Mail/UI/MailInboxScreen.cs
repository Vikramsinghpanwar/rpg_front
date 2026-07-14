using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Mail.Models;
using Features.Mail.Controllers;
using System.Threading.Tasks;

namespace Features.Mail.UI
{
    public class MailInboxScreen : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] public MailController mailController;

        [Header("List Panel")]
        [SerializeField] private Transform mailListContainer;
        [SerializeField] private GameObject mailListItemPrefab;
        [SerializeField] private ScrollRect mailListScrollRect;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private TMP_Text pageInfoText;

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private TextMeshProUGUI detailSubjectText;
        [SerializeField] private TextMeshProUGUI detailBodyText;
        [SerializeField] private TextMeshProUGUI detailCreatedAtText;
        [SerializeField] private GameObject detailAttachmentsContainer;
        [SerializeField] private TextMeshProUGUI detailAttachmentsText;
        [SerializeField] private GameObject detailRewardsContainer;
        [SerializeField] private TextMeshProUGUI detailRewardsText;
        [SerializeField] private Button markReadButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button closeDetailButton;

        [Header("Empty State")]
        [SerializeField] private GameObject emptyStatePanel;

        [Header("Category Toggle")]
        [SerializeField] private Button broadcastToggleButton;
        [SerializeField] private Button personalToggleButton;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite unselectedSprite;

        [Header("Loading")]
        [SerializeField] private GameObject loadingOverlay;

        private List<MailListItem> listItems = new List<MailListItem>();
        private Action<MailItem> onMailSelected;
        private string currentCategory = MailAudienceType.ALL;

        void Awake()
        {
            if (mailController == null)
            {
                mailController = FindObjectOfType<MailController>();
                if (mailController == null)
                {
                    var go = new GameObject("MailController");
                    mailController = go.AddComponent<MailController>();
                    mailController.inboxScreenUI = this;
                }
            }

            if (mailController != null && mailController.inboxScreenUI == null)
            {
                mailController.inboxScreenUI = this;
            }

            HideLoading();
            Clear();
        }

        void Start()
        {
            if (prevPageButton != null)
            {
                prevPageButton.onClick.AddListener(OnPrevPageClicked);
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick.AddListener(OnNextPageClicked);
            }

            if (markReadButton != null)
            {
                markReadButton.onClick.AddListener(OnMarkReadClicked);
            }

            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(OnDeleteClicked);
            }

            if (closeDetailButton != null)
            {
                closeDetailButton.onClick.AddListener(OnCloseDetailClicked);
            }

            if (broadcastToggleButton != null)
            {
                broadcastToggleButton.onClick.AddListener(switchToBroadcast);
            }

            if (personalToggleButton != null)
            {
                personalToggleButton.onClick.AddListener(switchToPersonal);
            }
        }

        void OnEnable()
        {
            switchToBroadcast();
            if (mailController != null)
            {
                mailController.OnMailsLoaded += OnMailsLoaded;
                mailController.OnMailSelected += OnMailDetailSelected;
                mailController.OnUnreadCountUpdated += OnUnreadCountUpdated;
                mailController.OnMailMarkedAsRead += OnMailMarkedAsReadPreview;
                mailController.OnMailDeleted += OnMailDeletedPreview;
            }

            UpdatePageNavState(mailController != null ? 1 : 0, 0);
        }

        void OnDisable()
        {
            if (mailController != null)
            {
                mailController.OnMailsLoaded -= OnMailsLoaded;
                mailController.OnMailSelected -= OnMailDetailSelected;
                mailController.OnUnreadCountUpdated -= OnUnreadCountUpdated;
                mailController.OnMailMarkedAsRead -= OnMailMarkedAsReadPreview;
                mailController.OnMailDeleted -= OnMailDeletedPreview;
            }
        }

        public void UpdatePageNavState(int currentPage, int totalPages)
        {
            if (prevPageButton != null)
            {
                prevPageButton.gameObject.SetActive(currentPage > 1);
            }

            if (nextPageButton != null)
            {
                nextPageButton.gameObject.SetActive(currentPage > 0 && currentPage < totalPages);
            }

            if (pageInfoText != null)
            {
                if (totalPages > 0)
                {
                    pageInfoText.text = $"Page {currentPage} of {totalPages}";
                }
                else
                {
                    pageInfoText.text = string.Empty;
                }
            }
        }

        private void OnPrevPageClicked()
        {
            if (mailController != null)
            {
                _ = mailController.GoToPrevPage();
            }
        }

        private void OnNextPageClicked()
        {
            if (mailController != null)
            {
                _ = mailController.GoToNextPage();
            }
        }

        private void switchToBroadcast()
        {
            currentCategory = MailAudienceType.ALL;
            updateCategoryToggles();
            refreshMailList();
        }

        private void switchToPersonal()
        {
            currentCategory = MailAudienceType.SINGLE;
            updateCategoryToggles();
            refreshMailList();
        }

        private void updateCategoryToggles()
        {
            bool isBroadcast = currentCategory == MailAudienceType.ALL;
            if (broadcastToggleButton != null) broadcastToggleButton.image.sprite = isBroadcast && selectedSprite != null ? selectedSprite : unselectedSprite;
            if (personalToggleButton != null) personalToggleButton.image.sprite = !isBroadcast && selectedSprite != null ? selectedSprite : unselectedSprite;
        }

        private void refreshMailList()
        {
            if (mailController == null) return;
            var allMails = mailController.AllMails;
            SetMailList(allMails);
        }

        void OnMailsLoaded(List<MailItem> mails)
        {
            SetMailList(mails);
        }

        void OnMailDetailSelected(MailItem item)
        {
            ShowMailDetail(item);
        }

        void OnUnreadCountUpdated(int count)
        {
        }

        void OnMailMarkedAsReadPreview(string mailId)
        {
            _ = RefreshUnreadBadge();
        }

        void OnMailDeletedPreview(string mailId)
        {
            _ = RefreshUnreadBadge();
        }

        public async Task RefreshUnreadBadge()
        {
            if (mailController != null)
            {
                await mailController.LoadUnreadCount();
            }
        }

        public void SetMailList(IReadOnlyList<MailItem> mails)
        {
            ClearList();

            var filtered = filterByCategory(mails);

            if (filtered == null || filtered.Count == 0)
            {
                Debug.LogWarning("[MailInboxScreen] SetMailList received null or empty list");
                ShowEmptyState(true);
                return;
            }

            ShowEmptyState(false);

            if (mailListContainer == null)
            {
                Debug.LogError("[MailInboxScreen] mailListContainer is null. Assign in Inspector.");
                return;
            }

            if (mailListItemPrefab == null)
            {
                Debug.LogError("[MailInboxScreen] mailListItemPrefab is null. Assign in Inspector.");
                return;
            }

            Debug.Log($"[MailInboxScreen] Setting mail list with {filtered.Count} items");

            foreach (var mail in filtered)
            {
                var itemGO = Instantiate(mailListItemPrefab, mailListContainer);
                var itemUI = itemGO.GetComponent<MailListItem>();

                if (itemUI != null)
                {
                    itemUI.Setup(mail, OnListItemClicked);
                    listItems.Add(itemUI);
                }
                else
                {
                    Debug.LogWarning("[MailInboxScreen] MailListItem component not found on prefab");
                }
            }

            Debug.Log($"[MailInboxScreen] Created {listItems.Count} list items");

            if (mailListScrollRect != null)
            {
                mailListScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private List<MailItem> filterByCategory(IReadOnlyList<MailItem> mails)
        {
            if (string.IsNullOrEmpty(currentCategory) || currentCategory == MailAudienceType.MULTI || mails == null)
                return mails != null ? new List<MailItem>(mails) : new List<MailItem>();

            return mails.Where(m => m != null && m.audienceType == currentCategory).ToList();
        }

        public void ShowMailDetail(MailItem item)
        {
            if (item == null)
            {
                ClearMailDetail();
                return;
            }

            if (detailSubjectText != null)
            {
                detailSubjectText.text = item.subject ?? string.Empty;
            }

            if (detailBodyText != null)
            {
                detailBodyText.text = item.body ?? string.Empty;
            }

            if (detailCreatedAtText != null)
            {
                detailCreatedAtText.text = FormatCreatedAt(item.createdAt);
            }

            bool hasAttachment = MailUtils.HasAttachment(item.attachments);
            bool hasRewards = MailUtils.HasAttachment(item.rewardAttachments);

            if (detailAttachmentsContainer != null)
            {
                detailAttachmentsContainer.SetActive(hasAttachment);
            }

            if (detailAttachmentsText != null && hasAttachment)
            {
                detailAttachmentsText.text = "Attachments: " + MailUtils.FormatAttachmentCount(item.attachments);
            }

            if (detailRewardsContainer != null)
            {
                detailRewardsContainer.SetActive(hasRewards);
            }

            if (detailRewardsText != null && hasRewards)
            {
                detailRewardsText.text = "Rewards: " + MailUtils.FormatAttachmentCount(item.rewardAttachments);
            }

            if (detailPanel != null)
            {
                detailPanel.SetActive(true);
            }

            if (markReadButton != null)
            {
                markReadButton.gameObject.SetActive(!item.isRead);
            }
        }

        public void ClearMailDetail()
        {
            if (detailSubjectText != null)
            {
                detailSubjectText.text = string.Empty;
            }

            if (detailBodyText != null)
            {
                detailBodyText.text = string.Empty;
            }

            if (detailCreatedAtText != null)
            {
                detailCreatedAtText.text = string.Empty;
            }

            if (detailAttachmentsContainer != null)
            {
                detailAttachmentsContainer.SetActive(false);
            }

            if (detailRewardsContainer != null)
            {
                detailRewardsContainer.SetActive(false);
            }

            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
        }

        public void UpdateListItem(int index, MailItem updatedItem)
        {
            if (index < 0 || index >= listItems.Count) return;

            var itemUI = listItems[index];
            if (itemUI != null)
            {
                itemUI.UpdateItem(updatedItem);
            }
        }

        public void RemoveListItem(int index)
        {
            if (index < 0 || index >= listItems.Count) return;

            var itemUI = listItems[index];
            if (itemUI != null)
            {
                Destroy(itemUI.gameObject);
            }

            listItems.RemoveAt(index);

            SetMailList(mailController.AllMails);
        }

        public void ShowEmptyState(bool show)
        {
            if (emptyStatePanel != null)
            {
                emptyStatePanel.SetActive(show);
            }

            if (mailListContainer != null && show)
            {
                foreach (Transform child in mailListContainer)
                {
                    if (child != null) Destroy(child.gameObject);
                }
            }
        }

        public void ShowLoading()
        {
            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(true);
            }
        }

        public void HideLoading()
        {
            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(false);
            }
        }

        public void Clear()
        {
            ClearList();
            ClearMailDetail();
            ShowEmptyState(false);
        }

        private void ClearList()
        {
            foreach (var item in listItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }

            listItems.Clear();

            if (mailListContainer != null)
            {
                foreach (Transform child in mailListContainer)
                {
                    if (child != null) Destroy(child.gameObject);
                }
            }
        }

        private async void OnListItemClicked(MailItem item)
        {
            if (item == null) return;

            if (mailController != null)
            {
                mailController.LoadMailDetail(item.id);
            }

            if (!item.isRead && mailController != null)
            {
                await mailController.MarkAsRead(item.id);
            }
        }

        public void SetOnMailSelected(Action<MailItem> callback)
        {
            onMailSelected = callback;
        }

        private async void OnMarkReadClicked()
        {
            if (mailController != null && mailController.CurrentMail != null)
            {
                await mailController.MarkAsRead(mailController.CurrentMail.id);
            }
        }

        private async void OnDeleteClicked()
        {
            if (mailController != null && mailController.CurrentMail != null)
            {
                bool deleted = await mailController.DeleteMail(mailController.CurrentMail.id);
                if (deleted)
                {
                    ClearMailDetail();
                }
            }
        }

        private void OnCloseDetailClicked()
        {
            ClearMailDetail();
        }

        private static string FormatCreatedAt(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return string.Empty;

            if (DateTime.TryParse(dateString, out var date))
            {
                var utc = date.ToUniversalTime();
                return utc.ToString("MMM dd, yyyy");
            }

            return dateString;
        }

        private void OnDestroy()
        {
            if (markReadButton != null)
            {
                markReadButton.onClick.RemoveListener(OnMarkReadClicked);
            }

            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveListener(OnDeleteClicked);
            }

            if (closeDetailButton != null)
            {
                closeDetailButton.onClick.RemoveListener(OnCloseDetailClicked);
            }

            onMailSelected = null;
        }
    }
}
