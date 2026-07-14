using System;
using UnityEngine;

namespace Core.Managers
{
    public enum SettingType
    {
        Music,
        Sound,
        Vibration
    }

    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        const string KEY_MUSIC = "settings.music.enabled";
        const string KEY_SOUND = "settings.sound.enabled";
        const string KEY_VIBRATION = "settings.vibration.enabled";
        const string KEY_MIGRATION = "settings.migration.v1";

        const int DEFAULT_MUSIC = 1;
        const int DEFAULT_SOUND = 1;
        const int DEFAULT_VIBRATION = 1;

        public event Action<SettingType, bool> OnSettingChanged;

        bool _music = true;
        bool _sound = true;
        bool _vibration = true;

        public static void Initialize()
        {
            if (Instance == null)
            {
                var go = new GameObject("[SettingsManager]");
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<SettingsManager>();
            }
            Instance.RunMigration();
            Instance.Load();
        }

        void RunMigration()
        {
            if (PlayerPrefs.HasKey(KEY_MIGRATION))
                return;

            if (!PlayerPrefs.HasKey(KEY_MUSIC))
                PlayerPrefs.SetInt(KEY_MUSIC, PlayerPrefs.GetInt("isMusicOn", DEFAULT_MUSIC));

            if (!PlayerPrefs.HasKey(KEY_SOUND))
                PlayerPrefs.SetInt(KEY_SOUND, PlayerPrefs.GetInt("isSoundOn", DEFAULT_SOUND));

            if (!PlayerPrefs.HasKey(KEY_VIBRATION))
                PlayerPrefs.SetInt(KEY_VIBRATION, PlayerPrefs.GetInt("isVibrationOn", DEFAULT_VIBRATION));

            PlayerPrefs.SetInt(KEY_MIGRATION, 1);
            PlayerPrefs.Save();
        }

        void Load()
        {
            _music = PlayerPrefs.GetInt(KEY_MUSIC, DEFAULT_MUSIC) == 1;
            _sound = PlayerPrefs.GetInt(KEY_SOUND, DEFAULT_SOUND) == 1;
            _vibration = PlayerPrefs.GetInt(KEY_VIBRATION, DEFAULT_VIBRATION) == 1;
        }

        public bool IsMusicEnabled => _music;
        public bool IsSoundEnabled => _sound;
        public bool IsVibrationEnabled => _vibration;

        public void SetMusicEnabled(bool value)
        {
            if (_music == value) return;
            _music = value;
            PlayerPrefs.SetInt(KEY_MUSIC, value ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingChanged?.Invoke(SettingType.Music, value);
        }

        public void SetSoundEnabled(bool value)
        {
            if (_sound == value) return;
            _sound = value;
            PlayerPrefs.SetInt(KEY_SOUND, value ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingChanged?.Invoke(SettingType.Sound, value);
        }

        public void SetVibrationEnabled(bool value)
        {
            if (_vibration == value) return;
            _vibration = value;
            PlayerPrefs.SetInt(KEY_VIBRATION, value ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingChanged?.Invoke(SettingType.Vibration, value);
        }
    }
}
