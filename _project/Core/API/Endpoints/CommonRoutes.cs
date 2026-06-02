namespace Core.API.Endpoints
{
    // Endpoints that don't belong to a single domain — public version-check, matchmaking
    // entry, etc. See USER_API_CONTRACTS.md > Public Lobby & Version / Play.
    public static class CommonRoutes
    {
        // Version check (public, no auth, IP-throttled).
        public static readonly string VersionCheck = $"{ApiRoutes.V1}/version-control/public/check";

        // Lobby bootstrap surface (config-service direct). Not currently mounted on the
        // gateway — players reach this through BootstrapRoutes.Player.
        public static readonly string LobbyBootstrap = $"{ApiRoutes.V1}/lobby/bootstrap";

        // Matchmaking entry: POST /play/join (NOT under /api/v1).
        public static readonly string PlayJoin = $"{ApiRoutes.Play}/join";
    }
}
