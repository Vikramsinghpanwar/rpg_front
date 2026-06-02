using System;
using System.Threading.Tasks;
using Core.Auth;
using Core.Bootstrap;
using Core.Models;
using UnityEngine;

namespace Core.Session
{
    // Top-level orchestrator. Place a [SessionManager] GameObject in your boot scene (or
    // call SessionManager.Instance.Initialize() from your AppRoot). It:
    //   1. Runs AuthManager.Initialize() to restore the prior session (if any).
    //   2. If authenticated, kicks BootstrapService.Refresh() so wallet/profile/lobby are
    //      ready by the time the first feature screen opens.
    //   3. Fires OnSessionReady or OnUnauthenticated so the scene can decide whether to
    //      show the login screen or the lobby.
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        public event Action<BootstrapResponse> OnSessionReady;
        public event Action OnUnauthenticated;

        public bool IsReady { get; private set; }
        public BootstrapResponse Current => BootstrapService.Instance?.Current;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[SessionManager]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<SessionManager>();
        }

        public async Task Initialize()
        {
            IsReady = false;
            await AuthManager.Instance.Initialize();

            if (!AuthManager.Instance.IsAuthenticated)
            {
                OnUnauthenticated?.Invoke();
                return;
            }

            await BootstrapService.Instance.Refresh();
            IsReady = true;
            OnSessionReady?.Invoke(BootstrapService.Instance.Current);
        }

        // Sign in flows return here so the screen doesn't need to wire bootstrap separately.
        public async Task<bool> SignInWithOtp(string challengeId, string code)
        {
            await AuthManager.Instance.VerifyOtp(challengeId, code);
            if (!AuthManager.Instance.IsAuthenticated) return false;
            await BootstrapService.Instance.Refresh();
            IsReady = true;
            OnSessionReady?.Invoke(BootstrapService.Instance.Current);
            return true;
        }

        public async Task<bool> SignInWithGoogle(string idToken)
        {
            await AuthManager.Instance.LoginWithGoogle(idToken);
            if (!AuthManager.Instance.IsAuthenticated) return false;
            await BootstrapService.Instance.Refresh();
            IsReady = true;
            OnSessionReady?.Invoke(BootstrapService.Instance.Current);
            return true;
        }

        public async Task SignOut()
        {
            await AuthManager.Instance.Logout();
            IsReady = false;
            OnUnauthenticated?.Invoke();
        }
    }
}
