using System;
using System.Collections.Generic;

namespace Core.VersionControl
{
    [Serializable]
    public sealed class VersionAnalyticsEvent
    {
        public string EventName { get; set; }
        public string CurrentVersion { get; set; }
        public string LatestVersion { get; set; }
        public string Platform { get; set; }
        public string BuildNumber { get; set; }
        public string Outcome { get; set; }
        public string Environment { get; set; }
        public string Locale { get; set; }
        public string UserId { get; set; }
        public string Error { get; set; }
        public string Country { get; set; }
        public string AppFlavor { get; set; }
        public System.DateTime TimestampUtc { get; set; }
    }
}
