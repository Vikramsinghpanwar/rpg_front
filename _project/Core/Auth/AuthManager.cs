using System;
using System.Threading.Tasks;
using Core.API;
using Core.Services;
using UnityEngine;

namespace Core.Auth
{
    // Stateful auth orchestrator. Owns:
    //   • the current AuthState (transitions fire OnStateChanged)
    //   • the cached AuthMe (decoded from JWT introspection)
    //   • a forced-logout pathway when refresh fails
    //
    // Tokens are persisted by TokenProvider, NOT here — TokenProvider also drives the
    // refresh-on-401 inside ApiClient. AuthManager just sits on top and orchestrates the
    // user-visible flow (sign in with OTP, get profile, log out, listen for re-login signal).
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        public event Action<AuthState> OnStateChanged;

        public AuthState CurrentState { get; private set; } = AuthState.Uninitialized;
        public AuthMe CurrentUser { get; private set; }
        public bool IsAuthenticated => CurrentState == AuthState.Authenticated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[AuthManager]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<AuthManager>();
            Instance.WireTokenProviderEvents();
        }

        void WireTokenProviderEvents()
        {
            var tp = TokenProvider.Instance;
            if (tp == null) return;
            tp.OnForceReLogin += HandleForceReLogin;
        }

        void OnDestroy()
        {
            if (TokenProvider.Instance != null)
                TokenProvider.Instance.OnForceReLogin -= HandleForceReLogin;
        }

        // Run once at app startup (or after scene reload). Loads stored tokens via
        // TokenProvider and probes /auth/me to confirm the session is still valid.
        public async Task Initialize()
        {
            var tp = TokenProvider.Instance;
            if (tp == null || string.IsNullOrEmpty(tp.AccessToken))
            {
                SetState(AuthState.Unauthenticated);
                return;
            }

            // Proactive refresh: try to refresh if token is expired or about to expire
            if (!tp.HasValidToken && tp.HasRefreshToken)
            {
                Debug.Log("[AuthManager] Access token expired, attempting refresh...");
                bool refreshed = await tp.TryRefreshAsync();
                if (!refreshed)
                {
                    Debug.Log("[AuthManager] Refresh failed, user must re-authenticate");
                    SetState(AuthState.Unauthenticated);
                    return;
                }
                Debug.Log("[AuthManager] Refresh successful");
            }

            try
            {
                CurrentUser = await AuthService.GetMe();
                SetState(AuthState.Authenticated);
            }
            catch (ApiException e) when (e.StatusCode == 401)
            {
                SetState(AuthState.Unauthenticated);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AuthManager] Initialize failed: {ex.Message}");
                SetState(AuthState.Unauthenticated);
            }
        }

        // ── OTP flow ────────────────────────────────────────────────────────────
        public Task<OtpRequestResponse> RequestOtp(string mobile, string channel = AuthChannels.Sms)
            => AuthService.RequestOtp(mobile, channel);

        public async Task<PlayerLoginResponse> VerifyOtp(string challengeId, string code)
        {
            var response = await AuthService.VerifyOtp(challengeId, code);
            ApplyLogin(response);
            return response;
        }

        // ── Google flow ────────────────────────────────────────────────────────
        public async Task<PlayerLoginResponse> LoginWithGoogle(string idToken)
        {
            var response = await AuthService.LoginWithGoogle(idToken);
            ApplyLogin(response);
            return response;
        }

        // ── Logout ─────────────────────────────────────────────────────────────
        public async Task Logout()
        {
            var tp = TokenProvider.Instance;
            var refreshToken = tp?.RefreshToken;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try { await AuthService.Logout(refreshToken); }
                catch (Exception e) { Debug.LogWarning($"[AuthManager] Logout API error (continuing): {e.Message}"); }
            }
            ForceLogout();
        }

        public void ForceLogout()
        {
            CurrentUser = null;
            TokenProvider.Instance?.Clear();
            SetState(AuthState.Unauthenticated);
        }

        void HandleForceReLogin()
        {
            CurrentUser = null;
            SetState(AuthState.SessionExpired);
            // SessionExpired → Unauthenticated so the login screen can take over.
            SetState(AuthState.Unauthenticated);
        }

        void ApplyLogin(PlayerLoginResponse response)
        {
            if (response == null || string.IsNullOrEmpty(response.AccessToken))
            {
                SetState(AuthState.Unauthenticated);
                return;
            }
            TokenProvider.Instance.SetTokens(
                response.AccessToken,
                response.RefreshToken,
                response.ExpiresIn);
            // Build a minimal AuthMe from the login response so the UI has a user without a roundtrip.
            CurrentUser = new AuthMe
            {
                Sub = response.User?.Id,
                Email = response.User?.Email
            };
            SetState(AuthState.Authenticated);
        }

        void SetState(AuthState next)
        {
            if (CurrentState == next) return;
            CurrentState = next;
            OnStateChanged?.Invoke(next);
        }
    }
}
