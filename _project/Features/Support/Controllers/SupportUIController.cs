using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Utilities;
using Features.Support.Models;
using Core.Managers;
using Core.API;
using Core.API.Endpoints;
using Features.Profile.Services;
using Features.Support.UI;
using UnityEngine.Networking;
using Features.Lobby.UI;

namespace Features.Support.Controllers
{
    public class SupportUIController : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject historyPanel;
        [SerializeField] private GameObject createPanel;
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private GameObject chatPanel;

        [Header("History Panel")]
        [SerializeField] private Button viewHistoryButton;
        [SerializeField] private Transform ticketsContainer;
        [SerializeField] private GameObject ticketItemPrefab;
        [SerializeField] private TMP_Dropdown statusFilterDropdown;
        [SerializeField] private Button createNewButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private RefreshButtonAnimator refreshListAnimator;
        [SerializeField] private TMP_Text pageText;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;

        [Header("Create Ticket Panel")]
        [SerializeField] private TMP_InputField subjectInput;
        [SerializeField] private TMP_InputField bodyInput;
        [SerializeField] private TMP_Dropdown categoryDropdown;
        [SerializeField] private TMP_Dropdown priorityDropdown;
        [SerializeField] private Transform attachmentsContainer;
        [SerializeField] private GameObject attachmentItemPrefab;
        [SerializeField] private Button addAttachmentButton;
        [SerializeField] private TMP_Text attachmentCountText;
        [SerializeField] private int maxAttachments = 5;
        [SerializeField] private Button submitButton;

        [Header("Ticket Detail Panel")]
        [SerializeField] private TMP_Text ticketSubjectText;
        [SerializeField] private TMP_Text ticketStatusText;
        [SerializeField] private TMP_Text ticketPriorityText;
        [SerializeField] private TMP_Text ticketCategoryText;
        [SerializeField] private TMP_Text ticketCreatedText;
        [SerializeField] private ChatMessageListView messageListView;
        [SerializeField] private TMP_InputField replyInput;
        [SerializeField] private Button sendReplyButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button refreshDetailButton;
        [SerializeField] private RefreshButtonAnimator refreshDetailAnimator;

        [Header("Loading/Error")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private TMP_Text emptyStateText;

        private SupportController controller;
        private List<TicketItem> currentTicketItems = new List<TicketItem>();
        private List<string> currentAttachmentMediaIds = new List<string>();
        private readonly Dictionary<string, Texture2D> _thumbnails = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, SupportAttachmentDto> _attachmentMeta = new Dictionary<string, SupportAttachmentDto>();
        private readonly Dictionary<string, string> _pendingTempIds = new Dictionary<string, string>();
        private string currentTicketId;
        private int currentTicketVersion;
        private string currentDedupKey;
        private int currentPage = 1;
        private int totalPages = 1;
        private string currentFilter = null;

        [Header("Toggle Sprites")]
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite unselectedSprite;
        [SerializeField] private Image createToggleImage;
        [SerializeField] private Image historyToggleImage;


        void Awake()
        {
            PopupOpen.OnPopupOpened -= OnSupportPopupOpened;
            PopupOpen.OnPopupOpened += OnSupportPopupOpened;
        }

        void OnDestroy()
        {
            PopupOpen.OnPopupOpened -= OnSupportPopupOpened;
        }

        void OnSupportPopupOpened(Image img)
        {
            if (img == null || img.gameObject == null) return;
            if (string.Equals(img.gameObject.name, "Support"))
            {
                ShowCreatePanel();
            }
        }

        void OnEnable()
        {
            ShowCreatePanel();
            ResolveRefreshListAnimator();
        }

        private void ResolveRefreshListAnimator()
        {
            if (refreshListAnimator != null) return;
            if (refreshButton == null) return;

            var animators = refreshButton.GetComponentsInParent<RefreshButtonAnimator>(true);
            if (animators != null && animators.Length > 0)
            {
                refreshListAnimator = animators[0];
                return;
            }

            refreshListAnimator = refreshButton.GetComponent<RefreshButtonAnimator>();
            if (refreshListAnimator == null)
            {
                refreshListAnimator = refreshButton.gameObject.AddComponent<RefreshButtonAnimator>();
            }
            if (refreshListAnimator != null && refreshListAnimator.targetRect == null)
            {
                var btnRect = refreshButton.GetComponent<RectTransform>();
                if (btnRect != null) refreshListAnimator.targetRect = btnRect;
            }
        }

        public void Initialize(SupportController supportController)
        {
            controller = supportController;

            viewHistoryButton?.onClick.AddListener(ShowHistoryPanel);
            createNewButton?.onClick.AddListener(ShowCreatePanel);
            refreshButton?.onClick.AddListener(RefreshTickets);
            submitButton?.onClick.AddListener(OnSubmitTicket);
            sendReplyButton?.onClick.AddListener(OnSendReply);
            backButton?.onClick.AddListener(ShowHistoryPanel);
            refreshDetailButton?.onClick.AddListener(RefreshCurrentTicket);
            addAttachmentButton?.onClick.AddListener(OnAddAttachment);
            prevPageButton?.onClick.AddListener(PreviousPage);
            nextPageButton?.onClick.AddListener(NextPage);

            if (statusFilterDropdown != null)
            {
                statusFilterDropdown.ClearOptions();
                statusFilterDropdown.AddOptions(new List<string> { "All", "Open", "In Progress", "Awaiting Reply", "Resolved", "Closed" });
                statusFilterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }

            if (categoryDropdown != null)
            {
                categoryDropdown.ClearOptions();
                categoryDropdown.AddOptions(new List<string>
                {
                    "SELECT_CATEGORY", // "Select Category"
                    controller.GetCategoryDisplayText(TicketCategory.ACCOUNT),
                    controller.GetCategoryDisplayText(TicketCategory.PAYMENT),
                    controller.GetCategoryDisplayText(TicketCategory.GAMEPLAY),
                    controller.GetCategoryDisplayText(TicketCategory.OTHER)
                });
            }

            if (priorityDropdown != null)
            {
                priorityDropdown.ClearOptions();
                priorityDropdown.AddOptions(new List<string>
                {
                    controller.GetPriorityDisplayText(TicketPriority.LOW),
                    controller.GetPriorityDisplayText(TicketPriority.MEDIUM),
                    controller.GetPriorityDisplayText(TicketPriority.HIGH),
                    controller.GetPriorityDisplayText(TicketPriority.URGENT)
                });
                priorityDropdown.value = 1;
            }

            ShowCreatePanel();
        }

        private void OnFilterChanged(int index)
        {
            switch (index)
            {
                case 1: currentFilter = TicketStatus.OPEN; break;
                case 2: currentFilter = TicketStatus.IN_PROGRESS; break;
                case 3: currentFilter = TicketStatus.PENDING; break;
                case 4: currentFilter = TicketStatus.RESOLVED; break;
                case 5: currentFilter = TicketStatus.CLOSED; break;
                default: currentFilter = null; break;
            }
            currentPage = 1;
            RefreshTickets();
        }

        private async void RefreshTickets()
        {
            loadingOverlay?.SetActive(true);
            emptyStateText?.gameObject.SetActive(false);
            refreshListAnimator?.StartSpin();

            try
            {
                var response = await controller.GetMyTickets(currentPage, 20, currentFilter);

                if (response != null)
                {
                    UpdateTicketsList(response.items);
                    totalPages = response.meta?.totalPages ?? 1;
                    UpdatePaginationUI();

                    if (response.items == null || response.items.Length == 0)
                        emptyStateText?.gameObject.SetActive(true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[SupportUIController] RefreshTickets failed: " + ex.Message);
            }
            finally
            {
                loadingOverlay?.SetActive(false);
                refreshListAnimator?.StopSpin();
            }
        }

        private void UpdateTicketsList(TicketResponse[] tickets)
        {
            foreach (var item in currentTicketItems)
            {
                if (item != null && item.gameObject != null)
                    Destroy(item.gameObject);
            }
            currentTicketItems.Clear();

            if (tickets == null) return;

            foreach (var ticket in tickets)
            {
                var go = Instantiate(ticketItemPrefab, ticketsContainer);
                var item = go.GetComponent<TicketItem>();
                item.Setup(ticket, controller, OnTicketSelected);
                currentTicketItems.Add(item);
            }
        }

        private void OnTicketSelected(string ticketId)
        {
            currentTicketId = ticketId;
            _ = ShowTicketDetail(ticketId);
        }

        private async Task ShowTicketDetail(string ticketId)
        {
            loadingOverlay?.SetActive(true);

            var detail = await controller.GetTicketDetail(ticketId);

            loadingOverlay?.SetActive(false);

            if (detail != null)
            {
                UpdateDetailUI(detail);
                ShowDetailPanel();
            }
        }

        private async void RefreshCurrentTicket()
        {
            if (string.IsNullOrEmpty(currentTicketId)) return;
            refreshDetailAnimator?.StartSpin();
            try { await ShowTicketDetail(currentTicketId); }
            catch (Exception ex) { Debug.LogError("[SupportUIController] RefreshCurrentTicket failed: " + ex.Message); }
            finally { refreshDetailAnimator?.StopSpin(); }
        }

        private void UpdateDetailUI(TicketDetailResponse detail)
        {
            currentTicketVersion = detail.data.version;

            ticketSubjectText.text = detail.data.subject;
            ticketStatusText.text = controller.GetStatusDisplayText(detail.data.status);
            ticketStatusText.color = ColorUtility.TryParseHtmlString(controller.GetStatusColor(detail.data.status), out Color color) ? color : Color.white;
            ticketPriorityText.text = controller.GetPriorityDisplayText(detail.data.priority);
            ticketPriorityText.color = controller.GetPriorityColor(detail.data.priority);
            ticketCategoryText.text = controller.GetCategoryDisplayText(detail.data.category);
            ticketCreatedText.text = FormatDate(detail.data.createdAt);

            if (messageListView != null)
            {
                messageListView.RenderMessages(detail.data.messages, controller);
            }

            replyInput.text = "";
        }

        private async void OnSendReply()
        {
            if (string.IsNullOrEmpty(replyInput.text))
            {
                PopupManager.Instance.ShowError("Please enter a message");
                return;
            }

            var result = await controller.ReplyToTicket(
                currentTicketId,
                replyInput.text,
                currentTicketVersion);

            switch (result)
            {
                case ReplyResult.Success:
                    currentAttachmentMediaIds.Clear();
                    UpdateAttachmentsUI();
                    await ShowTicketDetail(currentTicketId);
                    break;

                case ReplyResult.VersionConflict:
                    PopupManager.Instance.Show(
                        "Ticket updated",
                        "New messages have arrived on this ticket. Please review and try sending again.",
                        "OK");
                    await ShowTicketDetail(currentTicketId);
                    break;

                case ReplyResult.TicketClosed:
                    await ShowTicketDetail(currentTicketId);
                    break;

                case ReplyResult.NetworkError:
                default:
                    break;
            }
        }

        private void ShowCreatePanel()
        {
            historyPanel?.SetActive(false);
            detailPanel?.SetActive(false);
            chatPanel?.SetActive(false);
            createPanel?.SetActive(true);

            subjectInput.text = "";
            bodyInput.text = "";
            categoryDropdown.value = 0;
            priorityDropdown.value = 1;
            currentAttachmentMediaIds.Clear();
            _attachmentMeta.Clear();
            _thumbnails.Clear();
            _pendingTempIds.Clear();

            UpdateAttachmentCountText();
            UpdateAttachmentsUI();

            currentDedupKey = Guid.NewGuid().ToString();

            if (createToggleImage != null && selectedSprite != null) createToggleImage.sprite = selectedSprite;
            if (historyToggleImage != null && unselectedSprite != null) historyToggleImage.sprite = unselectedSprite;

        }

        private void ShowHistoryPanel()
        {
            createPanel?.SetActive(false);
            detailPanel?.SetActive(false);
            chatPanel?.SetActive(false);
            historyPanel?.SetActive(true);
            RefreshTickets();
            if (createToggleImage != null && unselectedSprite != null) createToggleImage.sprite = unselectedSprite;
            if (historyToggleImage != null && selectedSprite != null) historyToggleImage.sprite = selectedSprite;

        }

        private void ShowDetailPanel()
        {
            historyPanel?.SetActive(false);
            createPanel?.SetActive(false);
            chatPanel?.SetActive(false);
            detailPanel?.SetActive(true);
        }

        private async void OnSubmitTicket()
        {
            subjectInput.text = "Ticket";
            if (string.IsNullOrEmpty(subjectInput.text))
            {
                PopupManager.Instance.ShowError("Please enter a subject");
                return;
            }

            if (string.IsNullOrEmpty(bodyInput.text))
            {
                PopupManager.Instance.ShowError("Please describe your issue");
                return;
            }

            if (categoryDropdown == null || categoryDropdown.value == 0)
            {
                PopupManager.Instance.ShowError("Select a category");
                return;
            }

            string category = GetCategoryValue(categoryDropdown.value);
            string priority = "MEDIUM"; // GetPriorityValue(priorityDropdown.value);

            var result = await controller.CreateTicket(
                subjectInput.text,
                bodyInput.text,
                category,
                priority,
                currentAttachmentMediaIds.ToArray(),
                currentDedupKey);

            if (result != null)
            {
                ShowHistoryPanel();
            }
        }

        private async void OnAddAttachment()
        {
            Debug.Log("[attachment-debug] OnAddAttachment START");
            if (currentAttachmentMediaIds.Count >= maxAttachments)
            {
                PopupManager.Instance.ShowError($"Maximum {maxAttachments} attachments allowed.");
                return;
            }
            if (NativeGallery.IsMediaPickerBusy()) return;

            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
            {
                Debug.Log($"[attachment-debug] Gallery callback path={(path ?? "null")}");
                if (path == null) return;

                string error;
                if (!AvatarValidator.IsValid(path, out error))
                {
                    Debug.LogWarning($"[attachment-debug] Validation failed: {error}");
                    PopupManager.Instance.ShowError(error);
                    return;
                }

                Debug.Log($"[attachment-debug] Validation OK, path={path}");

                string tempId = $"temp_{Guid.NewGuid():N}";
                Debug.Log($"[attachment-debug] Generated tempId={tempId}");

                byte[] imageBytes = ImageCompressionUtility.CompressTicketAttachment(path);
                if (imageBytes == null)
                {
                    PopupManager.Instance.ShowError("Failed to process image. Please try another.");
                    return;
                }

                string optimizedFileName = Path.GetFileNameWithoutExtension(path) + ".jpg";
                Debug.Log($"[attachment-debug] Compressed size={imageBytes.Length} fileName={optimizedFileName}");

                Texture2D localTex = null;
                try
                {
                    localTex = NativeGallery.LoadImageAtPath(path, 512, true);
                    Debug.Log($"[attachment-debug] LoadImageAtPath result={(localTex != null ? $"{localTex.width}x{localTex.height}" : "NULL")}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[attachment-debug] LoadImageAtPath threw: {ex.Message}");
                }

                if (localTex != null)
                {
                    _thumbnails[tempId] = localTex;
                    _attachmentMeta[tempId] = new SupportAttachmentDto
                    {
                        Key = tempId,
                        FileName = optimizedFileName,
                        MimeType = "image/jpeg"
                    };
                    _pendingTempIds[tempId] = path;
                    Debug.Log($"[attachment-debug] Cached local thumbnail under tempId={tempId}");
                }
                else
                {
                    _attachmentMeta[tempId] = new SupportAttachmentDto
                    {
                        Key = tempId,
                        FileName = optimizedFileName,
                        MimeType = "image/jpeg"
                    };
                    _pendingTempIds[tempId] = path;
                    Debug.Log($"[attachment-debug] No local texture, stored meta only for tempId={tempId}");
                }

                currentAttachmentMediaIds.Add(tempId);
                UpdateAttachmentsUI();
                Debug.Log($"[attachment-debug] Added tempId to list, count={currentAttachmentMediaIds.Count}");

                _ = UploadAttachmentAsync(imageBytes, optimizedFileName, tempId);
            }, "Select attachment image");

            if (permission == NativeGallery.Permission.Denied)
            {
                Debug.LogWarning("[attachment-debug] Gallery permission DENIED");
                PopupManager.Instance.ShowError("Gallery access denied.");
            }
        }

        private async Task UploadAttachmentAsync(byte[] imageBytes, string fileName, string tempId)
        {
            Debug.Log($"[attachment-debug] UploadAttachmentAsync START tempId={tempId} size={imageBytes.Length} fileName={fileName}");
            LoadingManager.Instance?.Show("Uploading attachment...");

            try
            {
                var extraFields = new Dictionary<string, string>();
                extraFields["category"] = "TICKET_ATTACHMENT";

                var resp = await ApiClient.Instance.PostMultipart<Core.API.AvatarUploadResponse>(
                    Core.API.Endpoints.UserRoutes.UploadAvatar,
                    "file",
                    imageBytes,
                    fileName,
                    GetMimeType(fileName),
                    extraFields,
                    null);

                LoadingManager.Instance?.Hide();

                Debug.Log($"[attachment-debug] Upload response success={resp?.Success} statusCode={resp?.StatusCode} id={resp?.Data?.Id} publicUrl={resp?.Data?.PublicUrl}");

                if (resp?.Data == null || string.IsNullOrEmpty(resp.Data.Id))
                {
                    Debug.LogError("[attachment-debug] Upload returned null/empty id — FAILED");
                    PopupManager.Instance.ShowError("Upload failed — no media ID returned.");
                    return;
                }

                string mediaId = resp.Data.Id;

                if (_pendingTempIds.TryGetValue(tempId, out var originalPath))
                {
                    Debug.Log($"[attachment-debug] Found pending tempId, originalPath={originalPath}");
                }

                _attachmentMeta[mediaId] = new SupportAttachmentDto
                {
                    Key = mediaId,
                    FileName = fileName,
                    MimeType = !string.IsNullOrEmpty(resp.Data.MimeType) ? resp.Data.MimeType : GetMimeType(fileName)
                };

                if (_thumbnails.TryGetValue(tempId, out var localTex))
                {
                    Debug.Log($"[attachment-debug] Remapping texture {localTex.width}x{localTex.height} from tempId={tempId} → mediaId={mediaId}");
                    _thumbnails.Remove(tempId);
                    _thumbnails[mediaId] = localTex;
                }
                else
                {
                    Debug.LogWarning($"[attachment-debug] No cached texture for tempId={tempId} — will rely on remote fetch");
                    if (!string.IsNullOrEmpty(resp.Data.PublicUrl))
                    {
                        _ = CacheThumbnailAsync(mediaId, resp.Data.PublicUrl);
                    }
                }

                int idx = currentAttachmentMediaIds.IndexOf(tempId);
                if (idx >= 0)
                {
                    currentAttachmentMediaIds[idx] = mediaId;
                    Debug.Log($"[attachment-debug] Replaced tempId at index {idx} → mediaId={mediaId}");
                }
                else
                {
                    Debug.LogWarning($"[attachment-debug] tempId={tempId} not found in list — adding mediaId as new entry");
                    currentAttachmentMediaIds.Add(mediaId);
                }

                _pendingTempIds.Remove(tempId);
                UpdateAttachmentsUI();
                Debug.Log($"[attachment-debug] UpdateAttachmentsUI done, list count={currentAttachmentMediaIds.Count}, thumbnails count={_thumbnails.Count}");
                PopupManager.Instance.ShowSuccess("Attachment added.");
            }
            catch (Exception ex)
            {
                LoadingManager.Instance?.Hide();
                Debug.LogError($"[attachment-debug] Upload exception: {ex.Message}\n{ex.StackTrace}");
                PopupManager.Instance.ShowError("Upload failed. Please try again.");
            }
        }

        private async Task CacheThumbnailAsync(string mediaId, string url)
        {
            try
            {
                Debug.Log($"[attachment-debug] CacheThumbnailAsync START mediaId={mediaId} url={url}");
                using (var req = UnityWebRequestTexture.GetTexture(url))
                {
                    req.timeout = 15;
                    var op = req.SendWebRequest();
                    while (!op.isDone)
                    {
                        await Task.Yield();
                    }
                    Debug.Log($"[attachment-debug] CacheThumbnailAsync done result={req.result} mediaId={mediaId}");
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        var tex = DownloadHandlerTexture.GetContent(req);
                        if (tex != null)
                        {
                            Debug.Log($"[attachment-debug] Texture cached {tex.width}x{tex.height}");
                            _thumbnails[mediaId] = tex;
                            UpdateAttachmentsUI();
                        }
                        else
                        {
                            Debug.LogWarning("[attachment-debug] DownloadHandlerTexture.GetContent returned null");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[attachment-debug] CacheThumbnailAsync exception: {ex.Message}");
            }
        }

        private static string GetMimeType(string fileName)
        {
            var ext = fileName.ToLowerInvariant();
            if (ext.EndsWith(".png")) return "image/png";
            if (ext.EndsWith(".jpg") || ext.EndsWith(".jpeg")) return "image/jpeg";
            return "application/octet-stream";
        }

        private void UpdateAttachmentsUI()
        {
            foreach (Transform child in attachmentsContainer)
                Destroy(child.gameObject);

            foreach (var mediaId in currentAttachmentMediaIds)
            {
                var go = Instantiate(attachmentItemPrefab, attachmentsContainer);

                var image = go.GetComponentInChildren<Image>();
                if (image != null)
                {
                    if (_thumbnails.TryGetValue(mediaId, out var tex))
                    {
                        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        image.color = Color.white;
                    }
                    else
                    {
                        image.color = new Color(1, 1, 1, 0);
                    }
                }

                var text = go.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    if (_attachmentMeta.TryGetValue(mediaId, out var meta))
                        text.text = meta.FileName;
                    else
                        text.text = mediaId;
                }

                var removeBtn = go.GetComponentInChildren<Button>();
                if (removeBtn != null)
                {
                    string id = mediaId;
                    removeBtn.onClick.AddListener(() => RemoveAttachment(id));
                }
            }
            UpdateAttachmentCountText();
        }

        private void UpdateAttachmentCountText()
        {
            if (attachmentCountText != null)
            {
                attachmentCountText.text = $"Attachments: {currentAttachmentMediaIds.Count} / {maxAttachments}";
            }
        }

        private void RemoveAttachment(string mediaId)
        {
            currentAttachmentMediaIds.Remove(mediaId);
            _attachmentMeta.Remove(mediaId);
            if (_thumbnails.TryGetValue(mediaId, out var tex))
            {
                _thumbnails.Remove(mediaId);
                if (tex != null) Destroy(tex);
            }
            _pendingTempIds.Remove(mediaId);
            UpdateAttachmentsUI();
        }

        private string GetCategoryValue(int dropdownIndex)
        {
            var displayText = categoryDropdown.options[dropdownIndex].text;
            if (displayText == controller.GetCategoryDisplayText(TicketCategory.ACCOUNT)) return TicketCategory.ACCOUNT;
            if (displayText == controller.GetCategoryDisplayText(TicketCategory.PAYMENT)) return TicketCategory.PAYMENT;
            if (displayText == controller.GetCategoryDisplayText(TicketCategory.GAMEPLAY)) return TicketCategory.GAMEPLAY;
            return TicketCategory.OTHER;
        }

        private string GetPriorityValue(int dropdownIndex)
        {
            var displayText = priorityDropdown.options[dropdownIndex].text;
            if (displayText == controller.GetPriorityDisplayText(TicketPriority.LOW)) return TicketPriority.LOW;
            if (displayText == controller.GetPriorityDisplayText(TicketPriority.MEDIUM)) return TicketPriority.MEDIUM;
            if (displayText == controller.GetPriorityDisplayText(TicketPriority.HIGH)) return TicketPriority.HIGH;
            return TicketPriority.URGENT;
        }

        private void PreviousPage()
        {
            if (currentPage > 1)
            {
                currentPage--;
                RefreshTickets();
            }
        }

        private void NextPage()
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                RefreshTickets();
            }
        }

        private void UpdatePaginationUI()
        {
            if (pageText != null)
                pageText.text = $"{currentPage} / {totalPages}";

            if (prevPageButton != null)
                prevPageButton.interactable = currentPage > 1;

            if (nextPageButton != null)
                nextPageButton.interactable = currentPage < totalPages;
        }

        private string FormatDate(string isoDate)
        {
            if (DateTime.TryParse(isoDate, out var dt))
                return dt.ToString("MMM dd, yyyy HH:mm");
            return isoDate;
        }
    }
}
