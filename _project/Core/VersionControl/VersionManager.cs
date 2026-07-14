using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Core.Services;
using Features.Profile.Services;
using UnityEngine;

namespace Core.VersionControl
{
    public class VersionManager : MonoBehaviour
    {
        public static VersionManager Instance { get; private set; }

        [Header("Default Request Settings")]
        [SerializeField] private string platform = "android";
        [SerializeField] private string appFlavor = "";
        [SerializeField] private string country = "IN";

        public event Action<VersionInfo> OnCheckCompleted;
        public event Action<VersionInfo> OnOutcomeChanged;
        public event Action<Exception, string> OnCheckFailed;

        public bool IsActive { get; private set; }
        public bool HasData => CurrentInfo != null;
        [SerializeField] public VersionInfo CurrentInfo { get; private set; }
        public DateTime LastCheckTimeUtc => CurrentInfo?.LastCheckTimeUtc ?? DateTime.MinValue;
        [SerializeField] public VersionStatus CurrentStatus => CurrentInfo?.Status ?? VersionStatus.UpToDate;
        public string CurrentOutcome => CurrentInfo?.Outcome ?? string.Empty;
        public bool IsForceBlocked => CurrentStatus == VersionStatus.ForceUpdate || CurrentStatus == VersionStatus.Unsupported;
        public bool IsMaintenance => CurrentStatus == VersionStatus.Maintenance;
        public bool IsSoftUpdate => CurrentStatus == VersionStatus.SoftUpdate;
        public bool IsUpToDate => CurrentStatus == VersionStatus.UpToDate;

        readonly ConcurrentQueue<VersionAnalyticsEvent> analyticsQueue = new ConcurrentQueue<VersionAnalyticsEvent>();
        bool analyticsProcessing;
        bool inFlight;
        bool softShownThisLaunch;
        float lastResumeRecheckTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[VersionManager]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<VersionManager>();
            Instance.CollectDefaultRequestFields();
        }

        void CollectDefaultRequestFields()
        {
            if (Application.platform == RuntimePlatform.Android)
                platform = "android";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                platform = "ios";
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
                platform = "web";
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                platform = "windows";
            else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
                platform = "macos";
            else
                platform = Application.platform.ToString().ToLowerInvariant();
            platform = PlayerPrefs.GetString("vc_platform", platform);
            if (string.IsNullOrEmpty(platform)) platform = "unknown";
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            _ = ProcessAnalyticsQueueLoop();
        }

        public VersionCheckRequest BuildDefaultRequest()
        {
            var req = new VersionCheckRequest
            {
                Platform = NormalizePlatform(platform),
                CurrentVersion = Application.version,
                BuildNumber = GetBuildNumber(),
                Environment = GetEnvironment(),
                Locale = GetLocale(),
                Country = GetCountry(),
                AppFlavor = string.IsNullOrEmpty(appFlavor) ? null : appFlavor
            };

            var tp = TokenProvider.Instance;
            if (tp != null && !string.IsNullOrEmpty(tp.AccessToken))
            {
                var userId = ProfileService.GetCurrentUserId();
                if (!string.IsNullOrEmpty(userId))
                    req.UserId = userId;
            }

            return req;
        }

        string GetBuildNumber()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using (var packageManager = context.Call<AndroidJavaObject>("getPackageManager"))
                using (var packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", context.Call<string>("getPackageName"), 0))
                {
                    return packageInfo.Get<int>("versionCode").ToString();
                }
            }
            catch { }
#endif
            return PlayerPrefs.GetInt("BuildNumber", 0).ToString();
        }

        string GetEnvironment()
        {
#if UNITY_EDITOR
            return "INTERNAL";
#elif DEVELOPMENT_BUILD
            return "INTERNAL";
#else
            return "PRODUCTION";
#endif
        }

        string GetLocale()
        {
            try
            {
                var culture = System.Globalization.CultureInfo.CurrentCulture;
                if (!string.IsNullOrEmpty(culture?.Name))
                    return culture.Name;
            }
            catch { }
            return "en-US";
        }

        string GetCountry()
        {
            try
            {
                var culture = System.Globalization.CultureInfo.CurrentCulture;
                if (!string.IsNullOrEmpty(culture?.Name))
                {
                    var parts = culture.Name.Split('-');
                    if (parts.Length > 1) return parts[1].ToUpperInvariant();
                }
            }
            catch { }
            return PlayerPrefs.GetString("vc_country", "IN");
        }

        string NormalizePlatform(string platform)
        {
            if (string.IsNullOrEmpty(platform)) return "DESKTOP";
            switch (platform.ToLowerInvariant())
            {
                case "android": return "ANDROID";
                case "ios":
                case "iphoneplayer": return "IOS";
                case "web":
                case "webglplayer": return "WEB";
                default: return "DESKTOP";
            }
        }

        public void Initialize(VersionCheckRequest defaultReq)
        {
            VersionService.Instance.Initialize(defaultReq);
            VersionLog.OutcomeHandled(Guid.NewGuid().ToString("N"), "initialized");
        }

        public void SetOutcome(VersionInfo info)
        {
            if (info == null) return;
            CurrentInfo = info;
            OnCheckCompleted?.Invoke(CurrentInfo);
            OnOutcomeChanged?.Invoke(CurrentInfo);
        }

        public bool IsInitialized => VersionService.Instance.IsInitialized;

        public async Task<bool> PerformVersionCheckAsync(VersionCheckRequest overrideReq = null)
        {
            if (inFlight)
            {
                Debug.LogWarning("[VC] Check skipped: already in-flight");
                return false;
            }

            inFlight = true;
            IsActive = true;
            var req = overrideReq ?? BuildDefaultRequest();
            var checkId = Guid.NewGuid().ToString("N");
            var start = DateTime.UtcNow;

            bool result = false;
            try
            {
                VersionLog.Request(checkId, req);
                var (resp, err) = await VersionService.Instance.CheckAsync(req);

                if (err != null)
                {
                    CurrentInfo = new VersionInfo
                    {
                        Status = VersionStatus.ForceUpdate,
                        Outcome = "error",
                        LastCheckTimeUtc = DateTime.UtcNow
                    };
                    OnCheckFailed?.Invoke(new Exception(err), checkId);
                    OnCheckCompleted?.Invoke(CurrentInfo);
                    OnOutcomeChanged?.Invoke(CurrentInfo);
                    EnqueueAnalytics(new VersionAnalyticsEvent
                    {
                        EventName = "VersionCheckFailed",
                        CurrentVersion = req.CurrentVersion,
                        Platform = req.Platform,
                        BuildNumber = req.BuildNumber,
                        Error = err,
                        TimestampUtc = start
                    });
                    inFlight = false;
                    return false;
                }

                VersionLog.Success(checkId, resp, req, start);

                if (!resp.IsValid)
                {
                    VersionLog.Failed(checkId, req, "invalid_response_missing_gate_or_status", 0, start);
                    CurrentInfo = new VersionInfo
                    {
                        Status = VersionStatus.ForceUpdate,
                        Outcome = "invalid_response",
                        LastCheckTimeUtc = DateTime.UtcNow
                    };
                    OnCheckCompleted?.Invoke(CurrentInfo);
                    OnOutcomeChanged?.Invoke(CurrentInfo);
                    EnqueueAnalytics(new VersionAnalyticsEvent
                    {
                        EventName = "VersionCheckFailed",
                        CurrentVersion = req.CurrentVersion,
                        LatestVersion = resp.LatestVersion,
                        Platform = req.Platform,
                        BuildNumber = req.BuildNumber,
                        Outcome = "invalid_response",
                        Environment = req.Environment,
                        Locale = req.Locale,
                        UserId = req.UserId,
                        Country = req.Country,
                        AppFlavor = req.AppFlavor,
                        TimestampUtc = start,
                        Error = "invalid_response_missing_gate_or_status"
                    });
                    inFlight = false;
                    return false;
                }

                var statusStr = resp.Status.ToString();

                CurrentInfo = new VersionInfo
                {
                    Status = resp.Status,
                    LatestVersion = resp.LatestVersion,
                    UpdateUrl = resp.UpdateUrl,
                    ReleaseNotes = resp.ReleaseNotes,
                    MaintenanceMessage = resp.MaintenanceMessage,
                    MaintenanceUntil = resp.MaintenanceUntil,
                    CanContinue = resp.CanContinue,
                    LastCheckTimeUtc = DateTime.UtcNow,
                    Outcome = statusStr
                };

                string eventName;
                switch (resp.Status)
                {
                    case VersionStatus.UpToDate:
                        eventName = "VersionCheckSucceeded";
                        softShownThisLaunch = false;
                        result = true;
                        break;
                    case VersionStatus.SoftUpdate:
                        eventName = !softShownThisLaunch ? "SoftUpdateShown" : "VersionCheckSucceeded";
                        softShownThisLaunch = true;
                        result = true;
                        break;
                    case VersionStatus.ForceUpdate:
                    case VersionStatus.Unsupported:
                        eventName = "ForceUpdateShown";
                        result = false;
                        break;
                    case VersionStatus.Maintenance:
                        eventName = "MaintenanceShown";
                        result = false;
                        break;
                    default:
                        VersionLog.Failed(checkId, req, "unknown_status_" + statusStr, 0, start);
                        CurrentInfo = new VersionInfo
                        {
                            Status = VersionStatus.ForceUpdate,
                            Outcome = "unknown_status",
                            LastCheckTimeUtc = DateTime.UtcNow
                        };
                        OnCheckCompleted?.Invoke(CurrentInfo);
                        OnOutcomeChanged?.Invoke(CurrentInfo);
                        EnqueueAnalytics(new VersionAnalyticsEvent
                        {
                            EventName = "VersionCheckFailed",
                            CurrentVersion = req.CurrentVersion,
                            LatestVersion = resp.LatestVersion,
                            Platform = req.Platform,
                            BuildNumber = req.BuildNumber,
                            Outcome = "unknown_status",
                            Environment = req.Environment,
                            Locale = req.Locale,
                            UserId = req.UserId,
                            Country = req.Country,
                            AppFlavor = req.AppFlavor,
                            TimestampUtc = start,
                            Error = "unknown_status_" + statusStr
                        });
                        inFlight = false;
                        return false;
                }

                EnqueueAnalytics(new VersionAnalyticsEvent
                {
                    EventName = eventName,
                    CurrentVersion = req.CurrentVersion,
                    LatestVersion = resp.LatestVersion,
                    Platform = req.Platform,
                    BuildNumber = req.BuildNumber,
                    Outcome = statusStr,
                    Environment = req.Environment,
                    Locale = req.Locale,
                    UserId = req.UserId,
                    Country = req.Country,
                    AppFlavor = req.AppFlavor,
                    TimestampUtc = start
                });

                OnCheckCompleted?.Invoke(CurrentInfo);
                OnOutcomeChanged?.Invoke(CurrentInfo);
                inFlight = false;
                return result;
            }
            catch (Exception ex)
            {
                VersionLog.Failed(checkId, req, ex.Message, 0, start);
                CurrentInfo = new VersionInfo
                {
                    Status = VersionStatus.ForceUpdate,
                    Outcome = "exception",
                    LastCheckTimeUtc = DateTime.UtcNow
                };
                OnCheckFailed?.Invoke(ex, checkId);
                OnCheckCompleted?.Invoke(CurrentInfo);
                OnOutcomeChanged?.Invoke(CurrentInfo);
                EnqueueAnalytics(new VersionAnalyticsEvent
                {
                    EventName = "VersionCheckFailed",
                    CurrentVersion = req.CurrentVersion,
                    Platform = req.Platform,
                    BuildNumber = req.BuildNumber,
                    Error = ex.Message,
                    TimestampUtc = start
                });
                inFlight = false;
                return false;
            }
        }

        public async Task RecheckOnResumeAsync()
        {
            if (!HasData) return;
            if (Time.time - lastResumeRecheckTime < 2f) return;
            lastResumeRecheckTime = Time.time;

            switch (CurrentStatus)
            {
                case VersionStatus.SoftUpdate:
                case VersionStatus.ForceUpdate:
                case VersionStatus.Unsupported:
                case VersionStatus.Maintenance:
                    VersionLog.ResumeRecheck(Guid.NewGuid().ToString("N"), CurrentStatus.ToString());
                    await PerformVersionCheckAsync();
                    break;
            }
        }

        void EnqueueAnalytics(VersionAnalyticsEvent evt)
        {
            if (evt == null) return;
            analyticsQueue.Enqueue(evt);
        }

        async Task ProcessAnalyticsQueueLoop()
        {
            while (true)
            {
                await Task.Yield();
                if (analyticsProcessing) continue;
                if (analyticsQueue.IsEmpty) continue;

                analyticsProcessing = true;
                while (analyticsQueue.TryDequeue(out var evt))
                {
                    await ProcessAnalytics(evt);
                }
                analyticsProcessing = false;
            }
        }

        Task ProcessAnalytics(VersionAnalyticsEvent evt)
        {
            return null;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) return;
            _ = RecheckOnResumeAsync();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                _ = RecheckOnResumeAsync();
            }
        }

        public void ResetSoftShown()
        {
            softShownThisLaunch = false;
        }
    }
}
