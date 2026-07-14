using Core.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

namespace Features.Auth.UI
{
    public class AuthScreen : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject loadingPanel;

        [Header("Inputs")]
        [SerializeField] private TMP_InputField userMobile;
        [SerializeField] private TMP_InputField userPromocode;
        [SerializeField] private Button loginButton;

        [Header("Status")]
        [SerializeField] private Text errorText;

        [Header("OTP Screen")]
        [SerializeField] private OTPScreen otpScreen;

        private string otpChallengeId;
        private static bool clipboardChecked;

        private async void Start()
        {
            Application.runInBackground = true;
            loadingPanel.SetActive(false);

            if (!clipboardChecked)
            {
                clipboardChecked = true;

                string clipboardText = ClipboardUtility.GetClipboardText();
                Debug.Log("[PromoClipboard] Clipboard Content: " + (clipboardText != null ? clipboardText : "null"));

                if (ClipboardUtility.IsValidPromoCode(clipboardText))
                {
                    Debug.Log("[PromoClipboard] Valid Promo Found");

                    if (string.IsNullOrEmpty(userPromocode.text))
                    {
                        userPromocode.text = clipboardText.Trim();
                        Debug.Log("[PromoClipboard] Promo Applied");
                    }
                }
                else
                {
                    Debug.Log("[PromoClipboard] Invalid Clipboard Content");
                }
            }

            loginButton.onClick.AddListener(RequestOtp);
        }

        public async void RequestOtp()
        {
            string mobile = userMobile.text;

            if (string.IsNullOrEmpty(mobile) || mobile.Length != 10)
            {
                errorText.text = "Invalid mobile number";
                return;
            }

            loadingPanel.SetActive(true);

            try
            {
                var res = await Core.Auth.AuthManager.Instance.RequestOtp("+91" + mobile, "sms");
                Core.Auth.AuthManager.Instance.PendingPromoCode = string.IsNullOrWhiteSpace(userPromocode.text)
                    ? null
                    : userPromocode.text.Trim().ToUpperInvariant();
                otpChallengeId = res.ChallengeId;
                loadingPanel.SetActive(false);
                Toast.Instance.ShowSuccess("OTP sent.");
                otpScreen.Show(otpChallengeId);
            }
            catch (Core.API.ApiException ex)
            {
                Toast.Instance.ShowError(ex.Message);
                loadingPanel.SetActive(false);
            }
            catch (Exception)
            {
                loadingPanel.SetActive(false);
                throw;
            }
        }

        public void ResetLogin()
        {
            userMobile.text = "";
            userPromocode.text = "";
            Core.Auth.AuthManager.Instance.PendingPromoCode = null;
            loadingPanel.SetActive(false);
        }
    }
}