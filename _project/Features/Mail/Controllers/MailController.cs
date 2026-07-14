using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Features.Mail.Models;
using Core.API;
using Core.API.Endpoints;
using Core.Managers;
using Features.Mail.UI;

namespace Features.Mail.Controllers
{
    public class MailController : MonoBehaviour
    {
        [Header("UI")]
        internal MailInboxScreen inboxScreenUI;

        [Header("Public Endpoint (optional fallback)")]
        [Tooltip("If true, fetches MailRoutes.List when no bootstrap data is present.")]
        [SerializeField] private bool fetchPublicWhenEmpty = false;

        readonly List<MailItem> allMails = new List<MailItem>();
        MailItem currentMail;
        int currentPage = 1;
        int totalPages = 1;
        int pageLimit = 20;
        bool isLoadingInbox;
        bool isMarkAsReadProgress;
        bool isDeleteProgress;

        [SerializeField] public bool HasMails => allMails.Count > 0;
        public IReadOnlyList<MailItem> AllMails => allMails;
        public MailItem CurrentMail => currentMail;

        public event Action<List<MailItem>> OnMailsLoaded;
        public event Action<MailItem> OnMailSelected;
        public event Action<int> OnUnreadCountUpdated;
        public event Action<string> OnMailMarkedAsRead;
        public event Action<string> OnMailDeleted;
        public event Action<string> OnError;

        void OnDestroy()
        {
            OnMailsLoaded = null;
            OnMailSelected = null;
            OnUnreadCountUpdated = null;
            OnMailMarkedAsRead = null;
            OnMailDeleted = null;
            OnError = null;
        }

        public async Task<int> LoadUnreadCount()
        {
            try
            {
                var envelope = await ApiClient.Instance.Get<MailUnreadCountEnvelope>(MailRoutes.UnreadCount);
                int count = envelope?.data?.unread ?? 0;
                OnUnreadCountUpdated?.Invoke(count);
                return count;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MailController] LoadUnreadCount failed: {e.Message}");
                OnError?.Invoke(e.Message);
                return 0;
            }
        }

        public async Task<bool> LoadInbox(bool forceRefresh = false)
        {
            if (isLoadingInbox)
            {
                Debug.LogWarning("[MailController] LoadInbox skipped: already loading");
                return false;
            }

            isLoadingInbox = true;
            if (forceRefresh) currentPage = 1;

            try
            {
                string route = MailRoutes.List(currentPage, pageLimit);
                Debug.Log($"[MailController] LoadInbox fetching: {route}");
                var result = await ApiClient.Instance.Get<MailListResponse>(route);
                var items = result?.data?.ToList() ?? new List<MailItem>();
                Debug.Log($"[MailController] LoadInbox received {items.Count} items from data array");
                items = items.OrderByDescending(m => DateTime.Parse(m.createdAt)).ToList();

                if (currentPage == 1)
                {
                    allMails.Clear();
                    foreach (var m in items)
                    {
                        allMails.Add(m);
                    }
                }
                else
                {
                    foreach (var m in items)
                    {
                        allMails.Add(m);
                    }
                }

                allMails.Sort((a, b) => DateTime.Compare(DateTime.Parse(b.createdAt), DateTime.Parse(a.createdAt)));

                totalPages = result?.meta?.totalPages ?? totalPages;
                Debug.Log($"[MailController] LoadInbox total pages: {totalPages}, current page: {currentPage}");

                OnMailsLoaded?.Invoke(new List<MailItem>(allMails));

                if (inboxScreenUI != null)
                {
                    Debug.Log($"[MailController] Calling inboxScreenUI.SetMailList with {allMails.Count} items");
                    inboxScreenUI.SetMailList(allMails);
                    inboxScreenUI.UpdatePageNavState(currentPage, totalPages);
                }
                else
                {
                    Debug.LogWarning("[MailController] inboxScreenUI is null — UI will not update");
                }

                return allMails.Count > 0;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MailController] LoadInbox failed: {e.Message}");
                OnError?.Invoke(e.Message);
                Toast.Instance.ShowError("Failed to load inbox.");
                return false;
            }
            finally
            {
                isLoadingInbox = false;
            }
        }

        public async Task GoToPage(int page)
        {
            if (isLoadingInbox) return;
            if (page < 1) return;
            if (page > totalPages) return;
            if (page == currentPage) return;

            currentPage = page;
            await LoadInbox(false);
        }

        public async Task GoToPrevPage()
        {
            if (currentPage > 1)
            {
                await GoToPage(currentPage - 1);
            }
        }

        public async Task GoToNextPage()
        {
            if (currentPage < totalPages)
            {
                await GoToPage(currentPage + 1);
            }
        }

        public async Task<MailItem> LoadMailDetail(string mailId)
        {
            try
            {
                var response = await ApiClient.Instance.Get<MailDetailResponse>(MailRoutes.Detail(mailId));
                var item = response?.data;

                if (item != null)
                {
                    currentMail = item;

                    int index = allMails.FindIndex(m => m.id == mailId);
                    if (index >= 0)
                    {
                        allMails[index] = item;
                    }

                    OnMailSelected?.Invoke(item);

                    if (inboxScreenUI != null)
                    {
                        inboxScreenUI.ShowMailDetail(item);
                        if (index >= 0)
                        {
                            inboxScreenUI.UpdateListItem(index, item);
                        }
                    }
                }

                return item;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MailController] LoadMailDetail failed: {e.Message}");
                OnError?.Invoke(e.Message);
                Toast.Instance.ShowError("Failed to load mail.");
                return null;
            }
        }

        public async Task<bool> MarkAsRead(string mailId)
        {
            if (isMarkAsReadProgress) return false;

            var existing = allMails.FirstOrDefault(m => m.id == mailId);
            if (existing == null) return false;
            if (existing.isRead) return true;

            isMarkAsReadProgress = true;
            try
            {
                var response = await ApiClient.Instance.Patch<object>(MailRoutes.MarkRead(mailId), null);

                int index = allMails.FindIndex(m => m.id == mailId);
                if (index >= 0)
                {
                    allMails[index].isRead = true;
                    allMails[index].readAt = DateTime.UtcNow.ToString("o");
                }

                currentMail = allMails[index];

                OnMailMarkedAsRead?.Invoke(mailId);

                if (inboxScreenUI != null)
                {
                    inboxScreenUI.UpdateListItem(index, allMails[index]);
                    inboxScreenUI.ShowMailDetail(allMails[index]);
                }

                _ = LoadUnreadCount();
                Toast.Instance.ShowSuccess("Mail marked as read.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MailController] MarkAsRead failed: {e.Message}");
                OnError?.Invoke(e.Message);
                Toast.Instance.ShowError("Failed to mark mail as read.");
                return false;
            }
            finally
            {
                isMarkAsReadProgress = false;
            }
        }

        public async Task<bool> DeleteMail(string mailId)
        {
            if (isDeleteProgress) return false;

            isDeleteProgress = true;
            try
            {
                await ApiClient.Instance.Delete<object>(MailRoutes.Delete(mailId));

                int index = allMails.FindIndex(m => m.id == mailId);
                if (index >= 0)
                {
                    bool wasUnread = !allMails[index].isRead;
                    allMails.RemoveAt(index);

                    if (currentMail != null && currentMail.id == mailId)
                    {
                        currentMail = null;
                        if (inboxScreenUI != null)
                        {
                            inboxScreenUI.ClearMailDetail();
                        }
                    }

                    OnMailDeleted?.Invoke(mailId);

                    if (inboxScreenUI != null)
                    {
                        inboxScreenUI.RemoveListItem(index);
                        inboxScreenUI.ShowEmptyState(allMails.Count == 0);
                    }
                }

                _ = LoadUnreadCount();
                Toast.Instance.ShowSuccess("Mail deleted.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MailController] DeleteMail failed: {e.Message}");
                OnError?.Invoke(e.Message);
                Toast.Instance.ShowError("Failed to delete mail.");
                return false;
            }
            finally
            {
                isDeleteProgress = false;
            }
        }

        public bool CanLoadMore() => currentPage < totalPages && !isLoadingInbox;
        public int GetUnreadCount() => allMails.Count(m => !m.isRead);

        public void Clear()
        {
            allMails.Clear();
            currentMail = null;
            currentPage = 1;
            totalPages = 1;
            inboxScreenUI?.Clear();
        }



    }
}
