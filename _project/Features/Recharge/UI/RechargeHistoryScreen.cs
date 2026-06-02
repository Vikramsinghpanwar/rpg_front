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
        [SerializeField] private Button refreshButton;
        [SerializeField] private TMP_Text refreshCooldownText;
        [SerializeField] private GameObject emptyStateText;

        [Header("Config")]
        [SerializeField] private float refreshCooldownSeconds = 3f;

        readonly List<RechargeHistoryItem> items = new List<RechargeHistoryItem>();
        float lastRefreshTime = -999f;

        void Awake()
        {
            if (controller == null) controller = FindObjectOfType<RechargeController>();
            if (controller == null)
            {
                var go = new GameObject("RechargeController");
                controller = go.AddComponent<RechargeController>();
            }

            if (refreshButton != null) refreshButton.onClick.AddListener(OnRefreshClicked);

            controller.OnHistoryLoaded += OnHistoryLoaded;
        }

        void OnEnable() => _ = LoadHistory(forceRefresh: false);

        void OnDestroy()
        {
            if (controller != null) controller.OnHistoryLoaded -= OnHistoryLoaded;
        }

        async System.Threading.Tasks.Task LoadHistory(bool forceRefresh)
        {
            await controller.FetchHistory(forceRefresh);
            if (controller.HasNonTerminalRecharges())
            {
                _ = controller.RefreshNonTerminalRecharges();
            }
        }

        void OnRefreshClicked()
        {
            if (Time.time - lastRefreshTime < refreshCooldownSeconds)
            {
                float remaining = refreshCooldownSeconds - (Time.time - lastRefreshTime);
                PopupManager.Instance?.Show("Cooldown", $"Please wait {remaining:F0} seconds before refreshing again", "OK");
                return;
            }
            lastRefreshTime = Time.time;
            _ = LoadHistory(forceRefresh: true);
        }

        void OnHistoryLoaded(List<Models.RechargeRecord> history)
        {
            foreach (var i in items) if (i != null) Destroy(i.gameObject);
            items.Clear();

            bool hasItems = history != null && history.Count > 0;
            if (emptyStateText != null) emptyStateText.SetActive(!hasItems);
            if (!hasItems || historyItemPrefab == null || historyContainer == null) return;

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
        }

        void Close() => Destroy(gameObject);

        void Update()
        {
            if (refreshCooldownText == null || refreshButton == null) return;
            if (lastRefreshTime <= 0f) { refreshCooldownText.text = ""; refreshButton.interactable = true; return; }
            float remaining = refreshCooldownSeconds - (Time.time - lastRefreshTime);
            if (remaining > 0)
            {
                refreshCooldownText.text = $"Refresh ({remaining:F0}s)";
                refreshButton.interactable = false;
            }
            else
            {
                refreshCooldownText.text = "";
                refreshButton.interactable = true;
            }
        }
    }
}
