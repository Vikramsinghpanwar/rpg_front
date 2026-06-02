using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Features.Announcement.Models;
using Core.API;
using Core.API.Endpoints;
using Core.Bootstrap;
using Features.Announcement.UI;

namespace Features.Announcement.Controllers
{
    public class AnnouncementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AnnouncementScreenUI screenUI;

        [Header("Public Endpoint (optional fallback)")]
        [Tooltip("If true, fetches AnnouncementRoutes.PublicActive when no bootstrap data is present.")]
        [SerializeField] private bool fetchPublicWhenEmpty = true;

        readonly List<AnnouncementItem> allAnnouncements = new List<AnnouncementItem>();
        AnnouncementDisplay currentDisplay;

        public bool HasAnnouncements => allAnnouncements.Count > 0;

        public event Action<List<AnnouncementItem>> OnAnnouncementsLoaded;

        void OnEnable()
        {
            if (BootstrapService.Instance != null)
                BootstrapService.Instance.OnBootstrapUpdated += OnBootstrapUpdated;
        }

        void OnDisable()
        {
            if (BootstrapService.Instance != null)
                BootstrapService.Instance.OnBootstrapUpdated -= OnBootstrapUpdated;
        }

        void OnBootstrapUpdated(Core.Models.BootstrapResponse response)
        {
            _ = InitializeFromBootstrap();
        }

        public async Task<bool> InitializeFromBootstrap()
        {
            var slice = BootstrapService.Instance?.GetLobbySlice<LobbyAnnouncementsSlice>("announcements")
                        ?? GetSliceFallback();

            if (slice?.announcements != null && slice.announcements.Count > 0)
            {
                SetAnnouncements(slice.announcements);
                return true;
            }

            if (fetchPublicWhenEmpty)
            {
                return await FetchPublicActive();
            }

            allAnnouncements.Clear();
            screenUI?.Clear();
            return false;
        }

        LobbyAnnouncementsSlice GetSliceFallback()
        {
            var lobby = BootstrapService.Instance?.LobbyRaw;
            if (lobby == null) return null;

            try
            {
                var nested = lobby["data"]?["announcements"];
                if (nested != null)
                {
                    return new LobbyAnnouncementsSlice
                    {
                        announcements = nested.ToObject<List<AnnouncementItem>>()
                    };
                }
            }
            catch { /* fall through */ }
            return null;
        }

        public async Task<bool> FetchPublicActive(string region = null, string locale = null, string appVersion = null)
        {
            try
            {
                var items = await ApiClient.Instance.Get<List<AnnouncementItem>>(
                    AnnouncementRoutes.PublicActive(region, locale, appVersion));
                SetAnnouncements(items ?? new List<AnnouncementItem>());
                return allAnnouncements.Count > 0;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AnnouncementController] public active fetch failed: {e.Message}");
                return false;
            }
        }

        void SetAnnouncements(List<AnnouncementItem> raw)
        {
            var now = DateTime.UtcNow;
            allAnnouncements.Clear();
            foreach (var a in raw.Where(a => !IsExpired(a, now)).OrderByDescending(a => a.priority))
            {
                allAnnouncements.Add(a);
            }

            OnAnnouncementsLoaded?.Invoke(allAnnouncements);

            if (screenUI != null)
            {
                screenUI.SetAnnouncementList(allAnnouncements);
                if (allAnnouncements.Count > 0) SelectAnnouncement(allAnnouncements[0]);
            }
        }

        static bool IsExpired(AnnouncementItem a, DateTime now)
        {
            if (string.IsNullOrEmpty(a.endTime)) return false;
            if (!DateTime.TryParse(a.endTime, out var end)) return false;
            return now > end.ToUniversalTime();
        }

        public void SelectAnnouncement(AnnouncementItem item)
        {
            if (item == null) return;
            currentDisplay = new AnnouncementDisplay
            {
                Id = item.id,
                Title = item.title,
                Content = item.content,
                Type = item.type
            };
            screenUI?.ShowAnnouncementDetail(currentDisplay);
        }

        public void SelectAnnouncementById(string id)
        {
            var a = allAnnouncements.Find(x => x.id == id);
            if (a != null) SelectAnnouncement(a);
        }

        public void Clear()
        {
            allAnnouncements.Clear();
            currentDisplay = null;
            screenUI?.Clear();
        }
    }
}