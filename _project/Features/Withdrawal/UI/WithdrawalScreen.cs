using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Withdrawal.Models;
using Features.Withdrawal.Controllers;
using Core.Managers;
using Core.Utils;
using Core.Bootstrap;
using Core.Models;
using System.Threading.Tasks;
using Features.Lobby.UI;

namespace Features.Withdrawal.UI
{
    public class WithdrawalScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WithdrawalController controller;

        [Header("Balance Display")]
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private TextMeshProUGUI bonusBalanceText;
        [SerializeField] private TextMeshProUGUI withdrawableBalanceText;
        [SerializeField] private Button refreshBalanceButton;
        [SerializeField] private RefreshButtonAnimator refreshButtonAnimator;

        [Header("Bank Details Display")]
        [SerializeField] private TextMeshProUGUI bankNameText;
        [SerializeField] private TextMeshProUGUI bankAccountNumberText;
        [SerializeField] private TextMeshProUGUI bankIfscText;
        [SerializeField] private TextMeshProUGUI bankHolderNameText;

        [Header("Add Bank Panel")]
        [SerializeField] private TMP_InputField addBankAccountInput;
        [SerializeField] private TMP_InputField addBankIfscInput;
        [SerializeField] private TMP_InputField addBankHolderInput;
        [SerializeField] private TMP_Dropdown bankNameDropdown;
        [SerializeField] private Button saveBankButton;

        [Header("Amount Input")]
        [SerializeField] private TMP_InputField amountInput;

        [Header("Presets")]
        [SerializeField] private Transform presetContainer;
        [SerializeField] private GameObject presetButtonPrefab;

        [Header("Submit")]
        [SerializeField] private Button submitButton;
        [SerializeField] private TextMeshProUGUI feePreviewText;

        [Header("History Pagination")]
        [SerializeField] private Transform historyContainer;
        [SerializeField] private GameObject historyItemPrefab;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text pageInfoText;
        [SerializeField] private GameObject emptyHistoryText;

        [Header("Navigation")]
        [SerializeField] private Button withdrawalToggleButton;
        [SerializeField] private Button historyToggleButton;
        [SerializeField] private Button addBankButton;
        [SerializeField] private GameObject historyPanel;
        [SerializeField] private GameObject withdrawalPanel;
        [SerializeField] private GameObject addBankPanel;

        [Header("Toggle Sprites")]
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite unselectedSprite;
        [SerializeField] private Image withdrawalToggleImage;
        [SerializeField] private Image historyToggleImage;
        [SerializeField] private Image addBankToggleImage;

        private long _currentBalance = 0;
        private long _requestedAmount = 0;
        private bool _isLoading = false;
        private bool _suppressPresetClear = false;
        private readonly List<Button> presetButtons = new List<Button>();

        private void Awake()
        {
            if (controller == null) controller = FindObjectOfType<WithdrawalController>();
            if (controller == null)
            {
                var go = new GameObject("WithdrawalController");
                controller = go.AddComponent<WithdrawalController>();
            }
        }

        private void OnEnable()
        {
            ShowWithdrawalTab();
            if (controller != null)
            {
                controller.OnSavedBankAccountUpdated += OnSavedBankAccountUpdated;
            }

            ApplyBootstrapBankDetails();
            UpdateBankDetailsDisplay();
            ValidateSubmitButton();
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.OnSavedBankAccountUpdated -= OnSavedBankAccountUpdated;
            }
        }

        private void Start()
        {
            SetupUI();
            RegisterEvents();
            ResolveRefreshAnimator();
            InitializeScreen();
        }

        private void ResolveRefreshAnimator()
        {
            if (refreshButtonAnimator != null) return;
            if (refreshBalanceButton == null) return;

            var animators = refreshBalanceButton.GetComponentsInParent<RefreshButtonAnimator>(true);
            if (animators != null && animators.Length > 0)
            {
                refreshButtonAnimator = animators[0];
                return;
            }

            refreshButtonAnimator = refreshBalanceButton.GetComponent<RefreshButtonAnimator>();
            if (refreshButtonAnimator == null)
            {
                refreshButtonAnimator = refreshBalanceButton.gameObject.AddComponent<RefreshButtonAnimator>();
            }
            if (refreshButtonAnimator != null && refreshButtonAnimator.targetRect == null)
            {
                var btnRect = refreshBalanceButton.GetComponent<RectTransform>();
                if (btnRect != null) refreshButtonAnimator.targetRect = btnRect;
            }
        }

        private void SetupUI()
        {
            Debug.Log("[WithdrawalScreen] SetupUI START");

            if (amountInput == null)
            {
                Debug.LogError("[WithdrawalScreen] FATAL — amountInput is null! Assign it in the Inspector. Aborting SetupUI.");
                return;
            }
            amountInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            amountInput.onValueChanged.AddListener(OnAmountChanged);
            Debug.Log("[WithdrawalScreen] amountInput wired");

            if (submitButton == null)
            {
                Debug.LogError("[WithdrawalScreen] FATAL — submitButton is null! Assign it in the Inspector. Aborting SetupUI.");
                return;
            }
            submitButton.onClick.AddListener(OnSubmitClicked);
            Debug.Log("[WithdrawalScreen] submitButton.onClick wired to OnSubmitClicked");

            if (refreshBalanceButton == null)
            {
                Debug.LogError("[WithdrawalScreen] refreshBalanceButton is null — balance refresh button will not work");
            }
            else
            {
                refreshBalanceButton.onClick.AddListener(OnRefreshBalanceClicked);
                Debug.Log("[WithdrawalScreen] refreshBalanceButton wired");
            }

            Debug.Log("[WithdrawalScreen] SetupUI - Navigation Buttons");
            if (addBankButton != null) addBankButton.onClick.AddListener(ShowAddBankTab);
            if (withdrawalToggleButton != null) withdrawalToggleButton.onClick.AddListener(ShowWithdrawalTab);
            if (historyToggleButton != null) historyToggleButton.onClick.AddListener(ShowHistoryTab);

            if (previousButton != null) previousButton.onClick.AddListener(OnPreviousClicked);
            if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);

            if (saveBankButton != null) saveBankButton.onClick.AddListener(OnSaveBankClicked);
            else Debug.LogWarning("[WithdrawalScreen] saveBankButton is null — not wired in Inspector");

            Debug.Log("[WithdrawalScreen] SetupUI - Populating Bank Details");
            ApplyBootstrapBankDetails();
            Debug.Log("[WithdrawalScreen] SetupUI COMPLETE");
        }

        private void ShowWithdrawalTab()
        {
            withdrawalPanel?.SetActive(true);
            historyPanel?.SetActive(false);
            addBankPanel?.SetActive(false);
            if (withdrawalToggleImage != null && selectedSprite != null) withdrawalToggleImage.sprite = selectedSprite;
            if (historyToggleImage != null && unselectedSprite != null) historyToggleImage.sprite = unselectedSprite;
            if (addBankToggleImage != null && unselectedSprite != null) addBankToggleImage.sprite = unselectedSprite;
        }

        private void ShowHistoryTab()
        {
            historyPanel?.SetActive(true);
            withdrawalPanel?.SetActive(false);
            addBankPanel?.SetActive(false);
            if (historyToggleImage != null && selectedSprite != null) historyToggleImage.sprite = selectedSprite;
            if (withdrawalToggleImage != null && unselectedSprite != null) withdrawalToggleImage.sprite = unselectedSprite;
            if (addBankToggleImage != null && unselectedSprite != null) addBankToggleImage.sprite = unselectedSprite;
        }

        private void ShowAddBankTab()
        {
            addBankPanel?.SetActive(true);
            withdrawalPanel?.SetActive(false);
            historyPanel?.SetActive(false);

            Debug.Log($"[WithdrawalScreen] ShowAddBankTab: panelActive={addBankPanel != null && addBankPanel.activeSelf} saveBtn={saveBankButton} accInput={addBankAccountInput} ifscInput={addBankIfscInput} holderInput={addBankHolderInput} dropdown={bankNameDropdown}");
            if (addBankToggleImage != null && selectedSprite != null) addBankToggleImage.sprite = selectedSprite;
            if (withdrawalToggleImage != null && unselectedSprite != null) withdrawalToggleImage.sprite = unselectedSprite;
            if (historyToggleImage != null && unselectedSprite != null) historyToggleImage.sprite = unselectedSprite;
        }

        private void RegisterEvents()
        {
            if (controller == null) return;

            controller.OnHistoryUpdated += OnHistoryUpdated;
            controller.OnWithdrawalCreated += OnWithdrawalCreated;
            controller.OnWithdrawalCancelled += OnWithdrawalCancelled;
            controller.OnBalanceCheckNeeded += OnBalanceNeedsRefresh;
            controller.OnWithdrawalPresetsLoaded += OnWithdrawalPresetsLoaded;
            BootstrapService.Instance.OnBootstrapUpdated += OnBootstrapUpdated;
        }

        private void OnDestroy()
        {
            if (controller == null) return;

            controller.OnHistoryUpdated -= OnHistoryUpdated;
            controller.OnWithdrawalCreated -= OnWithdrawalCreated;
            controller.OnWithdrawalCancelled -= OnWithdrawalCancelled;
            controller.OnBalanceCheckNeeded -= OnBalanceNeedsRefresh;
            controller.OnWithdrawalPresetsLoaded -= OnWithdrawalPresetsLoaded;
            BootstrapService.Instance.OnBootstrapUpdated -= OnBootstrapUpdated;

            if (refreshButtonAnimator != null)
                refreshButtonAnimator.ForceStop();
        }

        private async void InitializeScreen()
        {
            await LoadHistory();
            controller.RefreshWithdrawalPresets();
            OnWithdrawalPresetsLoaded(controller.WithdrawalPresets);
            UpdateFeePreview();
            UpdateBankDetailsDisplay();
            ValidateSubmitButton();
            _currentBalance = BootstrapService.Instance?.Wallet?.withdrawable_amount ?? 0;
            balanceText.text = MoneyFormatter.FormatPaisa(BootstrapService.Instance.Wallet?.available_balance ?? 0);
            bonusBalanceText.text = MoneyFormatter.FormatPaisa(BootstrapService.Instance.Wallet?.bonus_balance ?? 0);
            withdrawableBalanceText.text = MoneyFormatter.FormatPaisa(_currentBalance);
        }

        private void OnBootstrapUpdated(BootstrapResponse resp)
        {
            controller.RefreshWithdrawalPresets();
            OnWithdrawalPresetsLoaded(controller.WithdrawalPresets);
            ApplyBootstrapBankDetails();
            UpdateBankDetailsDisplay();
            UpdateFeePreview();
            ValidateSubmitButton();
            _currentBalance = BootstrapService.Instance?.Wallet?.withdrawable_amount ?? 0;
            balanceText.text = MoneyFormatter.FormatPaisa(BootstrapService.Instance.Wallet?.available_balance ?? 0);
            bonusBalanceText.text = MoneyFormatter.FormatPaisa(BootstrapService.Instance.Wallet?.bonus_balance ?? 0);
            withdrawableBalanceText.text = MoneyFormatter.FormatPaisa(_currentBalance);
        }

        private void OnWithdrawalPresetsLoaded(List<WithdrawalPreset> presets)
        {
            foreach (var btn in presetButtons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }
            presetButtons.Clear();

            if (presetContainer == null || presetButtonPrefab == null) return;

            if (presets == null || presets.Count == 0)
            {
                return;
            }

            foreach (var preset in presets)
            {
                var go = Instantiate(presetButtonPrefab, presetContainer);
                var presetBtn = go.GetComponent<WithdrawalPresetButton>();
                if (presetBtn != null)
                {
                    presetBtn.Setup(preset.amount, this);
                    presetButtons.Add(go.GetComponent<Button>());
                }
                else
                {
                    Debug.LogError("[WithdrawalScreen] presetButtonPrefab is missing WithdrawalPresetButton component!");
                }
            }
        }

        public void SelectPreset(int amountInRupees, WithdrawalPresetButton selectedButton = null)
        {
            ClearPresetSelection();
            if (selectedButton != null)
            {
                selectedButton.SetSelected(true);
            }

            _suppressPresetClear = true;
            amountInput.text = amountInRupees.ToString();
            _suppressPresetClear = false;

            _requestedAmount = amountInRupees * 100;
            UpdateFeePreview();
            ValidateSubmitButton();
        }

        private void ClearPresetSelection()
        {
            foreach (var btn in presetButtons)
            {
                if (btn != null)
                {
                    var presetBtn = btn.GetComponent<WithdrawalPresetButton>();
                    if (presetBtn != null)
                    {
                        presetBtn.SetSelected(false);
                    }
                }
            }
        }

        private void UpdateBankDetailsDisplay()
        {
            var saved = controller != null ? controller.CurrentBankAccount : null;
            if (saved != null)
            {
                bankNameText.text = $"{(string.IsNullOrEmpty(saved.bank_name) ? "N/A" : saved.bank_name)}";
                bankAccountNumberText.text = $"{(string.IsNullOrEmpty(saved.masked_account_number) ? "N/A" : saved.masked_account_number)}";
                bankIfscText.text = $"{(string.IsNullOrEmpty(saved.ifsc_code) ? "N/A" : saved.ifsc_code)}";
                bankHolderNameText.text = $"{(string.IsNullOrEmpty(saved.account_holder_name) ? "N/A" : saved.account_holder_name)}";
                return;
            }

            var payoutMethods = BootstrapService.Instance.Current?.payout_methods;
            var bank = payoutMethods != null && payoutMethods.has_bank ? payoutMethods.bank : null;
            if (bank != null)
            {
                bankNameText.text = $"Bank: {(string.IsNullOrEmpty(bank.bank_name) ? "N/A" : bank.bank_name)}";
                bankAccountNumberText.text = $"Account: {(string.IsNullOrEmpty(bank.masked_account_number) ? "N/A" : bank.masked_account_number)}";
                bankIfscText.text = $"IFSC: {(string.IsNullOrEmpty(bank.ifsc_code) ? "N/A" : bank.ifsc_code)}";
                bankHolderNameText.text = $"Holder: {(string.IsNullOrEmpty(bank.account_holder_name) ? "N/A" : bank.account_holder_name)}";
                return;
            }

            bankNameText.text = "No saved bank details.";
            bankAccountNumberText.text = "No saved bank details.";
            bankIfscText.text = "No saved bank details.";
            bankHolderNameText.text = "No saved bank details.";
        }

        private async void OnRefreshBalanceClicked()
        {
            refreshButtonAnimator?.StartSpin();
            try { await RefreshBalance(); }
            catch { }
            finally { refreshButtonAnimator?.StopSpin(); }
        }

        private async Task RefreshBalance()
        {
            if (controller == null) return;

            _currentBalance = await controller.GetWithdrawableBalance();
            balanceText.text = MoneyFormatter.FormatPaisa(BootstrapService.Instance.Wallet.available_balance);
            bonusBalanceText.text = MoneyFormatter.FormatPaisa(BootstrapService.Instance.Wallet.bonus_balance);
            withdrawableBalanceText.text = MoneyFormatter.FormatPaisa(_currentBalance);

            ValidateSubmitButton();
        }

        private void OnAmountChanged(string value)
        {
            if (!_suppressPresetClear)
            {
                ClearPresetSelection();
            }

            string cleaned = value?.Replace(",", "") ?? "";
            if (long.TryParse(cleaned, out long rupees))
            {
                _requestedAmount = rupees * 100;
            }
            else
            {
                _requestedAmount = 0;
            }

            UpdateFeePreview();
            ValidateSubmitButton();
        }

        private void UpdateFeePreview()
        {
            if (_requestedAmount <= 0)
            {
                feePreviewText.text = "Enter amount to see breakdown";
                return;
            }

            var cfg = BootstrapService.Instance.WalletConfig;
            if (cfg == null)
            {
                feePreviewText.text = "Enter amount to see breakdown";
                return;
            }

            var (commissionPaisa, netPayoutPaisa) = BootstrapService.Instance.CalculateWithdrawalCommission(_requestedAmount);

            if (cfg.withdrawalCommissionType == "PERCENTAGE")
            {
                feePreviewText.text = $"Requested = {MoneyFormatter.FormatPaisa(_requestedAmount)}\n" +
                    $"Gateway Fee( {cfg.withdrawalCommissionValue}%) = {MoneyFormatter.FormatPaisa(commissionPaisa)}\n" +
                    $"<size=100%><b>You Receive = {MoneyFormatter.FormatPaisa(netPayoutPaisa)}</b></size>";
            }
            else if (cfg.withdrawalCommissionType == "FIXED")
            {
                feePreviewText.text = $"Requested = {MoneyFormatter.FormatPaisa(_requestedAmount)}\n" +
                    $"Gateway Fee = {MoneyFormatter.FormatPaisa(commissionPaisa)}\n" +
                    $"<size=100%><b>You Receive = {MoneyFormatter.FormatPaisa(netPayoutPaisa)}</b></size>";
            }
            else
            {
                feePreviewText.text = $"Amount: {MoneyFormatter.FormatPaisa(_requestedAmount)}\n" +
                    "<size=80%>Gateway fees and taxes are calculated when you submit.</size>";
            }
        }

        private void ApplyBootstrapBankDetails()
        {
            var saved = controller != null ? controller.CurrentBankAccount : null;
            if (saved == null)
            {
                var bootstrap = BootstrapService.Instance.Current;
                if (bootstrap != null && bootstrap.payout_methods != null)
                {
                    controller?.ApplySavedBankAccount(bootstrap.payout_methods);
                }
            }
        }

        private void OnSavedBankAccountUpdated(Core.Models.SavedBankAccount saved)
        {
            UpdateBankDetailsDisplay();
            ValidateSubmitButton();
        }

        private void OnSaveBankClicked()
        {
            Debug.Log("[WithdrawalScreen] OnSaveBankClicked invoked");

            if (saveBankButton == null)
            {
                Debug.LogError("[WithdrawalScreen] saveBankButton is null! Assign it in the Inspector.");
                return;
            }

            if (addBankAccountInput == null)
            {
                Debug.LogError("[WithdrawalScreen] addBankAccountInput is null! Assign it in the Inspector.");
                return;
            }

            if (addBankIfscInput == null)
            {
                Debug.LogError("[WithdrawalScreen] addBankIfscInput is null! Assign it in the Inspector.");
                return;
            }

            if (addBankHolderInput == null)
            {
                Debug.LogError("[WithdrawalScreen] addBankHolderInput is null! Assign it in the Inspector.");
                return;
            }

            if (bankNameDropdown == null)
            {
                Debug.LogError("[WithdrawalScreen] bankNameDropdown is null! Assign it in the Inspector.");
                return;
            }

            string account = addBankAccountInput.text.Trim();
            string ifsc = addBankIfscInput.text.Trim().ToUpper();
            string holder = addBankHolderInput.text.Trim();
            string bankName = bankNameDropdown.options.Count > bankNameDropdown.value
                ? bankNameDropdown.options[bankNameDropdown.value].text
                : string.Empty;

            Debug.Log($"[WithdrawalScreen] OnSaveBankClicked: acc={account} ifsc={ifsc} holder={holder} bank={bankName}");

            if (account.Length == 0)
            {
                PopupManager.Instance?.ShowError("Please enter account number.");
                Debug.LogWarning("[WithdrawalScreen] Bank save failed: empty account number");
                return;
            }

            if (holder.Length == 0)
            {
                PopupManager.Instance?.ShowError("Please enter account holder name.");
                Debug.LogWarning("[WithdrawalScreen] Bank save failed: empty holder");
                return;
            }

            if (ifsc.Length == 0)
            {
                PopupManager.Instance?.ShowError("Please enter IFSC code.");
                Debug.LogWarning("[WithdrawalScreen] Bank save failed: empty IFSC");
                return;
            }

            if (ifsc.Length != 11)
            {
                PopupManager.Instance?.ShowError("IFSC code must be 11 characters.");
                Debug.LogWarning("[WithdrawalScreen] Bank save failed: IFSC length != 11");
                return;
            }

            if (account.Length < 9 || account.Length > 18)
            {
                PopupManager.Instance?.ShowError("Account number must be 9-18 digits.");
                Debug.LogWarning("[WithdrawalScreen] Bank save failed: invalid account length");
                return;
            }

            if (holder.Length > 50)
            {
                PopupManager.Instance?.ShowError("Holder name must be 50 characters or less.");
                Debug.LogWarning("[WithdrawalScreen] Bank save failed: holder name too long");
                return;
            }

            if (bankName.Length == 0)
            {
                PopupManager.Instance?.ShowError("Please select a bank.");
                Debug.LogWarning("[WithdrawalScreen] Bank save failed: no bank selected");
                return;
            }

            _ = SaveBankToBackend(holder, bankName, account, ifsc);
        }

        private async Task SaveBankToBackend(string holder, string bankName, string account, string ifsc)
        {
            if (controller == null)
            {
                PopupManager.Instance?.ShowError("Controller unavailable. Please try again.");
                return;
            }

            var saved = await controller.SaveBankAccount(holder, bankName, account, ifsc);
            if (saved != null)
            {
                ShowWithdrawalTab();
                UpdateBankDetailsDisplay();
                ValidateSubmitButton();
            }
        }

        private void ValidateSubmitButton()
        {
            return; // TEMP — disable submit button for now
            if (controller == null) return;

            bool hasSavedBank = controller.CurrentBankAccount != null;
            bool hasConfig = BootstrapService.Instance.WalletConfig != null;

            bool isValid = _requestedAmount > 0 &&
                           _currentBalance >= 0 &&
                           _requestedAmount <= _currentBalance &&
                           hasSavedBank &&
                           hasConfig;

            submitButton.interactable = isValid;

            Debug.Log($"[WithdrawalScreen] ValidateSubmit: requested={_requestedAmount} balance={_currentBalance} cfg={hasConfig} savedBank={hasSavedBank} valid={isValid}");
        }

        private async void OnSubmitClicked()
        {
            if (_isLoading)
            {
                Debug.LogWarning("[WithdrawalScreen] OnSubmitClicked ABORTED — already loading");
                return;
            }

            _isLoading = true;
            submitButton.interactable = true;

            Debug.Log("[WithdrawalScreen] OnSubmitClicked ENTER — amount=" + _requestedAmount
                + " balance=" + _currentBalance
                + " min=" + controller.MinWithdrawalAmount
                + " savedBank=" + (controller.CurrentBankAccount != null));

            if (_requestedAmount <= 0)
            {
                Debug.LogWarning("[WithdrawalScreen] OnSubmitClicked ABORTED — amount <= 0");
                PopupManager.Instance?.ShowError("Please enter a valid amount.");
                _isLoading = false;
                return;
            }

            var cfg = BootstrapService.Instance.WalletConfig;
            if (cfg == null)
            {
                Debug.LogError("[WithdrawalScreen] OnSubmitClicked ABORTED — WalletConfig is null");
                PopupManager.Instance?.ShowError("Withdrawal configuration unavailable. Please try again.");
                _isLoading = false;
                return;
            }

            // Check bank details availbility
            if (controller.CurrentBankAccount == null)
            {
                Debug.LogWarning("[WithdrawalScreen] OnSubmitClicked ABORTED — no saved bank account");
                PopupManager.Instance?.ShowError("No saved bank account. Please add your bank details first.");
                _isLoading = false;
                return;
            }

            if (_requestedAmount < controller.MinWithdrawalAmount)
            {
                Debug.LogWarning("[WithdrawalScreen] OnSubmitClicked ABORTED — amount < min: " + _requestedAmount + " < " + controller.MinWithdrawalAmount);
                PopupManager.Instance?.ShowError("Minimum withdrawal is " + MoneyFormatter.FormatPaisa(controller.MinWithdrawalAmount));
                _isLoading = false;
                return;
            }

            if (_currentBalance < 0)
            {
                Debug.LogError("[WithdrawalScreen] OnSubmitClicked ABORTED — balance is -1 (fetch error)");
                PopupManager.Instance?.ShowError("Balance unavailable. Please try again.");
                _isLoading = false;
                return;
            }

            if (_requestedAmount > _currentBalance)
            {
                Debug.LogWarning("[WithdrawalScreen] OnSubmitClicked ABORTED — amount > balance: " + _requestedAmount + " > " + _currentBalance);
                PopupManager.Instance?.ShowError("Insufficient balance. Available: " + MoneyFormatter.FormatPaisa(_currentBalance));
                _isLoading = false;
                return;
            }

            Debug.Log("[WithdrawalScreen] OnSubmitClicked PASSED all guards — calling controller.CreateWithdrawal");

            var accountDetails = new Dictionary<string, string>
            {
                ["account_number"] = controller.CurrentBankAccount.masked_account_number,
                ["ifsc"] = controller.CurrentBankAccount.ifsc_code,
                ["holder_name"] = controller.CurrentBankAccount.account_holder_name
            };

            await controller.CreateWithdrawal(_requestedAmount, PayoutMethod.BANK, accountDetails);

            _isLoading = false;
            ValidateSubmitButton();
        }

        private void ClearForm()
        {
            amountInput.text = "";
            bankAccountNumberText.text = "";
            bankIfscText.text = "";
            bankHolderNameText.text = "";
            _requestedAmount = 0;
            UpdateFeePreview();
        }

        private async Task LoadHistory()
        {
            if (controller == null) return;

            await controller.RefreshHistory();
        }

        private void OnHistoryUpdated(List<WithdrawalItem> withdrawals)
        {
            Debug.Log($"[WithdrawalScreen] OnHistoryUpdated: count={withdrawals?.Count ?? 0} container={historyContainer != null} prefab={historyItemPrefab != null}");

            if (historyContainer == null) return;

            foreach (Transform child in historyContainer)
            {
                if (child.GetComponent<WithdrawalHistoryItem>() != null)
                    Destroy(child.gameObject);
            }

            if (withdrawals == null || withdrawals.Count == 0)
            {
                Debug.Log("[WithdrawalScreen] History is empty, showing empty text");
                if (emptyHistoryText != null) emptyHistoryText.SetActive(true);
                UpdateHistoryPaginationUI();
                return;
            }

            Debug.Log($"[WithdrawalScreen] Populating {withdrawals.Count} history items");
            if (emptyHistoryText != null) emptyHistoryText.SetActive(false);

            if (historyItemPrefab == null)
            {
                Debug.LogError("[WithdrawalScreen] historyItemPrefab is null! Cannot instantiate history items.");
                UpdateHistoryPaginationUI();
                return;
            }

            foreach (var withdrawal in withdrawals)
            {
                var itemObj = Instantiate(historyItemPrefab, historyContainer);
                var itemUI = itemObj.GetComponent<WithdrawalHistoryItem>();
                if (itemUI != null)
                {
                    itemUI.Setup(withdrawal, controller);
                }
                else
                {
                    Debug.LogError("[WithdrawalScreen] historyItemPrefab is missing WithdrawalHistoryItem component!");
                }
            }

            UpdateHistoryPaginationUI();
        }

        private void UpdateHistoryPaginationUI()
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

        private void OnPreviousClicked()
        {
            if (controller == null) return;
            _ = controller.PreviousPage();
        }

        private void OnNextClicked()
        {
            if (controller == null) return;
            _ = controller.NextPage();
        }

        private void OnWithdrawalCreated(WithdrawalItem withdrawal)
        {
            ClearForm();
            RefreshBalance();
        }

        private void OnWithdrawalCancelled(WithdrawalItem withdrawal)
        {
            RefreshBalance();
        }

        private async void OnBalanceNeedsRefresh()
        {
            await RefreshBalance();
        }

        private void CloseScreen()
        {
            gameObject.SetActive(false);
        }

        public void OnBankAccountChanged(string value)
        {
            ValidateSubmitButton();
        }

        public void OnBankIfscChanged(string value)
        {
            ValidateSubmitButton();
        }

        public void OnBankHolderChanged(string value)
        {
            ValidateSubmitButton();
        }
    }
}
