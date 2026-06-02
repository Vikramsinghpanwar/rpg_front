namespace Core.API.Endpoints
{
    // /api/v1/wallets/* and /api/v1/transactions/* — see USER_API_CONTRACTS.md > Wallet & Balance.
    // Not currently exposed to the player through the gateway (the wallet block on
    // BootstrapResponse covers everything the UI needs), but kept here so a future
    // direct-exposure feature can use the canonical paths.
    public static class WalletRoutes
    {
        static readonly string WalletsBase      = $"{ApiRoutes.V1}/wallets";
        static readonly string TransactionsBase = $"{ApiRoutes.V1}/transactions";

        public static string Balance(string userId)     => $"{WalletsBase}/{userId}/balance";
        public static string WalletRows(string userId)  => $"{WalletsBase}/{userId}/wallets";
        public static string Transactions(string userId, int limit = 20, int offset = 0)
            => $"{TransactionsBase}/{userId}?{ApiQueryParams.Limit}={limit}&{ApiQueryParams.Offset}={offset}";
        public static string Locked(string userId)      => $"{TransactionsBase}/{userId}/locked";
        public static string LockEligibility(string userId, long amountPaisa)
            => $"{TransactionsBase}/{userId}/eligibility?amount={amountPaisa}";
    }
}
