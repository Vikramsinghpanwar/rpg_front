using System;
using UnityEngine;
using Core.Config;
using Core.Bootstrap;

namespace Features.Share.Services
{
    public static class ShareService
    {
        public static string ResolveShareUrl()
        {
            string promoCode = BootstrapService.Instance?.ReferralCode;
            if (string.IsNullOrWhiteSpace(promoCode))
                return null;

            return $"{ServerConfig.LandingPageUrl}?shareCode={promoCode}";
        }

        public static string ResolveShareMessage()
        {
            string promoCode = BootstrapService.Instance?.ReferralCode;
            if (string.IsNullOrWhiteSpace(promoCode))
                return null;

            string shareUrl = ResolveShareUrl();
            if (string.IsNullOrEmpty(shareUrl))
                return $"Join The Black Pearl! Use my promo code: {promoCode}";

            return $"Join The Black Pearl and earn rewards!\n\nUse my promo code: {promoCode}\n\nDownload: {shareUrl}";
        }

        public static void CopyToClipboard(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("[ShareService] CopyToClipboard called with null or empty text");
                return;
            }

            try
            {
                GUIUtility.systemCopyBuffer = text;
                Debug.Log("[ShareService] Copied to clipboard: " + text.Substring(0, Math.Min(80, text.Length)) + "...");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ShareService] Clipboard copy failed: " + ex.Message);
                throw;
            }
        }

        public static void OpenWhatsAppShare(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("[ShareService] OpenWhatsAppShare called with null or empty message");
                return;
            }

            string encoded = UnityEngine.WWW.EscapeURL(message);
            string url = $"https://api.whatsapp.com/send?text={encoded}";
            Debug.Log("[ShareService] Opening WhatsApp share URL");
            Application.OpenURL(url);
        }

        public static void OpenTelegramShare(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("[ShareService] OpenTelegramShare called with null or empty message");
                return;
            }

            string encoded = UnityEngine.WWW.EscapeURL(message);
            string url = $"https://t.me/share/url?url={encoded}";
            Debug.Log("[ShareService] Opening Telegram share URL");
            Application.OpenURL(url);
        }

        public static bool HasShareData()
        {
            return !string.IsNullOrWhiteSpace(UserDetail.PromoCode);
        }
    }
}
