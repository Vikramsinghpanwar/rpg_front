namespace Core.API.Endpoints
{
    // CGS (Casino Game Service) — bets are HTTP-authoritative.
    // Socket.IO is notify-only; never emit bets or money operations through the socket.
    public static class CGSRoutes
    {
        const string CgsBase = "/cgs";

        // POST — place a bet. Requires Idempotency-Key header (UUID per logical bet attempt).
        public static readonly string PlaceBet = $"{CgsBase}/bets";

        // POST — cash out an active multiplier bet (Crash, Aviator).
        public static readonly string Cashout  = $"{CgsBase}/cashout";
    }
}
