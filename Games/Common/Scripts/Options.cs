using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using Core.Managers;
using Core.Services;

public class Options : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject rulesPanel;
    public AudioManager audioManagerRef;
    public GameObject musicOff;
    public GameObject soundOff;
    public GameObject vibrationOff;
    public AudioSource tapAudio;
    bool _isRules;

    void Start()
    {
        if (audioManagerRef == null)
            audioManagerRef = FindObjectOfType<AudioManager>();

        SyncSprites();
        _isRules = false;
        rulesPanel.SetActive(false);
        menuPanel.SetActive(false);
    }

    void OnEnable()
    {
        SettingsManager.Instance.OnSettingChanged += OnSettingChanged;
    }

    void OnDisable()
    {
        SettingsManager.Instance.OnSettingChanged -= OnSettingChanged;
    }

    void OnSettingChanged(SettingType type, bool value)
    {
        if (type == SettingType.Music || type == SettingType.Sound || type == SettingType.Vibration)
            SyncSprites();
    }

    void SyncSprites()
    {
        if (musicOff != null)
            musicOff.SetActive(!SettingsManager.Instance.IsMusicEnabled);

        if (soundOff != null)
            soundOff.SetActive(!SettingsManager.Instance.IsSoundEnabled);

        if (vibrationOff != null)
            vibrationOff.SetActive(!SettingsManager.Instance.IsVibrationEnabled);
    }

    public void MenuBtn()
    {
        tapAudio.Play();
        menuPanel.SetActive(!menuPanel.activeInHierarchy);
    }

    public void GoHome()
    {
        tapAudio.Play();
        SceneManager.LoadScene("Lobby");
    }

    public void ToggleMusic()
    {
        tapAudio.Play();
        SettingsManager.Instance.SetMusicEnabled(!SettingsManager.Instance.IsMusicEnabled);
        if (audioManagerRef != null)
            audioManagerRef.Music();
    }

    public void ToggleSound()
    {
        tapAudio.Play();
        SettingsManager.Instance.SetSoundEnabled(!SettingsManager.Instance.IsSoundEnabled);
        if (audioManagerRef != null)
            audioManagerRef.Sound();
    }

    public void ToggleVibration()
    {
        tapAudio.Play();
        SettingsManager.Instance.SetVibrationEnabled(!SettingsManager.Instance.IsVibrationEnabled);
    }

    public void Rules()
    {
        tapAudio.Play();
        _isRules = !_isRules;
        rulesPanel.SetActive(_isRules);
        menuPanel.SetActive(false);
    }
}
