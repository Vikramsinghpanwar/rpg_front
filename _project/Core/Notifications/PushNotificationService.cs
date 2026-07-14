namespace Core.Notifications
{
    using System;
    using Firebase;
    using Firebase.Messaging;
    using Newtonsoft.Json;
    using UnityEngine;

    public class PushNotificationService : MonoBehaviour
    {
        public static PushNotificationService Instance { get; private set; }

        bool _initialized;

        public static void EnsureInitialized()
        {
            if (Instance != null) return;
            var go = new GameObject("[PushNotificationService]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<PushNotificationService>();
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    Debug.Log("[PushNotificationService] Firebase Ready");
                    CreateAndroidNotificationChannel();
                    FirebaseMessaging.TokenReceived += OnTokenReceived;
                    FirebaseMessaging.MessageReceived += OnMessageReceived;

                    FirebaseMessaging.RequestPermissionAsync();
                }
                else
                {
                    Debug.LogError("[PushNotificationService] Firebase Dependency Error: " + task.Result);
                }
            });
        }

        void CreateAndroidNotificationChannel()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var notificationManager = activity.Call<AndroidJavaObject>("getSystemService", "notification"))
                {
                    if (notificationManager == null)
                    {
                        Debug.LogWarning("[PushNotificationService] Android NotificationManager is null");
                        return;
                    }

                    var channelClass = new AndroidJavaClass("android/app/Notification$Channel");
                    var channel = channelClass.Call<AndroidJavaObject>("<init>", "default", "Default", 4);
                    notificationManager.Call("createNotificationChannel", channel);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[PushNotificationService] Channel setup failed: " + ex.Message);
            }
#endif
        }

        void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            Debug.Log("[PushNotificationService] TokenReceived: " + token.Token);
            PlayerPrefs.SetString("fcm_token", token.Token);
            PlayerPrefs.Save();

            _ = DeviceRegistrationService.RegisterDeviceAsync(token.Token);
        }

        void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.Log("[PushNotificationService] MessageReceived from: " + e.Message.From);

            if (e.Message == null)
            {
                Debug.LogWarning("[PushNotificationService] Ignored: message is null");
                return;
            }

            var data = e.Message.Data;
            if (data == null || data.Count == 0)
            {
                Debug.LogWarning("[PushNotificationService] Ignored: message has no data payload");
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(data);
                var payload = JsonConvert.DeserializeObject<PushNotificationPayload>(json);
                if (payload == null)
                {
                    Debug.LogWarning("[PushNotificationService] Failed to parse payload");
                    return;
                }

                Debug.Log($"[PushNotificationService] Payload type={payload.type}, title={payload.title}");

                var type = ParseType(payload.type);
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (Toast.Instance != null && !string.IsNullOrEmpty(payload.title))
                    {
                        Toast.Instance.ShowSuccess(payload.title);
                    }

                    NotificationRouter.Enqueue(type, payload.data);
                    NotificationRouter.TryFlush();
                });
            }
            catch (Exception ex)
            {
                Debug.LogError("[PushNotificationService] Payload parse error: " + ex.Message);
            }
        }

        static NotificationType ParseType(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return NotificationType.Unknown;

            return raw.ToUpperInvariant() switch
            {
                "ADMIN" => NotificationType.Admin,
                "RECHARGE_SUCCESS" => NotificationType.RechargeSuccess,
                "WITHDRAWAL_SUCCESS" => NotificationType.WithdrawalSuccess,
                "REFERRAL_BONUS" => NotificationType.ReferralBonus,
                _ => NotificationType.Unknown
            };
        }
    }
}
