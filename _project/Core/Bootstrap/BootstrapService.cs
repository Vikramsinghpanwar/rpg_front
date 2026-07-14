using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.API;
using Core.API.Endpoints;
using Core.Models;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Core.Bootstrap
{
    public class BootstrapService : MonoBehaviour
    {
        public static BootstrapService Instance { get; private set; }

        // [Header("Raw Bootstrap Data")]
        public BootstrapResponse Current { get; private set; }
        public JToken LobbyRaw { get; private set; }
        public JToken NoticesRaw { get; private set; }
        public DateTime LastFetchedAtUtc { get; private set; }

        public event Action<BootstrapResponse> OnBootstrapUpdated;
        public event Action<string> OnBootstrapFailed;

        Task<BootstrapResponse> inFlight;
        readonly object refreshLock = new object();

        public static void Initialize()
        {
            if (Instance == null)
            {
                var go = new GameObject("[BootstrapService]");
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<BootstrapService>();
            }
        }

        public bool HasData => Current != null;
        public BootstrapWallet Wallet => Current?.wallet;
        public BootstrapProfile Profile => Current?.profile;
        public string ReferralCode => Current?.referral_code;
        public List<Features.Rewards.Models.ReferredUser> ReferredUsers => Current?.referred_users;

        public WalletConfig WalletConfig => Current?.wallet_config?.data?.walletConfig;

        public bool TryGetRechargePresets(out List<RechargePreset> presets)
        {
            presets = null;
            var data = Current?.wallet_config?.data;
            if (data?.rechargePresets == null || data.rechargePresets.Count == 0) return false;
            presets = new List<RechargePreset>(data.rechargePresets.Count);
            foreach (var p in data.rechargePresets)
            {
                if (p != null && p.amount > 0) presets.Add(p);
            }
            presets.Sort((a, b) => a.amount.CompareTo(b.amount));
            return presets.Count > 0;
        }

        public bool TryGetWithdrawalPresets(out List<WithdrawalPreset> presets)
        {
            presets = null;
            var data = Current?.wallet_config?.data;
            if (data?.withdrawalPresets == null || data.withdrawalPresets.Count == 0) return false;
            presets = new List<WithdrawalPreset>(data.withdrawalPresets.Count);
            foreach (var p in data.withdrawalPresets)
            {
                if (p != null && p.amount > 0) presets.Add(p);
            }
            presets.Sort((a, b) => a.amount.CompareTo(b.amount));
            return presets.Count > 0;
        }

        public (long commissionPaisa, long netPayoutPaisa) CalculateWithdrawalCommission(long amountPaisa)
        {
            var cfg = WalletConfig;
            if (cfg == null) return (0, amountPaisa);

            long commission = 0;
            if (cfg.withdrawalCommissionType == "PERCENTAGE")
            {
                commission = (long)Math.Round(amountPaisa * cfg.withdrawalCommissionValue / 100f);
            }
            else if (cfg.withdrawalCommissionType == "FIXED")
            {
                commission = cfg.withdrawalCommissionValue * 100;
            }
            long net = amountPaisa - commission;
            if (net < 0) net = 0;
            return (commission, net);
        }

        public void ApplyProfilePatch(BootstrapProfile patchedProfile)
        {
            if (patchedProfile == null) return;
            if (Current == null)
            {
                Current = new BootstrapResponse { profile = patchedProfile };
            }
            else
            {
                Current.profile = patchedProfile;
            }
            if (Current.profile != null)
            {
                Current.profile.updated_at = DateTime.UtcNow.ToString("o");
            }
            LastFetchedAtUtc = DateTime.UtcNow;
            OnBootstrapUpdated?.Invoke(Current);
        }

        public Core.Models.BootstrapGateData GetLobbyGate()
        {
            return Current?.lobby?.data?.gate;
        }

        public Task<BootstrapResponse> GetOrFetch()
        {
            if (Current != null) return Task.FromResult(Current);
            return Refresh();
        }

        public Task<BootstrapResponse> Refresh()
        {
            lock (refreshLock)
            {
                if (inFlight != null)
                {
                    // Debug.Log("[notice-debug] Bootstrap Refresh: in-flight refresh already running, returning existing task");
                    return inFlight;
                }
                inFlight = RunRefresh();
                return inFlight;
            }
        }

        async Task<BootstrapResponse> RunRefresh()
        {
            try
            {
                // Debug.Log("[notice-debug] Bootstrap RunRefresh START");
                var raw = await ApiClient.Instance.Get<JObject>(BootstrapRoutes.Player);
                if (raw == null)
                {
                    Debug.LogWarning("[notice-debug] Bootstrap RunRefresh: raw response is NULL");
                    OnBootstrapFailed?.Invoke("empty_response");
                    return null;
                }

                var resp = raw.ToObject<BootstrapResponse>();
                LobbyRaw = raw["lobby"];
                NoticesRaw = raw["notices"];
                Current = resp;
                LastFetchedAtUtc = DateTime.UtcNow;

                OnBootstrapUpdated?.Invoke(resp);

                if (string.IsNullOrEmpty(Current.referral_code))
                {
                    _ = HydrateReferralCodeIfMissing();
                }

                return resp;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BootstrapService] refresh failed: {ex.Message}");
                OnBootstrapFailed?.Invoke(ex.Message);
                return null;
            }
            finally
            {
                lock (refreshLock) inFlight = null;
            }
        }

        public T GetLobbySlice<T>(string key) where T : class
        {
            if (LobbyRaw == null || string.IsNullOrEmpty(key)) return null;
            var token = LobbyRaw[key];
            if (token == null) return null;
            try { return token.ToObject<T>(); }
            catch { return null; }
        }

        public JToken GetNoticesData()
        {
            if (NoticesRaw == null)
            {
                // Debug.Log("[notice-debug] GetNoticesData: NoticesRaw is NULL");
                return null;
            }

            // Debug.Log("[notice-debug] GetNoticesData: NoticesRaw type=" + NoticesRaw.Type +", keys=" + string.Join(",", NoticesRaw.Children<JProperty>().Select(p => p.Name)));

            if (NoticesRaw.Type == JTokenType.Array)
            {
                // Debug.Log("[notice-debug] GetNoticesData: NoticesRaw is direct array, returning it");
                return NoticesRaw;
            }

            var direct = NoticesRaw["data"];
            if (direct == null)
            {
                // Debug.Log("[notice-debug] GetNoticesData: NoticesRaw['data'] is NULL");
            }
            else
            {
                // Debug.Log("[notice-debug] GetNoticesData: NoticesRaw['data'] type=" + direct.Type +", keys=" + string.Join(",", direct.Children<JProperty>().Select(p => p.Name)));
            }

            if (direct == null) return null;

            if (direct.Type == JTokenType.Array)
            {
                // Debug.Log("[notice-debug] GetNoticesData: NoticesRaw['data'] is array, returning it");
                return direct;
            }

            var nested = direct["notices"];
            if (nested == null)
            {
                // Debug.Log("[notice-debug] GetNoticesData: NoticesRaw['data']['notices'] is NULL");
            }
            else
            {
                // Debug.Log("[notice-debug] GetNoticesData: NoticesRaw['data']['notices'] type=" + nested.Type);
            }

            if (nested != null && nested.Type == JTokenType.Array)
            {
                // Debug.Log("[notice-debug] GetNoticesData: returning NoticesRaw['data']['notices']");
                return nested;
            }

            var topNotices = NoticesRaw["notices"];
            if (topNotices != null)
            {
                // Debug.Log("[notice-debug] GetNoticesData: NoticesRaw['notices'] type=" + topNotices.Type);
                if (topNotices.Type == JTokenType.Array) return topNotices;
            }

            // Debug.Log("[notice-debug] GetNoticesData: falling back to NoticesRaw['data'] object");
            return direct;
        }

        public string GetWalletErrorReason()
        {
            if (Current?.errors == null) return null;
            Current.errors.TryGetValue("wallet", out var reason);
            return reason;
        }

        public async Task RefreshWallet(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;
            if (Current == null) Current = new Core.Models.BootstrapResponse();

            try
            {
                var balance = await ApiClient.Instance.Get<Core.Models.WalletBalanceResponse>(WalletRoutes.MeBalance());
                if (balance != null)
                {
                    if (Current.wallet == null) Current.wallet = new Core.Models.BootstrapWallet();
                    Current.wallet.deposit_balance = balance.deposit_balance;
                    Current.wallet.win_balance = balance.win_balance;
                    Current.wallet.bonus_balance = balance.bonus_balance;
                    Current.wallet.user_id = balance.user_id;
                    Current.wallet.currency = balance.currency;
                    Current.wallet.withdrawable_amount = balance.withdrawable_amount;
                    Current.wallet.available_balance = balance.available_balance;
                    Current.wallet.locked_balance = balance.locked_balance;
                    Current.wallet.total_balance = balance.total_balance;
                }

                LastFetchedAtUtc = DateTime.UtcNow;
                OnBootstrapUpdated?.Invoke(Current);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BootstrapService] RefreshWallet failed: {ex.Message}");
            }
        }

        public void ApplyWalletBalance(Core.Models.WalletBalanceResponse balance)
        {
            if (balance == null) return;
            if (Current == null) Current = new Core.Models.BootstrapResponse();
            if (Current.wallet == null) Current.wallet = new Core.Models.BootstrapWallet();

            Current.wallet.deposit_balance     = balance.deposit_balance;
            Current.wallet.win_balance         = balance.win_balance;
            Current.wallet.bonus_balance       = balance.bonus_balance;
            Current.wallet.user_id             = balance.user_id;
            Current.wallet.currency            = balance.currency;
            Current.wallet.withdrawable_amount = balance.withdrawable_amount;
            Current.wallet.available_balance   = balance.available_balance;
            Current.wallet.locked_balance      = balance.locked_balance;
            Current.wallet.total_balance       = balance.total_balance;

            LastFetchedAtUtc = DateTime.UtcNow;
            OnBootstrapUpdated?.Invoke(Current);
        }

        public async Task HydrateReferralCodeIfMissing()
        {
            if (Current != null && !string.IsNullOrEmpty(Current.referral_code)) return;

            try
            {
                var raw = await ApiClient.Instance.Get<JObject>(Core.API.Endpoints.RewardRoutes.ReferralMe);
                if (raw == null) return;

                if (Current == null) Current = new BootstrapResponse();
                var codeToken = raw["referral_code"] ?? raw["referralCode"];
                Current.referral_code = codeToken?.ToString() ?? string.Empty;
                var usersToken = raw["referred_users"] ?? raw["referredUsers"];
                Current.referred_users = usersToken != null
                    ? usersToken.ToObject<List<Features.Rewards.Models.ReferredUser>>()
                    : new List<Features.Rewards.Models.ReferredUser>();

                LastFetchedAtUtc = DateTime.UtcNow;
                OnBootstrapUpdated?.Invoke(Current);
            }
            catch { }
        }
    }
}
