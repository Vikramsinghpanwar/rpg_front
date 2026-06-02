using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        private async void Start()
        {
            Application.runInBackground = true;


            await Core.Auth.AuthManager.Instance.Initialize();

            if (Core.Auth.AuthManager.Instance.IsAuthenticated)
            {
                Debug.Log("User already authenticated, bootstrapping session...");
                await Core.Session.SessionManager.Instance.Initialize();
                // Auto-login: redirect to lobby
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                loadingPanel.SetActive(false);
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
                otpChallengeId = res.ChallengeId;
                errorText.text = "OTP sent";
                otpScreen.Show(otpChallengeId);
            }
            catch (Core.API.ApiException ex)
            {
                errorText.text = ex.Message;
                loadingPanel.SetActive(false);
            }
        }

        public void ResetLogin()
        {
            userMobile.text = "";
            userPromocode.text = "";
            loadingPanel.SetActive(false);
        }
    }
}