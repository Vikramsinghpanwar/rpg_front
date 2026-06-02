using System;
using System.Collections.Generic;

namespace Core.Models
{
    // Canonical shape for GET /api/v1/bootstrap. Features that need wallet/profile
    // read from here; features that need lobby (announcements, etc.) deserialize the
    // opaque `lobby` field with their own DTO via Newtonsoft.
    [Serializable]
    public class BootstrapResponse
    {
        public BootstrapProfile profile;
        public BootstrapWallet wallet;
        public object lobby;
        public string server_time;
        public Dictionary<string, string> errors;
    }

    [Serializable]
    public class BootstrapProfile
    {
        public string public_id;
        public string username;
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
