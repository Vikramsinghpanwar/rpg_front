namespace Core.API.Endpoints
{
    // /api/v1/rewards/* — see USER_API_CONTRACTS.md > Rewards (daily, referral).
    public static class RewardRoutes
    {
        public static readonly string Base = $"{ApiRoutes.V1}/rewards";

        public static readonly string DailyStatus  = $"{Base}/daily/status";
        public static readonly string DailyClaim   = $"{Base}/daily/claim";

        public static readonly string ReferralMe   = $"{Base}/referrals/me";
        public static readonly string ReferralBind = $"{Base}/referrals/bind";
    }
}
