namespace Core.API.Endpoints
{
    // /api/v1/mail/* — see API contracts: Mail (inbox, detail, read, delete, unread-count).
    public static class MailRoutes
    {
        public static readonly string Base = $"{ApiRoutes.V1}/mail";

        public static readonly string UnreadCount = $"{Base}/unread-count";

        public static string List(int page = 1, int limit = 20)
        {
            return $"{Base}?{ApiQueryParams.Page}={page}&{ApiQueryParams.Limit}={limit}";
        }

        public static string Detail(string id)
        {
            return $"{Base}/{id}";
        }

        public static string MarkRead(string id)
        {
            return $"{Base}/{id}/read";
        }

        public static string Delete(string id)
        {
            return $"{Base}/{id}";
        }
    }
}
