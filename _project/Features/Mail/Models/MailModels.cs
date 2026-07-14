using System;
using System.Collections.Generic;

namespace Features.Mail.Models
{
    [Serializable]
    public class MailUnreadCountEnvelope
    {
        public bool success;
        public int statusCode;
        public MailUnreadCountData data;
    }

    [Serializable]
    public class MailUnreadCountData
    {
        public int unread;
    }

    [Serializable]
    public class MailUnreadCountResponse
    {
        public int unread;
    }

    [Serializable]
    public class MailListResponse
    {
        public bool success;
        public int statusCode;
        public MailItem[] data;
        public MailPaginationMeta meta;
    }

    [Serializable]
    public class MailPaginationMeta
    {
        public int page;
        public int limit;
        public int total;
        public int totalPages;
    }

    [Serializable]
    public class MailDetailResponse
    {
        public bool success;
        public int statusCode;
        public MailItem data;
    }

    [Serializable]
    public class MailItem
    {
        public string id;
        public string subject;
        public string body;
        public string audienceType;
        public bool isRead;
        public string readAt;
        public string createdAt;
        public object attachments;
        public object rewardAttachments;
    }

    [Serializable]
    public class MailAttachment
    {
        public string type;
        public string url;
        public string name;
    }

    [Serializable]
    public class MailRewardAttachment
    {
        public string type;
        public string rewardId;
        public long amount;
        public string description;
    }

    public static class MailAudienceType
    {
        public const string SINGLE = "SINGLE";
        public const string MULTI = "MULTI";
        public const string ALL = "ALL";
    }
}
