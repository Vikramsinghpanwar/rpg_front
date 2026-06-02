using System;
using System.Collections.Generic;

namespace Features.DailyBonus.Models
{
    [Serializable]
    public class DailyBonusStatusResponse
    {
        public bool can_claim;
        public long bonus_amount_paisa;
        public string currency;
        public string next_claim_at;
        public string last_claim_at;
        public int current_streak;
        public string message;
    }

    [Serializable]
    public class DailyBonusClaimResponse
    {
        public string reward_id;
        public long amount_paisa;
        public string currency;
        public string claimed_at;
        public string reference_id;
        public bool replayed;
        public string message;
    }

    // UI Model - Pre-calculated day data for display
    public class DailyBonusDayData
    {
        public int dayNumber;
        public long amountPaisa;
        public bool isClaimed;
        public bool isToday;
        public bool isAvailable;
        public DateTime claimDate;
    }
}