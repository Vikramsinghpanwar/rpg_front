using System.Collections.Generic;

namespace Core.API
{
    // Canonical names for every HTTP header used by the platform. Features must build
    // header dicts using these constants — never typed string literals.
    public static class ApiHeaders
    {
        // Standard
        public const string Authorization = "Authorization";
        public const string ContentType   = "Content-Type";
        public const string Accept        = "Accept";

        // Idempotency (wallet-service POSTs require UUIDv4)
        public const string IdempotencyKey = "Idempotency-Key";

        // Client identity (forwarded by the gateway for telemetry / segmentation)
        public const string ClientVersion = "X-Client-Version";
        public const string Platform      = "X-Platform";
        public const string DeviceId      = "X-Device-ID";
        public const string RequestId     = "X-Request-Id";

        // Withdrawal preflight flags forwarded to wallet-service tiering
        public const string UserTier     = "X-User-Tier";
        public const string KycStatus    = "X-KYC-Status";
        public const string PanVerified  = "X-PAN-Verified";
        public const string BankVerified = "X-Bank-Verified";

        // Lobby surface
        public const string IfNoneMatch  = "If-None-Match";
        public const string UserSegments = "X-User-Segments";
        public const string LobbySchema  = "X-Lobby-Schema";

        // Common content-type values
        public const string ApplicationJson = "application/json";

        // ── Builders ────────────────────────────────────────────────────────────
        public static Dictionary<string, string> WithIdempotency(string key)
            => new Dictionary<string, string> { { IdempotencyKey, key } };

        public static Dictionary<string, string> WithBearer(string token)
            => new Dictionary<string, string> { { Authorization, $"Bearer {token}" } };
    }
}
