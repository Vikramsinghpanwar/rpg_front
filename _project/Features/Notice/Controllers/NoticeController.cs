using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Features.Notice.Models;
using Features.Notice.Services;
using Core.Bootstrap;
using Features.Notice.UI;
using Newtonsoft.Json.Linq;

namespace Features.Notice.Controllers
{
    public class NoticeController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private NoticePanelView panelView;

        readonly List<NoticeItem> allNotices = new List<NoticeItem>();
        NoticeItem currentNotice;

        public bool HasNotices => allNotices.Count > 0;
        public IReadOnlyList<NoticeItem> AllNotices => allNotices;

        public event Action<List<NoticeItem>> OnNoticesLoaded;
        public event Action<NoticeDisplay> OnNoticeSelected;

        void OnEnable()
        {
            Debug.Log("[NoticeController] OnEnable");
            if (BootstrapService.Instance != null)
                BootstrapService.Instance.OnBootstrapUpdated += OnBootstrapUpdated;

            if (panelView != null)
            {
                panelView.SetOnNoticeSelected(id => SelectNoticeById(id));
            }
        }

        void OnDisable()
        {
            Debug.Log("[NoticeController] OnDisable");
            if (BootstrapService.Instance != null)
                BootstrapService.Instance.OnBootstrapUpdated -= OnBootstrapUpdated;
        }

        void OnBootstrapUpdated(Core.Models.BootstrapResponse response)
        {
            _ = InitializeFromBootstrap();
        }

        public async Task<bool> InitializeFromBootstrap()
        {
            Debug.Log("[NoticeController] InitializeFromBootstrap called");
            Debug.Log("[NoticeController] InitializeFromBootstrap: BootstrapService.Instance=" + (BootstrapService.Instance != null)
                      + ", LobbyRaw=" + (BootstrapService.Instance != null && BootstrapService.Instance.LobbyRaw != null)
                      + ", NoticesData=" + (BootstrapService.Instance?.GetNoticesData() != null));

            if (BootstrapService.Instance != null && BootstrapService.Instance.NoticesRaw != null)
            {
                Debug.Log("[NoticeController] NoticesRaw direct: type=" + BootstrapService.Instance.NoticesRaw.Type
                          + ", raw=" + BootstrapService.Instance.NoticesRaw.ToString());
            }
            if (BootstrapService.Instance != null && BootstrapService.Instance.LobbyRaw != null)
            {
                var lobbyDataToken = BootstrapService.Instance.LobbyRaw["data"];
                if (lobbyDataToken != null)
                {
                    var noticesToken = lobbyDataToken["notices"];
                    if (noticesToken != null)
                    {
                        Debug.Log("[NoticeController] LobbyRaw.data.notices: type=" + noticesToken.Type
                                  + ", raw=" + noticesToken.ToString());
                    }
                    else
                    {
                        Debug.LogWarning("[NoticeController] LobbyRaw.data.notices token is NULL");
                    }
                }
                else
                {
                    Debug.LogWarning("[NoticeController] LobbyRaw.data token is NULL");
                }
            }

            var noticesData = BootstrapService.Instance?.GetNoticesData();
            List<NoticeItem> notices = null;

            if (noticesData != null)
            {
                try
                {
                    notices = noticesData.ToObject<List<NoticeItem>>();
                    // Debug.Log($"[NoticeController] Direct notices.data parse: {(notices != null ? notices.Count : 0)} items");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[NoticeController] notices.data deserialize failed: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[NoticeController] BootstrapService.GetNoticesData() returned null");
            }

            if ((notices == null || notices.Count == 0) && BootstrapService.Instance?.LobbyRaw != null)
            {
                Debug.Log("[NoticeController] Trying lobby fallback...");
                notices = GetNoticesFromLobbyFallback();
                Debug.Log("[NoticeController] Lobby fallback result: notices=" + (notices != null ? notices.Count.ToString() : "null"));
            }

            if (notices != null && notices.Count > 0)
            {
                Debug.Log($"[notice-debug] InitializeFromBootstrap: SUCCESS setting {notices.Count} notices");
                SetNotices(notices);
                return true;
            }

            // Debug.Log("[notice-debug] InitializeFromBootstrap: RESULT=0 notices, calling Clear()");
            allNotices.Clear();
            panelView?.Clear();
            return false;
        }

        List<NoticeItem> GetNoticesFromLobbyFallback()
        {
            var lobby = BootstrapService.Instance?.LobbyRaw;
            if (lobby == null) return null;

            try
            {
                JToken noticesToken = null;

                var dataToken = lobby["data"];
                if (dataToken != null)
                {
                    noticesToken = dataToken["notices"];
                }

                if (noticesToken == null)
                {
                    noticesToken = lobby["notices"];
                }

                if (noticesToken == null || noticesToken.Type != JTokenType.Array) return null;

                var result = noticesToken.ToObject<List<NoticeItem>>();
                // Debug.Log($"[NoticeController] Lobby fallback: parsed {result?.Count ?? 0} notices");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NoticeController] Lobby fallback exception: {ex.Message}");
                return null;
            }
        }

        void SetNotices(List<NoticeItem> raw)
        {
            // Debug.Log("[notice-debug] SetNotices: count=" + (raw != null ? raw.Count : 0) + ", panelView=" + (panelView != null));
            allNotices.Clear();
            foreach (var n in raw)
            {
                allNotices.Add(n);
            }

            OnNoticesLoaded?.Invoke(allNotices);

            if (panelView != null)
            {
                // Debug.Log("[notice-debug] SetNotices: calling panelView.SetNotices with " + allNotices.Count + " items");
                try
                {
                    panelView.SetNotices(allNotices);
                    // Debug.Log("[notice-debug] SetNotices: panelView.SetNotices RETURNED ok, allNotices.Count=" + allNotices.Count);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[notice-debug] SetNotices: panelView.SetNotices THREW: " + ex.GetType().Name + " - " + ex.Message + "\n" + ex.StackTrace);
                }
                if (allNotices.Count > 0)
                {
                    // Debug.Log("[notice-debug] SetNotices: about to call SelectNotice for item 0, id=" + allNotices[0].id);
                    try
                    {
                        SelectNotice(allNotices[0]);
                        // Debug.Log("[notice-debug] SetNotices: SelectNotice RETURNED ok");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("[notice-debug] SetNotices: SelectNotice THREW: " + ex.GetType().Name + " - " + ex.Message + "\n" + ex.StackTrace);
                    }
                }
                else
                {
                    Debug.LogWarning("[notice-debug] SetNotices: allNotices.Count is 0, skipping SelectNotice");
                }
            }
            else
            {
                Debug.LogWarning("[notice-debug] SetNotices: panelView is NULL, cannot update UI");
            }
        }

        public void SelectNotice(NoticeItem item)
        {
            if (item == null)
            {
                Debug.LogWarning("[notice-debug] SelectNotice called with null");
                return;
            }

            currentNotice = item;
            var imageUrl = GetFirstImageUrl(item);

            // Debug.Log("[notice-debug] SelectNotice: id=" + item.id + ", panelView=" + (panelView != null) +", panelViewType=" + (panelView != null ? panelView.GetType().FullName : "null") + ", panelViewGO=" + (panelView != null ? panelView.gameObject.name : "null") + ", hasImage=" + !string.IsNullOrEmpty(imageUrl));

            var display = new NoticeDisplay
            {
                Id = item.id,
                Title = item.title,
                Content = item.content,
                ImageUrl = imageUrl
            };

            // Debug.Log("[notice-debug] SelectNotice: calling panelView.ShowNoticeDetail + SetButtonSelected");
            panelView?.ShowNoticeDetail(display);
            panelView?.SetButtonSelected(item.id);
            OnNoticeSelected?.Invoke(display);

            if (!string.IsNullOrEmpty(imageUrl))
            {
                // Debug.Log("[notice-debug] SelectNotice: loading image for notice " + item.id + ": " + imageUrl);
                _ = LoadAndShowImage(item.id, imageUrl);
            }
            else
            {
                // Debug.Log("[notice-debug] SelectNotice: no image for notice " + item.id + ", calling ShowPlaceholder");
                panelView?.ShowPlaceholder();
            }
        }

        public void SelectNoticeById(string id)
        {
            // Debug.Log($"[NoticeController] SelectNoticeById called: id={id}, allNotices.Count={allNotices.Count}");
            var n = allNotices.Find(x => x.id == id);
            // Debug.Log($"[NoticeController] SelectNoticeById found: {(n != null ? n.title : "null")}");
            if (n != null) SelectNotice(n);
        }

        async Task LoadAndShowImage(string noticeId, string imageUrl)
        {
            try
            {
                var sprite = await NoticeImageCache.GetImageAsync(noticeId, imageUrl);
                if (currentNotice != null && currentNotice.id == noticeId)
                {
                    panelView?.SetBannerImage(sprite);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NoticeController] Image load failed: {ex.Message}");
                if (currentNotice != null && currentNotice.id == noticeId)
                {
                    panelView?.ShowPlaceholder();
                }
            }
        }

        static string GetFirstImageUrl(NoticeItem item)
        {
            if (item?.images == null || item.images.Count == 0) return null;

            var sorted = item.images.OrderBy(i => i.sortOrder).ToList();
            return sorted.First().url;
        }

        public void Clear()
        {
            allNotices.Clear();
            currentNotice = null;
            panelView?.Clear();
        }

        public void Render()
        {
            if (panelView == null)
            {
                Debug.LogWarning("[notice-debug] Render: panelView is NULL");
                return;
            }
            if (allNotices.Count == 0)
            {
                // Debug.Log("[notice-debug] Render: no notices to render");
                panelView?.Clear();
                return;
            }

            // Debug.Log("[notice-debug] Render: rendering " + allNotices.Count + " notices");
            panelView.SetNotices(allNotices);
            SelectNotice(allNotices[0]);
        }
    }
}
