using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Recharge.Controllers;
using Features.Recharge.Models;
using Core.Managers;

namespace Features.Recharge.UI
{
    public class RechargeGatewayPopup : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private RechargeController controller;

        [Header("Layout")]
        [SerializeField] private Transform gatewayContainer;
        [SerializeField] private GameObject gatewayButtonPrefab;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TMP_Text confirmButtonText;

        [Header("Status")]
        [SerializeField] private TMP_Text errorText;

        public event System.Action OnPaid;
        public event System.Action OnClosed;

        long amountPaisa;
        GatewayInfo selectedGateway;
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
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);

            if (confirmButtonText != null)
                confirmButtonText.text = "Pay Now";

            controller.OnGatewaysLoaded += PopulateGateways;
            controller.OnError += ShowError;
        }

        void OnEnable()
        {
            if (controller.GetGateways().Count == 0) _ = controller.LoadGateways();
            else PopulateGateways(controller.GetGateways());

            selectedGateway = null;
            if (errorText != null) errorText.gameObject.SetActive(false);
            UpdateConfirmButton();
        }

        void OnDestroy()
        {
            if (controller == null) return;
            controller.OnGatewaysLoaded -= PopulateGateways;
            controller.OnError -= ShowError;
        }

        public void SetAmount(long amount)
        {
            amountPaisa = amount;
        }

        void PopulateGateways(List<GatewayInfo> gateways)
        {
            foreach (var b in gatewayButtons) if (b != null) Destroy(b.gameObject);
            gatewayButtons.Clear();
            if (gatewayContainer == null || gatewayButtonPrefab == null) return;

            Debug.Log("[PaymentGateway] Filtering Lobby gateways...");
            int gatewayCounter = 0;
            foreach (var gateway in gateways)
            {
                gatewayCounter++;
                if (gateway.status != "active") continue;
                if (!gateway.enabled_lobby) continue;

                Debug.Log($"[PaymentGateway] Filtered Lobby Gateway - id: {gateway.id}, enabled_lobby: {gateway.enabled_lobby}, enabled_ingame: {gateway.enabled_ingame}");

                var go = Instantiate(gatewayButtonPrefab, gatewayContainer);
                var btn = go.GetComponent<RechargeGatewayButton>();
                if (btn == null) btn = go.AddComponent<RechargeGatewayButton>();
                btn.Setup(gateway, OnGatewayClicked, gatewayCounter);
                gatewayButtons.Add(btn);
            }

            if (gatewayButtons.Count > 0 && selectedGateway == null)
            {
                SelectGateway(gatewayButtons[0].Gateway);
            }

            UpdateConfirmButton();
        }

        void OnGatewayClicked(GatewayInfo gateway)
        {
            SelectGateway(gateway);
        }

        void SelectGateway(GatewayInfo gateway)
        {
            selectedGateway = gateway;
            string sourceInfo = controller != null ? $"cache source: {controller.LoadedSource}" : "unknown cache";
            Debug.Log($"[PaymentGateway] Selected gateway in Lobby: {gateway?.id} ({sourceInfo})");

            foreach (var b in gatewayButtons) b.SetSelected(b.Gateway?.id == gateway.id);
            UpdateConfirmButton();
        }

        void UpdateConfirmButton()
        {
            if (confirmButton != null)
                confirmButton.interactable = selectedGateway != null;
        }

        async void OnConfirmClicked()
        {
            if (selectedGateway == null)
            {
                ShowError("NO_GATEWAY", "Please select a payment method");
                return;
            }

            var url = await controller.CreateRecharge(amountPaisa, selectedGateway.id);
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
                _ = controller.RefreshHistorySilent();
                OnPaid?.Invoke();
                Close();
            }
        }

        void ShowError(string code, string message)
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

        void Close()
        {
            OnClosed?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
