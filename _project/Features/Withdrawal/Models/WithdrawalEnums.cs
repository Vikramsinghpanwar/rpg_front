namespace Features.Withdrawal.Models
{
    public static class WithdrawalStatus
    {
        public const string PENDING_REVIEW = "pending_review";
        public const string APPROVED = "approved";
        public const string PAYOUT_INITIATED = "payout_initiated";
        public const string PAYOUT_PROCESSING = "payout_processing";
        public const string PAYOUT_SUCCEEDED = "payout_succeeded";
        public const string REJECTED = "rejected";
        public const string PAYOUT_FAILED = "payout_failed";
        public const string CANCELLED_BY_USER = "cancelled_by_user";
        public const string EXPIRED = "expired";

        public static bool IsTerminal(string status)
        {
            return status == PAYOUT_SUCCEEDED || status == REJECTED || 
                   status == PAYOUT_FAILED || status == CANCELLED_BY_USER || 
                   status == EXPIRED;
        }

        public static string GetBadgeColor(string status)
        {
            switch (status)
            {
                case PENDING_REVIEW:
                case APPROVED:
                case PAYOUT_INITIATED:
                case PAYOUT_PROCESSING:
                    return "#FFA500"; // Amber
                case PAYOUT_SUCCEEDED:
                    return "#00FF00"; // Green
                case REJECTED:
                case PAYOUT_FAILED:
                    return "#FF4444"; // Red
                case CANCELLED_BY_USER:
                case EXPIRED:
                    return "#888888"; // Grey
                default:
                    return "#FFFFFF";
            }
        }

        public static string GetDisplayText(string status)
        {
            switch (status)
            {
                case PENDING_REVIEW: return "Pending Review";
                case APPROVED: return "Approved";
                case PAYOUT_INITIATED: return "Payout Initiated";
                case PAYOUT_PROCESSING: return "Processing";
                case PAYOUT_SUCCEEDED: return "Paid Out";
                case REJECTED: return "Rejected";
                case PAYOUT_FAILED: return "Payout Failed";
                case CANCELLED_BY_USER: return "Cancelled";
                case EXPIRED: return "Expired";
                default: return status;
            }
        }
    }

    public static class PayoutMethod
    {
        public const string UPI = "UPI";
        public const string BANK = "BANK";
    }
}