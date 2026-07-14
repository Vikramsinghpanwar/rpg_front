using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Managers;

public class AudioManager : MonoBehaviour
{
    public List<AudioSource> soundsList;
    public AudioSource BGM;

    void Start()
    {
        ApplyMusic();
        ApplySound();
    }

    void OnEnable()
    {
        SettingsManager.Instance.OnSettingChanged += OnSettingChanged;
    }

    void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingChanged -= OnSettingChanged;
    }

    void OnSettingChanged(SettingType type, bool value)
    {
        switch (type)
        {
            case SettingType.Music:
                ApplyMusic();
                break;
            case SettingType.Sound:
                ApplySound();
                break;
        }
    }

    public void Music()
    {
        SettingsManager.Instance.SetMusicEnabled(!SettingsManager.Instance.IsMusicEnabled);
    }

    public void Sound()
    {
        SettingsManager.Instance.SetSoundEnabled(!SettingsManager.Instance.IsSoundEnabled);
    }

    void ApplyMusic()
    {
        if (BGM == null) return;
        BGM.volume = SettingsManager.Instance.IsMusicEnabled ? 1f : 0f;
    }

    void ApplySound()
    {
        if (soundsList == null) return;
        foreach (AudioSource aud in soundsList)
        {
            if (aud != null)
                aud.volume = SettingsManager.Instance.IsSoundEnabled ? 1f : 0f;
        }
    }
}
