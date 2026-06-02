using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Support.Models;
using Features.Support.Controllers;

namespace Features.Support.UI
{
    public class TicketItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text subjectText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text priorityText;
        [SerializeField] private TMP_Text categoryText;
        [SerializeField] private TMP_Text previewText;
        [SerializeField] private Image statusIndicator;
        [SerializeField] private Button viewButton;
        
        private string ticketId;
        private SupportController controller;
        private System.Action<string> onSelected;
        
        public void Setup(TicketResponse ticket, SupportController supportController, System.Action<string> selectionCallback)
        {
            ticketId = ticket.id;
            controller = supportController;
            onSelected = selectionCallback;
            
            subjectText.text = ticket.subject;
            statusText.text = controller.GetStatusDisplayText(ticket.status);
            statusText.color = ColorUtility.TryParseHtmlString(controller.GetStatusColor(ticket.status), out Color color) ? color : Color.white;
            dateText.text = FormatDate(ticket.createdAt);
            priorityText.text = controller.GetPriorityDisplayText(ticket.priority);
            priorityText.color = controller.GetPriorityColor(ticket.priority);
            categoryText.text = controller.GetCategoryDisplayText(ticket.category);
            
            // Preview - truncate description
            string preview = ticket.description?.Length > 80 ? ticket.description.Substring(0, 80) + "..." : ticket.description;
            previewText.text = preview;
            
            // Status indicator dot
            if (statusIndicator != null)
                statusIndicator.color = statusText.color;
            
            // Wire up view button
            if (viewButton != null)
            {
                viewButton.onClick.RemoveAllListeners();
                viewButton.onClick.AddListener(() => onSelected?.Invoke(ticketId));
            }
            else
            {
                // Fallback: use root Button component if viewButton not assigned
                var rootButton = GetComponent<Button>();
                if (rootButton != null)
                {
                    rootButton.onClick.RemoveAllListeners();
                    rootButton.onClick.AddListener(() => onSelected?.Invoke(ticketId));
                }
            }
        }
        
        private string FormatDate(string isoDate)
        {
            if (System.DateTime.TryParse(isoDate, out var dt))
                return dt.ToString("MMM dd");
            return isoDate;
        }
    }
}