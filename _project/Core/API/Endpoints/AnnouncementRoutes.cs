using System.Collections.Generic;

namespace Core.API.Endpoints
{
    // /api/v1/announcements/* — public, no auth. See USER_API_CONTRACTS.md > Public Announcements.
    public static class AnnouncementRoutes
    {
        public static readonly string Base = $"{ApiRoutes.V1}/announcements";

        public static readonly string PublicActiveBase = $"{Base}/public/active";

        public static string PublicActive(string region = null, string locale = null,
                                          string appVersion = null, string gameId = null,
                                          string segment = null)
        {
            var query = new Dictionary<string, string>
            {
                { ApiQueryParams.Region,     region },
                { ApiQueryParams.Locale,     locale },
                { ApiQueryParams.AppVersion, appVersion },
                { ApiQueryParams.GameId,     gameId },
                { ApiQueryParams.Segment,    segment },
            };
            return PublicActiveBase + ApiQueryParams.Build(query);
        }
    }
}
