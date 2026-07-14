using System;
using System.Threading.Tasks;
using UnityEngine;
using Features.Support.Models;
using Core.API;
using Core.API.Endpoints;
using Core.Managers;
using System.Linq;

namespace Features.Support.Controllers
{
    public class SupportController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SupportUIController uiController;

        void Awake()
        {
            if (uiController == null) uiController = GetComponent<SupportUIController>();
        }

        void Start() => uiController?.Initialize(this);

        public async Task<TicketDetailResponse> GetTicketDetail(string ticketId)
        {
            try
            {
                return await ApiClient.Instance.Get<TicketDetailResponse>(SupportRoutes.Detail(ticketId));
            }
            catch (RateLimitedException e)
            {
                PopupManager.Instance?.ShowError($"Slow down — try again in {e.RetryAfterSeconds}s.");
                return null;
            }
            catch (ApiException e)
            {
                Debug.LogError($"GetTicketDetail error: {e.Message}");
                PopupManager.Instance?.ShowError("Failed to load ticket details");
                return null;
            }
        }

        public async Task<TicketListResponse> GetMyTickets(int page = 1, int limit = 20, string status = null)
        {
            try
            {
                return await ApiClient.Instance.Get<TicketListResponse>(SupportRoutes.Mine(page, limit, status));
            }
            catch (RateLimitedException e)
            {
                PopupManager.Instance?.ShowError($"Slow down — try again in {e.RetryAfterSeconds}s.");
                return null;
            }
            catch (ApiException e)
            {
                Debug.LogError($"GetMyTickets error: {e.Message}");
                PopupManager.Instance?.ShowError("Failed to load tickets");
                return null;
            }
        }

        public async Task<CreatedTicketResponse> CreateTicket(string subject, string body, string category,
            string priority, string[] attachmentMediaIds = null, string dedupKey = null)
        {
            LoadingManager.Instance?.Show("Creating ticket...");
            try
            {
                var request = new CreateTicketRequest
                {
                    subject = subject,
                    description = body,
                    category = category,
                    priority = priority ?? TicketPriority.MEDIUM,
                    attachmentMediaIds = attachmentMediaIds ?? Array.Empty<string>(),
                    dedupKey = dedupKey ?? Guid.NewGuid().ToString()
                };

                var response = await ApiClient.Instance.Post<CreatedTicketResponse>(SupportRoutes.Create, request);
                if (response != null) PopupManager.Instance?.ShowSuccess("Ticket created successfully!");
                else PopupManager.Instance?.ShowError("Failed to create ticket");
                return response;
            }
            catch (RateLimitedException e)
            {
                PopupManager.Instance?.ShowError($"Too many tickets — try again in {e.RetryAfterSeconds}s.");
                return null;
            }
            catch (ApiException e)
            {
                Debug.LogError($"CreateTicket error: {e.Message}");
                PopupManager.Instance?.ShowError("Failed to create ticket");
                return null;
            }
            finally
            {
                LoadingManager.Instance?.Hide();
            }
        }

        public async Task<ReplyResult> ReplyToTicket(string ticketId, string message, int version)
        {
            LoadingManager.Instance?.Show("Sending reply...");
            try
            {
                var request = new ReplyRequest
                {
                    body = message,
                };
                await ApiClient.Instance.Post<object>(SupportRoutes.Replies(ticketId), request);
                PopupManager.Instance?.ShowSuccess("Reply sent!");
                return ReplyResult.Success;
            }
            catch (ApiException e) when (e.StatusCode == 409)
            {
                return ReplyResult.VersionConflict;
            }
            catch (ApiException e) when (e.StatusCode == 400)
            {
                Debug.LogWarning($"ReplyToTicket 400: {e.Message}");
                PopupManager.Instance?.ShowError("This ticket is closed. You cannot reply.");
                return ReplyResult.TicketClosed;
            }
            catch (RateLimitedException e)
            {
                PopupManager.Instance?.ShowError($"Slow down — try again in {e.RetryAfterSeconds}s.");
                return ReplyResult.NetworkError;
            }
            catch (Exception e)
            {
                Debug.LogError($"ReplyToTicket error: {e.Message}");
                PopupManager.Instance?.ShowError("Network error sending reply");
                return ReplyResult.NetworkError;
            }
            finally
            {
                LoadingManager.Instance?.Hide();
            }
        }

        public string GetStatusColor(string status) => status switch
        {
            TicketStatus.OPEN or TicketStatus.IN_PROGRESS => "#FFA500",
            TicketStatus.PENDING => "#FFD700",
            TicketStatus.RESOLVED => "#00AA00",
            TicketStatus.CLOSED => "#888888",
            _ => "#FFFFFF"
        };

        public string GetStatusDisplayText(string status) => status switch
        {
            TicketStatus.OPEN => "Open",
            TicketStatus.PENDING => "Awaiting Reply",
            TicketStatus.IN_PROGRESS => "In Progress",
            TicketStatus.RESOLVED => "Resolved",
            TicketStatus.CLOSED => "Closed",
            _ => status
        };

        public string GetPriorityDisplayText(string priority) => priority switch
        {
            TicketPriority.LOW => "Low",
            TicketPriority.MEDIUM => "Medium",
            TicketPriority.HIGH => "High",
            TicketPriority.URGENT => "Urgent",
            _ => priority
        };

        public Color GetPriorityColor(string priority) => priority switch
        {
            TicketPriority.URGENT => new Color(0.9f, 0.2f, 0.2f),
            TicketPriority.HIGH => new Color(0.9f, 0.4f, 0.2f),
            TicketPriority.MEDIUM => new Color(1f, 0.6f, 0f),
            TicketPriority.LOW => new Color(0.4f, 0.8f, 0.4f),
            _ => Color.white
        };

        public string GetCategoryDisplayText(string category) => category switch
        {
            TicketCategory.ACCOUNT => "Account",
            TicketCategory.PAYMENT => "Payment",
            TicketCategory.GAMEPLAY => "Gameplay",
            TicketCategory.OTHER => "Other",
            _ => category
        };
    }
}
