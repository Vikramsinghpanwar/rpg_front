using System;
using System.Collections.Generic;

namespace Features.Rewards.Models
{
    [Serializable]
    public class RewardHistoryItem
    {
        public string id;
        public string type;
        public string title;
        public long amount;
        public string status;
        public string walletType;
        public string referenceId;
        public string createdAt;
    }

    [Serializable]
    public class RewardHistoryResponse
    {
        public List<RewardHistoryItem> items;
        public Pagination pagination;
    }

    [Serializable]
    public class Pagination
    {
        public int page;
        public int limit;
        public int total;
        public bool hasNext;
    }

    [Serializable]
    public class ReferralMeResponse
    {
        public string referralCode;
        public List<ReferredUser> referredUsers;
    }

    [Serializable]
    public class ReferredUser
    {
        public string publicId;
        public string username;
        public string createdAt;
    }
}
