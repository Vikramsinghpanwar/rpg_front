using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Recharge.Controllers;
using Features.Recharge.Models;
using Core.Managers;

namespace Features.Recharge.UI
{
    // Drop this prefab into a scene, wire the SerializeFields, press Play.
    // Auth/ApiClient/LoadingManager/PopupManager are bootstrapped by Core before any scene loads.
    public class RechargeScreen : MonoBehaviour
    {
        [Header("Controller")]
        [Tooltip("Assign in inspector. Falls back to FindObjectOfType then auto-creates if missing.")]
        [SerializeField] private RechargeController controller;

        [Header("Layout")]
        [SerializeField] private Transform amountContainer;
        [SerializeField] private Transform gatewayContainer;
        [SerializeField] private GameObject amountButtonPrefab;
        [SerializeField] private GameObject gatewayButtonPrefab;

        [Header("Buttons")]
        [SerializeField] private Button rechargeButton;
        [SerializeField] private Button rechargeToggle_button;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button historyButton;

        [Header("Status")]
        [SerializeField] private TMP_Text errorText;

        [Header("History Screen")]
        [Tooltip("Optional. If assigned, History button opens this prefab; otherwise it tries Resources.")]
        [SerializeField] private GameObject historyScreen;
        [SerializeField] private GameObject rechargeScreen;

        RechargeAmountPreset selectedAmount;
        GatewayInfo selectedGateway;
        readonly List<RechargeAmountButton> amountButtons = new List<RechargeAmountButton>();
        readonly List<RechargeGatewayButton> gatewayButtons = new List<RechargeGatewayButton>();

        void Awake()
        {
            if (controller == null) controller = FindObjectOfType<RechargeController>();
            if (controller == null)
            {
                var go = new GameObject("RechargeController");
                controller = go.AddComponent<RechargeController>();
            }

            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (refreshButton != null) refreshButton.onClick.AddListener(() => _ = controller.LoadGateways());
            if (rechargeButton != null) rechargeButton.onClick.AddListener(OnRechargeClicked);
            if (rechargeToggle_button != null) rechargeToggle_button.onClick.AddListener(OpenRechargeScreen);
            if (historyButton != null) historyButton.onClick.AddListener(OpenHistory);

            controller.OnGatewaysLoaded += OnGatewaysLoaded;
            controller.OnAmountsLoaded += OnAmountsLoaded;
            controller.OnRechargeCreated += OnRechargeCreated;
            controller.OnError += OnError;
        }

        void OnEnable()
        {
            if (controller.GetGateways().Count == 0) _ = controller.LoadGateways();
            else OnGatewaysLoaded(controller.GetGateways());

            OnAmountsLoaded(controller.GetDefaultAmounts());

            selectedAmount = null;
            selectedGateway = null;
            if (errorText != null) errorText.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (controller == null) return;
            controller.OnGatewaysLoaded -= OnGatewaysLoaded;
            controller.OnAmountsLoaded -= OnAmountsLoaded;
            controller.OnRechargeCreated -= OnRechargeCreated;
            controller.OnError -= OnError;
        }

        void OnGatewaysLoaded(List<GatewayInfo> gateways)
        {
            foreach (var b in gatewayButtons) if (b != null) Destroy(b.gameObject);
            gatewayButtons.Clear();
            if (gatewayContainer == null || gatewayButtonPrefab == null) return;

            foreach (var gateway in gateways)
            {
                if (gateway.status != "active") continue;
                if (!gateway.enabled_lobby && !gateway.enabled_ingame) continue;

                var go = Instantiate(gatewayButtonPrefab, gatewayContainer);
                var btn = go.GetComponent<RechargeGatewayButton>();
                if (btn == null) btn = go.AddComponent<RechargeGatewayButton>();
                btn.Setup(gateway, SelectGateway);
                gatewayButtons.Add(btn);
            }

            if (gatewayButtons.Count > 0 && selectedGateway == null)
            {
                SelectGateway(gatewayButtons[0].Gateway);
            }
        }

        void OnAmountsLoaded(List<RechargeAmountPreset> amounts)
        {
            foreach (var a in amountButtons) if (a != null) Destroy(a.gameObject);
            amountButtons.Clear();
            if (amountContainer == null || amountButtonPrefab == null) return;

            foreach (var amount in amounts)
            {
                var go = Instantiate(amountButtonPrefab, amountContainer);
                var amountBtn = go.GetComponent<RechargeAmountButton>();
                if (amountBtn != null)
                {
                    amountBtn.Setup(amount, this);
                    amountButtons.Add(amountBtn);
                }
            }

            if (amountButtons.Count > 0 && selectedAmount == null) amountButtons[0].Select();
        }

        void SelectGateway(GatewayInfo gateway)
        {
            selectedGateway = gateway;
            foreach (var b in gatewayButtons) b.SetSelected(b.Gateway?.id == gateway.id);
        }

        public void SelectAmount(RechargeAmountPreset amount, RechargeAmountButton button)
        {
            selectedAmount = amount;
            foreach (var btn in amountButtons) btn.SetSelected(btn == button);
        }

        async void OnRechargeClicked()
        {
            if (selectedAmount == null) { ShowInlineError("Please select an amount"); return; }
            if (selectedGateway == null) { ShowInlineError("Please select a payment method"); return; }

            var url = await controller.CreateRecharge(selectedAmount.amount_paisa, selectedGateway.id);
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
                Close();
            }
        }
        async void OpenRechargeScreen()
        {
            historyScreen?.SetActive(false);
            rechargeScreen?.SetActive(true);
        }

        void OnRechargeCreated(CreateRechargeResponse response)
        {
            PopupManager.Instance?.Show(
                "Recharge Initiated",
                $"Your recharge of {RechargeController.FormatAmount(response.amount)} has been initiated. Complete payment in the browser.",
                "OK");
        }

        void OnError(string code, string message) => ShowInlineError(message);

        void ShowInlineError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
                CancelInvoke(nameof(HideError));
                Invoke(nameof(HideError), 3f);
            }
            else
            {
                PopupManager.Instance?.ShowError(message);
            }
        }

        void HideError()
        {
            if (errorText != null) errorText.gameObject.SetActive(false);
        }

        public void OpenHistory()
        {
            historyScreen?.SetActive(true);
            rechargeScreen?.SetActive(false);
            historyScreen = historyScreen ?? Resources.Load<GameObject>("RechargeHistoryScreen");
        }

        void Close() => Destroy(gameObject);
    }
}
