namespace Core.API.Endpoints
{
    // /api/v1/player/withdrawals/* — see USER_API_CONTRACTS.md > Withdrawals (player).
    public static class WithdrawalRoutes
    {
        public static readonly string Base = $"{ApiRoutes.V1}/player/withdrawals";

        public static readonly string Create = Base;

        public static string List(int limit, int offset)
            => $"{Base}?{ApiQueryParams.Limit}={limit}&{ApiQueryParams.Offset}={offset}";

        public static string Detail(string id) => $"{Base}/{id}";
        public static string Cancel(string id) => $"{Base}/{id}/cancel";
    }
}
