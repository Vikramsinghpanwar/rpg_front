using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Features.Auth.UI
{
    public class OTPScreen : MonoBehaviour
    {
        [SerializeField] private GameObject otpPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TMP_InputField otpIF;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private Button verifyButton;

        private string otpChallengeId;

        private void Start()
        {
            verifyButton.onClick.AddListener(VerifyOtp);
        }

        public void Show(string challengeId)
        {
            otpChallengeId = challengeId;
            otpPanel.SetActive(true);
            loadingPanel.SetActive(false);
        }

        public async void VerifyOtp()
        {
            loadingPanel.SetActive(true);
            otpPanel.SetActive(false);

            try
            {
                bool ok = await Core.Session.SessionManager.Instance.SignInWithOtp(otpChallengeId, otpIF.text);

                if (!ok)
                {
                    Toast.Instance.ShowError("Login failed.");
                    loadingPanel.SetActive(false);
                    otpPanel.SetActive(true);
                    return;
                }

                if (Core.Session.SessionManager.Instance.VersionGateCleared)
                {
                    Toast.Instance.ShowSuccess("Login successful.");

                    string fcmToken = PlayerPrefs.GetString("fcm_token", null);
                    if (!string.IsNullOrEmpty(fcmToken))
                    {
                        _ = Core.Notifications.DeviceRegistrationService.RegisterDeviceAsync(fcmToken);
                    }
                }
                else
                {
                    Toast.Instance.ShowError("Update required or maintenance in progress.");
                    otpPanel.SetActive(true);
                    loadingPanel.SetActive(false);
                }
            }
            catch (Core.API.ApiException ex)
            {
                Core.Auth.AuthManager.Instance.PendingPromoCode = null;
                Toast.Instance.ShowError(ex.Message);
                loadingPanel.SetActive(false);
                otpPanel.SetActive(true);
            }
        }

        public void BackToLogin()
        {
            otpPanel.SetActive(false);
            Core.Auth.AuthManager.Instance.PendingPromoCode = null;
        }
    }
}