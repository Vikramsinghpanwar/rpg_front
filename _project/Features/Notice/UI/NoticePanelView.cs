using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Notice.Models;
using Features.Notice.Services;

namespace Features.Notice.UI
{
    public class NoticePanelView : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        [Header("List Panel")]
        [SerializeField] private Transform listContainer;
        [SerializeField] private GameObject listItemPrefab;
        [SerializeField] private ScrollRect listScrollRect;

        [Header("Detail Panel")]
        [SerializeField] private Image detailBannerImage;
        [SerializeField] private TextMeshProUGUI detailTitleText;
        [SerializeField] private TextMeshProUGUI detailContentText;
        [SerializeField] private GameObject detailImageContainer;
        [SerializeField] private GameObject detailContentContainer;

        [Header("Empty State")]
        [SerializeField] private GameObject emptyStatePanel;

        [Header("Loading")]
        [SerializeField] private GameObject loadingOverlay;

        private List<NoticeButtonItem> listItems = new List<NoticeButtonItem>();
        private Action<string> onNoticeSelected;
        private Sprite pendingPreviewSprite;

        void Awake()
        {
            // Debug.Log("[NoticePanelView] Awake: listContainer=" + (listContainer != null)
            //           + ", listItemPrefab=" + (listItemPrefab != null)
            //           + ", detailTitleText=" + (detailTitleText != null)
            //           + ", detailContentContainer=" + (detailContentContainer != null)
            //           + ", detailImageContainer=" + (detailImageContainer != null));
            HideLoading();
            Clear();
            CloseButtonSetup();
        }

        void CloseButtonSetup()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    Clear();
                    gameObject.SetActive(false);
                });
            }
        }

        public void SetNotices(List<NoticeItem> notices)
        {
            // Debug.Log("[notice-debug] NoticePanelView.SetNotices: count=" + (notices != null ? notices.Count : 0) +", listItemPrefab=" + (listItemPrefab != null) + ", listContainer=" + (listContainer != null));
            ClearList();

            if (notices == null || notices.Count == 0)
            {
                ShowEmptyState(true);
                return;
            }

            if (listItemPrefab == null)
            {
                Debug.LogError("[NoticePanelView] listItemPrefab is null — cannot populate notice list");
                ShowEmptyState(true);
                return;
            }

            if (listContainer == null)
            {
                Debug.LogError("[NoticePanelView] listContainer is null — cannot populate notice list");
                ShowEmptyState(true);
                return;
            }

            // Debug.Log($"[NoticePanelView] Populating notice list with {notices.Count} items; listContainer.parent.activeInHierarchy={listContainer?.parent?.activeInHierarchy}");

            ShowEmptyState(false);

            // Debug.Log("[notice-debug] NoticePanelView.SetNotices: emptyState hidden, iterating " + notices.Count + " notices");

            foreach (var notice in notices)
            {
                // Debug.Log("[notice-debug] NoticePanelView.SetNotices: instantiating item for notice id=" + notice.id);
                var itemGO = Instantiate(listItemPrefab, listContainer);
                var itemUI = itemGO.GetComponent<NoticeButtonItem>();

                if (itemUI != null)
                {
                    // Debug.Log("[notice-debug] NoticePanelView.SetNotices: NoticeButtonItem found, calling Setup");
                    itemUI.Setup(notice, OnListItemClicked);
                    listItems.Add(itemUI);
                    // Debug.Log("[notice-debug] NoticePanelView.SetNotices: added notice button: " + notice.title);
                }
                else
                {
                    Debug.LogError("[notice-debug] NoticePanelView.SetNotices: NoticeButtonItem component MISSING on listItemPrefab");
                }
            }

            if (listScrollRect != null)
            {
                listScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void OnListItemClicked(NoticeItem item)
        {
            Debug.Log($"[NoticePanelView] OnListItemClicked: id={item?.id}, title={item?.title}, onNoticeSelected={onNoticeSelected != null}");
            onNoticeSelected?.Invoke(item.id);
        }

        public void ShowNoticeDetail(NoticeDisplay display)
        {
            // Debug.Log("[notice-debug] ShowNoticeDetail ENTERED: display=" + (display != null) + ", gameObject.activeInHierarchy=" + gameObject.activeInHierarchy);
            if (display == null)
            {
                Debug.LogWarning("[notice-debug] ShowNoticeDetail: display is NULL");
                ClearDetail();
                return;
            }

            // Debug.Log("[notice-debug] ShowNoticeDetail: Title=" + display.Title +", ContentLen=" + (display.Content != null ? display.Content.Length : 0) + ", HasImage=" + display.HasImage + ", detailTitleText=" + (detailTitleText != null) + ", detailContentText=" + (detailContentText != null) + ", detailImageContainer=" + (detailImageContainer != null) + ", detailBannerImage=" + (detailBannerImage != null));

            if (detailTitleText != null)
            {
                detailTitleText.text = display.Title ?? string.Empty;
                // Debug.Log("[notice-debug] ShowNoticeDetail: set detailTitleText to '" + (display.Title ?? "") + "'");
            }
            else
            {
                Debug.LogWarning("[notice-debug] ShowNoticeDetail: detailTitleText is NULL");
            }

            if (detailContentText != null)
            {
                detailContentText.text = display.Content ?? string.Empty;
                // Debug.Log("[notice-debug] ShowNoticeDetail: set detailContentText, len=" + ((display.Content ?? "").Length));
            }
            else
            {
                Debug.LogWarning("[notice-debug] ShowNoticeDetail: detailContentText is NULL");
            }

            if (detailImageContainer != null)
            {
                detailImageContainer.SetActive(display.HasImage);
                // Debug.Log("[notice-debug] ShowNoticeDetail: detailImageContainer.SetActive(" + display.HasImage + ")");
            }
            else
            {
                Debug.LogWarning("[notice-debug] ShowNoticeDetail: detailImageContainer is NULL");
            }

            if (detailContentContainer != null)
            {
                bool hasContent = !string.IsNullOrEmpty(display.Content);
                detailContentContainer.SetActive(hasContent);
                // Debug.Log("[notice-debug] ShowNoticeDetail: detailContentContainer.SetActive(" + hasContent + ")");
            }
            else
            {
                Debug.LogWarning("[notice-debug] ShowNoticeDetail: detailContentContainer is NULL");
            }
        }

        public void SetBannerImage(Sprite sprite)
        {
            // Debug.Log("[notice-debug] SetBannerImage: sprite=" + (sprite != null) + ", detailBannerImage=" + (detailBannerImage != null));
            if (detailBannerImage != null)
            {
                detailBannerImage.sprite = sprite;
                if (detailImageContainer != null) detailImageContainer.SetActive(sprite != null);
            }
            else
            {
                Debug.LogWarning("[notice-debug] SetBannerImage: detailBannerImage is NULL");
            }
        }

        public void ShowPlaceholder()
        {
            // Debug.Log("[notice-debug] ShowPlaceholder: calling NoticeImageCache.LoadPlaceholder");
            SetBannerImage(NoticeImageCache.LoadPlaceholder());
        }

        public void SetOnNoticeSelected(Action<string> callback)
        {
            Debug.Log($"[NoticePanelView] SetOnNoticeSelected called: callback={callback != null}");
            onNoticeSelected = callback;
        }

        public void SetButtonSelected(string noticeId)
        {
            foreach (var item in listItems)
            {
                if (item == null) continue;
                bool selected = item.NoticeId == noticeId;
                item.SetSelected(selected);
            }
        }

        public void ShowLoading()
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(true);
        }

        public void HideLoading()
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(false);
        }

        private void ShowEmptyState(bool show)
        {
            if (emptyStatePanel != null) emptyStatePanel.SetActive(show);

            if (listContainer != null && show)
            {
                foreach (Transform child in listContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void ClearList()
        {
            foreach (var item in listItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            listItems.Clear();

            if (listContainer != null)
            {
                foreach (Transform child in listContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void ClearDetail()
        {
            if (detailTitleText != null) detailTitleText.text = string.Empty;
            if (detailContentText != null) detailContentText.text = string.Empty;
            if (detailBannerImage != null) detailBannerImage.sprite = null;
            if (detailImageContainer != null) detailImageContainer.SetActive(false);
            if (detailContentContainer != null) detailContentContainer.SetActive(false);
        }

        public void Clear()
        {
            ClearList();
            ClearDetail();
            ShowEmptyState(false);
        }

        void OnDestroy()
        {
            onNoticeSelected = null;
        }
    }
}
