using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Features.Support.Models
{

    [JsonObject(MemberSerialization.OptIn)]
    public class SupportAttachmentDto
    {
        [JsonProperty("key")]
        public string Key;

        [JsonProperty("fileName")]
        public string FileName;

        [JsonProperty("mimeType")]
        public string MimeType;
    }

    [Serializable]
    public class CreateTicketRequest
    {
        public string subject;
        public string description;
        public string category;
        public string priority;
        public string[] attachmentMediaIds;
        public object metadata;
        public string dedupKey;
    }

    [Serializable]
    public class CreatedTicketResponse
    {
        public string id;
        public string status;
        public string createdAt;
        public string slaDeadline;
        public string priority;
    }



    [Serializable]
    public class TicketResponse
    {
        public string id;
        public string ticketNumber;
        public string subject;
        public string description;
        public string category;
        public string priority;
        public string status;
        public string createdAt;
        public string updatedAt;
        public int unreadMessages;
    }

    [Serializable]
    public class TicketDetailResponse
    {
        public bool success;
        public int statusCode;
        public TicketDetailData data;  // <-- Wrapper for the actual ticket
    }

    [Serializable]
    public class TicketDetailData
    {
        public string id;
        public string ticketNumber;
        public string subject;
        public string body;
        public string category;
        public string priority;
        public string status;
        public string createdAt;
        public string updatedAt;
        public int version;
        public string slaDeadline;
        public string firstResponseDeadline;
        public TicketMessage[] messages;
        public TicketTimelineEvent[] timeline;
    }

    public enum SupportSenderType
    {
        User,
        Admin,
        System,
        Moderator,
        Bot
    }

    [Serializable]
    public class TicketMessage
    {
        public string id;
        [JsonProperty("message")]      // API uses "message" not "body"
        public string body;
        [JsonProperty("senderType")]   // API uses "senderType" not "authorType"
        public string authorType;
        [JsonProperty("senderName")]
        public string authorName;
        public string createdAt;
        public SupportAttachmentDto[] attachments;
        public bool isInternal;

        public SupportSenderType SenderType
        {
            get
            {
                if (string.IsNullOrEmpty(authorType)) return SupportSenderType.Admin;
                return authorType.ToLowerInvariant() switch
                {
                    "user" => SupportSenderType.User,
                    "admin" => SupportSenderType.Admin,
                    "agent" => SupportSenderType.Admin,
                    "system" => SupportSenderType.System,
                    "moderator" => SupportSenderType.Moderator,
                    "bot" => SupportSenderType.Bot,
                    _ => SupportSenderType.Admin
                };
            }
        }

        public bool IsUser => SenderType == SupportSenderType.User;
    }
    public enum ReplyResult
    {
        Success,
        VersionConflict,
        TicketClosed,
        NetworkError
    }


    [Serializable]
    public class TicketTimelineEvent
    {
        public string type;
        public string description;
        public string createdAt;
        public string actor;
    }

    [Serializable]
    public class ReplyRequest
    {
        [JsonProperty("message")]
        public string body;
    }

    [Serializable]
    public class TicketListResponse
    {
        public bool success;
        public int statusCode;
        [JsonProperty("data")]
        public TicketResponse[] items;
        public PaginationMeta meta;
    }

    [Serializable]
    public class PaginationMeta
    {
        public int page;
        public int limit;
        public int total;
        public int totalPages;
    }

    // Enums as strings - match backend
    public static class TicketStatus
    {
        public const string OPEN = "OPEN";
        public const string PENDING = "PENDING";
        public const string IN_PROGRESS = "IN_PROGRESS";
        public const string RESOLVED = "RESOLVED";
        public const string CLOSED = "CLOSED";
    }

    public static class TicketPriority
    {
        public const string LOW = "LOW";
        public const string MEDIUM = "MEDIUM";
        public const string HIGH = "HIGH";
        public const string URGENT = "URGENT";
    }

    public static class TicketCategory
    {
        public const string ACCOUNT = "ACCOUNT_ISSUE";
        public const string PAYMENT = "PAYMENT_ISSUE";
        public const string GAMEPLAY = "GAMEPLAY_ISSUE";
        public const string OTHER = "OTHER";
    }

    public static class AuthorType
    {
        public const string USER = "user";
        public const string AGENT = "agent";
    }
}