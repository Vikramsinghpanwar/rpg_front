using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Share.Controllers;
using Features.Share.Services;
using Features.Rewards.UI;
using Features.Rewards.Models;
using Core.Bootstrap;
using Core.Models;
using Core.Managers;
using Features.Rewards.Controllers;

namespace Features.Share.UI
{
    public class ShareScreen : MonoBehaviour
    {
        [Header("Promo Code Section")]
        [SerializeField] private TMP_Text promoCodeText;
        [SerializeField] private Button copyPromoButton;

        [Header("Referral Link Section")]
        [SerializeField] private TMP_Text referralLinkText;
        [SerializeField] private Button copyLinkButton;

        [Header("Referral Section")]
        [SerializeField] private Button openReferralPanelButton;
        [SerializeField] private Button closeReferralPanelButton;
        [SerializeField] private GameObject referralPanel;
        [SerializeField] private TMP_Text referralCodeText;
        [SerializeField] private Button copyReferralCodeButton;
        [SerializeField] private Transform referredUsersContainer;
        [SerializeField] private GameObject referredUserItemPrefab;
        [SerializeField] private GameObject emptyReferralText;

        [Header("MLM Plan")]
        [SerializeField] private GameObject mlmSection;
        [SerializeField] private Transform mlmLevelsContainer;
        [SerializeField] private GameObject mlmLevelItemPrefab;
        [SerializeField] private GameObject mlmEmptyText;

        [Header("Share Actions")]
        [SerializeField] private Button whatsappShareButton;
        [SerializeField] private Button telegramShareButton;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        private ShareController shareController;
        private ReferralController referralController;
        private readonly List<ReferralUserItem> referralItems = new List<ReferralUserItem>();
        private readonly List<MlmLevelItem> mlmLevelItems = new List<MlmLevelItem>();

        void Awake()
        {
            if (shareController == null)
                shareController = FindObjectOfType<ShareController>();
            if (referralController == null)
                referralController = FindObjectOfType<ReferralController>();
        }

        void OnEnable()
        {
            Debug.Log("[ShareScreen] OnEnable");
            RefreshUI();
            if (BootstrapService.Instance != null)
                BootstrapService.Instance.OnBootstrapUpdated += OnBootstrapUpdated;
        }

        void Start()
        {
            if (copyPromoButton != null)
                copyPromoButton.onClick.AddListener(OnCopyPromoClicked);

            if (copyLinkButton != null)
                copyLinkButton.onClick.AddListener(OnCopyLinkClicked);

            if (whatsappShareButton != null)
                whatsappShareButton.onClick.AddListener(OnWhatsAppShareClicked);

            if (telegramShareButton != null)
                telegramShareButton.onClick.AddListener(OnTelegramShareClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);

            if (copyReferralCodeButton != null)
                copyReferralCodeButton.onClick.AddListener(OnCopyReferralCodeClicked);

            if (shareController != null)
            {
                shareController.OnShareDataUpdated += OnShareDataUpdated;
            }
            if (referralController != null)
            {
                referralController.OnReferralDataUpdated += OnReferralDataUpdated;
                referralController.OnReferredUsersUpdated += OnReferredUsersUpdated;
            }
            if (openReferralPanelButton != null)
                openReferralPanelButton.onClick.AddListener(OnOpenReferralPanelClicked);

            if (closeReferralPanelButton != null)
                closeReferralPanelButton.onClick.AddListener(OnCloseReferralPanelClicked);
        }

        void OnDestroy()
        {
            if (copyPromoButton != null)
                copyPromoButton.onClick.RemoveListener(OnCopyPromoClicked);

            if (copyLinkButton != null)
                copyLinkButton.onClick.RemoveListener(OnCopyLinkClicked);

            if (whatsappShareButton != null)
                whatsappShareButton.onClick.RemoveListener(OnWhatsAppShareClicked);

            if (telegramShareButton != null)
                telegramShareButton.onClick.RemoveListener(OnTelegramShareClicked);

            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClicked);

            if (copyReferralCodeButton != null)
                copyReferralCodeButton.onClick.RemoveListener(OnCopyReferralCodeClicked);

            if (shareController != null)
            {
                shareController.OnShareDataUpdated -= OnShareDataUpdated;
            }
            if (referralController != null)
            {
                referralController.OnReferralDataUpdated -= OnReferralDataUpdated;
                referralController.OnReferredUsersUpdated -= OnReferredUsersUpdated;
            }
            if (BootstrapService.Instance != null)
            {
                BootstrapService.Instance.OnBootstrapUpdated -= OnBootstrapUpdated;
            }
        }

        void RefreshUI()
        {
            if (shareController == null)
            {
                Debug.LogWarning("[ShareScreen] Controller is null, aborting RefreshUI");
                ShowEmptyState();
                return;
            }

            if (string.IsNullOrEmpty(shareController.PromoCode) || string.IsNullOrEmpty(shareController.ShareUrl))
            {
                _ = shareController.RefreshShareDataAsync();
            }

            UpdatePromoCodeDisplay(shareController.PromoCode);
            UpdateReferralLinkDisplay(shareController.ShareUrl);

            UpdateReferralCodeFromBootstrap();
            UpdateReferredUsersFromBootstrap();
            UpdateMlmFromBootstrap();
        }

        void UpdatePromoCodeDisplay(string promoCode)
        {
            if (promoCodeText != null)
            {
                if (string.IsNullOrEmpty(promoCode))
                {
                    promoCodeText.text = "Not available";
                    promoCodeText.color = Color.gray;
                }
                else
                {
                    promoCodeText.text = promoCode;
                    promoCodeText.color = Color.white;
                }
            }

            if (copyPromoButton != null)
            {
                copyPromoButton.interactable = !string.IsNullOrEmpty(promoCode);
            }
        }

        void UpdateReferralLinkDisplay(string shareUrl)
        {
            if (referralLinkText != null)
            {
                if (string.IsNullOrEmpty(shareUrl))
                {
                    referralLinkText.text = "Not available";
                    referralLinkText.color = Color.gray;
                }
                else
                {
                    referralLinkText.text = shareUrl;
                    referralLinkText.color = Color.white;
                }
            }

            if (copyLinkButton != null)
            {
                copyLinkButton.interactable = !string.IsNullOrEmpty(shareUrl);
            }
        }

        void ShowEmptyState()
        {
            if (promoCodeText != null)
            {
                promoCodeText.text = "Not available";
                promoCodeText.color = Color.gray;
            }

            if (referralLinkText != null)
            {
                referralLinkText.text = "Not available";
                referralLinkText.color = Color.gray;
            }

            if (copyPromoButton != null)
                copyPromoButton.interactable = false;

            if (copyLinkButton != null)
                copyLinkButton.interactable = false;
        }

        void OnCopyPromoClicked()
        {
            if (shareController == null || string.IsNullOrEmpty(shareController.PromoCode))
            {
                Toast.Instance?.ShowError("Promo code is not available");
                return;
            }

            try
            {
                ShareService.CopyToClipboard(shareController.PromoCode);
                Toast.Instance?.ShowSuccess("Promo code copied!");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ShareScreen] Copy promo code failed: " + ex.Message);
                Toast.Instance?.ShowError("Failed to copy promo code");
            }
        }

        void OnCopyLinkClicked()
        {
            if (shareController == null || string.IsNullOrEmpty(shareController.ShareUrl))
            {
                Toast.Instance?.ShowError("Share link is not available");
                return;
            }

            try
            {
                ShareService.CopyToClipboard(shareController.ShareUrl);
                Toast.Instance?.ShowSuccess("Link copied!");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ShareScreen] Copy link failed: " + ex.Message);
                Toast.Instance?.ShowError("Failed to copy link");
            }
        }

        void OnWhatsAppShareClicked()
        {
            if (shareController == null || string.IsNullOrEmpty(shareController.ShareMessage))
            {
                Toast.Instance?.ShowError("Share message is not available");
                return;
            }

            try
            {
                ShareService.OpenWhatsAppShare(shareController.ShareMessage);
                Toast.Instance?.ShowSuccess("Opening WhatsApp...");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ShareScreen] WhatsApp share failed: " + ex.Message);
                Toast.Instance?.ShowError("Failed to open WhatsApp");
            }
        }

        void OnTelegramShareClicked()
        {
            if (shareController == null || string.IsNullOrEmpty(shareController.ShareMessage))
            {
                Toast.Instance?.ShowError("Share message is not available");
                return;
            }

            try
            {
                ShareService.OpenTelegramShare(shareController.ShareMessage);
                Toast.Instance?.ShowSuccess("Opening Telegram...");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ShareScreen] Telegram share failed: " + ex.Message);
                Toast.Instance?.ShowError("Failed to open Telegram");
            }
        }

        void OnShareDataUpdated(string promoCode)
        {
            UpdatePromoCodeDisplay(promoCode);
        }

        void OnReferralDataUpdated(string referralCode)
        {
            UpdateReferralCodeDisplay(referralCode);
        }

        void OnBootstrapUpdated(Core.Models.BootstrapResponse response)
        {
            RefreshUI();
        }

        void UpdateReferralCodeFromBootstrap()
        {
            var code = BootstrapService.Instance?.ReferralCode;
            UpdateReferralCodeDisplay(code);
        }

        void UpdateReferredUsersFromBootstrap()
        {
            var users = BootstrapService.Instance?.ReferredUsers
                        ?? new List<Features.Rewards.Models.ReferredUser>();
            OnReferredUsersUpdated(users);
        }

        void OnReferredUsersUpdated(List<ReferredUser> users)
        {
            if (referredUsersContainer == null) return;

            foreach (var item in referralItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            referralItems.Clear();

            bool hasUsers = users != null && users.Count > 0;
            if (emptyReferralText != null) emptyReferralText.SetActive(!hasUsers);

            if (!hasUsers || referredUserItemPrefab == null) return;

            int index = 1;
            foreach (var user in users)
            {
                var go = Instantiate(referredUserItemPrefab, referredUsersContainer);
                var item = go.GetComponent<ReferralUserItem>();
                if (item != null)
                {
                    item.Setup(index, user.publicId, user.username, user.createdAt);
                    referralItems.Add(item);
                }
                index++;
            }
        }

        void UpdateMlmFromBootstrap()
        {
            var bootstrap = BootstrapService.Instance;
            if (bootstrap == null || bootstrap.Current == null || bootstrap.Current.rewards == null)
            {
                SetMlmVisible(false);
                return;
            }

            var mlm = bootstrap.Current.rewards.mlm;
            if (mlm == null || !mlm.enabled || mlm.levels == null || mlm.levels.Count == 0)
            {
                SetMlmVisible(false);
                return;
            }

            SetMlmVisible(true);
            RenderMlmLevels(mlm.levels);
        }

        void SetMlmVisible(bool visible)
        {
            if (mlmSection != null) mlmSection.SetActive(visible);
        }

        void RenderMlmLevels(List<Core.Models.MlmLevel> levels)
        {
            if (mlmLevelsContainer == null) return;

            foreach (var item in mlmLevelItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            mlmLevelItems.Clear();

            if (mlmEmptyText != null) mlmEmptyText.SetActive(false);

            if (levels == null || levels.Count == 0) return;

            if (mlmLevelItemPrefab != null)
            {
                foreach (var level in levels)
                {
                    var go = Instantiate(mlmLevelItemPrefab, mlmLevelsContainer);
                    var item = go.GetComponent<MlmLevelItem>();
                    if (item != null)
                    {
                        item.Setup(level.level, level.commission);
                        mlmLevelItems.Add(item);
                    }
                }
            }
            else
            {
                foreach (var level in levels)
                {
                    var go = CreateMlmLevelRow(mlmLevelsContainer, level.level, level.commission);
                    mlmLevelItems.Add(go.GetComponent<MlmLevelItem>());
                }
            }
        }

        GameObject CreateMlmLevelRow(Transform parent, int level, double commission)
        {
            var go = new GameObject("MLMLevelItem");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(564, 40);
            go.AddComponent<CanvasRenderer>();
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = Color.clear;
            var mlmItem = go.AddComponent<MlmLevelItem>();

            var levelGO = new GameObject("LevelLabel");
            levelGO.transform.SetParent(go.transform, false);
            var levelRect = levelGO.AddComponent<RectTransform>();
            levelRect.sizeDelta = new Vector2(156, 40);
            levelRect.anchoredPosition = new Vector2(-204.19f, 0);
            var levelText = levelGO.AddComponent<TMP_Text>();
            if (levelText != null && promoCodeText != null && promoCodeText.font != null)
            {
                levelText.font = promoCodeText.font;
                levelText.fontSize = 18;
                levelText.alignment = TMPro.TextAlignmentOptions.Center;
            }
            levelText.text = $"Level {level}";

            var commissionGO = new GameObject("CommissionLabel");
            commissionGO.transform.SetParent(go.transform, false);
            var commissionRect = commissionGO.AddComponent<RectTransform>();
            commissionRect.sizeDelta = new Vector2(156, 40);
            commissionRect.anchoredPosition = new Vector2(204.19f, 0);
            var commissionText = commissionGO.AddComponent<TMP_Text>();
            if (commissionText != null && promoCodeText != null && promoCodeText.font != null)
            {
                commissionText.font = promoCodeText.font;
                commissionText.fontSize = 18;
                commissionText.alignment = TMPro.TextAlignmentOptions.Center;
            }
            commissionText.text = $"{commission}%";

            return go;
        }

        void UpdateReferralCodeDisplay(string referralCode)
        {
            if (referralCodeText != null)
            {
                if (string.IsNullOrEmpty(referralCode))
                {
                    referralCodeText.text = "Not available";
                    referralCodeText.color = Color.gray;
                }
                else
                {
                    referralCodeText.text = referralCode;
                    referralCodeText.color = Color.white;
                }
            }

            if (copyReferralCodeButton != null)
            {
                copyReferralCodeButton.interactable = !string.IsNullOrEmpty(referralCode);
            }
        }

        void OnCopyReferralCodeClicked()
        {
            if (BootstrapService.Instance == null || string.IsNullOrEmpty(BootstrapService.Instance.ReferralCode))
            {
                Toast.Instance?.ShowError("Referral code is not available");
                return;
            }

            try
            {
                ShareService.CopyToClipboard(BootstrapService.Instance.ReferralCode);
                Toast.Instance?.ShowSuccess("Referral code copied!");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ShareScreen] Copy referral code failed: " + ex.Message);
                Toast.Instance?.ShowError("Failed to copy referral code");
            }
        }

        private void OnCloseClicked()
        {
            gameObject.SetActive(false);
        }

        private void OnCloseReferralPanelClicked()
        {
            if (referralPanel != null)
            {
                referralPanel.SetActive(false);
            }
        }

        private void OnOpenReferralPanelClicked()
        {
            if (referralPanel != null)
            {
                referralPanel.SetActive(true);
            }

            if (referralController != null)
            {
                _ = referralController.LoadReferralData();
            }
        }
    }
}
