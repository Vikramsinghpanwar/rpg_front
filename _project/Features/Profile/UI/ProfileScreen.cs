using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core.Bootstrap;
using Core.Utils;

namespace Features.Profile.UI
{
    public class ProfileScreen : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button editProfileButton;

        [Header("TMP References")]
        [SerializeField] private TMP_Text nNameTMP;
        [SerializeField] private TMP_Text walletTMP;
        [SerializeField] private TMP_Text userIdTMP;
        [SerializeField] private TMP_Text bonusBalanceTMP;
        [SerializeField] private TMP_Text mobileNumberTMP;

        [Header("Profile Image")]
        [SerializeField] private Image profileImage;

        [Header("Profile Image Selector")]
        [SerializeField] private ProfileImageSelector profileImageSelector;

        void EditProfile()
        {
            if (profileImageSelector != null)
            {
                profileImageSelector.SelectProfilePicture();
            }
        }

        void Start()
        {
            RefreshFromBootstrap();
        }

        void Awake()
        {
            if (editProfileButton != null) editProfileButton.onClick.AddListener(EditProfile);
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
            Debug.Log($"[ProfileScreen] Complete Profile data: {profile}");
            Debug.Log($"[ProfileScreen] Complete Wallet data: {wallet}");
            if (profile != null)
            {
                if (nNameTMP != null) nNameTMP.text = profile.username;
                if (userIdTMP != null) userIdTMP.text = profile.public_id;
                if (mobileNumberTMP != null)
                {
                    mobileNumberTMP.text = string.IsNullOrEmpty(profile.mobile_number) ? "Mobile number not set" : profile.mobile_number;
                }
                // ADD THIS: Load avatar sprite if available
                if (profileImage != null && !string.IsNullOrEmpty(profile.avatar))
                {
                    // Assuming avatar is a sprite name or resource path
                    var avatarSprite = Resources.Load<Sprite>(profile.avatar);
                    if (avatarSprite != null) profileImage.sprite = avatarSprite;
                }
            }

            if (wallet != null)
            {
                if (walletTMP != null) walletTMP.text = MoneyFormatter.FormatPaisa(wallet.deposit_balance + wallet.win_balance);
                if (bonusBalanceTMP != null) bonusBalanceTMP.text = MoneyFormatter.FormatPaisa(wallet.bonus_balance);
            }
        }
    }
}