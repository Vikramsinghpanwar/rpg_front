using System;
using System.Collections.Generic;

namespace Features.Notice.Models
{
    [Serializable]
    public class NoticeImage
    {
        public string url;
        public int sortOrder;
    }

    [Serializable]
    public class NoticeItem
    {
        public string id;
        public string title;
        public string content;
        public List<NoticeImage> images;
    }

    [Serializable]
    public class LobbyNoticesSlice
    {
        public List<NoticeItem> notices;
    }

    public class NoticeDisplay
    {
        public string Id;
        public string Title;
        public string Content;
        public string ImageUrl;
        public bool HasImage => !string.IsNullOrEmpty(ImageUrl);
    }
}
