using System;

namespace Core.VersionControl
{
    [Serializable]
    public class VersionInfo
    {
        public VersionStatus Status { get; set; }
        public string LatestVersion { get; set; }
        public string UpdateUrl { get; set; }
        public string ReleaseNotes { get; set; }
        public string MaintenanceMessage { get; set; }
        public string MaintenanceUntil { get; set; }
        public bool CanContinue { get; set; }
        public System.DateTime LastCheckTimeUtc { get; set; }
        public string Outcome { get; set; }
    }
}
