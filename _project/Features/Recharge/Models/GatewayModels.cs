using System;
using Core.Utils;

namespace Features.Recharge.Models
{
    [Serializable]
    public class GatewayInfo
    {
        public string id;
        public string name;
        public string checkout_base_url;
        public bool enabled_lobby;
        public bool enabled_ingame;
        public string status;
    }

    // Pre-defined amounts - these would come from bootstrap in real implementation
    // For MVP, we'll hardcode common values with fallback to bootstrap
    [Serializable]
    public class RechargeAmountPreset
    {
        public long amount_paisa;
        public string display_text;
        public long bonus_amount_paisa;

        public RechargeAmountPreset(long amount, string display = null, long bonus = 0)
        {
            amount_paisa = amount;
            display_text = display ?? MoneyFormatter.FormatPaisaRoundedRupees(amount);
            bonus_amount_paisa = bonus;
        }
    }
}