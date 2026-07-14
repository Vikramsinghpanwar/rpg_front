using System;
using UnityEngine;
using UnityEngine.UI;
using Core.Managers;
using Core.Services;
using Core.Auth;
using Core.Session;

namespace Features.Lobby.UI
{
    public class SettingsScreen : MonoBehaviour
    {
        [Header("Toggle Buttons")]
        [SerializeField] private Button musicToggleBtn;
        [SerializeField] private Button soundToggleBtn;
        [SerializeField] private Button vibrationToggleBtn;
        [SerializeField] private Button logoutBtn;

        [Header("Toggle Icons")]
        [SerializeField] private Image musicIcon;
        [SerializeField] private Image soundIcon;
        [SerializeField] private Image vibrationIcon;

        [Header("Sprites")]
        [SerializeField] private Sprite musicOnSpr;
        [SerializeField] private Sprite musicOffSpr;
        [SerializeField] private Sprite soundOnSpr;
        [SerializeField] private Sprite soundOffSpr;
        [SerializeField] private Sprite vibOnSpr;
        [SerializeField] private Sprite vibOffSpr;

        bool _subscribed;

        void Awake()
        {
            if (musicToggleBtn != null)
                musicToggleBtn.onClick.AddListener(OnMusicClicked);
            if (soundToggleBtn != null)
                soundToggleBtn.onClick.AddListener(OnSoundClicked);
            if (vibrationToggleBtn != null)
                vibrationToggleBtn.onClick.AddListener(OnVibrationClicked);
            if (logoutBtn != null)
                logoutBtn.onClick.AddListener(OnLogoutClicked);
        }

        void OnEnable()
        {
            if (_subscribed)
                return;

            SettingsManager.Instance.OnSettingChanged += OnSettingChanged;
            SessionManager.Instance.OnUnauthenticated += OnUnauthenticated;
            _subscribed = true;

            SyncSprites();
        }

        void OnDisable()
        {
            if (!_subscribed)
                return;

            if (SettingsManager.Instance != null)
                SettingsManager.Instance.OnSettingChanged -= OnSettingChanged;
            if (SessionManager.Instance != null)
                SessionManager.Instance.OnUnauthenticated -= OnUnauthenticated;
            _subscribed = false;
        }

        void OnDestroy()
        {
            if (_subscribed)
            {
                if (SettingsManager.Instance != null)
                    SettingsManager.Instance.OnSettingChanged -= OnSettingChanged;
                if (SessionManager.Instance != null)
                    SessionManager.Instance.OnUnauthenticated -= OnUnauthenticated;
                _subscribed = false;
            }
        }

        void OnSettingChanged(SettingType type, bool _)
        {
            if (type == SettingType.Music || type == SettingType.Sound || type == SettingType.Vibration)
                SyncSprites();
        }

        void SyncSprites()
        {
            if (musicIcon != null)
                musicIcon.sprite = SettingsManager.Instance.IsMusicEnabled ? musicOnSpr : musicOffSpr;
            if (soundIcon != null)
                soundIcon.sprite = SettingsManager.Instance.IsSoundEnabled ? soundOnSpr : soundOffSpr;
            if (vibrationIcon != null)
                vibrationIcon.sprite = SettingsManager.Instance.IsVibrationEnabled ? vibOnSpr : vibOffSpr;
        }

        void OnMusicClicked()
        {
            SettingsManager.Instance.SetMusicEnabled(!SettingsManager.Instance.IsMusicEnabled);
        }

        void OnSoundClicked()
        {
            SettingsManager.Instance.SetSoundEnabled(!SettingsManager.Instance.IsSoundEnabled);
        }

        void OnVibrationClicked()
        {
            SettingsManager.Instance.SetVibrationEnabled(!SettingsManager.Instance.IsVibrationEnabled);
        }

        void OnLogoutClicked()
        {
            Core.Managers.PopupManager.Instance.ShowConfirm(
                "Logout",
                "Are you sure you want to logout?",
                "Logout",
                "Cancel",
                onConfirm: async () => await DoLogout(),
                onCancel: null
            );
        }

        async System.Threading.Tasks.Task DoLogout()
        {
            await SessionManager.Instance.SignOut();
        }

        void OnUnauthenticated()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Auth");
        }
    }
}
