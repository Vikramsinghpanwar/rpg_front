using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core.Bootstrap;
using Core.Managers;
using Features.Profile.Services;

namespace Features.Profile.UI
{
    public class UsernameEditPopup : MonoBehaviour
    {
        [Header("Popup Root")]
        [SerializeField] private GameObject popupRoot;

        [Header("Username Input")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_Text errorText;

        [Header("Actions")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private GameObject loadingOverlay;

        string _userId;

        void Awake()
        {
            if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
            if (cancelButton != null) cancelButton.onClick.AddListener(Close);
            if (usernameInput != null)
            {
                usernameInput.onValueChanged.AddListener(_ => ValidateAndUpdateUI());
                usernameInput.onEndEdit.AddListener(_ => ValidateAndUpdateUI());
            }
        }

        void OnDestroy()
        {
            if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveClicked);
            if (cancelButton != null) cancelButton.onClick.RemoveListener(Close);
            if (usernameInput != null)
            {
                usernameInput.onValueChanged.RemoveListener(_ => ValidateAndUpdateUI());
                usernameInput.onEndEdit.RemoveListener(_ => ValidateAndUpdateUI());
            }
        }

        public void Open()
        {
            if (popupRoot != null) popupRoot.SetActive(true);

            var profile = BootstrapService.Instance?.Profile;
            _userId = profile?.public_id ?? ProfileService.GetCurrentUserId();

            if (usernameInput != null)
            {
                usernameInput.text = profile?.username ?? string.Empty;
                ValidateAndUpdateUI();
            }

            SetInteractable(true);
        }

        public void Close()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
            if (loadingOverlay != null) loadingOverlay.SetActive(false);
        }

        async void OnSaveClicked()
        {
            if (!IsInteractionAllowed()) return;
            if (!ValidateUsername(out var error))
            {
                ShowError(error);
                return;
            }

            SetInteractable(false);

            try
            {
                var updated = await ProfileService.UpdateProfileAsync(usernameInput.text.Trim(), null);
                if (updated == null)
                {
                    ShowError("Unable to update username. Please try again.");
                    SetInteractable(true);
                    return;
                }

                BootstrapService.Instance.ApplyProfilePatch(updated);
                Close();
                PopupManager.Instance.ShowSuccess("Username updated.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UsernameEditPopup] Save failed: {ex.Message}");
                string message = ex.Message.Contains("409") || ex.Message.Contains("Username already taken", System.StringComparison.OrdinalIgnoreCase)
                    ? "Username already exists."
                    : "Unable to update username. Please try again.";
                ShowError(message);
                SetInteractable(true);
            }
        }

        void ValidateAndUpdateUI()
        {
            bool valid = ValidateUsername(out _);
            if (saveButton != null) saveButton.interactable = valid;
            ShowError(valid ? null : "Username must be 3-50 alphanumeric characters.");
        }

        bool ValidateUsername(out string error)
        {
            error = null;
            var raw = usernameInput?.text ?? string.Empty;
            var trimmed = raw.Trim();

            if (trimmed.Length < 3 || trimmed.Length > 50)
            {
                error = "Username must be between 3 and 50 characters.";
                return false;
            }

            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];
                if (!char.IsLetterOrDigit(c))
                {
                    error = "Username must contain only letters and numbers.";
                    return false;
                }
            }

            return true;
        }

        void ShowError(string message)
        {
            if (errorText == null) return;
            errorText.text = message ?? string.Empty;
            errorText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        bool IsInteractionAllowed()
        {
            if (loadingOverlay != null && loadingOverlay.activeSelf) return false;
            return saveButton != null && saveButton.interactable;
        }

        void SetInteractable(bool interactable)
        {
            if (saveButton != null) saveButton.interactable = interactable;
            if (cancelButton != null) cancelButton.interactable = interactable;
            if (usernameInput != null) usernameInput.interactable = interactable;
            if (loadingOverlay != null) loadingOverlay.SetActive(!interactable);
        }
    }
}
