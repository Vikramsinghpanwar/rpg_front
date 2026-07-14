using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Notice.Models;
using System;

namespace Features.Notice.UI
{
    public class NoticeButtonItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button clickButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite unselectedSprite;

        private NoticeItem currentItem;
        private Action<NoticeItem> onClickCallback;

        void Awake()
        {
            // Debug.Log($"[NoticeButtonItem] Awake on {gameObject.name}: clickButton={clickButton != null}");
            if (clickButton != null)
                clickButton.onClick.AddListener(OnClick);
        }

        public void Setup(NoticeItem item, Action<NoticeItem> callback)
        {
            currentItem = item;
            onClickCallback = callback;

            // Debug.Log($"[NoticeButtonItem] Setup: id={item.id}, title={item.title}, titleTextRef={titleText != null}");

            if (titleText != null)
                titleText.text = item.title ?? string.Empty;
            else
                Debug.LogWarning("[NoticeButtonItem] titleText is null — button text will be empty");

            // Debug.Log($"[NoticeButtonItem] clickButton={clickButton != null}, selectedIndicator={selectedIndicator != null}");
        }

        public void SetSelected(bool selected)
        {

            if (backgroundImage != null)
            {
                if (selected && selectedSprite != null)
                    backgroundImage.sprite = selectedSprite;
                else if (!selected && unselectedSprite != null)
                    backgroundImage.sprite = unselectedSprite;
            }
        }

        public string NoticeId => currentItem != null ? currentItem.id : null;

        private void OnClick()
        {
            // Debug.Log($"[NoticeButtonItem] OnClick fired: id={currentItem?.id}, title={currentItem?.title}");
            onClickCallback?.Invoke(currentItem);
        }

        void OnDestroy()
        {
            if (clickButton != null)
                clickButton.onClick.RemoveListener(OnClick);

            // Debug.Log($"[NoticeButtonItem] Destroyed: id={currentItem?.id}");
        }
    }
}
