using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Core.API;
using Core.API.Endpoints;
using Core.Managers;
using Core.Utils;
using Features.GameHistory.Models;

namespace Features.GameHistory.Controllers
{
    public class GameHistoryController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private int historyPageSize = 20;

        public int CurrentHistoryPage { get; private set; } = 1;
        public int TotalHistoryPages { get; private set; } = 1;
        public bool HasMoreHistory { get; private set; }

        public event Action<List<GameHistoryItem>> OnHistoryLoaded;
        public event Action<int, int> OnPaginationChanged;
        public event Action<string, string> OnError;

        readonly List<GameHistoryItem> cachedHistory = new List<GameHistoryItem>();
        bool isLoadingHistory;
        string currentGameType;

        public List<GameHistoryItem> GetCachedHistory() => cachedHistory;

        public async Task FetchHistory(bool forceRefresh = false)
        {
            if (isLoadingHistory) return;

            int page = forceRefresh ? 1 : CurrentHistoryPage;
            int offset = (page - 1) * historyPageSize;

            isLoadingHistory = true;
            LoadingManager.Instance?.Show("Loading history...");
            try
            {
                var response = await ApiClient.Instance.Get<GameHistoryResponse>(
                    GameHistoryRoutes.List(historyPageSize, offset, currentGameType));

                cachedHistory.Clear();
                if (response != null && response.items != null)
                {
                    cachedHistory.AddRange(response.items);
                }

                CurrentHistoryPage = page;
                TotalHistoryPages = response != null && response.total > 0
                    ? (int)Mathf.Ceil((float)response.total / historyPageSize)
                    : 1;
                HasMoreHistory = response != null && (offset + historyPageSize) < response.total;

                OnHistoryLoaded?.Invoke(new List<GameHistoryItem>(cachedHistory));
                OnPaginationChanged?.Invoke(CurrentHistoryPage, TotalHistoryPages);
            }
            catch (ApiException e)
            {
                Debug.LogError($"Load game history failed: {e.Message}");
                var message = FriendlyMessage(e);
                OnError?.Invoke(e.ErrorCode ?? "HISTORY_FAILED", message);
                Toast.Instance.ShowError(message);
            }
            finally
            {
                isLoadingHistory = false;
                LoadingManager.Instance?.Hide();
            }
        }

        public Task NextPage()
        {
            if (isLoadingHistory || !HasMoreHistory) return Task.CompletedTask;
            CurrentHistoryPage++;
            return FetchHistory();
        }

        public Task PreviousPage()
        {
            if (isLoadingHistory || CurrentHistoryPage <= 1) return Task.CompletedTask;
            CurrentHistoryPage--;
            return FetchHistory();
        }

        public Task RefreshHistory() => FetchHistory(forceRefresh: true);

        public void SetGameTypeFilter(string gameType)
        {
            currentGameType = gameType;
            CurrentHistoryPage = 1;
        }

        public static string FormatPaisa(long paisa)
        {
            return MoneyFormatter.FormatPaisa(paisa);
        }

        public static string FormatDate(string iso)
        {
            if (MoneyFormatter.TryParseDate(iso, out var localTime))
            {
                return localTime.ToString("dd MMM yyyy, hh:mm tt");
            }
            return iso ?? "";
        }

        public static Color GetNetResultColor(long netResult)
        {
            if (netResult > 0) return new Color(0.2f, 0.8f, 0.2f);
            if (netResult < 0) return new Color(0.9f, 0.2f, 0.2f);
            return Color.white;
        }

        static string FriendlyMessage(ApiException e)
        {
            return e.StatusCode switch
            {
                0 => "No internet connection. Please check your network.",
                400 => "Invalid request. Please try again.",
                422 => "Unable to load game history. Please try again.",
                502 => "Service temporarily unavailable. Please try again.",
                503 => "Game history service is down. Please try later.",
                _ => "Failed to load game history. Please try again."
            };
        }
    }
}
