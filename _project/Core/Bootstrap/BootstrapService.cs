using System;
using System.Threading.Tasks;
using Core.API;
using Core.API.Endpoints;
using Core.Models;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Core.Bootstrap
{
    // Single source of truth for GET /api/v1/bootstrap. Features should read profile / wallet
    // / lobby data through here instead of refetching the endpoint themselves. The opaque
    // `lobby` blob is exposed as a JToken so each feature can deserialize the slice it cares
    // about (announcements, games, sections, etc.) without sharing a giant DTO.
    public class BootstrapService : MonoBehaviour
    {
        public static BootstrapService Instance { get; private set; }

        public BootstrapResponse Current { get; private set; }
        public JToken LobbyRaw { get; private set; }
        public DateTime LastFetchedAtUtc { get; private set; }

        public event Action<BootstrapResponse> OnBootstrapUpdated;
        public event Action<string> OnBootstrapFailed;

        Task<BootstrapResponse> inFlight;
        readonly object refreshLock = new object();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[BootstrapService]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<BootstrapService>();
        }

        public bool HasData => Current != null;
        public BootstrapWallet Wallet => Current?.wallet;
        public BootstrapProfile Profile => Current?.profile;

        // Returns the cached payload if present. Use Refresh() when you need fresh state.
        public Task<BootstrapResponse> GetOrFetch()
        {
            if (Current != null) return Task.FromResult(Current);
            return Refresh();
        }

        public Task<BootstrapResponse> Refresh()
        {
            lock (refreshLock)
            {
                if (inFlight != null) return inFlight;
                inFlight = RunRefresh();
                return inFlight;
            }
        }

        async Task<BootstrapResponse> RunRefresh()
        {
            try
            {
                var raw = await ApiClient.Instance.Get<JObject>(BootstrapRoutes.Player);
                if (raw == null)
                {
                    OnBootstrapFailed?.Invoke("empty_response");
                    return null;
                }

                var resp = raw.ToObject<BootstrapResponse>();
                LobbyRaw = raw["lobby"];
                Current = resp;
                LastFetchedAtUtc = DateTime.UtcNow;

                OnBootstrapUpdated?.Invoke(resp);
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

        // Helper for features that own a slice of the lobby blob (announcements, etc.).
        // Returns default(T) if the key is missing or the cast fails — never throws.
        public T GetLobbySlice<T>(string key) where T : class
        {
            if (LobbyRaw == null || string.IsNullOrEmpty(key)) return null;
            var token = LobbyRaw[key];
            if (token == null) return null;
            try { return token.ToObject<T>(); }
            catch { return null; }
        }

        public string GetWalletErrorReason()
        {
            if (Current?.errors == null) return null;
            Current.errors.TryGetValue("wallet", out var reason);
            return reason;
        }
    }
}
