using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Core.API;
using Core.API.Endpoints;
using Core.Bootstrap;
using Core.Managers;
using Core.Models;
using Core.Utils;
using Features.Recharge.Models;
using UnityEngine.SceneManagement;

namespace Features.Recharge.Controllers
{
    public class RechargeController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private int historyRefreshCooldownSeconds = 3;
        [SerializeField] private int historyPageSize = 20;

        public long MinRechargeAmount => BootstrapService.Instance.WalletConfig?.minRechargeAmount * 100 ?? 0;

        public int CurrentHistoryPage { get; private set; } = 1;
        public int TotalHistoryPages { get; private set; } = 1;
        public bool HasMoreHistory { get; private set; }

        public event Action<List<GatewayInfo>> OnGatewaysLoaded;
        public event Action<List<RechargeAmountPreset>> OnAmountsLoaded;
        public event Action<CreateRechargeResponse> OnRechargeCreated;
        public event Action<List<RechargeRecord>> OnHistoryLoaded;
        public event Action<string, string> OnError;
        public event Action<int, int> OnPaginationChanged;

        readonly List<GatewayInfo> cachedGateways = new List<GatewayInfo>();
        readonly List<RechargeAmountPreset> cachedAmounts = new List<RechargeAmountPreset>();
        readonly List<RechargeRecord> cachedHistory = new List<RechargeRecord>();
        bool isLoadingHistory;

        public string LoadedSource { get; private set; }

        public List<GatewayInfo> GetGateways() => cachedGateways;
        public List<RechargeRecord> GetCachedHistory() => cachedHistory;

        public async Task LoadGateways(string source = "lobby", bool silent = false)
        {
            Debug.Log($"[PaymentGateway] Request started (source: {source})");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (!silent)
            {
                LoadingManager.Instance?.Show("Loading payment methods...");
            }
            try
            {
                var response = await ApiClient.Instance.Get<List<GatewayInfo>>(RechargeRoutes.Gateways(source));
                stopwatch.Stop();

                cachedGateways.Clear();
                if (response != null) cachedGateways.AddRange(response);
                LoadedSource = source;

                Debug.Log($"[PaymentGateway] Response received ({cachedGateways.Count} gateways, {stopwatch.ElapsedMilliseconds}ms)");
                foreach (var gw in cachedGateways)
                {
                    Debug.Log($"[PaymentGateway] Gateway Response - id: {gw.id}, enabled_lobby: {gw.enabled_lobby}, enabled_ingame: {gw.enabled_ingame}, status: {gw.status}");
                }
                Debug.Log($"[PaymentGateway] Source: fresh API ({source})");
                Debug.Log("[PaymentGateway] Cache updated");

                if (cachedGateways.Count == 0)
                {
                    OnError?.Invoke("NO_GATEWAYS", "No payment gateways available");
                }
                else
                {
                    OnGatewaysLoaded?.Invoke(cachedGateways);
                }

                Debug.Log("[PaymentGateway] UI refreshed");
            }
            catch (ApiException e)
            {
                stopwatch.Stop();
                Debug.LogError($"Load gateways failed: {e.Message}");
                OnError?.Invoke("GATEWAY_LOAD_FAILED", "Failed to load payment methods");
                Debug.LogWarning("[RechargeController] Toast.ShowError queued: Failed to load payment methods.");
                Toast.Instance.ShowError("Failed to load payment methods.");
            }
            finally
            {
                if (!silent)
                {
                    LoadingManager.Instance?.Hide();
                }
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
                RefreshAmountsFromBootstrap();
            }
            OnAmountsLoaded?.Invoke(cachedAmounts);
        }

        public List<RechargeAmountPreset> GetDefaultAmounts()
        {
            if (cachedAmounts.Count > 0) return cachedAmounts;
            RefreshAmountsFromBootstrap();
            return cachedAmounts;
        }

        public void RefreshAmountsFromBootstrap()
        {
            cachedAmounts.Clear();
            if (BootstrapService.Instance.TryGetRechargePresets(out var presets))
            {
                foreach (var p in presets)
                {
                    var display = MoneyFormatter.FormatPaisaRoundedRupees(p.amount * 100);
                    var bonus = p.bonusAmount * 100;
                    cachedAmounts.Add(new RechargeAmountPreset(p.amount * 100, display, bonus));
                }
            }
            else
            {
                BuildFallbackAmounts(cachedAmounts);
            }
        }

        void BuildFallbackAmounts(List<RechargeAmountPreset> target)
        {
            target.Clear();
            var fallback = new[] { 100, 200, 500, 1000, 2000, 5000 };
            foreach (var r in fallback)
            {
                target.Add(new RechargeAmountPreset(r * 100));
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
                    source = SceneManager.GetActiveScene().name.ToLowerInvariant() == "lobby" ? "lobby" : "in-game",
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

        public async Task FetchHistory(bool forceRefresh = false)
        {
            if (isLoadingHistory) return;

            int page = forceRefresh ? 1 : CurrentHistoryPage;
            int offset = (page - 1) * historyPageSize;

            isLoadingHistory = true;
            LoadingManager.Instance?.Show("Loading history...");
            try
            {
                var response = await ApiClient.Instance.Get<RechargeRecord[]>(RechargeRoutes.List(historyPageSize, offset));

                cachedHistory.Clear();
                if (response != null)
                {
                    cachedHistory.AddRange(response);
                }

                CurrentHistoryPage = page;
                TotalHistoryPages = response != null && response.Length > 0
                    ? (int)Mathf.Ceil((float)response.Length / historyPageSize)
                    : 1;
                HasMoreHistory = response != null && response.Length >= historyPageSize;

                OnHistoryLoaded?.Invoke(new List<RechargeRecord>(cachedHistory));
                OnPaginationChanged?.Invoke(CurrentHistoryPage, TotalHistoryPages);
            }
            catch (ApiException e)
            {
                Debug.LogError($"Load history failed: {e.Message}");
                OnError?.Invoke("HISTORY_FAILED", "Failed to load recharge history");
                Toast.Instance.ShowError("Failed to load recharge history.");
            }
            finally
            {
                isLoadingHistory = false;
                LoadingManager.Instance?.Hide();
            }
        }

        public async Task NextPage()
        {
            if (isLoadingHistory || !HasMoreHistory) return;
            CurrentHistoryPage++;
            await FetchHistory();
        }

        public async Task PreviousPage()
        {
            if (isLoadingHistory || CurrentHistoryPage <= 1) return;
            CurrentHistoryPage--;
            await FetchHistory();
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

            if (anyChanged) OnHistoryLoaded?.Invoke(new List<RechargeRecord>(cachedHistory));
        }

        public bool HasNonTerminalRecharges()
        {
            for (int i = 0; i < cachedHistory.Count; i++)
            {
                if (!IsTerminal(cachedHistory[i].status)) return true;
            }
            return false;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            string ts = DateTime.Now.ToString("HH:mm:ss.fff");
            if (!hasFocus)
            {
                Debug.Log($"[{ts}] [Lifecycle] OnApplicationFocus(false)");
                return;
            }
            Debug.Log($"[{ts}] [Lifecycle] OnApplicationFocus(true) - hasNonTerminalRecharges={HasNonTerminalRecharges()}");
            if (!HasNonTerminalRecharges()) return;
            _ = RefreshNonTerminalRecharges();
        }

        public async Task RefreshHistorySilent()
        {
            try
            {
                var response = await ApiClient.Instance.Get<RechargeRecord[]>(RechargeRoutes.List(historyPageSize, 0));
                if (response != null)
                {
                    cachedHistory.Clear();
                    if (response != null)
                    {
                        cachedHistory.AddRange(response);
                    }
                    OnHistoryLoaded?.Invoke(cachedHistory);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Silent history refresh failed: {e.Message}");
                Toast.Instance.ShowWarning("Could not refresh recharge history.");
            }
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
