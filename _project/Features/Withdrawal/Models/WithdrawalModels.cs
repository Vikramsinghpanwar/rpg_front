using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Withdrawal.Models
{
    [Serializable]
    public class CreateWithdrawalRequest
    {
        public string idempotency_key;
        public long amount;
        public string currency = "INR";
        public string payout_method;
        public Dictionary<string, string> account_details;
    }

    [Serializable]
    public class CreateWithdrawalResponse
    {
        public string id;
        public string status;
        public long requested_amount;
        public long fee_amount;
        public long tax_amount;
        public long net_payout_amount;
        public string masked_account;
        public string debit_transaction_id;
        public string expires_at;
    }

    [Serializable]
    public class WithdrawalItem
    {
        public string id;
        public string idempotency_key;
        public string user_id;
        public string user_tier;
        public long requested_amount;
        public long fee_amount;
        public long tax_amount;
        public long net_payout_amount;
        public string currency;
        public string status;
        public string payout_method;
        public string masked_account;
        public string debit_transaction_id;
        public string refund_transaction_id;
        public string payout_transaction_id;
        public string provider;
        public string provider_payout_id;
        public string failure_code;
        public string failure_reason;
        public string requested_at;
        public string reviewed_at;
        public string payout_initiated_at;
        public string completed_at;
        public string expires_at;
    }

    [Serializable]
    public class CancelWithdrawalResponse
    {
        public string id;
        public string status;
        public string refund_transaction_id;
    }

    [Serializable]
    public class AccountDetailsUpi
    {
        public string vpa;
    }

    [Serializable]
    public class AccountDetailsBank
    {
        public string account_number;
        public string ifsc;
        public string holder_name;
    }
}