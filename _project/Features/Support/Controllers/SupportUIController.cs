using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Support.Models;
using Core.Managers;
using Features.Support.UI;

namespace Features.Support.Controllers
{
    public class SupportUIController : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject historyPanel;
        [SerializeField] private GameObject createPanel;
        [SerializeField] private GameObject detailPanel;

        [Header("History Panel")]
        [SerializeField] private Button viewHistoryButton;
        [SerializeField] private Transform ticketsContainer;
        [SerializeField] private GameObject ticketItemPrefab;
        [SerializeField] private TMP_Dropdown statusFilterDropdown;
        [SerializeField] private Button createNewButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private TMP_Text pageText;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;

        [Header("Create Ticket Panel")]
        [SerializeField] private TMP_InputField subjectInput;
        [SerializeField] private TMP_InputField bodyInput;
        [SerializeField] private TMP_Dropdown categoryDropdown;
        [SerializeField] private TMP_Dropdown priorityDropdown;
        [SerializeField] private Transform attachmentsContainer;
        [SerializeField] private GameObject attachmentItemPrefab;
        [SerializeField] private Button addAttachmentButton;
        [SerializeField] private Button submitButton;

        [Header("Ticket Detail Panel")]
        [SerializeField] private TMP_Text ticketSubjectText;
        [SerializeField] private TMP_Text ticketStatusText;
        [SerializeField] private TMP_Text ticketPriorityText;
        [SerializeField] private TMP_Text ticketCategoryText;
        [SerializeField] private TMP_Text ticketCreatedText;
        [SerializeField] private Transform messagesContainer;
        [SerializeField] private GameObject messageItemPrefab;
        [SerializeField] private TMP_InputField replyInput;
        [SerializeField] private Button sendReplyButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button refreshDetailButton;

        [Header("Loading/Error")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private TMP_Text emptyStateText;

        private SupportController controller;
        private List<TicketItem> currentTicketItems = new List<TicketItem>();
        private List<string> currentAttachmentUrls = new List<string>();
        private string currentTicketId;
        private int currentTicketVersion;
        private string currentDedupKey;
        private int currentPage = 1;
        private int totalPages = 1;
        private string currentFilter = null;

        public void Initialize(SupportController supportController)
        {
            controller = supportController;

            // Setup buttons
            viewHistoryButton?.onClick.AddListener(ShowHistoryPanel);
            createNewButton?.onClick.AddListener(ShowCreatePanel);
            refreshButton?.onClick.AddListener(RefreshTickets);
            submitButton?.onClick.AddListener(OnSubmitTicket);
            sendReplyButton?.onClick.AddListener(OnSendReply);
            backButton?.onClick.AddListener(ShowHistoryPanel);
            refreshDetailButton?.onClick.AddListener(RefreshCurrentTicket);
            addAttachmentButton?.onClick.AddListener(OnAddAttachment);
            prevPageButton?.onClick.AddListener(PreviousPage);
            nextPageButton?.onClick.AddListener(NextPage);

            // Setup dropdowns
            if (statusFilterDropdown != null)
            {
                statusFilterDropdown.ClearOptions();
                statusFilterDropdown.AddOptions(new List<string> { "All", "Open", "In Progress", "Awaiting Reply", "Resolved", "Closed" });
                statusFilterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }

            if (categoryDropdown != null)
            {
                categoryDropdown.ClearOptions();
                categoryDropdown.AddOptions(new List<string>
                {
                    controller.GetCategoryDisplayText(TicketCategory.ACCOUNT),
                    controller.GetCategoryDisplayText(TicketCategory.PAYMENT),
                    controller.GetCategoryDisplayText(TicketCategory.GAMEPLAY),
                    controller.GetCategoryDisplayText(TicketCategory.OTHER)
                });
            }

            if (priorityDropdown != null)
            {
                priorityDropdown.ClearOptions();
                priorityDropdown.AddOptions(new List<string>
                {
                    controller.GetPriorityDisplayText(TicketPriority.LOW),
                    controller.GetPriorityDisplayText(TicketPriority.MEDIUM),
                    controller.GetPriorityDisplayText(TicketPriority.HIGH),
                    controller.GetPriorityDisplayText(TicketPriority.URGENT)
                });
                priorityDropdown.value = 1; // Default to MEDIUM
            }

            ShowHistoryPanel();
        }

        private void OnFilterChanged(int index)
        {
            switch (index)
            {
                case 1: currentFilter = TicketStatus.OPEN; break;
                case 2: currentFilter = TicketStatus.IN_PROGRESS; break;
                case 3: currentFilter = TicketStatus.PENDING; break;
                case 4: currentFilter = TicketStatus.RESOLVED; break;
                case 5: currentFilter = TicketStatus.CLOSED; break;
                default: currentFilter = null; break;
            }
            currentPage = 1;
            RefreshTickets();
        }

        private async void RefreshTickets()
        {
            loadingOverlay?.SetActive(true);
            emptyStateText?.gameObject.SetActive(false);

            var response = await controller.GetMyTickets(currentPage, 20, currentFilter);

            loadingOverlay?.SetActive(false);

            if (response != null)
            {
                UpdateTicketsList(response.items);
                totalPages = response.meta?.totalPages ?? 1;
                UpdatePaginationUI();

                if (response.items == null || response.items.Length == 0)
                    emptyStateText?.gameObject.SetActive(true);
            }
        }

        private void UpdateTicketsList(TicketResponse[] tickets)
        {
            // Clear existing
            foreach (var item in currentTicketItems)
            {
                if (item != null && item.gameObject != null)
                    Destroy(item.gameObject);
            }
            currentTicketItems.Clear();

            if (tickets == null) return;

            foreach (var ticket in tickets)
            {
                var go = Instantiate(ticketItemPrefab, ticketsContainer);
                var item = go.GetComponent<TicketItem>();
                item.Setup(ticket, controller, OnTicketSelected);
                currentTicketItems.Add(item);
            }
        }

        private void OnTicketSelected(string ticketId)
        {
            currentTicketId = ticketId;
            _ = ShowTicketDetail(ticketId);
        }

        private async System.Threading.Tasks.Task ShowTicketDetail(string ticketId)
        {
            loadingOverlay?.SetActive(true);

            var detail = await controller.GetTicketDetail(ticketId);

            loadingOverlay?.SetActive(false);

            if (detail != null)
            {
                UpdateDetailUI(detail);
                ShowDetailPanel();
            }
        }

        private async void RefreshCurrentTicket()
        {
            if (string.IsNullOrEmpty(currentTicketId)) return;
            await ShowTicketDetail(currentTicketId);
        }

        private void UpdateDetailUI(TicketDetailResponse detail)
        {
            currentTicketVersion = detail.data.version;

            ticketSubjectText.text = detail.data.subject;
            ticketStatusText.text = controller.GetStatusDisplayText(detail.data.status);
            ticketStatusText.color = ColorUtility.TryParseHtmlString(controller.GetStatusColor(detail.data.status), out Color color) ? color : Color.white;
            ticketPriorityText.text = controller.GetPriorityDisplayText(detail.data.priority);
            ticketPriorityText.color = controller.GetPriorityColor(detail.data.priority);
            ticketCategoryText.text = controller.GetCategoryDisplayText(detail.data.category);
            ticketCreatedText.text = FormatDate(detail.data.createdAt);

            // Clear and rebuild messages
            foreach (Transform child in messagesContainer)
                Destroy(child.gameObject);

            if (detail.data.messages != null)
            {
                foreach (var msg in detail.data.messages)
                {
                    var go = Instantiate(messageItemPrefab, messagesContainer);
                    var item = go.GetComponent<MessageItem>();
                    item.Setup(msg, controller);
                }
            }

            // Clear reply input
            replyInput.text = "";
        }

        private async void OnSendReply()
        {
            if (string.IsNullOrEmpty(replyInput.text))
            {
                PopupManager.Instance.ShowError("Please enter a message");
                return;
            }

            var result = await controller.ReplyToTicket(
                currentTicketId,
                replyInput.text,
                currentAttachmentUrls.ToArray(),
                currentTicketVersion);

            switch (result)
            {
                case ReplyResult.Success:
                    currentAttachmentUrls.Clear();
                    UpdateAttachmentsUI();
                    await ShowTicketDetail(currentTicketId);
                    break;

                case ReplyResult.VersionConflict:
                    // New activity arrived between detail-load and our submit. Refetch
                    // (which updates currentTicketVersion) and let the user retry — we keep
                    // replyInput.text so they don't lose their typing.
                    PopupManager.Instance.Show(
                        "Ticket updated",
                        "New messages have arrived on this ticket. Please review and try sending again.",
                        "OK");
                    await ShowTicketDetail(currentTicketId);
                    break;

                case ReplyResult.TicketClosed:
                    await ShowTicketDetail(currentTicketId);
                    break;

                case ReplyResult.NetworkError:
                default:
                    break;
            }
        }

        private void ShowCreatePanel()
        {
            historyPanel?.SetActive(false);
            detailPanel?.SetActive(false);
            createPanel?.SetActive(true);

            subjectInput.text = "";
            bodyInput.text = "";
            categoryDropdown.value = 3; // OTHER
            priorityDropdown.value = 1; // MEDIUM
            currentAttachmentUrls.Clear();
            UpdateAttachmentsUI();

            // One dedup key per panel session. A double-tap on Submit reuses the same key,
            // so the server's 24h idempotency window collapses both attempts into one ticket.
            // Opening a fresh panel for a new ticket regenerates here.
            currentDedupKey = Guid.NewGuid().ToString();
        }

        private void ShowHistoryPanel()
        {
            createPanel?.SetActive(false);
            detailPanel?.SetActive(false);
            historyPanel?.SetActive(true);
            RefreshTickets();
        }

        private void ShowDetailPanel()
        {
            historyPanel?.SetActive(false);
            createPanel?.SetActive(false);
            detailPanel?.SetActive(true);
        }

        private async void OnSubmitTicket()
        {
            if (string.IsNullOrEmpty(subjectInput.text))
            {
                PopupManager.Instance.ShowError("Please enter a subject");
                return;
            }

            if (string.IsNullOrEmpty(bodyInput.text))
            {
                PopupManager.Instance.ShowError("Please describe your issue");
                return;
            }

            string category = GetCategoryValue(categoryDropdown.value);
            string priority = GetPriorityValue(priorityDropdown.value);

            var result = await controller.CreateTicket(
                subjectInput.text,
                bodyInput.text,
                category,
                priority,
                currentAttachmentUrls.ToArray(),
                currentDedupKey);

            if (result != null)
            {
                ShowHistoryPanel();
            }
        }

        private void OnAddAttachment()
        {
            // For MVP, just simulate adding a URL
            // In production, implement file picker and upload to cloud storage
            PopupManager.Instance.ShowInfo("File upload would go here\nIn production, use native file picker + cloud storage upload");

            // Simulate adding an attachment URL
            string mockUrl = $"https://example.com/uploads/{Guid.NewGuid()}.jpg";
            currentAttachmentUrls.Add(mockUrl);
            UpdateAttachmentsUI();
        }

        private void UpdateAttachmentsUI()
        {
            foreach (Transform child in attachmentsContainer)
                Destroy(child.gameObject);

            foreach (var url in currentAttachmentUrls)
            {
                var go = Instantiate(attachmentItemPrefab, attachmentsContainer);
                var text = go.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = System.IO.Path.GetFileName(url);

                var removeBtn = go.GetComponentInChildren<Button>();
                if (removeBtn != null)
                {
                    string urlCopy = url;
                    removeBtn.onClick.AddListener(() => RemoveAttachment(urlCopy));
                }
            }
        }

        private void RemoveAttachment(string url)
        {
            currentAttachmentUrls.Remove(url);
            UpdateAttachmentsUI();
        }

        private string GetCategoryValue(int dropdownIndex)
        {
            var displayText = categoryDropdown.options[dropdownIndex].text;
            if (displayText == controller.GetCategoryDisplayText(TicketCategory.ACCOUNT)) return TicketCategory.ACCOUNT;
            if (displayText == controller.GetCategoryDisplayText(TicketCategory.PAYMENT)) return TicketCategory.PAYMENT;
            if (displayText == controller.GetCategoryDisplayText(TicketCategory.GAMEPLAY)) return TicketCategory.GAMEPLAY;
            return TicketCategory.OTHER;
        }

        private string GetPriorityValue(int dropdownIndex)
        {
            var displayText = priorityDropdown.options[dropdownIndex].text;
            if (displayText == controller.GetPriorityDisplayText(TicketPriority.LOW)) return TicketPriority.LOW;
            if (displayText == controller.GetPriorityDisplayText(TicketPriority.MEDIUM)) return TicketPriority.MEDIUM;
            if (displayText == controller.GetPriorityDisplayText(TicketPriority.HIGH)) return TicketPriority.HIGH;
            return TicketPriority.URGENT;
        }

        private void PreviousPage()
        {
            if (currentPage > 1)
            {
                currentPage--;
                RefreshTickets();
            }
        }

        private void NextPage()
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                RefreshTickets();
            }
        }

        private void UpdatePaginationUI()
        {
            if (pageText != null)
                pageText.text = $"{currentPage} / {totalPages}";

            if (prevPageButton != null)
                prevPageButton.interactable = currentPage > 1;

            if (nextPageButton != null)
                nextPageButton.interactable = currentPage < totalPages;
        }

        private string FormatDate(string isoDate)
        {
            if (System.DateTime.TryParse(isoDate, out var dt))
                return dt.ToString("MMM dd, yyyy HH:mm");
            return isoDate;
        }
    }
}