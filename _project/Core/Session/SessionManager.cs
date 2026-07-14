using System;
using System.Threading.Tasks;
using Core.API;
using Core.Auth;
using Core.Bootstrap;
using Core.Models;
using Core.VersionControl;
using UnityEngine;

namespace Core.Session
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        public event Action<BootstrapResponse> OnSessionReady;
        public event Action<string> OnSessionFailed;
        public event Action OnUnauthenticated;

        public bool IsReady { get; private set; }
        public bool VersionGateCleared { get; private set; }
        public BootstrapResponse Current => BootstrapService.Instance?.Current;

        readonly object _initLock = new object();
        bool _initializing;

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
            lock (_initLock)
            {
                if (_initializing) return;
                _initializing = true;
            }

            IsReady = false;
            VersionGateCleared = false;
            var versionFailReason = (string)null;

            try
            {
                await AuthManager.Instance.Initialize();

                if (!AuthManager.Instance.IsAuthenticated)
                {
                    OnUnauthenticated?.Invoke();
                    return;
                }

                await BootstrapService.Instance.Refresh();

                bool gateEvaluated = await EvaluateBootstrapGateAsync();

                if (!gateEvaluated)
                {
                    await RunVersionGate();
                }

                if (!VersionGateCleared)
                {
                    if (!VersionManager.Instance.IsForceBlocked && !VersionManager.Instance.IsMaintenance)
                    {
                        versionFailReason = "version_check_incomplete";
                    }
                    else
                    {
                        versionFailReason = "version_blocked";
                    }
                }
                else
                {
                    IsReady = true;
                    OnSessionReady?.Invoke(BootstrapService.Instance.Current);
                }
            }
            catch (ApiException ex)
            {
                VersionLog.Failed(Guid.NewGuid().ToString("N"), null, ex.Message, ex.StatusCode, DateTime.UtcNow);
                OnSessionFailed?.Invoke($"api:{ex.StatusCode}:{ex.Message}");
            }
            catch (Exception ex)
            {
                VersionLog.Failed(Guid.NewGuid().ToString("N"), null, ex.Message, 0, DateTime.UtcNow);
                OnSessionFailed?.Invoke(ex.Message);
            }
            finally
            {
                lock (_initLock) _initializing = false;
            }
        }

        /// <summary>
        /// PRIMARY startup gate: reads lobby.data.gate from the bootstrap response.
        /// Returns true if a valid gate was present and processed.
        /// Sets VersionGateCleared based on the gate action.
        /// </summary>
        string UpdateUrlFromGate(BootstrapGateData gate)
        {
            if (gate == null) return null;
            if (!string.IsNullOrEmpty(gate.updateUrl)) return gate.updateUrl;
            return Constants.APK_LANDING_PAGE_URL;
        }

        async Task<bool> EvaluateBootstrapGateAsync()
        {
            var bootstrap = BootstrapService.Instance?.Current;
            var gate = bootstrap?.lobby?.data?.gate;

            if (gate == null || string.IsNullOrEmpty(gate.action))
            {
                Debug.LogWarning("[BootstrapGate] Missing — falling back to RunVersionGate");
                return false;
            }

            string action = gate.action.ToUpperInvariant();
            Debug.Log($"[BootstrapGate] Action={action} currentVersion={gate.currentVersion} minVersion={gate.minVersion} latestVersion={gate.latestVersion}");

            VersionStatus? status = action switch
            {
                "ALLOW" => VersionStatus.UpToDate,
                "SOFT_UPDATE" => VersionStatus.SoftUpdate,
                "FORCE_UPDATE" => VersionStatus.ForceUpdate,
                "UNSUPPORTED" => VersionStatus.Unsupported,
                "MAINTENANCE" => VersionStatus.Maintenance,
                _ => (VersionStatus?)null
            };

            if (!status.HasValue)
            {
                Debug.LogWarning($"[BootstrapGate] Unknown action={action}, falling back to RunVersionGate");
                return false;
            }

            bool blocked = status.Value is VersionStatus.ForceUpdate
                        or VersionStatus.Unsupported
                        or VersionStatus.Maintenance;

            Debug.Log($"[BootstrapGate] VersionStatus={status.Value} Decision={(blocked ? "BLOCK" : "ALLOW")} StartupBlocked={blocked}");

            VersionGateCleared = !blocked;

            if (VersionManager.Instance != null)
            {
                if (VersionUIController.Instance != null)
                {
                    VersionUIController.Instance.gameObject.SetActive(true);
                }

                var maintenance = gate.maintenance;
                var info = new VersionInfo
                {
                    Status = status.Value,
                    LatestVersion = gate.latestVersion,
                    UpdateUrl = UpdateUrlFromGate(gate),
                    ReleaseNotes = gate.releaseNotes,
                    MaintenanceMessage = maintenance != null ? maintenance.message : null,
                    MaintenanceUntil = maintenance != null ? maintenance.until : null,
                    LastCheckTimeUtc = DateTime.UtcNow,
                    Outcome = "bootstrap_gate"
                };

                VersionManager.Instance.SetOutcome(info);
            }

            return true;
        }

        async Task RunVersionGate()
        {
            VersionGateCleared = false;

            if (VersionManager.Instance == null)
            {
                VersionGateCleared = true;
                return;
            }

            Debug.Log("[LegacyVersionGate] Initialize()");

            var defaultReq = VersionManager.Instance.BuildDefaultRequest();
            VersionManager.Instance.Initialize(defaultReq);

            VersionUIController.Instance.gameObject.SetActive(true);

            Debug.Log("[LegacyVersionGate] PerformVersionCheckAsync()");
            bool ok = await VersionManager.Instance.PerformVersionCheckAsync();
            Debug.Log($"[LegacyVersionGate] PerformVersionCheckAsync result={ok}");
            if (!ok)
            {
                VersionGateCleared = false;
                return;
            }

            var status = VersionManager.Instance.CurrentStatus;
            switch (status)
            {
                case VersionStatus.UpToDate:
                    VersionGateCleared = true;
                    break;
                case VersionStatus.SoftUpdate:
                    VersionGateCleared = true;
                    break;
                case VersionStatus.Maintenance:
                    VersionGateCleared = false;
                    break;
                case VersionStatus.ForceUpdate:
                case VersionStatus.Unsupported:
                    VersionGateCleared = false;
                    break;
            }

            Debug.Log($"[LegacyVersionGate] Final decision=VersionGateCleared={VersionGateCleared}");
        }

        public async void ForceVersionRecheck()
        {
            if (VersionManager.Instance == null) return;
            VersionGateCleared = false;
            IsReady = false;

            bool gateEvaluated = await EvaluateBootstrapGateAsync();
            if (!gateEvaluated)
            {
                await RunVersionGate();
            }

            if (VersionGateCleared)
            {
                IsReady = true;
                OnSessionReady?.Invoke(BootstrapService.Instance.Current);
            }
        }

        public async Task<bool> SignInWithOtp(string challengeId, string code)
        {
            await AuthManager.Instance.VerifyOtp(challengeId, code);
            if (!AuthManager.Instance.IsAuthenticated) return false;
            await BootstrapService.Instance.Refresh();

            bool gateEvaluated = await EvaluateBootstrapGateAsync();
            if (gateEvaluated && VersionGateCleared)
            {
                IsReady = true;
                Toast.Instance.ShowSuccess("Signed in successfully.");
                OnSessionReady?.Invoke(BootstrapService.Instance.Current);
            }
            else if (!gateEvaluated)
            {
                await RunVersionGate();
                if (VersionGateCleared)
                {
                    IsReady = true;
                    Toast.Instance.ShowSuccess("Signed in successfully.");
                    OnSessionReady?.Invoke(BootstrapService.Instance.Current);
                }
            }

            return true;
        }

        public async Task<bool> SignInWithGoogle(string idToken)
        {
            await AuthManager.Instance.LoginWithGoogle(idToken);
            if (!AuthManager.Instance.IsAuthenticated) return false;
            await BootstrapService.Instance.Refresh();

            bool gateEvaluated = await EvaluateBootstrapGateAsync();
            if (gateEvaluated && VersionGateCleared)
            {
                IsReady = true;
                Toast.Instance.ShowSuccess("Signed in successfully.");
                OnSessionReady?.Invoke(BootstrapService.Instance.Current);
            }
            else if (!gateEvaluated)
            {
                await RunVersionGate();
                if (VersionGateCleared)
                {
                    IsReady = true;
                    Toast.Instance.ShowSuccess("Signed in successfully.");
                    OnSessionReady?.Invoke(BootstrapService.Instance.Current);
                }
            }

            return true;
        }

        public async Task SignOut()
        {
            await AuthManager.Instance.Logout();
            IsReady = false;
            VersionGateCleared = false;
            OnUnauthenticated?.Invoke();
        }
    }
}
