using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.GameHistory.Controllers;
using Features.GameHistory.Models;
using Core.Managers;

namespace Features.GameHistory.UI
{
    public class GameHistoryScreen : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private GameHistoryController controller;

        [Header("UI References")]
        [SerializeField] private Transform historyContainer;
        [SerializeField] private GameObject historyItemPrefab;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text pageInfoText;
        [SerializeField] private GameObject emptyStateText;

        [Header("Navigation")]
        [SerializeField] private Button closeButton;

        readonly List<GameHistoryEntry> items = new List<GameHistoryEntry>();
        int currentPage = 1;
        int totalPages = 1;

        void Awake()
        {
            if (controller == null) controller = FindObjectOfType<GameHistoryController>();
            if (controller == null)
            {
                var go = new GameObject("GameHistoryController");
                controller = go.AddComponent<GameHistoryController>();
            }

            if (previousButton != null) previousButton.onClick.AddListener(OnPreviousClicked);
            if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);

            controller.OnHistoryLoaded += OnHistoryLoaded;
            controller.OnPaginationChanged += OnPaginationChanged;
            controller.OnError += OnError;
        }

        void OnEnable() => _ = LoadHistory(true);

        void OnDestroy()
        {
            if (controller != null)
            {
                controller.OnHistoryLoaded -= OnHistoryLoaded;
                controller.OnPaginationChanged -= OnPaginationChanged;
                controller.OnError -= OnError;
            }
        }

        async Task LoadHistory(bool forceRefresh)
        {
            if (controller == null) return;
            await controller.FetchHistory(forceRefresh);
        }

        void OnPreviousClicked()
        {
            currentPage = controller.CurrentHistoryPage;
            _ = controller.PreviousPage();
        }

        void OnNextClicked()
        {
            currentPage = controller.CurrentHistoryPage;
            _ = controller.NextPage();
        }

        void OnPaginationChanged(int page, int total)
        {
            currentPage = page;
            totalPages = total;
            UpdatePaginationUI();
        }

        void OnHistoryLoaded(List<GameHistoryItem> history)
        {
            foreach (var i in items) if (i != null) Destroy(i.gameObject);
            items.Clear();

            bool hasItems = history != null && history.Count > 0;
            if (emptyStateText != null) emptyStateText.SetActive(!hasItems);
            if (!hasItems || historyItemPrefab == null || historyContainer == null)
            {
                UpdatePaginationUI();
                return;
            }

            foreach (var entry in history)
            {
                var go = Instantiate(historyItemPrefab, historyContainer);
                var item = go.GetComponent<GameHistoryEntry>();
                if (item != null)
                {
                    item.Setup(entry);
                    items.Add(item);
                }
            }

            UpdatePaginationUI();
        }

        void UpdatePaginationUI()
        {
            bool canPrev = controller != null && controller.CurrentHistoryPage > 1;
            bool canNext = controller != null && controller.HasMoreHistory;

            if (previousButton != null) previousButton.interactable = canPrev;
            if (nextButton != null) nextButton.interactable = canNext;

            int page = controller != null ? controller.CurrentHistoryPage : currentPage;
            int total = controller != null ? controller.TotalHistoryPages : totalPages;
            if (pageInfoText != null)
            {
                pageInfoText.text = total > 1 ? $"Page {page} / {total}" : $"Page {page}";
            }
        }

        void OnError(string code, string message)
        {
            PopupManager.Instance?.ShowError(message);
        }

        void Close()
        {
            if (gameObject != null)
                gameObject.SetActive(false);
        }
    }
}
