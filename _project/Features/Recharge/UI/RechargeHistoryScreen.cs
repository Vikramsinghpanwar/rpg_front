using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Recharge.Controllers;
using Core.Managers;

namespace Features.Recharge.UI
{
    public class RechargeHistoryScreen : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private RechargeController controller;

        [Header("UI References")]
        [SerializeField] private Transform historyContainer;
        [SerializeField] private GameObject historyItemPrefab;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text pageInfoText;
        [SerializeField] private GameObject emptyStateText;

        readonly List<RechargeHistoryItem> items = new List<RechargeHistoryItem>();
        int currentPage = 1;
        int totalPages = 1;

        void Awake()
        {
            if (controller == null) controller = FindObjectOfType<RechargeController>();
            if (controller == null)
            {
                var go = new GameObject("RechargeController");
                controller = go.AddComponent<RechargeController>();
            }

            if (previousButton != null) previousButton.onClick.AddListener(OnPreviousClicked);
            if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);

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

        async System.Threading.Tasks.Task LoadHistory(bool forceRefresh)
        {
            if (controller == null) return;
            await controller.FetchHistory(forceRefresh);
            if (controller.HasNonTerminalRecharges())
            {
                _ = controller.RefreshNonTerminalRecharges();
            }
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

        void OnHistoryLoaded(List<Models.RechargeRecord> history)
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

            foreach (var record in history)
            {
                var go = Instantiate(historyItemPrefab, historyContainer);
                var item = go.GetComponent<RechargeHistoryItem>();
                if (item != null)
                {
                    item.Setup(record);
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

        void Close() => Destroy(gameObject);
    }
}
