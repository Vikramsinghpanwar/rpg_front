using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Core.Services
{
    public class TokenProvider : MonoBehaviour
    {
        public static TokenProvider Instance { get; private set; }

        public event Action<string> OnTokenRefreshed;
        public event Action OnForceReLogin;

        const string KEY_ACCESS = "auth.access_token";
        const string KEY_REFRESH = "auth.refresh_token";
        const string KEY_EXPIRES_AT = "auth.expires_at_unix";

        string accessToken;
        string refreshToken;
        long expiresAtUnix;

        string cachedUserId;

        Task<bool> inFlightRefresh;
        readonly object refreshLock = new object();

        // Func<string, Task<RefreshResult>> refreshCallback;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[TokenProvider]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<TokenProvider>();
            Instance.Load();
        }

        // Storage uses PlayerPrefs for dev. Production must move tokens to platform
        // secure storage (iOS Keychain / Android EncryptedSharedPreferences) before launch.
        void Load()
        {
            accessToken = PlayerPrefs.GetString(KEY_ACCESS, null);
            refreshToken = PlayerPrefs.GetString(KEY_REFRESH, null);
            expiresAtUnix = long.TryParse(PlayerPrefs.GetString(KEY_EXPIRES_AT, "0"), out var v) ? v : 0L;
            cachedUserId = ExtractSubject(accessToken);
        }

        void Save()
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                PlayerPrefs.DeleteKey(KEY_ACCESS);
                PlayerPrefs.DeleteKey(KEY_REFRESH);
                PlayerPrefs.DeleteKey(KEY_EXPIRES_AT);
            }
            else
            {
                PlayerPrefs.SetString(KEY_ACCESS, accessToken);
                PlayerPrefs.SetString(KEY_REFRESH, refreshToken ?? "");
                PlayerPrefs.SetString(KEY_EXPIRES_AT, expiresAtUnix.ToString());
            }
            PlayerPrefs.Save();
        }

        public string AccessToken => accessToken;
        public string RefreshToken => refreshToken;
        public bool HasValidToken => !string.IsNullOrEmpty(accessToken) && NowUnix() < expiresAtUnix;
        public bool HasRefreshToken => !string.IsNullOrEmpty(refreshToken);

        public string GetUserId() => cachedUserId;

        public void SetTokens(string access, string refresh, int expiresInSeconds)
        {
            accessToken = access;
            refreshToken = refresh;
            expiresAtUnix = NowUnix() + Math.Max(0, expiresInSeconds);
            cachedUserId = ExtractSubject(access);
            Save();
            OnTokenRefreshed?.Invoke(access);
        }

        public void Clear()
        {
            accessToken = null;
            refreshToken = null;
            expiresAtUnix = 0;
            cachedUserId = null;
            Save();
        }

        // public void RegisterRefreshCallback(Func<string, Task<RefreshResult>> callback)
        // {
        //     refreshCallback = callback;
        // }

        public Task<bool> TryRefreshAsync()
        {
            lock (refreshLock)
            {
                if (inFlightRefresh != null)
                {
                    Debug.Log("[TokenProvider] Refresh already in progress, returning existing task");
                    return inFlightRefresh;
                }
                // if (refreshCallback == null)
                // {
                //     Debug.LogWarning("[TokenProvider] Refresh failed: refreshCallback is NULL (ApiClient not initialized?)");
                //     return Task.FromResult(false);
                // }
                // if (string.IsNullOrEmpty(refreshToken))
                // {
                //     Debug.LogWarning($"[TokenProvider] Refresh failed: refreshToken is empty (stored value: '{refreshToken}')");
                //     return Task.FromResult(false);
                // }
                // Debug.Log("[TokenProvider] Conditions met for refresh, starting new refresh task");
                // if (refreshCallback == null || string.IsNullOrEmpty(refreshToken))
                // {
                //     Debug.LogWarning("[TokenProvider] Refresh failed: callback not registered");
                //     return Task.FromResult(false);
                // }
                Debug.Log("[TokenProvider] Starting new refresh task");
                inFlightRefresh = RunRefresh();
                Debug.Log("[TokenProvider] Refresh task started");
                return inFlightRefresh;
            }
        }

        // async Task<bool> RunRefresh()
        // {
        //     Debug.Log("[TokenProvider] Running token refresh...");
        //     try
        //     {
        //         Debug.Log("[TokenProvider] Starting token refresh...");
        //         var result = await refreshCallback(refreshToken);
        //         Debug.Log($"[TokenProvider] Refresh response: {(result?.AccessToken != null ? "valid" : "INVALID")}, raw result: {JsonConvert.SerializeObject(result)}");
        //         if (result == null)
        //         {
        //             Debug.LogWarning("[TokenProvider] Refresh failed: server returned null response");
        //         }
        //         else if (string.IsNullOrEmpty(result.AccessToken))
        //         {
        //             Debug.LogWarning($"[TokenProvider] Refresh failed: access_token empty, refresh_token='{result.RefreshToken}', expires_in={result.ExpiresInSeconds}");
        //         }
        //         if (result != null && !string.IsNullOrEmpty(result.AccessToken))
        //         {
        //             Debug.Log("[TokenProvider] Refresh successful");
        //             SetTokens(result.AccessToken, result.RefreshToken ?? refreshToken, result.ExpiresInSeconds);
        //             return true;
        //         }

        //         Debug.LogWarning("[TokenProvider] Refresh failed: Invalid response from server");
        //         Clear();
        //         OnForceReLogin?.Invoke();
        //         return false;
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogWarning($"[TokenProvider] Refresh failed with exception: {ex.Message}");
        //         Clear();
        //         OnForceReLogin?.Invoke();
        //         return false;
        //     }
        //     finally
        //     {
        //         lock (refreshLock) inFlightRefresh = null;
        //     }
        // }

        async Task<bool> RunRefresh()
        {
            Debug.Log("[TokenProvider] Running token refresh via AuthService...");
            try
            {
                var tokenPair = await AuthService.Refresh(refreshToken);

                if (tokenPair == null)
                {
                    Debug.LogWarning("[TokenProvider] Refresh failed: server returned null TokenPair");
                    Clear();
                    OnForceReLogin?.Invoke();
                    return false;
                }

                Debug.Log($"[TokenProvider] Refresh response received: access_token= {(string.IsNullOrEmpty(tokenPair.AccessToken) ? "EMPTY" : "present")}, expires_in={tokenPair.ExpiresIn}");

                // Map TokenPair to RefreshResult
                var result = new RefreshResult
                {
                    AccessToken = tokenPair.AccessToken,
                    RefreshToken = tokenPair.RefreshToken,
                    ExpiresInSeconds = tokenPair.ExpiresIn
                };

                if (string.IsNullOrEmpty(result.AccessToken))
                {
                    Debug.LogWarning($"[TokenProvider] Refresh failed: access_token empty, refresh_token='{result.RefreshToken}', expires_in={result.ExpiresInSeconds}");
                    Clear();
                    OnForceReLogin?.Invoke();
                    return false;
                }

                Debug.Log("[TokenProvider] Refresh successful");
                SetTokens(result.AccessToken, result.RefreshToken ?? refreshToken, result.ExpiresInSeconds);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TokenProvider] Refresh failed with exception: {ex.Message}");
                Clear();
                OnForceReLogin?.Invoke();
                return false;
            }
            finally
            {
                lock (refreshLock) inFlightRefresh = null;
            }
        }
        static long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        static string ExtractSubject(string jwt)
        {
            if (string.IsNullOrEmpty(jwt)) return null;
            var parts = jwt.Split('.');
            if (parts.Length < 2) return null;
            try
            {
                var payload = Base64UrlDecode(parts[1]);
                var obj = JObject.Parse(payload);
                return obj.Value<string>("sub");
            }
            catch
            {
                return null;
            }
        }

        static string Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }

        public class RefreshResult
        {
            [JsonProperty("access_token")] public string AccessToken;
            [JsonProperty("refresh_token")] public string RefreshToken;
            [JsonProperty("expires_in")] public int ExpiresInSeconds;
        }
    }
}
