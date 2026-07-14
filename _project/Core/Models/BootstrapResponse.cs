using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Features.DailyBonus.Models;

namespace Core.Models
{
    [Serializable]
    public class BootstrapResponse
    {
        public BootstrapProfile profile;
        public BootstrapWallet wallet;
        public object notices;
        public BootstrapLobby lobby;
        public WalletConfigResponse wallet_config;
        public string server_time;
        public Dictionary<string, string> errors;
        public GamesConfigResponse games;
        public PayoutMethods payout_methods;
        public string referral_code;
        public List<Features.Rewards.Models.ReferredUser> referred_users;
        public BootstrapRewards rewards;
    }

    [Serializable]
    public class BootstrapLobby
    {
        [JsonProperty("success")]
        public bool success { get; set; }

        [JsonProperty("data")]
        public BootstrapLobbyData data { get; set; }
    }

    [Serializable]
    public class BootstrapLobbyData
    {
        [JsonProperty("schemaVersion")]
        public int schemaVersion { get; set; }

        [JsonProperty("etag")]
        public string etag { get; set; }

        [JsonProperty("generatedAt")]
        public string generatedAt { get; set; }

        [JsonProperty("serverTime")]
        public string serverTime { get; set; }

        [JsonProperty("gate")]
        public BootstrapGateData gate { get; set; }

        [JsonProperty("liveEvents")]
        public object liveEvents { get; set; }

        [JsonProperty("announcements")]
        public object announcements { get; set; }

        [JsonProperty("featureFlags")]
        public object featureFlags { get; set; }

        [JsonProperty("experiments")]
        public object experiments { get; set; }
    }

    [Serializable]
    public class BootstrapGateData
    {
        [JsonProperty("action")]
        public string action { get; set; }

        [JsonProperty("platform")]
        public string platform { get; set; }

        [JsonProperty("currentVersion")]
        public string currentVersion { get; set; }

        [JsonProperty("minVersion")]
        public string minVersion { get; set; }

        [JsonProperty("latestVersion")]
        public string latestVersion { get; set; }

        [JsonProperty("softUpdateVersion")]
        public object softUpdateVersion { get; set; }

        [JsonProperty("forceUpdateVersion")]
        public object forceUpdateVersion { get; set; }

        [JsonProperty("updateUrl")]
        public string updateUrl { get; set; }

        [JsonProperty("releaseNotes")]
        public string releaseNotes { get; set; }

        [JsonProperty("maintenance")]
        public BootstrapMaintenance maintenance { get; set; }
    }

    [Serializable]
    public class BootstrapMaintenance
    {
        [JsonProperty("active")]
        public bool active { get; set; }

        [JsonProperty("message")]
        public string message { get; set; }

        [JsonProperty("until")]
        public string until { get; set; }
    }

    [Serializable]
    public class BootstrapRewards
    {
        public DailyBonusStatusResponse daily_bonus;
        public SevenDayBonus seven_day_bonus;
        public BootstrapMlm mlm;
    }

    [Serializable]
    public class SevenDayBonus
    {
        public int current_day;
        public List<int> claimed_days;
        public List<long> days_paisa;
        public string timezone;
    }

    [Serializable]
    public class BootstrapMlm
    {
        public bool enabled;
        public List<MlmLevel> levels;
    }

    [Serializable]
    public class MlmLevel
    {
        public int level;
        public double commission;
    }

    [Serializable]
    public class PayoutMethods
    {
        public bool has_bank;
        public bool has_upi;
        public BootstrapPayoutMethodBank bank;
    }

    [Serializable]
    public class BootstrapPayoutMethodBank
    {
        public string masked_account_number;
        public string bank_name;
        public string ifsc_code;
        public string account_holder_name;
        public bool is_verified;
    }

    [Serializable]
    public class SavedBankAccount
    {
        public string id;
        public string method_type;
        public string bank_name;
        public string account_holder_name;
        public string masked_account_number;
        public string ifsc_code;
        public bool is_verified;
        public bool is_default;
        public string status;
        public string verification_state;
        public string created_at;
        public string updated_at;
    }

    [Serializable]
    public class BootstrapProfile
    {
        public string public_id;
        public string username;
        [JsonProperty("mobile")]
        public string mobile_number;
        public string avatar;
        public string status;
        public string created_at;
        public string updated_at;
    }

    [Serializable]
    public class BootstrapWallet
    {
        public string user_id;
        public long deposit_balance;
        public long win_balance;
        public long bonus_balance;
        public long withdrawable_amount;
        public long locked_balance;
        public long available_balance;
        public long total_balance;

        public string currency;
    }

    public static class BootstrapErrorReason
    {
        public const string ServiceUnavailable = "service_unavailable";
        public const string Timeout = "timeout";
        public const string NotFound = "not_found";
        public const string UnauthorizedDownstream = "unauthorized_downstream";
        public const string ServiceThrottled = "service_throttled";
        public const string DownstreamError = "downstream_error";

        public static string Friendly(string code)
        {
            switch (code)
            {
                case Timeout: return "the request timed out";
                case ServiceThrottled: return "too many requests";
                case ServiceUnavailable: return "the service is temporarily down";
                case UnauthorizedDownstream: return "your session may have expired";
                case NotFound: return "data not found";
                case DownstreamError: return "an internal error occurred";
                default: return "please try again";
            }
        }
    }
}
