using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Announcement.Models;
using System;

namespace Features.Announcement.UI
{
    public class AnnouncementListItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI typeBadgeText;
        [SerializeField] private Image typeBadgeBackground;
        [SerializeField] private Button clickButton;
        [SerializeField] private GameObject selectedIndicator;
        
        private AnnouncementItem currentItem;
        private Action<AnnouncementItem> onClickCallback;
        
        private void Awake()
        {
            if (clickButton != null)
                clickButton.onClick.AddListener(OnClick);
        }
        
        public void Setup(AnnouncementItem item, Action<AnnouncementItem> callback)
        {
            currentItem = item;
            onClickCallback = callback;
            
            if (titleText != null)
                titleText.text = item.title;
            
            if (typeBadgeText != null)
                typeBadgeText.text = FormatType(item.type);
            
            if (typeBadgeBackground != null)
            {
                // Color based on type
                var color = GetTypeColor(item.type);
                typeBadgeBackground.color = color;
            }
        }
        
        private string FormatType(string type)
        {
            return type switch
            {
                "MAINTENANCE" => "Maintenance",
                "EVENT" => "Event",
                "PROMO" => "Promotion",
                "SYSTEM" => "System",
                _ => type ?? "Notice"
            };
        }
        
        private Color GetTypeColor(string type)
        {
            return type switch
            {
                "MAINTENANCE" => new Color(0.9f, 0.3f, 0.2f), // Red
                "EVENT" => new Color(0.2f, 0.7f, 0.3f),      // Green
                "PROMO" => new Color(0.9f, 0.6f, 0.1f),      // Orange
                "SYSTEM" => new Color(0.3f, 0.5f, 0.9f),     // Blue
                _ => new Color(0.5f, 0.5f, 0.5f)              // Gray
            };
        }
        
        public void SetSelected(bool selected)
        {
            if (selectedIndicator != null)
                selectedIndicator.SetActive(selected);
        }
        
        private void OnClick()
        {
            onClickCallback?.Invoke(currentItem);
        }
        
        private void OnDestroy()
        {
            if (clickButton != null)
                clickButton.onClick.RemoveListener(OnClick);
        }
    }
}