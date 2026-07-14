using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core.Bootstrap;
using Core.Managers;
using Core.Utilities;
using Features.Profile.Services;

namespace Features.Profile.UI
{
    public class AvatarChangePopup : MonoBehaviour
    {
        [Header("Popup Root")]
        [SerializeField] private GameObject popupRoot;

        [Header("Current Avatar Preview")]
        [SerializeField] private Image currentAvatarImage;

        [Header("Actions")]
        [SerializeField] private Button uploadButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private GameObject loadingOverlay;

        [Header("Default Avatar Grid")]
        [SerializeField] private Transform avatarGridContainer;
        [SerializeField] private GameObject avatarItemPrefab;

        const string DefaultAvatarResourcesPath = "Avatar";

        string _pendingUploadPath;
        byte[] _pendingUploadBytes;
        Texture2D _previewTexture;
        string _selectedUrl;
        Sprite _uploadedSprite;
        Sprite[] _availableSprites;

        void Awake()
        {
            if (uploadButton != null) uploadButton.onClick.AddListener(OnUploadClicked);
            if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
            if (cancelButton != null) cancelButton.onClick.AddListener(Close);

            _availableSprites = Resources.LoadAll<Sprite>(DefaultAvatarResourcesPath);
        }

        void OnDestroy()
        {
            if (uploadButton != null) uploadButton.onClick.RemoveListener(OnUploadClicked);
            if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveClicked);
            if (cancelButton != null) cancelButton.onClick.RemoveListener(Close);
            if (_uploadedSprite != null)
            {
                Destroy(_uploadedSprite);
                _uploadedSprite = null;
            }
            if (_previewTexture != null)
            {
                Destroy(_previewTexture);
                _previewTexture = null;
            }
        }

        public void Open()
        {
            if (popupRoot != null) popupRoot.SetActive(true);
            UpdatePreviewFromProfile();
            BuildAvatarGrid();
            SetInteractable(true);
        }

        public void Close()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
            _pendingUploadPath = null;
            _pendingUploadBytes = null;
            if (_uploadedSprite != null)
            {
                Destroy(_uploadedSprite);
                _uploadedSprite = null;
            }
            if (_previewTexture != null)
            {
                Destroy(_previewTexture);
                _previewTexture = null;
            }
            if (loadingOverlay != null) loadingOverlay.SetActive(false);
        }

        void UpdatePreviewFromProfile()
        {
            if (currentAvatarImage == null || BootstrapService.Instance?.Profile == null) return;

            var profile = BootstrapService.Instance.Profile;
            _selectedUrl = !string.IsNullOrEmpty(profile.avatar) ? profile.avatar : string.Empty;

            if (!string.IsNullOrEmpty(_selectedUrl))
            {
                var s = Resources.Load<Sprite>(profile.avatar);
                if (s != null)
                {
                    currentAvatarImage.sprite = s;
                    return;
                }
            }

            currentAvatarImage.sprite = null;
        }

        void BuildAvatarGrid()
        {
            if (avatarGridContainer == null || avatarItemPrefab == null) return;

            foreach (Transform c in avatarGridContainer)
            {
                Destroy(c.gameObject);
            }

            if (_availableSprites == null || _availableSprites.Length == 0)
            {
                _availableSprites = Resources.LoadAll<Sprite>(DefaultAvatarResourcesPath);
            }

            for (int i = 0; i < _availableSprites.Length; i++)
            {
                int index = i;
                var item = Instantiate(avatarItemPrefab, avatarGridContainer);

                var avatarSlot = item.transform.GetChild(0);
                var img = avatarSlot != null ? avatarSlot.GetComponent<Image>() : null;
                if (img == null) img = item.GetComponentInChildren<Image>();
                if (img != null)
                {
                    img.sprite = _availableSprites[i];
                }
                else
                {
                    Debug.LogWarning($"[AvatarChangePopup] No Image found on grid item {i}");
                }

                var label = item.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = _availableSprites[i].name;
                }

                var btn = item.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => SelectDefault(index));
                }
            }
        }

        void SelectDefault(int index)
        {
            if (!IsInteractionAllowed()) return;
            if (_availableSprites == null || index < 0 || index >= _availableSprites.Length) return;

            _pendingUploadPath = null;
            _pendingUploadBytes = null;
            _uploadedSprite = null;
            if (_previewTexture != null)
            {
                Destroy(_previewTexture);
                _previewTexture = null;
            }
            _selectedUrl = DefaultAvatarResourcesPath + "/" + _availableSprites[index].name;

            if (currentAvatarImage != null)
            {
                currentAvatarImage.sprite = _availableSprites[index];
            }
        }

        void OnUploadClicked()
        {
            if (!IsInteractionAllowed()) return;

            try
            {
                if (NativeGallery.IsMediaPickerBusy())
                {
                    Debug.Log("[AvatarChangePopup] Gallery picker busy");
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

                    var tex = NativeGallery.LoadImageAtPath(path, 512, false);
                    if (tex == null)
                    {
                        PopupManager.Instance.ShowError("Unable to read selected image.");
                        _pendingUploadBytes = null;
                        return;
                    }

                    _previewTexture = tex;
                    _pendingUploadPath = path;
                    _uploadedSprite = Sprite.Create(_previewTexture, new Rect(0, 0, _previewTexture.width, _previewTexture.height), new Vector2(0.5f, 0.5f));
                    _selectedUrl = null;

                    if (currentAvatarImage != null)
                    {
                        currentAvatarImage.sprite = _uploadedSprite;
                    }
                }, "Select avatar image");

                if (permission == NativeGallery.Permission.Denied)
                {
                    Debug.LogWarning("[AvatarChangePopup] Gallery permission denied");
                    PopupManager.Instance.ShowError("Gallery access denied.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AvatarChangePopup] Upload failed: {ex.Message}");
                PopupManager.Instance.ShowError("Unable to open gallery.");
            }
        }

        async void OnSaveClicked()
        {
            if (!IsInteractionAllowed()) return;

            SetInteractable(false);

            try
            {
                string urlToSave = _selectedUrl;

                if (!string.IsNullOrEmpty(_pendingUploadPath))
                {
                    string baseName = Path.GetFileNameWithoutExtension(_pendingUploadPath);
                    string optimizedFileName = baseName + ".jpg";
                    var uploadResp = await ProfileService.UploadAvatarAsync(_pendingUploadBytes, optimizedFileName, string.IsNullOrEmpty(_selectedUrl) ? null : _selectedUrl);
                    if (uploadResp == null || string.IsNullOrEmpty(uploadResp.Data?.PublicUrl))
                    {
                        PopupManager.Instance.ShowError("Failed to upload avatar.");
                        SetInteractable(true);
                        return;
                    }
                    urlToSave = uploadResp.Data?.PublicUrl;
                }

                if (string.IsNullOrEmpty(urlToSave) && _uploadedSprite == null)
                {
                    Toast.Instance.ShowWarning("No changes to save.");
                    Close();
                    return;
                }

                var updated = await ProfileService.UpdateProfileAsync(null, urlToSave);
                if (updated == null)
                {
                    PopupManager.Instance.ShowError("Unable to update avatar. Please try again.");
                    SetInteractable(true);
                    return;
                }

                BootstrapService.Instance.ApplyProfilePatch(updated);
                Close();
                PopupManager.Instance.ShowSuccess("Avatar updated.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AvatarChangePopup] Save failed: {ex.Message}");
                PopupManager.Instance.ShowError("Unable to update avatar. Please try again.");
                SetInteractable(true);
            }
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
            if (uploadButton != null) uploadButton.interactable = interactable;
            if (loadingOverlay != null) loadingOverlay.SetActive(!interactable);
        }
    }
}
