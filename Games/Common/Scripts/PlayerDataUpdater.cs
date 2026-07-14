using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Bootstrap;
using Features.Profile.Services;
using System;

public class PlayerDataUpdater : MonoBehaviour
{
    public Image profileImg;
    public TextMeshProUGUI playerNameTMP;

    const string DefaultAvatarResourcesPath = "Avatar";
    Sprite[] _defaultAvatarSprites;

    void Awake()
    {
        _defaultAvatarSprites = Resources.LoadAll<Sprite>(DefaultAvatarResourcesPath);
    }

    void OnEnable()
    {
        if (BootstrapService.Instance != null)
            BootstrapService.Instance.OnBootstrapUpdated += OnBootstrapUpdated;
    }

    void OnDisable()
    {
        if (BootstrapService.Instance != null)
            BootstrapService.Instance.OnBootstrapUpdated -= OnBootstrapUpdated;
    }

    void Start()
    {
        RefreshFromBootstrap();
    }

    void OnBootstrapUpdated(Core.Models.BootstrapResponse _)
    {
        RefreshFromBootstrap();
    }

    void RefreshFromBootstrap()
    {
        var profile = BootstrapService.Instance?.Profile;
        if (profile == null) return;

        if (playerNameTMP != null)
            playerNameTMP.text = profile.username ?? string.Empty;

        if (profileImg == null) return;

        if (!string.IsNullOrEmpty(profile.avatar))
        {
            var fallback = Resources.Load<Sprite>(profile.avatar);
            if (fallback != null)
            {
                profileImg.sprite = fallback;
            }
            else
            {
                LoadRemoteAvatarAsync(profile.avatar);
            }
        }
        else
        {
            profileImg.sprite = null;
        }
    }

    async void LoadRemoteAvatarAsync(string avatarUrl)
    {
        try
        {
            var sprite = await AvatarLoader.LoadRemoteAsync(avatarUrl);
            if (sprite != null && profileImg != null)
                profileImg.sprite = sprite;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PlayerDataUpdater] Remote avatar load failed: {ex.Message}");
        }
    }
}