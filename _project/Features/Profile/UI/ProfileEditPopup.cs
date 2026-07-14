using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core.Bootstrap;
using Core.Managers;
using Core.Utils;
using Core.Utilities;
using Features.Profile.Services;

namespace Features.Profile.UI
{
    public class ProfileEditPopup : MonoBehaviour
    {
        [Header("Popup Root")]
        [SerializeField] private GameObject popupRoot;

        [Header("Username")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_Text usernameErrorText;

        [Header("Avatar")]
        [SerializeField] private Image currentAvatarImage;
        [SerializeField] private Button uploadAvatarButton;
        [SerializeField] private Transform avatarGridContainer;
        [SerializeField] private GameObject avatarItemPrefab;

        [Header("Actions")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private GameObject loadingOverlay;

        const string DefaultAvatarResourcesPath = "Avatar";

        string _userId;
        string _selectedRemoteAvatarUrl;
        string _pendingUploadPath;
        byte[] _pendingUploadBytes;
        Texture2D _previewTexture;
        Sprite[] _defaultAvatarSprites;

        void Awake()
        {
            if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
            if (cancelButton != null) cancelButton.onClick.AddListener(Close);
            if (uploadAvatarButton != null) uploadAvatarButton.onClick.AddListener(OnUploadClicked);

            if (usernameInput != null)
            {
                usernameInput.onValueChanged.AddListener(OnUsernameChanged);
                usernameInput.onEndEdit.AddListener(OnUsernameChanged);
            }

            _defaultAvatarSprites = Resources.LoadAll<Sprite>(DefaultAvatarResourcesPath);
        }

        void OnDestroy()
        {
            if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveClicked);
            if (cancelButton != null) cancelButton.onClick.RemoveListener(Close);
            if (uploadAvatarButton != null) uploadAvatarButton.onClick.RemoveListener(OnUploadClicked);
            if (usernameInput != null)
            {
                usernameInput.onValueChanged.RemoveListener(OnUsernameChanged);
                usernameInput.onEndEdit.RemoveListener(OnUsernameChanged);
            }
            _pendingUploadBytes = null;
            if (_previewTexture != null)
            {
                Destroy(_previewTexture);
                _previewTexture = null;
            }
        }

        public void Open()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(true);
            }

            var profile = BootstrapService.Instance?.Profile;
            _userId = profile?.public_id ?? ProfileService.GetCurrentUserId();

            if (usernameInput != null)
            {
                usernameInput.text = profile?.username ?? string.Empty;
                OnUsernameChanged(usernameInput.text);
            }

            _selectedRemoteAvatarUrl = profile?.avatar ?? string.Empty;
            _pendingUploadPath = null;
            _pendingUploadBytes = null;
            if (_previewTexture != null)
            {
                Destroy(_previewTexture);
                _previewTexture = null;
            }
            UpdateCurrentAvatarDisplay();

            PopulateAvatarGrid();
            SetInteractable(true);
        }

        public void Close()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }
            _pendingUploadPath = null;
            _pendingUploadBytes = null;
            if (_previewTexture != null)
            {
                Destroy(_previewTexture);
                _previewTexture = null;
            }
        }

        void OnUploadClicked()
        {
            if (!IsInteractionAllowed()) return;

            try
            {
                if (NativeGallery.IsMediaPickerBusy())
                {
                    Debug.Log("[ProfileEditPopup] Gallery picker busy");
                    return;
                }

                NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
                {
                    if (path == null) return;

                    string error;
                    if (!AvatarValidator.IsValid(path, out error))
                    {
                        PopupManager.Instance.ShowError(error);
                        return;
                    }

                    if (_previewTexture != null)
                    {
                        Destroy(_previewTexture);
                        _previewTexture = null;
                    }

                    _pendingUploadBytes = ImageCompressionUtility.CompressAvatar(path);
                    if (_pendingUploadBytes == null)
                    {
                        PopupManager.Instance.ShowError("Failed to process image. Please try another.");
                        return;
                    }

                    _pendingUploadPath = path;
                    var tex = NativeGallery.LoadImageAtPath(path, 512, false);
                    if (tex == null)
                    {
                        PopupManager.Instance.ShowError("Unable to read selected image.");
                        _pendingUploadBytes = null;
                        return;
                    }

                    _previewTexture = tex;

                    _selectedRemoteAvatarUrl = null;
                    if (currentAvatarImage != null)
                    {
                        currentAvatarImage.sprite = Sprite.Create(_previewTexture, new Rect(0, 0, _previewTexture.width, _previewTexture.height), new Vector2(0.5f, 0.5f));
                    }
                }, "Select avatar image");

                if (permission == NativeGallery.Permission.Denied)
                {
                    Debug.LogWarning("[ProfileEditPopup] Gallery permission denied");
                    PopupManager.Instance.ShowError("Gallery access denied.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProfileEditPopup] Upload button failed: {ex.Message}");
                PopupManager.Instance.ShowError("Unable to open gallery.");
            }
        }

        async void OnSaveClicked()
        {
            if (!IsInteractionAllowed()) return;
            if (!ValidateUsername(out var usernameError))
            {
                ShowUsernameError(usernameError);
                return;
            }

            SetInteractable(false);

            try
            {
                string avatarToSave = null;

                if (!string.IsNullOrEmpty(_pendingUploadPath) && _pendingUploadBytes != null)
                {
                    string baseName = Path.GetFileNameWithoutExtension(_pendingUploadPath);
                    string optimizedFileName = baseName + ".jpg";
                    var uploadResp = await ProfileService.UploadAvatarAsync(_pendingUploadBytes, optimizedFileName, string.IsNullOrEmpty(_selectedRemoteAvatarUrl) ? null : _selectedRemoteAvatarUrl);
                    if (uploadResp == null || string.IsNullOrEmpty(uploadResp.Data?.PublicUrl))
                    {
                        PopupManager.Instance.ShowError("Failed to upload avatar.");
                        SetInteractable(true);
                        return;
                    }

                    avatarToSave = uploadResp.Data.PublicUrl;
                }
                else if (!string.IsNullOrEmpty(_selectedRemoteAvatarUrl))
                {
                    avatarToSave = _selectedRemoteAvatarUrl;
                }

                var updatedProfile = await ProfileService.UpdateProfileAsync(usernameInput.text, avatarToSave);
                if (updatedProfile == null)
                {
                    PopupManager.Instance.ShowError("Unable to update profile. Please try again.");
                    SetInteractable(true);
                    return;
                }

                BootstrapService.Instance.ApplyProfilePatch(updatedProfile);
                Close();
                PopupManager.Instance.ShowSuccess("Profile updated.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProfileEditPopup] Save failed: {ex.Message}");
                string message = ex.Message.Contains("409") || ex.Message.Contains("Username already taken", StringComparison.OrdinalIgnoreCase)
                    ? "Username already exists."
                    : "Unable to update profile. Please try again.";

                PopupManager.Instance.ShowError(message);
                SetInteractable(true);
            }
        }

        void OnUsernameChanged(string value)
        {
            bool valid = ValidateUsername(out _);
            ShowUsernameError(valid ? null : "Username must be 3-50 alphanumeric characters.");
            UpdateSaveButtonState();
        }

        void PopulateAvatarGrid()
        {
            if (avatarGridContainer == null || avatarItemPrefab == null) return;

            foreach (Transform child in avatarGridContainer)
            {
                Destroy(child.gameObject);
            }

            if (_defaultAvatarSprites == null || _defaultAvatarSprites.Length == 0)
            {
                _defaultAvatarSprites = Resources.LoadAll<Sprite>(DefaultAvatarResourcesPath);
            }

            int idx = 0;
            foreach (var sprite in _defaultAvatarSprites)
            {
                var item = Instantiate(avatarItemPrefab, avatarGridContainer);

                var avatarSlot = item.transform.GetChild(0);
                var image = avatarSlot != null ? avatarSlot.GetComponent<Image>() : null;
                if (image == null) image = item.GetComponentInChildren<Image>();
                if (image != null)
                {
                    image.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"[ProfileEditPopup] No Image found on grid item {idx}");
                }

                int copy = idx;
                var button = item.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OnDefaultAvatarSelected(copy));
                }

                idx++;
            }
        }

        void OnDefaultAvatarSelected(int index)
        {
            if (!IsInteractionAllowed()) return;
            if (_defaultAvatarSprites == null || index < 0 || index >= _defaultAvatarSprites.Length) return;

            _selectedRemoteAvatarUrl = null;
            _pendingUploadPath = null;
            if (currentAvatarImage != null)
            {
                currentAvatarImage.sprite = _defaultAvatarSprites[index];
            }
        }

        void UpdateCurrentAvatarDisplay()
        {
            if (currentAvatarImage == null) return;

            if (!string.IsNullOrEmpty(_selectedRemoteAvatarUrl))
            {
                var sprite = Resources.Load<Sprite>(_selectedRemoteAvatarUrl);
                if (sprite != null)
                {
                    currentAvatarImage.sprite = sprite;
                    return;
                }
            }

            var profile = BootstrapService.Instance?.Profile;
            if (!string.IsNullOrEmpty(profile?.avatar))
            {
                var fallback = Resources.Load<Sprite>(profile.avatar);
                if (fallback != null)
                {
                    currentAvatarImage.sprite = fallback;
                    return;
                }
            }

            currentAvatarImage.sprite = null;
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

        void ShowUsernameError(string message)
        {
            if (usernameErrorText != null)
            {
                usernameErrorText.text = message ?? string.Empty;
                usernameErrorText.gameObject.SetActive(!string.IsNullOrEmpty(message));
            }
        }

        void UpdateSaveButtonState()
        {
            if (saveButton == null) return;
            var valid = !string.IsNullOrEmpty(_userId) && ValidateUsername(out _);
            saveButton.interactable = valid;
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
            if (uploadAvatarButton != null) uploadAvatarButton.interactable = interactable;
            if (usernameInput != null) usernameInput.interactable = interactable;
            if (loadingOverlay != null) loadingOverlay.SetActive(!interactable);
        }
    }
}
