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
        [SerializeField] private GameObject leftSpacer;
        [SerializeField] private GameObject rightSpacer;

        public void Setup(TicketMessage message, SupportController controller)
        {
            if (authorText != null)
            {
                authorText.text = message.IsUser ? "You" : (message.authorName ?? "Support");
            }

            if (bodyText != null)
            {
                bodyText.text = message.body;
            }

            if (timeText != null)
            {
                timeText.text = FormatTime(message.createdAt);
            }
            if (bgImage != null)
            {
                bgImage.color = message.IsUser ? userBg : agentBg;
            }
            if (leftSpacer != null && rightSpacer != null)
            {
                leftSpacer.SetActive(message.IsUser);
                rightSpacer.SetActive(!message.IsUser);
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