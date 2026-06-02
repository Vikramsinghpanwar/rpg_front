using System;
using System.Collections.Generic;

namespace Features.Announcement.Models
{
    // Wire shape used by both the lobby slice (BootstrapResponse.lobby.announcements) and
    // the public-active endpoint (PublicAnnouncementDto). Kept thin — frontend only needs
    // these fields to render. See USER_API_CONTRACTS.md > Public Announcements.
    [Serializable]
    public class AnnouncementItem
    {
        public string id;
        public string title;
        public string content;
        public string type;
        public string displayType;
        public int priority;
        public bool dismissible;
        public string ctaLabel;
        public string ctaUrl;
        public string startTime;
        public string endTime;
    }

    // Minimal slice extracted from BootstrapResponse.lobby (which is opaque object).
    // We deliberately do NOT mirror the full lobby DTO — each feature owns its own slice.
    [Serializable]
    public class LobbyAnnouncementsSlice
    {
        public List<AnnouncementItem> announcements;
    }

    public class AnnouncementDisplay
    {
        public string Id;
        public string Title;
        public string Content;
        public string Type;
        public bool HasContent => !string.IsNullOrEmpty(Content);
        public bool IsValid => HasContent;
    }
}
