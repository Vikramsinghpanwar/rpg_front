using System;
using UnityEngine;

namespace Core.VersionControl
{
    public static class VersionLog
    {
        public static void Request(string id, VersionCheckRequest req)
        {
            if (req == null) return;
            Debug.Log($"[VC] Request id={id} platform={req.Platform} currentVersion={req.CurrentVersion} build={req.BuildNumber} env={req.Environment} userId={req.UserId} locale={req.Locale} country={req.Country} flavor={req.AppFlavor}");
        }

        public static void Response(string id, int statusCode, string body, DateTime startUtc)
        {
            var dt = DateTime.UtcNow - startUtc;
            Debug.Log($"[VC] Response id={id} status={statusCode} elapsedMs={dt.TotalMilliseconds:F0} body={body}");
        }

        public static void Success(string id, VersionCheckResponse resp, VersionCheckRequest req, DateTime startUtc)
        {
            var dt = DateTime.UtcNow - startUtc;
            Debug.Log($"[VC] Success id={id} outcome={resp?.Status} latest={resp?.LatestVersion} updateUrl={resp?.UpdateUrl} maintenanceUntil={resp?.MaintenanceUntil} elapsedMs={dt.TotalMilliseconds:F0}");
        }

        public static void Failed(string id, VersionCheckRequest req, string error, int statusCode, DateTime startUtc)
        {
            var dt = DateTime.UtcNow - startUtc;
            Debug.LogWarning($"[VC] Failed id={id} error={error} status={statusCode} elapsedMs={dt.TotalMilliseconds:F0}");
        }

        public static void OutcomeHandled(string id, string outcome)
        {
            Debug.Log($"[VC] Handled id={id} outcome={outcome}");
        }

        public static void ResumeRecheck(string id, string reason)
        {
            Debug.Log($"[VC] Resume recheck id={id} reason={reason}");
        }
    }
}
