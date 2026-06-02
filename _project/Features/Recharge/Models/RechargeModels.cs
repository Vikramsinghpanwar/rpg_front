using System;
using System.Collections.Generic;

namespace Features.Recharge.Models
{
    [Serializable]
    public class CreateRechargeRequest
    {
        public string idempotency_key;
        public long amount;
        public string currency;
        public string provider;
        public Dictionary<string, object> client_metadata;
    }

    [Serializable]
    public class CreateRechargeResponse
    {
        public string id;
        public string status;
        public long amount;
        public long bonus_amount;
        public string currency;
        public string provider;
        public string provider_order_id;
        public CheckoutInfo checkout;
        public string expires_at;
    }

    [Serializable]
    public class CheckoutInfo
    {
        public string type;
        public string url;
        public string provider_ref;
    }

    [Serializable]
    public class RechargeRecord
    {
        public string id;
        public string idempotency_key;
        public string user_id;
        public long amount;
        public string currency;
        public string status;
        public string provider;
        public string provider_order_id;
        public string provider_payment_id;
        public string bonus_config_id;
        public long bonus_amount;
        public bool bonus_applied;
        public string success_transaction_id;
        public string bonus_transaction_id;
        public string failure_code;
        public string failure_reason;
        public Dictionary<string, object> client_metadata;
        public string created_at;
        public string updated_at;
        public string expires_at;
        public string completed_at;
    }
}
