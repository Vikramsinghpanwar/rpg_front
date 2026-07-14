namespace Core.Notifications
{
    using System;
    using System.Threading.Tasks;
    using Core.API;
    using UnityEngine;

    public static class DeviceRegistrationService
    {
        static string _lastSentToken;
        static DateTime _lastSentAt;
        const int DebounceMs = 5000;

        public static async Task RegisterDeviceAsync(string fcmToken)
        {
            if (string.IsNullOrEmpty(fcmToken))
            {
                Debug.LogWarning("[DeviceRegistration] Skipped: empty FCM token");
                return;
            }

            var now = DateTime.UtcNow;
            if (fcmToken == _lastSentToken && (now - _lastSentAt).TotalMilliseconds < DebounceMs)
            {
                Debug.Log("[DeviceRegistration] Debounced: same token within window");
                return;
            }

            if (!Core.Auth.AuthManager.Instance.IsAuthenticated)
            {
                Debug.LogWarning("[DeviceRegistration] Skipped: user not authenticated");
                return;
            }

            try
            {
                var request = new DeviceRegisterRequest
                {
                    FcmToken = fcmToken,
                    DeviceId = SystemInfo.deviceUniqueIdentifier,
                    Platform = "android",
                    AppVersion = Application.version
                };

                await ApiClient.Instance.Post<object>(Core.API.Endpoints.DeviceRoutes.Register, request);

                _lastSentToken = fcmToken;
                _lastSentAt = now;
                Debug.Log("[DeviceRegistration] Success");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[DeviceRegistration] Failed: " + ex.Message);
            }
        }

        public static void Reset()
        {
            _lastSentToken = null;
            _lastSentAt = default;
        }
    }
}
