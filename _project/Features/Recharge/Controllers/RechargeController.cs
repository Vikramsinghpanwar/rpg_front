using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Features.Recharge.Models;
using Core.API;
using Core.API.Endpoints;
using Core.Managers;
using Core.Utils;

namespace Features.Recharge.Controllers
{
    public class RechargeController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private string[] defaultAmounts = { "100", "200", "500", "1000", "2000", "5000" };
        [SerializeField] private int historyRefreshCooldownSeconds = 3;

        public event Action<List<GatewayInfo>> OnGatewaysLoaded;
        public event Action<List<RechargeAmountPreset>> OnAmountsLoaded;
        public event Action<CreateRechargeResponse> OnRechargeCreated;
        public event Action<List<RechargeRecord>> OnHistoryLoaded;
        public event Action<string, string> OnError;

        readonly List<GatewayInfo> cachedGateways = new List<GatewayInfo>();
        readonly List<RechargeAmountPreset> cachedAmounts = new List<RechargeAmountPreset>();
        List<RechargeRecord> cachedHistory = new List<RechargeRecord>();
        DateTime lastHistoryFetchTime = DateTime.MinValue;
        bool isLoadingHistory;

        public List<GatewayInfo> GetGateways() => cachedGateways;
        public List<RechargeRecord> GetCachedHistory() => cachedHistory;

        public async Task LoadGateways()
        {
            LoadingManager.Instance?.Show("Loading payment methods...");
            try
            {
                var response = await ApiClient.Instance.Get<List<GatewayInfo>>(RechargeRoutes.Gateways("lobby"));

                cachedGateways.Clear();
                if (response != null) cachedGateways.AddRange(response);

                if (cachedGateways.Count == 0)
                {
                    OnError?.Invoke("NO_GATEWAYS", "No payment gateways available");
                }
                else
                {
                    OnGatewaysLoaded?.Invoke(cachedGateways);
                }
            }
            catch (ApiException e)
            {
                Debug.LogError($"Load gateways failed: {e.Message}");
                OnError?.Invoke("GATEWAY_LOAD_FAILED", "Failed to load payment methods");
            }
            finally
            {
                LoadingManager.Instance?.Hide();
            }
        }

        public void SetAmountsFromBootstrap(List<RechargeAmountPreset> amounts)
        {
            cachedAmounts.Clear();
            if (amounts != null && amounts.Count > 0)
            {
                cachedAmounts.AddRange(amounts);
            }
            else
            {
                BuildDefaultAmounts(cachedAmounts);
            }
            OnAmountsLoaded?.Invoke(cachedAmounts);
        }

        public List<RechargeAmountPreset> GetDefaultAmounts()
        {
            if (cachedAmounts.Count > 0) return cachedAmounts;
            BuildDefaultAmounts(cachedAmounts);
            return cachedAmounts;
        }

        void BuildDefaultAmounts(List<RechargeAmountPreset> target)
        {
            target.Clear();
            foreach (var s in defaultAmounts)
            {
                if (int.TryParse(s, out var rupees)) target.Add(new RechargeAmountPreset(rupees * 100));
            }
        }

        public async Task<string> CreateRecharge(long amountPaisa, string providerId)
        {
            LoadingManager.Instance?.Show("Creating recharge...");
            try
            {
                var (key, headers) = IdempotencyHeader.New();
                var request = new CreateRechargeRequest
                {
                    idempotency_key = key,
                    amount = amountPaisa,
                    currency = "INR",
                    provider = providerId,
                    client_metadata = new Dictionary<string, object>
                    {
                        { "platform", NormalizedPlatform() },
                        { "app_version", Application.version }
                    }
                };

                var response = await ApiClient.Instance.Post<CreateRechargeResponse>(RechargeRoutes.Create, request, headers);

                if (response?.checkout != null && !string.IsNullOrEmpty(response.checkout.url))
                {
                    OnRechargeCreated?.Invoke(response);
                    _ = RefreshHistorySilent();
                    return response.checkout.url;
                }

                OnError?.Invoke("RECHARGE_FAILED", "Failed to create recharge request");
                return null;
            }
            catch (ApiException e)
            {
                Debug.LogError($"Create recharge failed [{e.StatusCode}]: {e.Message}");
                OnError?.Invoke(e.ErrorCode ?? "RECHARGE_FAILED", FriendlyMessage(e));
                return null;
            }
            finally
            {
                LoadingManager.Instance?.Hide();
            }
        }

        public async Task<List<RechargeRecord>> FetchHistory(bool forceRefresh = false, int limit = 50, int offset = 0)
        {
            if (isLoadingHistory) return cachedHistory;

            if (!forceRefresh && (DateTime.UtcNow - lastHistoryFetchTime).TotalSeconds < historyRefreshCooldownSeconds)
            {
                return cachedHistory;
            }

            isLoadingHistory = true;
            LoadingManager.Instance?.Show("Loading history...");
            try
            {
                var response = await ApiClient.Instance.Get<RechargeRecord[]>(RechargeRoutes.List(limit, offset));
                cachedHistory = response != null ? new List<RechargeRecord>(response) : new List<RechargeRecord>();
                lastHistoryFetchTime = DateTime.UtcNow;
                OnHistoryLoaded?.Invoke(cachedHistory);
                return cachedHistory;
            }
            catch (ApiException e)
            {
                Debug.LogError($"Load history failed: {e.Message}");
                OnError?.Invoke("HISTORY_FAILED", "Failed to load recharge history");
                return cachedHistory;
            }
            finally
            {
                isLoadingHistory = false;
                LoadingManager.Instance?.Hide();
            }
        }

        async Task RefreshHistorySilent()
        {
            try
            {
                var response = await ApiClient.Instance.Get<RechargeRecord[]>(RechargeRoutes.List(50, 0));
                if (response != null)
                {
                    cachedHistory = new List<RechargeRecord>(response);
                    OnHistoryLoaded?.Invoke(cachedHistory);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Silent history refresh failed: {e.Message}");
            }
        }

        public async Task<RechargeRecord> GetRechargeById(string rechargeId)
        {
            try
            {
                return await ApiClient.Instance.Get<RechargeRecord>(RechargeRoutes.Detail(rechargeId));
            }
            catch (ApiException e)
            {
                Debug.LogWarning($"GetRechargeById({rechargeId}) failed: {e.Message}");
                return null;
            }
        }

        public async Task RefreshNonTerminalRecharges()
        {
            Debug.Log("Refreshing non-terminal recharges...");
            if (cachedHistory.Count == 0) return;

            var pending = new List<RechargeRecord>();
            for (int i = 0; i < cachedHistory.Count; i++)
            {
                if (!IsTerminal(cachedHistory[i].status)) pending.Add(cachedHistory[i]);
            }
            if (pending.Count == 0) return;

            bool anyChanged = false;
            foreach (var r in pending)
            {
                var fresh = await GetRechargeById(r.id);
                if (fresh == null || fresh.status == r.status) continue;

                var idx = cachedHistory.FindIndex(x => x.id == r.id);
                if (idx >= 0)
                {
                    cachedHistory[idx] = fresh;
                    anyChanged = true;
                }
            }

            if (anyChanged) OnHistoryLoaded?.Invoke(cachedHistory);
        }

        public bool HasNonTerminalRecharges()
        {
            for (int i = 0; i < cachedHistory.Count; i++)
            {
                if (!IsTerminal(cachedHistory[i].status)) return true;
            }
            return false;
        }

        // The webhook credits the wallet server-side. When the player returns from the PSP
        // browser tab the only way to learn the new state until WebSocket lands is to refetch.
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus || !HasNonTerminalRecharges()) return;
            _ = RefreshNonTerminalRecharges();
        }

        static readonly HashSet<string> TerminalRechargeStatuses = new HashSet<string>
        {
            "succeeded", "failed", "expired", "cancelled"
        };

        public static bool IsTerminal(string status) => status != null && TerminalRechargeStatuses.Contains(status);

        public static string FormatAmount(long paisa) => MoneyFormatter.FormatPaisa(paisa);

        public static string GetStatusDisplay(string status) => status switch
        {
            "succeeded" => "Success",
            "failed" => "Failed",
            "expired" => "Expired",
            "cancelled" => "Cancelled",
            "initiated" => "Initiated",
            "awaiting_payment" => "Awaiting Payment",
            "provider_processing" => "Processing",
            _ => status
        };

        public static Color GetStatusColor(string status) => status switch
        {
            "succeeded" => new Color(0.2f, 0.8f, 0.2f),
            "failed" => new Color(0.9f, 0.2f, 0.2f),
            "expired" => new Color(0.6f, 0.6f, 0.6f),
            "cancelled" => new Color(0.6f, 0.6f, 0.6f),
            "initiated" => new Color(0.8f, 0.6f, 0.2f),
            "awaiting_payment" => new Color(0.8f, 0.6f, 0.2f),
            "provider_processing" => new Color(0.8f, 0.6f, 0.2f),
            _ => Color.white
        };

        static string NormalizedPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android: return "android";
                case RuntimePlatform.IPhonePlayer: return "ios";
                case RuntimePlatform.WebGLPlayer: return "web";
                default: return Application.platform.ToString().ToLowerInvariant();
            }
        }

        static string FriendlyMessage(ApiException e)
        {
            return e.StatusCode switch
            {
                409 => "This recharge was already submitted. Please refresh history.",
                422 => "You have too many open recharges. Complete or cancel them first.",
                503 => "Payment provider is unavailable. Try a different method.",
                _ => e.Message
            };
        }
    }
}
