using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Support.Models;
using Features.Support.Controllers;

namespace Features.Support.UI
{
    public class MessageItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text authorText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private Color userBg;
        [SerializeField] private Color agentBg;
        [SerializeField] private Image bgImage;
        [SerializeField] private RectTransform contentRect;

        public void Setup(TicketMessage message, SupportController controller)
        {
            bool isUser = message.authorType == AuthorType.USER;

            authorText.text = isUser ? "You" : (message.authorName ?? "Support");
            bodyText.text = message.body;
            timeText.text = FormatTime(message.createdAt);

            if (bgImage != null)
            {
                bgImage.color = isUser ? userBg : agentBg;
            }

            // Adjust layout based on author
            if (contentRect != null)
            {
                var anchoredPos = contentRect.anchoredPosition;
                anchoredPos.x = isUser ? 100 : 20;
                contentRect.anchoredPosition = anchoredPos;
            }
        }

        private string FormatTime(string isoDate)
        {
            if (System.DateTime.TryParse(isoDate, out var dt))
                return dt.ToString("HH:mm");
            return isoDate;
        }
    }
}