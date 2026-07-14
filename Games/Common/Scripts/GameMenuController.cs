using System;
using UnityEngine;
using Core.Managers;
using UnityEngine.SceneManagement;

public class GameMenuController : MonoBehaviour
{
    [Header("Customization")]
        [SerializeField] public bool customLobbyButtonBehavior = false;
    [Header("UI References")]
    [SerializeField] public GameObject menuPanel;
    [SerializeField] public GameObject musicOffIndicator;
    [SerializeField] public GameObject soundOffIndicator;
    [SerializeField] public GameObject vibrationOffIndicator;
    [SerializeField] public GameObject musicToggleButton;
    [SerializeField] public GameObject soundToggleButton;
    [SerializeField] public GameObject vibrationToggleButton;
    [SerializeField] public GameObject lobbyButton;

    [SerializeField] public GameObject rulesButton;
    [SerializeField] public GameObject helpButton;
    [SerializeField] public GameObject supportButton;

    public event Action OnResumeRequested;
    public event Action OnBackToLobbyRequested;
    public event Action OnRulesRequested;
    public event Action OnHelpRequested;
    public event Action OnSupportRequested;

    private bool _subscribed;
    private GameRulesPanel _gameRulesPanel;

    public static GameMenuController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _gameRulesPanel = FindFirstObjectByType<GameRulesPanel>();
        if (rulesButton != null)
            rulesButton.SetActive(_gameRulesPanel != null);
    }

    private void OnEnable()
    {
        if (_subscribed) return;
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingChanged += OnSettingChanged;
            _subscribed = true;
        }
        SyncUI();
    }

    void Start()
    {
        lobbyButton?.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnBackToLobbyClicked);
        rulesButton?.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnRulesClicked);
        // helpButton?.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnHelpClicked);
        // supportButton?.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnSupportClicked);
        musicToggleButton?.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnMusicToggle);
        soundToggleButton?.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnSoundToggle);
        vibrationToggleButton?.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnVibrationToggle);

        OnBackToLobbyRequested += OnBackToLobbyRequestedHandler;

    }

    private void OnBackToLobbyRequestedHandler()
    {
        if (customLobbyButtonBehavior)
        {
            Debug.Log("Custom lobby button behavior is enabled. Not loading scene.");
            return;
        }
        SceneManager.LoadScene("Lobby");
    }

    private void OnDisable()
    {
        if (!_subscribed) return;
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingChanged -= OnSettingChanged;
        }
        _subscribed = false;
    }

    private void OnDestroy()
    {
        if (_subscribed)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingChanged -= OnSettingChanged;
            }
            _subscribed = false;
        }
    }

    private void OnSettingChanged(SettingType type, bool value)
    {
        if (type == SettingType.Music || type == SettingType.Sound || type == SettingType.Vibration)
            SyncUI();
    }

    private void SyncUI()
    {
        if (SettingsManager.Instance == null) return;

        if (musicOffIndicator != null)
            musicOffIndicator.SetActive(!SettingsManager.Instance.IsMusicEnabled);
        if (soundOffIndicator != null)
            soundOffIndicator.SetActive(!SettingsManager.Instance.IsSoundEnabled);
        if (vibrationOffIndicator != null)
            vibrationOffIndicator.SetActive(!SettingsManager.Instance.IsVibrationEnabled);
    }

    public void OnResumeClicked()
    {
        OnResumeRequested?.Invoke();
    }

    public void OnBackToLobbyClicked()
    {
        OnBackToLobbyRequested?.Invoke();
    }

    public void OnRulesClicked()
    {
        HideMenu();
        _gameRulesPanel?.Show();
        OnRulesRequested?.Invoke();
    }

    public void OnHelpClicked()
    {
        OnHelpRequested?.Invoke();
    }

    public void OnSupportClicked()
    {
        OnSupportRequested?.Invoke();
    }

    public void OnMusicToggle()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetMusicEnabled(!SettingsManager.Instance.IsMusicEnabled);
    }

    public void OnSoundToggle()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetSoundEnabled(!SettingsManager.Instance.IsSoundEnabled);
    }

    public void OnVibrationToggle()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetVibrationEnabled(!SettingsManager.Instance.IsVibrationEnabled);
    }

    public void ShowMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            SyncUI();
        }
    }

    public void HideMenu()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    public void ShowRules()
    {
        _gameRulesPanel?.Show();
    }

    public void HideRules()
    {
        _gameRulesPanel?.Hide();
    }
}
