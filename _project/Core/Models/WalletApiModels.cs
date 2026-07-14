namespace Core.Models
{
    public class WalletBalanceResponse
    {
        public string user_id;
        public long deposit_balance;
        public long win_balance;
        public long bonus_balance;
        public long withdrawable_amount;
        public long available_balance;
        public long locked_balance;
        public long total_balance;
        public string currency;
    }

    public class WalletRow
    {
        public string id;
        public string user_id;
        public string wallet_type;
        public long balance;
        public string status;
        public int version;
        public string created_at;
        public string updated_at;
    }
}
