using System;
using System.Collections.Generic;

namespace Core.Models
{
    [Serializable]
    public class WalletConfigResponse
    {
        public bool success;
        public int statusCode;
        public string timestamp;
        public string path;
        public WalletConfigData data;
    }

    [Serializable]
    public class WalletConfigData
    {
        public WalletConfig walletConfig;
        public List<RechargePreset> rechargePresets;
        public List<WithdrawalPreset> withdrawalPresets;
        public int configVersion;
        public long generatedAt;
    }

    [Serializable]
    public class WalletConfig
    {
        public int minRechargeAmount;
        public int minWithdrawalAmount;
        public string withdrawalCommissionType;
        public int withdrawalCommissionValue;
    }

    [Serializable]
    public class RechargePreset
    {
        public int amount;
        public int bonusAmount;
    }

    [Serializable]
    public class WithdrawalPreset
    {
        public int amount;
    }
}
