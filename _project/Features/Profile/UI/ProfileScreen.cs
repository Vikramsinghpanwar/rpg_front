using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core.Bootstrap;
using Core.Utils;
using Features.Profile.Services;
using System;

namespace Features.Profile.UI
{
    public class ProfileScreen : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button editUsernameButton;
        [SerializeField] private Button editAvatarButton;

        [Header("TMP References")]
        [SerializeField] private TMP_Text nNameTMP;
        [SerializeField] private TMP_Text walletTMP;
        [SerializeField] private TMP_Text userIdTMP;
        [SerializeField] private TMP_Text bonusBalanceTMP;
        [SerializeField] private TMP_Text mobileNumberTMP;

        [Header("Profile Image")]
        [SerializeField] private Image profileImage;

        [Header("Profile Edit Popups")]
        [SerializeField] private UsernameEditPopup usernameEditPopup;
        [SerializeField] private AvatarChangePopup avatarChangePopup;

        void EditUsername()
        {
            if (usernameEditPopup != null)
            {
                usernameEditPopup.Open();
                return;
            }
            Debug.LogWarning("[ProfileScreen] UsernameEditPopup reference not assigned");
        }

        void EditAvatar()
        {
            if (avatarChangePopup != null)
            {
                avatarChangePopup.Open();
                return;
            }
            Debug.LogWarning("[ProfileScreen] AvatarChangePopup reference not assigned");
        }

        void Start()
        {
            RefreshFromBootstrap();
        }

        void Awake()
        {
            if (editUsernameButton != null) editUsernameButton.onClick.AddListener(EditUsername);
            if (editAvatarButton != null) editAvatarButton.onClick.AddListener(EditAvatar);
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

        void OnBootstrapUpdated(Core.Models.BootstrapResponse _)
        {
            RefreshFromBootstrap();
        }

        public void RefreshFromBootstrap()
        {
            Debug.Log("[ProfileScreen] Refreshing profile from bootstrap");
            if (BootstrapService.Instance?.HasData != true) return;

            var profile = BootstrapService.Instance.Profile;
            var wallet = BootstrapService.Instance.Wallet;
            // Debug.Log($"[ProfileScreen] Complete Profile data: {profile}");
            // Debug.Log($"[ProfileScreen] Complete Wallet data: {wallet}");
            if (profile != null)
            {
                if (nNameTMP != null) nNameTMP.text = profile.username;
                if (userIdTMP != null) userIdTMP.text = profile.public_id;
                if (mobileNumberTMP != null)
                {
                    mobileNumberTMP.text = string.IsNullOrEmpty(profile.mobile_number) ? "Mobile number not set" : profile.mobile_number;
                }

                if (profileImage != null && !string.IsNullOrEmpty(profile.avatar))
                {
                    var fallback = Resources.Load<Sprite>(profile.avatar);
                    if (fallback != null)
                    {
                        profileImage.sprite = fallback;
                    }
                    else
                    {
                        LoadRemoteAvatarAsync(profile.avatar);
                    }
                }
                else if (profileImage != null)
                {
                    profileImage.sprite = null;
                }
            }

            if (wallet != null)
            {
                if (walletTMP != null) walletTMP.text = MoneyFormatter.FormatPaisa(wallet.deposit_balance + wallet.win_balance);
                if (bonusBalanceTMP != null) bonusBalanceTMP.text = MoneyFormatter.FormatPaisa(wallet.bonus_balance);
            }
        }

        async void LoadRemoteAvatarAsync(string avatarUrl)
        {
            try
            {
                var sprite = await AvatarLoader.LoadRemoteAsync(avatarUrl);
                if (sprite != null && profileImage != null)
                {
                    profileImage.sprite = sprite;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ProfileScreen] Remote avatar load failed: {ex.Message}");
            }
        }
    }
}