namespace Core.API.Endpoints
{
    // /api/v1/support/tickets/* — see USER_API_CONTRACTS.md > Support Tickets (user).
    public static class SupportRoutes
    {
        public static readonly string Base = $"{ApiRoutes.V1}/support/tickets";

        public static readonly string Create = Base;
        public static string Detail(string id)   => $"{Base}/{id}";
        public static string Replies(string id)  => $"{Base}/{id}/replies";

        public static string Mine(int page, int limit, string status = null)
        {
            var url = $"{Base}/mine?{ApiQueryParams.Page}={page}&{ApiQueryParams.Limit}={limit}";
            if (!string.IsNullOrEmpty(status)) url += $"&{ApiQueryParams.Status}={status}";
            return url;
        }
    }
}
