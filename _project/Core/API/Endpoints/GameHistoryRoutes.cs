using System;

namespace Core.API.Endpoints
{
    public static class GameHistoryRoutes
    {
        public static readonly string Base = $"{ApiRoutes.V1}/player/game-history";

        public static string List(int limit, int offset, string gameType = null)
            => $"{Base}?{ApiQueryParams.Limit}={limit}&{ApiQueryParams.Offset}={offset}"
               + (string.IsNullOrEmpty(gameType) ? "" : $"&game_type={Uri.EscapeDataString(gameType)}");
    }
}
