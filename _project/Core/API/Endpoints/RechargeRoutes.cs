namespace Core.API.Endpoints
{
    // Player recharge (/api/v1/player/recharge/*) is versioned;
    // the gateway-listing and webhook endpoints sit at /api/recharge/* (un-versioned).
    public static class RechargeRoutes
    {
        public static readonly string PlayerBase = $"{ApiRoutes.V1}/player/recharge";
        public static readonly string UnversionedBase = $"{ApiRoutes.Unversioned}/recharge";

        public static readonly string Create = PlayerBase;

        public static string List(int limit, int offset)
            => $"{PlayerBase}?{ApiQueryParams.Limit}={limit}&{ApiQueryParams.Offset}={offset}";

        public static string Detail(string id)   => $"{PlayerBase}/{id}";
        public static string Cancel(string id)   => $"{PlayerBase}/{id}/cancel";

        // source = "lobby" | "ingame" — see GatewayInfo discovery in contracts.
        public static string Gateways(string source)
            => string.IsNullOrEmpty(source)
                ? $"{UnversionedBase}/gateways"
                : $"{UnversionedBase}/gateways?{ApiQueryParams.Source}={source}";

        public static string Webhook(string gateway, string rechargeId, string status)
            => $"{UnversionedBase}/webhook/{gateway}?{ApiQueryParams.RechargeId}={rechargeId}&{ApiQueryParams.Status}={status}";
    }
}
