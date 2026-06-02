using System.Collections.Generic;
using System.Text;

namespace Core.API
{
    // Canonical names for query-string parameters + a tiny builder. Endpoint methods that
    // accept pagination / filtering use these constants so we never end up with both
    // "page_size" and "limit" in the wild.
    public static class ApiQueryParams
    {
        public const string Limit  = "limit";
        public const string Offset = "offset";
        public const string Cursor = "cursor";
        public const string Page   = "page";
        public const string Status = "status";
        public const string Source = "source";

        // Lobby / version-check
        public const string Platform       = "platform";
        public const string AppVersion     = "appVersion";
        public const string Region         = "region";
        public const string Locale         = "locale";
        public const string DeviceType     = "deviceType";
        public const string CurrentVersion = "currentVersion";
        public const string UserId         = "userId";
        public const string Segment        = "segment";
        public const string GameId         = "gameId";

        // Recharge webhook
        public const string RechargeId = "recharge_id";

        // Build "?k1=v1&k2=v2" or "" when no non-empty values are supplied. Skips
        // null/empty values so the caller can pass optional filters without branching.
        public static string Build(IDictionary<string, string> kv)
        {
            if (kv == null || kv.Count == 0) return string.Empty;
            var sb = new StringBuilder();
            bool first = true;
            foreach (var p in kv)
            {
                if (string.IsNullOrEmpty(p.Value)) continue;
                sb.Append(first ? '?' : '&');
                sb.Append(System.Uri.EscapeDataString(p.Key));
                sb.Append('=');
                sb.Append(System.Uri.EscapeDataString(p.Value));
                first = false;
            }
            return sb.ToString();
        }
    }
}
