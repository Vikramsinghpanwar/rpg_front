using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Features.Rewards.Models;
using Core.API;
using Core.API.Endpoints;
using Core.Managers;
using Core.Utils;

namespace Features.Rewards.Controllers
{
    public class RewardHistoryController : MonoBehaviour
    {
        [Header("Pagination")]
        [SerializeField] private int historyPageSize = 20;

        public int CurrentHistoryPage { get; private set; } = 1;
        public int TotalHistoryPages { get; private set; } = 1;
        public bool HasMoreHistory { get; private set; }

        public event Action<List<RewardHistoryItem>> OnHistoryUpdated;
        public event Action<int, int> OnPaginationChanged;
        public event Action<string> OnError;

        readonly List<RewardHistoryItem> cachedHistory = new List<RewardHistoryItem>();
        bool isLoadingHistory;

        public List<RewardHistoryItem> GetCachedHistory() => cachedHistory;

        public async Task FetchHistory(bool forceRefresh = false)
        {
            if (isLoadingHistory) return;

            int page = forceRefresh ? 1 : CurrentHistoryPage;

            isLoadingHistory = true;
            LoadingManager.Instance?.Show("Loading reward history...");
            try
            {
                string url = $"{RewardRoutes.RewardHistory}?page={page}&limit={historyPageSize}";
                var wrapper = await ApiClient.Instance.Get<RewardHistoryResponse>(url);

                cachedHistory.Clear();
                if (wrapper != null && wrapper.items != null)
                {
                    cachedHistory.AddRange(wrapper.items);
                }

                CurrentHistoryPage = page;
                if (wrapper != null && wrapper.pagination != null)
                {
                    TotalHistoryPages = wrapper.pagination.total > 0
                        ? (int)Mathf.Ceil((float)wrapper.pagination.total / wrapper.pagination.limit)
                        : 1;
                    HasMoreHistory = wrapper.pagination.hasNext;
                }
                else
                {
                    TotalHistoryPages = 1;
                    HasMoreHistory = false;
                }

                OnHistoryUpdated?.Invoke(new List<RewardHistoryItem>(cachedHistory));
                OnPaginationChanged?.Invoke(CurrentHistoryPage, TotalHistoryPages);
            }
            catch (ApiException e)
            {
                Debug.LogError($"Load reward history failed: {e.Message}");
                OnError?.Invoke("HISTORY_FAILED");
                Toast.Instance.ShowError("Failed to load reward history.");
            }
            finally
            {
                isLoadingHistory = false;
                LoadingManager.Instance?.Hide();
            }
        }

        public Task RefreshHistory() => FetchHistory(forceRefresh: true);

        public async Task NextPage()
        {
            if (isLoadingHistory || !HasMoreHistory) return;
            CurrentHistoryPage++;
            await FetchHistory();
        }

        public async Task PreviousPage()
        {
            if (isLoadingHistory || CurrentHistoryPage <= 1) return;
            CurrentHistoryPage--;
            await FetchHistory();
        }
    }
}
