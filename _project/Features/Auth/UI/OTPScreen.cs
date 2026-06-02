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
                await Core.Auth.AuthManager.Instance.VerifyOtp(otpChallengeId, otpIF.text);
                await Core.Session.SessionManager.Instance.Initialize();
                errorText.text = "Login success";
                SceneManager.LoadScene("Lobby");
            }
            catch (Core.API.ApiException ex)
            {
                errorText.text = ex.Message;
                loadingPanel.SetActive(false);
                otpPanel.SetActive(true);
            }
        }

        public void BackToLogin()
        {
            otpPanel.SetActive(false);
        }
    }
}