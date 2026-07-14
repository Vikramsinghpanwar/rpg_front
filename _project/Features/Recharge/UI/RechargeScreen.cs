using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Recharge.Controllers;
using Features.Recharge.Models;
using Features.Recharge.Utils;
using Core.Managers;
using Core.Bootstrap;
using Core.Utils;
using Features.Lobby.UI;

namespace Features.Recharge.UI
{
    public class RechargeScreen : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private RechargeController controller;

        [Header("Layout")]
        [SerializeField] private Transform amountContainer;
        [SerializeField] private GameObject amountButtonPrefab;

        [Header("Amount Display")]
        [SerializeField] private TMP_Text amountSelectedText;
        [SerializeField] private TMP_Text bonusSelectedText;
        [SerializeField] private TMP_Text totalReceiveText;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button rechargeToggle_button;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button historyButton;
        [SerializeField] private RefreshButtonAnimator refreshButtonAnimator;

        [Header("Toggle Sprites")]
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite unselectedSprite;
        [SerializeField] private Image rechargeToggleImage;
        [SerializeField] private Image historyToggleImage;

        [Header("Custom Amount")]
        [SerializeField] private TMP_InputField customAmountInput;

        [Header("Gateway Popup")]
        [SerializeField] private RechargeGatewayPopup gatewayPopup;

        [Header("Status")]
        [SerializeField] private TMP_Text errorText;

        [Header("History Screen")]
        [SerializeField] private GameObject historyScreen;
        [SerializeField] private GameObject rechargeScreen;

        RechargeAmountPreset selectedAmount;
        long customAmountPaisa;
        long customBonusPaisa;
        bool _suppressCustomAmountEvent;
        readonly List<RechargeAmountButton> amountButtons = new List<RechargeAmountButton>();

        void Awake()
        {
            if (controller == null) controller = FindObjectOfType<RechargeController>();
            if (controller == null)
            {
                var go = new GameObject("RechargeController");
                controller = go.AddComponent<RechargeController>();
            }

            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (refreshButton != null) refreshButton.onClick.AddListener(OnRefreshClicked);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
            if (rechargeToggle_button != null) rechargeToggle_button.onClick.AddListener(OpenRechargeScreen);
            if (historyButton != null) historyButton.onClick.AddListener(OpenHistory);

            if (gatewayPopup != null)
            {
                gatewayPopup.OnPaid += OnPaidFromPopup;
                gatewayPopup.OnClosed += OnGatewayPopupClosed;
            }

            controller.OnAmountsLoaded += OnAmountsLoaded;
            controller.OnRechargeCreated += OnRechargeCreated;
            controller.OnError += OnError;
            controller.OnGatewaysLoaded += OnGatewaysLoaded;
            BootstrapService.Instance.OnBootstrapUpdated += OnBootstrapUpdated;

            if (customAmountInput != null)
            {
                customAmountInput.contentType = TMP_InputField.ContentType.IntegerNumber;
                customAmountInput.onValueChanged.AddListener(OnCustomAmountChanged);
            }
        }

        void OnEnable()
        {
            ResetToDefaultTab();
            selectedAmount = null;
            if (customAmountInput != null)
            {
                _suppressCustomAmountEvent = true;
                customAmountInput.text = "";
                _suppressCustomAmountEvent = false;
            }
            customAmountPaisa = 0;
            customBonusPaisa = 0;
            if (errorText != null) errorText.gameObject.SetActive(false);
            if (continueButton != null) continueButton.interactable = false;

            OnAmountsLoaded(controller.GetDefaultAmounts());
            UpdateAmountDisplay();
        }

        void OnDestroy()
        {
            if (controller == null) return;
            controller.OnAmountsLoaded -= OnAmountsLoaded;
            controller.OnRechargeCreated -= OnRechargeCreated;
            controller.OnError -= OnError;
            controller.OnGatewaysLoaded -= OnGatewaysLoaded;
            BootstrapService.Instance.OnBootstrapUpdated -= OnBootstrapUpdated;

            if (gatewayPopup != null)
            {
                gatewayPopup.OnPaid -= OnPaidFromPopup;
                gatewayPopup.OnClosed -= OnGatewayPopupClosed;
            }

            if (refreshButtonAnimator != null)
                refreshButtonAnimator.ForceStop();

            if (customAmountInput != null)
                customAmountInput.onValueChanged.RemoveListener(OnCustomAmountChanged);
        }

        void OnAmountsLoaded(List<RechargeAmountPreset> amounts)
        {
            foreach (var a in amountButtons) if (a != null) Destroy(a.gameObject);
            amountButtons.Clear();
            if (amountContainer == null || amountButtonPrefab == null) return;

            if (amounts == null || amounts.Count == 0)
            {
                if (errorText != null)
                {
                    errorText.text = "Recharge presets unavailable. Please try again later.";
                    errorText.gameObject.SetActive(true);
                }
                if (continueButton != null) continueButton.interactable = false;
                UpdateAmountDisplay();
                return;
            }

            amounts.Sort((a, b) => a.amount_paisa.CompareTo(b.amount_paisa));

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

            if (amountButtons.Count > 0 && selectedAmount == null && customAmountPaisa == 0)
            {
                amountButtons[0].Select();
            }
            else
            {
                UpdateAmountDisplay();
            }
        }

        void OnBootstrapUpdated(Core.Models.BootstrapResponse resp)
        {
            controller.RefreshAmountsFromBootstrap();
            OnAmountsLoaded(controller.GetDefaultAmounts());
            if (customAmountPaisa > 0)
            {
                var presets = controller != null ? controller.GetDefaultAmounts() : null;
                customBonusPaisa = RechargeBonusCalculator.GetApplicableBonus(customAmountPaisa, presets);
                UpdateAmountDisplay();
            }
        }

        void OnCustomAmountChanged(string value)
        {
            if (_suppressCustomAmountEvent) return;

            string cleaned = value?.Replace(",", "") ?? "";
            if (long.TryParse(cleaned, out long rupees) && rupees > 0)
            {
                customAmountPaisa = rupees * 100;
            }
            else
            {
                customAmountPaisa = 0;
            }

            selectedAmount = null;
            foreach (var btn in amountButtons) btn.SetSelected(false);

            var presets = controller != null ? controller.GetDefaultAmounts() : null;
            customBonusPaisa = RechargeBonusCalculator.GetApplicableBonus(customAmountPaisa, presets);

            UpdateAmountDisplay();
            ValidateContinueButton();
        }

        void ValidateContinueButton()
        {
            if (continueButton != null)
                continueButton.interactable = selectedAmount != null || customAmountPaisa > 0;
        }

        public void SelectAmount(RechargeAmountPreset amount, RechargeAmountButton button)
        {
            selectedAmount = amount;
            if (customAmountInput != null)
            {
                _suppressCustomAmountEvent = true;
                customAmountInput.text = "";
                _suppressCustomAmountEvent = false;
            }
            customAmountPaisa = 0;
            customBonusPaisa = 0;
            foreach (var btn in amountButtons) btn.SetSelected(btn == button);
            UpdateAmountDisplay();
            ValidateContinueButton();
        }

        void UpdateAmountDisplay()
        {
            long amountPaisa = selectedAmount != null ? selectedAmount.amount_paisa : customAmountPaisa;
            long bonusPaisa = selectedAmount != null ? selectedAmount.bonus_amount_paisa : customBonusPaisa;

            string amountStr = "";
            string bonusStr = "";
            string totalStr = "";

            if (amountPaisa > 0)
            {
                amountStr = MoneyFormatter.FormatPaisaRoundedRupees(amountPaisa);
                bonusStr = bonusPaisa > 0
                    ? $"+{MoneyFormatter.FormatPaisaRoundedRupees(bonusPaisa)} bonus"
                    : "";
                totalStr = MoneyFormatter.FormatPaisaRoundedRupees(amountPaisa + bonusPaisa);
            }

            if (amountSelectedText != null) amountSelectedText.text = amountStr;
            if (bonusSelectedText != null) bonusSelectedText.text = bonusStr;
            if (totalReceiveText != null) totalReceiveText.text = totalStr;
        }

        async void OnContinueClicked()
        {
            long currentAmount = selectedAmount != null ? selectedAmount.amount_paisa : customAmountPaisa;
            if (currentAmount <= 0)
            {
                ShowInlineError("Please select an amount first");
                return;
            }

            OpenGatewayPopup();
        }

        void OpenGatewayPopup()
        {
            if (gatewayPopup == null) return;

            long amount = selectedAmount != null ? selectedAmount.amount_paisa : customAmountPaisa;
            gatewayPopup.SetAmount(amount);
            gatewayPopup.gameObject.SetActive(true);
            gatewayPopup.transform.SetAsLastSibling();
        }

        void OnPaidFromPopup()
        {
            Close();
        }

        void OnGatewayPopupClosed()
        {
        }

        async void OpenRechargeScreen()
        {
            historyScreen?.SetActive(false);
            rechargeScreen?.SetActive(true);
            if (historyToggleImage != null && unselectedSprite != null) historyToggleImage.sprite = unselectedSprite;
            if (rechargeToggleImage != null && selectedSprite != null) rechargeToggleImage.sprite = selectedSprite;
        }

        void OnRechargeCreated(CreateRechargeResponse response)
        {
            long bonus = response.bonus_amount > 0 ? response.bonus_amount : 0;
            string bonusStr = bonus > 0 ? $"\nBonus: {RechargeController.FormatAmount(bonus)}" : "";
            string totalStr = bonus > 0 ? $"\nTotal credited: {RechargeController.FormatAmount(response.amount + bonus)}" : "";
            PopupManager.Instance?.Show(
                "Recharge Initiated",
                $"Your recharge of {RechargeController.FormatAmount(response.amount)} has been initiated. Complete payment in the browser.{bonusStr}{totalStr}",
                "OK");
        }

        void OnRefreshClicked()
        {
            refreshButtonAnimator?.StartSpin();
            _ = controller.LoadGateways();
        }

        void OnGatewaysLoaded(List<GatewayInfo> gateways)
        {
            refreshButtonAnimator?.StopSpin();
        }

        void OnError(string code, string message)
        {
            ShowInlineError(message);
            refreshButtonAnimator?.StopSpin();
        }

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
            if (rechargeToggleImage != null && unselectedSprite != null) rechargeToggleImage.sprite = unselectedSprite;
            if (historyToggleImage != null && selectedSprite != null) historyToggleImage.sprite = selectedSprite;
            historyScreen = historyScreen ?? Resources.Load<GameObject>("RechargeHistoryScreen");

            _ = controller.FetchHistory(true);
        }

        void ResetToDefaultTab()
        {
            OpenRechargeScreen();
        }

        void Close() => gameObject?.SetActive(false);
    }
}
