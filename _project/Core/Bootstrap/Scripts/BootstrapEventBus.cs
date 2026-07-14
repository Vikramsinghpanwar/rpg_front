using System;

namespace Core.Bootstrap
{
    /// <summary>
    /// Central event bus for bootstrap-stage lifecycle events.
    /// Milestone 2+3: infrastructure only. No subscribers yet.
    /// Future milestones will raise these events from AppBootstrapper
    /// and consume them from Auth / Session / Version systems.
    /// </summary>
    public static class BootstrapEventBus
    {
        public static event Action<string> OnSessionReady;
        public static event Action<string> OnSessionFailed;
        public static event Action OnUnauthenticated;
        public static event Action OnVersionGateCleared;
        public static event Action OnVersionGateBlocked;

        public static void RaiseSessionReady(string message = null)
        {
            OnSessionReady?.Invoke(message);
        }

        public static void RaiseSessionFailed(string reason)
        {
            OnSessionFailed?.Invoke(reason);
        }

        public static void RaiseUnauthenticated()
        {
            OnUnauthenticated?.Invoke();
        }

        public static void RaiseVersionGateCleared()
        {
            OnVersionGateCleared?.Invoke();
        }

        public static void RaiseVersionGateBlocked()
        {
            OnVersionGateBlocked?.Invoke();
        }

        /// <summary>
        /// Milestone 2+3: clear all subscriptions. Call on shutdown / logout if needed.
        /// </summary>
        public static void Reset()
        {
            OnSessionReady = null;
            OnSessionFailed = null;
            OnUnauthenticated = null;
            OnVersionGateCleared = null;
            OnVersionGateBlocked = null;
        }
    }
}
