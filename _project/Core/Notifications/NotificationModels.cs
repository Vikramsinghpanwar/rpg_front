namespace Core.Notifications
{
    using Newtonsoft.Json;

    public class DeviceRegisterRequest
    {
        [JsonProperty("fcm_token")] public string FcmToken;
        [JsonProperty("device_id")] public string DeviceId;
        [JsonProperty("platform")] public string Platform;
        [JsonProperty("app_version")] public string AppVersion;
    }

    public class PushNotificationPayload
    {
        public string type;
        public string title;
        public string body;
        public NotificationData data;
    }

    public class NotificationData
    {
        public string screen;
        public long amount;
        public string transactionId;
    }

    public enum NotificationType
    {
        Unknown,
        Admin,
        RechargeSuccess,
        WithdrawalSuccess,
        ReferralBonus
    }
}
