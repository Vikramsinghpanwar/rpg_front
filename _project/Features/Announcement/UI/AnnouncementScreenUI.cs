using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Announcement.Models;
using System;

namespace Features.Announcement.UI
{
    public class AnnouncementScreenUI : MonoBehaviour
    {
        [Header("List Panel")]
        [SerializeField] private Transform listContainer;
        [SerializeField] private GameObject listItemPrefab;
        [SerializeField] private ScrollRect listScrollRect;
        
        [Header("Detail Panel")]
        [SerializeField] private Image detailImage;
        [SerializeField] private TextMeshProUGUI detailTitleText;
        [SerializeField] private TextMeshProUGUI detailContentText;
        [SerializeField] private GameObject detailImageContainer;
        [SerializeField] private GameObject detailContentContainer;
        
        [Header("Empty State")]
        [SerializeField] private GameObject emptyStatePanel;
        
        [Header("Loading")]
        [SerializeField] private GameObject loadingOverlay;
        
        private List<AnnouncementListItem> listItems = new List<AnnouncementListItem>();
        private Action<string> onAnnouncementSelected;
        
        private void Awake()
        {
            HideLoading();
            Clear();
        }
        
        public void SetAnnouncementList(List<AnnouncementItem> announcements)
        {
            ClearList();
            
            if (announcements == null || announcements.Count == 0)
            {
                ShowEmptyState(true);
                return;
            }
            
            ShowEmptyState(false);
            
            foreach (var announcement in announcements)
            {
                var itemGO = Instantiate(listItemPrefab, listContainer);
                var itemUI = itemGO.GetComponent<AnnouncementListItem>();
                
                if (itemUI != null)
                {
                    itemUI.Setup(announcement, OnListItemClicked);
                    listItems.Add(itemUI);
                }
            }
            
            // Reset scroll position
            if (listScrollRect != null)
            {
                listScrollRect.verticalNormalizedPosition = 1f;
            }
        }
        
        private void OnListItemClicked(AnnouncementItem item)
        {
            onAnnouncementSelected?.Invoke(item.id);
        }
        
        public void ShowAnnouncementDetail(AnnouncementDisplay display)
        {
            if (display == null || !display.HasContent)
            {
                ClearDetail();
                return;
            }

            if (detailTitleText != null) detailTitleText.text = display.Title;

            if (detailImageContainer != null) detailImageContainer.SetActive(false);
            if (detailContentContainer != null) detailContentContainer.SetActive(true);
            if (detailContentText != null) detailContentText.text = display.Content;
        }


        public void SetOnAnnouncementSelected(Action<string> callback)
        {
            onAnnouncementSelected = callback;
        }
        
        public void ShowLoading()
        {
            if (loadingOverlay != null)
                loadingOverlay.SetActive(true);
        }
        
        public void HideLoading()
        {
            if (loadingOverlay != null)
                loadingOverlay.SetActive(false);
        }
        
        private void ShowEmptyState(bool show)
        {
            if (emptyStatePanel != null)
                emptyStatePanel.SetActive(show);
            
            if (listContainer != null && show)
            {
                // Clear any existing items when empty
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
                if (item != null)
                    Destroy(item.gameObject);
            }
            listItems.Clear();
            
            foreach (Transform child in listContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        private void ClearDetail()
        {
            if (detailTitleText != null)
                detailTitleText.text = "";
            
            if (detailContentText != null)
                detailContentText.text = "";
            
            if (detailImage != null)
                detailImage.sprite = null;
            
            if (detailImageContainer != null)
                detailImageContainer.SetActive(false);
            
            if (detailContentContainer != null)
                detailContentContainer.SetActive(false);
        }
        
        public void Clear()
        {
            ClearList();
            ClearDetail();
            ShowEmptyState(false);
        }
        
        private void OnDestroy()
        {
            onAnnouncementSelected = null;
        }
    }
}