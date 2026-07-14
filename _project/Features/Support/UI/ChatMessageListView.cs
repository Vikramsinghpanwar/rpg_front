using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Features.Support.Models;
using Features.Support.Controllers;

namespace Features.Support.UI
{
    public class ChatMessageListView : MonoBehaviour
    {
        [SerializeField] private MessageItem messagePrefab;
        [SerializeField] private Transform messagesContainer;
        [SerializeField] private ScrollRect chatScrollRect;

        public void Clear()
        {
            if (messagesContainer == null) return;
            foreach (Transform child in messagesContainer)
            {
                if (child != null && child.gameObject != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void RenderMessages(TicketMessage[] messages, SupportController controller)
        {
            Clear();
            if (messages == null) return;

            foreach (var msg in messages)
            {
                var prefab = GetPrefabForSender(msg.SenderType);
                if (prefab != null && messagesContainer != null)
                {
                    MessageItem item = Instantiate(prefab, messagesContainer);
                    item.Setup(msg, controller);
                }
            }

            ScrollToBottom();
        }

        private MessageItem GetPrefabForSender(SupportSenderType senderType)
        {
            return senderType switch
            {
                SupportSenderType.User => messagePrefab,
                SupportSenderType.Admin => messagePrefab,
                // Easy to expand with new prefab references here in the future
                // SupportSenderType.System => systemMessagePrefab,
                // SupportSenderType.Moderator => moderatorMessagePrefab,
                // SupportSenderType.Bot => botMessagePrefab,
                _ => messagePrefab
            };
        }

        public void ScrollToBottom()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(DeferredScrollToBottom());
            }
        }

        private IEnumerator DeferredScrollToBottom()
        {
            // Wait for end of frame so layout recalculations complete
            yield return new WaitForEndOfFrame();

            if (chatScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                var rect = chatScrollRect.content;
                if (rect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                }
                chatScrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}
