using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Withdrawal.Models;
using Features.Withdrawal.Controllers;
using Core.Managers;
using Core.Utils;

namespace Features.Withdrawal.UI
{
    public class WithdrawalScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WithdrawalController controller;

        [Header("Balance Display")]
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private Button refreshBalanceButton;

        [Header("Amount Input")]
        [SerializeField] private TMP_InputField amountInput;
        [SerializeField] private TextMeshProUGUI minAmountText;
        [SerializeField] private TextMeshProUGUI maxAmountText;
        [SerializeField] private Button addAllButton;

        [Header("Payout Method - Bank")]
        [SerializeField] private TMP_InputField bankAccountNumberInput;
        [SerializeField] private TMP_InputField bankIfscInput;
        [SerializeField] private TMP_InputField bankHolderNameInput;
        [SerializeField] private TextMeshProUGUI bankDetailsDisplayText;

        [Header("Submit")]
        [SerializeField] private Button submitButton;
        [SerializeField] private TextMeshProUGUI feePreviewText;

        [Header("History")]
        [SerializeField] private Transform historyContainer;
        [SerializeField] private GameObject historyItemPrefab;
        [SerializeField] private Button loadMoreButton;
        [SerializeField] private TextMeshProUGUI loadMoreText;
        [SerializeField] private GameObject emptyHistoryText;

        [Header("Navigation")]

        [SerializeField] private Button withdrawalToggleButton;
        [SerializeField] private Button historyToggleButton;
        [SerializeField] private Button addBankButton;
        [SerializeField] private GameObject historyPanel;
        [SerializeField] private GameObject withdrawalPanel;
        [SerializeField] private GameObject addBankPanel;

        private long _currentBalance = 0;
        private long _requestedAmount = 0;
        private bool _isLoading = false;

        private void Awake()
        {
            if (controller == null) controller = FindObjectOfType<WithdrawalController>();
            if (controller == null)
            {
                var go = new GameObject("WithdrawalController");
                controller = go.AddComponent<WithdrawalController>();
            }
        }

        private void Start()
        {
            SetupUI();
            RegisterEvents();
            InitializeScreen();
        }

        private void SetupUI()
        {
            Debug.Log("Setting up WithdrawalScreen UI");
            amountInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            amountInput.onValueChanged.AddListener(OnAmountChanged);

            submitButton.onClick.AddListener(OnSubmitClicked);

            refreshBalanceButton.onClick.AddListener(() => _ = RefreshBalance());
            //addAllButton.onClick.AddListener(AddAllBalance);

            //loadMoreButton.onClick.AddListener(OnLoadMoreClicked);

            Debug.Log("Setting up WithdrawalScreen UI - Navigation Buttons");
            if (addBankButton != null) addBankButton.onClick.AddListener(ShowAddBankTab);
            if (withdrawalToggleButton != null) withdrawalToggleButton.onClick.AddListener(ShowWithdrawalTab);
            if (historyToggleButton != null) historyToggleButton.onClick.AddListener(ShowHistoryTab);

            Debug.Log("Setting up WithdrawalScreen UI - Populating Bank Details");
            PopulateSavedBankDetails();
        }
        async void ShowWithdrawalTab()
        {
            Debug.Log("Switching to Withdrawal Tab");
            withdrawalPanel?.SetActive(true);
            historyPanel?.SetActive(false);
        }
        async void ShowHistoryTab()
        {
            Debug.Log("Switching to History Tab");
            historyPanel?.SetActive(true);
            withdrawalPanel?.SetActive(false);
        }

        async void ShowAddBankTab()
        {
            addBankPanel?.SetActive(true);
            withdrawalPanel?.SetActive(false);
            historyPanel?.SetActive(false);
        }



        private void RegisterEvents()
        {
            if (controller == null) return;

            controller.OnHistoryUpdated += OnHistoryUpdated;
            controller.OnWithdrawalCreated += OnWithdrawalCreated;
            controller.OnWithdrawalCancelled += OnWithdrawalCancelled;
            controller.OnBalanceCheckNeeded += OnBalanceNeedsRefresh;
        }

        private void OnDestroy()
        {
            if (controller == null) return;

            controller.OnHistoryUpdated -= OnHistoryUpdated;
            controller.OnWithdrawalCreated -= OnWithdrawalCreated;
            controller.OnWithdrawalCancelled -= OnWithdrawalCancelled;
            controller.OnBalanceCheckNeeded -= OnBalanceNeedsRefresh;
        }

        private async void InitializeScreen()
        {
            await RefreshBalance();
            await LoadHistory();
            UpdateFeePreview();
        }

        private async Task RefreshBalance()
        {
            if (controller == null) return;

            _currentBalance = await controller.GetWithdrawableBalance();
            balanceText.text = _currentBalance < 0 ? "—" : MoneyFormatter.FormatPaisa(_currentBalance);

            minAmountText.text = $"Min: {MoneyFormatter.FormatPaisa(controller.MinWithdrawalAmount)}";
            maxAmountText.text = $"Max: {MoneyFormatter.FormatPaisa(controller.MaxWithdrawalAmount)}";
        }

        private async void AddAllBalance()
        {
            amountInput.text = (_currentBalance).ToString();
            OnAmountChanged(amountInput.text);
        }

        private void OnAmountChanged(string value)
        {
            if (long.TryParse(value, out long amount))
            {
                _requestedAmount = amount;
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

            feePreviewText.text = $"Amount: {MoneyFormatter.FormatPaisa(_requestedAmount)}\n" +
                                "<size=80%>Fees and taxes are calculated when you submit.</size>";
        }

        private void PopulateSavedBankDetails()
        {
            if (!string.IsNullOrEmpty(UserDetail.Account))
            {
                bankAccountNumberInput.text = UserDetail.Account;
                if (!string.IsNullOrEmpty(UserDetail.IfscCode))
                    bankIfscInput.text = UserDetail.IfscCode;
                if (!string.IsNullOrEmpty(UserDetail.HolderBank))
                    bankHolderNameInput.text = UserDetail.HolderBank;

                if (bankDetailsDisplayText != null)
                {
                    string details = $"Saved Bank:\nAccount: {UserDetail.Account}";
                    if (!string.IsNullOrEmpty(UserDetail.IfscCode))
                        details += $"\nIFSC: {UserDetail.IfscCode}";
                    if (!string.IsNullOrEmpty(UserDetail.HolderBank))
                        details += $"\nHolder: {UserDetail.HolderBank}";

                    bankDetailsDisplayText.text = details;
                }
            }
        }

        private void ValidateSubmitButton()
        {
            bool isValid = _requestedAmount > 0 && _currentBalance >= 0 && _requestedAmount <= _currentBalance &&
                           !string.IsNullOrEmpty(bankAccountNumberInput.text) &&
                           !string.IsNullOrEmpty(bankIfscInput.text) &&
                           !string.IsNullOrEmpty(bankHolderNameInput.text);

            submitButton.interactable = isValid && !_isLoading;
        }

        private async void OnSubmitClicked()
        {
            if (_isLoading) return;

            _isLoading = true;
            submitButton.interactable = false;

            var accountDetails = new Dictionary<string, string>
            {
                ["account_number"] = bankAccountNumberInput.text.Trim(),
                ["ifsc"] = bankIfscInput.text.Trim().ToUpper(),
                ["holder_name"] = bankHolderNameInput.text.Trim()
            };

            await controller.CreateWithdrawal(_requestedAmount, PayoutMethod.BANK, accountDetails);

            if (!_isLoading)
            {
                ClearForm();
            }

            _isLoading = false;
            ValidateSubmitButton();
        }

        private void ClearForm()
        {
            amountInput.text = "";
            bankAccountNumberInput.text = "";
            bankIfscInput.text = "";
            bankHolderNameInput.text = "";
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
            foreach (Transform child in historyContainer)
            {
                if (child.GetComponent<WithdrawalHistoryItem>() != null)
                    Destroy(child.gameObject);
            }

            if (withdrawals == null || withdrawals.Count == 0)
            {
                emptyHistoryText.SetActive(true);
                loadMoreButton.gameObject.SetActive(false);
                return;
            }

            emptyHistoryText.SetActive(false);

            foreach (var withdrawal in withdrawals)
            {
                var itemObj = Instantiate(historyItemPrefab, historyContainer);
                var itemUI = itemObj.GetComponent<WithdrawalHistoryItem>();
                if (itemUI != null)
                {
                    itemUI.Setup(withdrawal, controller);
                }
            }

            loadMoreButton.gameObject.SetActive(true);
            loadMoreText.text = "Load More";
            loadMoreButton.interactable = true;
        }

        private async void OnLoadMoreClicked()
        {
            if (controller == null) return;

            loadMoreButton.interactable = false;
            loadMoreText.text = "Loading...";

            await controller.LoadMoreHistory();

            loadMoreButton.interactable = true;
            loadMoreText.text = "Load More";
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