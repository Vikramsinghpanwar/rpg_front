using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Rewards.Controllers;
using Features.Rewards.UI;
using Features.Rewards.Models;
using Core.Managers;

namespace Features.Rewards.UI
{
    public class RewardHistoryScreen : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private RewardHistoryController controller;

        [Header("UI References")]
        [SerializeField] private Transform historyContainer;
        [SerializeField] private GameObject historyItemPrefab;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text pageInfoText;
        [SerializeField] private GameObject emptyStateText;
        [SerializeField] private Button closeButton;

        readonly List<RewardHistoryRow> items = new List<RewardHistoryRow>();

        void Awake()
        {
            if (controller == null) controller = FindObjectOfType<RewardHistoryController>();
            if (controller == null)
            {
                var go = new GameObject("RewardHistoryController");
                controller = go.AddComponent<RewardHistoryController>();
            }

            if (previousButton != null) previousButton.onClick.AddListener(OnPreviousClicked);
            if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);

            controller.OnHistoryUpdated += OnHistoryUpdated;
            controller.OnPaginationChanged += OnPaginationChanged;
            controller.OnError += OnError;
        }

        void OnEnable() => _ = LoadHistory(true);

        void OnDestroy()
        {
            if (controller != null)
            {
                controller.OnHistoryUpdated -= OnHistoryUpdated;
                controller.OnPaginationChanged -= OnPaginationChanged;
                controller.OnError -= OnError;
            }
        }

        async System.Threading.Tasks.Task LoadHistory(bool forceRefresh)
        {
            if (controller == null) return;
            await controller.FetchHistory(forceRefresh);
        }

        void OnHistoryUpdated(List<Models.RewardHistoryItem> history)
        {
            foreach (var i in items)
            {
                if (i != null) Destroy(i.gameObject);
            }
            items.Clear();

            bool hasItems = history != null && history.Count > 0;
            if (emptyStateText != null) emptyStateText.SetActive(!hasItems);
            if (!hasItems || historyItemPrefab == null || historyContainer == null)
            {
                UpdatePaginationUI();
                return;
            }

            foreach (var record in history)
            {
                var go = Instantiate(historyItemPrefab, historyContainer);
                var item = go.GetComponent<RewardHistoryRow>();
                if (item != null)
                {
                    item.Setup(record);
                    items.Add(item);
                }
            }

            UpdatePaginationUI();
        }

        void OnPaginationChanged(int page, int total)
        {
            UpdatePaginationUI();
        }

        void UpdatePaginationUI()
        {
            if (controller == null) return;

            bool canPrev = controller.CurrentHistoryPage > 1;
            bool canNext = controller.HasMoreHistory;

            if (previousButton != null) previousButton.interactable = canPrev;
            if (nextButton != null) nextButton.interactable = canNext;

            int page = controller.CurrentHistoryPage;
            int total = controller.TotalHistoryPages;
            if (pageInfoText != null)
            {
                pageInfoText.text = total > 1 ? $"Page {page} / {total}" : $"Page {page}";
            }
        }

        void OnPreviousClicked()
        {
            if (controller == null) return;
            _ = controller.PreviousPage();
        }

        void OnNextClicked()
        {
            if (controller == null) return;
            _ = controller.NextPage();
        }

        void OnError(string message)
        {
            PopupManager.Instance?.ShowError("Failed to load reward history.");
        }

        void OnCloseClicked() => gameObject.SetActive(false);
    }
}
