namespace Core.API
{
    // Top-level path prefixes. Every concrete endpoint in Core/API/Endpoints/ composes from
    // these — so a future API version bump or path-prefix change is a one-line edit.
    public static class ApiRoutes
    {
        // /api/v1 — the versioned surface (auth, bootstrap, wallet, rewards, support, ...)
        public static readonly string V1 = $"/api/{ApiConfig.ApiVersion}";

        // /api — the un-versioned surface (recharge gateways, webhooks). Documented in
        // USER_API_CONTRACTS.md as path inconsistencies the player app must accept.
        public const string Unversioned = "/api";

        // /play — top-level matchmaking entry. Not under /api/v1.
        public const string Play = "/play";
    }
}
