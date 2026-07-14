using UnityEngine;

namespace Core.Services
{
    public static class VibrationService
    {
        public static void Vibrate()
        {
            if (Core.Managers.SettingsManager.Instance == null)
                return;
            if (!Core.Managers.SettingsManager.Instance.IsVibrationEnabled)
                return;
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
