using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Features.Withdrawal.Models;
using Core.API;
using Core.API.Endpoints;
using Core.Bootstrap;
using Core.Managers;
using Core.Models;
using Core.Utils;

namespace Features.Withdrawal.Controllers
{
    public class WithdrawalController : MonoBehaviour
    {
        [Header("Optional Headers")]
        [SerializeField] private string userTier = "standard";
        [SerializeField] private string kycStatus = "verified";
        [SerializeField] private bool panVerified = true;
        [SerializeField] private bool bankVerified = true;

        [Header("Pagination")]
        [SerializeField] private int historyPageSize = 20;

        public long MinWithdrawalAmount => BootstrapService.Instance.WalletConfig?.minWithdrawalAmount * 100 ?? 0;
        public long MaxWithdrawalAmount { get; private set; } = long.MaxValue;

        public int CurrentHistoryPage { get; private set; } = 1;
        public int TotalHistoryPages { get; private set; } = 1;
        public bool HasMoreHistory { get; private set; }

        public event Action<long, long, long> OnFeeCalculated;

        readonly List<WithdrawalItem> cachedWithdrawals = new List<WithdrawalItem>();
        bool isLoadingHistory;

        readonly List<WithdrawalPreset> cachedPresets = new List<WithdrawalPreset>();

        public event Action<List<WithdrawalItem>> OnHistoryUpdated;
        public event Action<WithdrawalItem> OnWithdrawalCreated;
        public event Action<WithdrawalItem> OnWithdrawalCancelled;
        public event Action OnBalanceCheckNeeded;
        public event Action<List<WithdrawalPreset>> OnWithdrawalPresetsLoaded;
        public event Action<Core.Models.SavedBankAccount> OnSavedBankAccountUpdated;

        public List<WithdrawalPreset> WithdrawalPresets => cachedPresets;

        public Core.Models.SavedBankAccount CurrentBankAccount { get; private set; }

        public async Task<long> GetWithdrawableBalance()
        {
            LoadingManager.Instance?.Show("Fetching balance...");
            try
            {
                var balance = await ApiClient.Instance.Get<Core.Models.WalletBalanceResponse>(WalletRoutes.MeBalance());
                if (balance != null) return balance.withdrawable_amount;

                PopupManager.Instance?.ShowError("Wallet balance is temporarily unavailable. Please try again.");
                return -1;
            }
            catch (ApiException e) when (e.StatusCode == 429)
            {
                PopupManager.Instance?.ShowError("Too many requests. Please wait a moment and try again.");
                return -1;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get withdrawable balance: {e.Message}");
                PopupManager.Instance?.ShowError("Failed to fetch balance. Please try again.");
                return -1;
            }
            finally
            {
                LoadingManager.Instance?.Hide();
            }
        }

        public async Task<CreateWithdrawalResponse> CreateWithdrawal(long amount, string payoutMethod, Dictionary<string, string> accountDetails)
        {
            var minAmt = MinWithdrawalAmount;
            var maxAmt = MaxWithdrawalAmount;
            var cfg = BootstrapService.Instance.WalletConfig;

            if (cfg == null)
            {
                PopupManager.Instance?.ShowError("Withdrawal configuration is unavailable. Please try again.");
                return null;
            }

            if (amount < minAmt)
            {
                PopupManager.Instance?.ShowError($"Minimum withdrawal is {MoneyFormatter.FormatPaisa(minAmt)}");
                return null;
            }

            var balance = await GetWithdrawableBalance();
            if (balance < 0) return null;
            if (amount > balance)
            {
                PopupManager.Instance?.ShowError($"Insufficient balance. Available: {MoneyFormatter.FormatPaisa(balance)}");
                return null;
            }

            var (commissionPaisa, netPayoutPaisa) = BootstrapService.Instance.CalculateWithdrawalCommission(amount);

            LoadingManager.Instance?.Show("Processing withdrawal request...");
            try
            {
                var (key, headers) = IdempotencyHeader.New();
                headers[ApiHeaders.UserTier] = userTier;
                headers[ApiHeaders.KycStatus] = kycStatus;
                headers[ApiHeaders.PanVerified] = panVerified ? "true" : "false";
                headers[ApiHeaders.BankVerified] = bankVerified ? "true" : "false";

                var request = new CreateWithdrawalRequest
                {
                    idempotency_key = key,
                    amount = amount,
                    currency = "INR",
                    payout_type = "BANK",
                    // payout_method = payoutMethod,
                    // account_details = payoutMethod == PayoutMethod.BANK && CurrentBankAccount != null
                    //     ? new Dictionary<string, string>
                    //     {
                    //         ["account_number"] = CurrentBankAccount.masked_account_number,
                    //         ["ifsc"] = CurrentBankAccount.ifsc_code,
                    //         ["holder_name"] = CurrentBankAccount.account_holder_name
                    //     }
                    //     : accountDetails
                };

                var response = await ApiClient.Instance.Post<CreateWithdrawalResponse>(WithdrawalRoutes.Create, request, headers);
                if (response == null) return null;

                OnWithdrawalCreated?.Invoke(new WithdrawalItem
                {
                    id = response.id,
                    status = response.status,
                    requested_amount = response.requested_amount,
                    fee_amount = response.fee_amount,
                    tax_amount = response.tax_amount,
                    net_payout_amount = response.net_payout_amount,
                    masked_account = response.masked_account
                });

                OnBalanceCheckNeeded?.Invoke();
                PopupManager.Instance?.ShowSuccess(
                    $"Withdrawal request submitted! Net payout: {MoneyFormatter.FormatPaisa(response.net_payout_amount)}");
                await RefreshHistory();
                return response;
            }
            catch (ApiException e)
            {
                HandleApiError(e);
                return null;
            }
            finally
            {
                LoadingManager.Instance?.Hide();
            }
        }

        public async Task FetchHistory(bool forceRefresh = false)
        {
            if (isLoadingHistory) return;

            int page = forceRefresh ? 1 : CurrentHistoryPage;
            int offset = (page - 1) * historyPageSize;

            isLoadingHistory = true;
            LoadingManager.Instance?.Show("Loading history...");
            try
            {
                var response = await ApiClient.Instance.Get<List<WithdrawalItem>>(WithdrawalRoutes.List(historyPageSize, offset));

                cachedWithdrawals.Clear();
                if (response != null)
                {
                    cachedWithdrawals.AddRange(response);
                }

                CurrentHistoryPage = page;
                TotalHistoryPages = response != null && response.Count > 0
                    ? (int)Mathf.Ceil((float)response.Count / historyPageSize)
                    : 1;
                HasMoreHistory = response != null && response.Count >= historyPageSize;

                OnHistoryUpdated?.Invoke(new List<WithdrawalItem>(cachedWithdrawals));
            }
            catch (ApiException e)
            {
                Debug.LogError($"Failed to get withdrawal history: {e.Message}");
                Toast.Instance.ShowError("Failed to load withdrawal history.");
                OnHistoryUpdated?.Invoke(new List<WithdrawalItem>(cachedWithdrawals));
            }
            finally
            {
                isLoadingHistory = false;
                LoadingManager.Instance?.Hide();
            }
        }

        public Task RefreshHistory() => FetchHistory(forceRefresh: true);

        public async Task NextPage()
        {
            if (isLoadingHistory || !HasMoreHistory) return;
            CurrentHistoryPage++;
            await FetchHistory();
        }

        public async Task PreviousPage()
        {
            if (isLoadingHistory || CurrentHistoryPage <= 1) return;
            CurrentHistoryPage--;
            await FetchHistory();
        }

        public async Task<WithdrawalItem> GetWithdrawal(string withdrawalId)
        {
            try
            {
                return await ApiClient.Instance.Get<WithdrawalItem>(WithdrawalRoutes.Detail(withdrawalId));
            }
            catch (ApiException e)
            {
                Debug.LogError($"Failed to get withdrawal {withdrawalId}: {e.Message}");
                Toast.Instance.ShowError("Failed to load withdrawal details.");
                return null;
            }
        }



        public async Task<Core.Models.SavedBankAccount> SaveBankAccount(string accountHolderName, string bankName, string accountNumber, string ifscCode)
        {
            LoadingManager.Instance?.Show("Saving bank details...");
            try
            {
                var request = new SaveBankAccountRequest
                {
                    account_holder_name = accountHolderName,
                    bank_name = bankName,
                    account_number = accountNumber,
                    ifsc_code = ifscCode
                };


                var response = await ApiClient.Instance.Put<SavePayoutMethodResponse>(WithdrawalRoutes.SaveBank, request);
                if (response != null && response.success && response.data != null)
                {
                    CurrentBankAccount = response.data;
                    OnSavedBankAccountUpdated?.Invoke(response.data);
                    PopupManager.Instance?.ShowSuccess("Bank account saved successfully.");
                    return response.data;
                }

                PopupManager.Instance?.ShowError("Failed to save bank account.");
                return null;
            }
            catch (ApiException e)
            {
                Debug.LogError($"Failed to save bank account: {e.Message}");
                PopupManager.Instance?.ShowError("Failed to save bank account. Please try again.");
                return null;
            }
            finally
            {
                LoadingManager.Instance?.Hide();
            }
        }

        public void ApplySavedBankAccount(Core.Models.PayoutMethods payoutMethods)
        {
            if (payoutMethods == null || !payoutMethods.has_bank || payoutMethods.bank == null) return;

            var bank = payoutMethods.bank;
            var normalized = new Core.Models.SavedBankAccount
            {
                bank_name = bank.bank_name,
                masked_account_number = bank.masked_account_number,
                ifsc_code = bank.ifsc_code,
                account_holder_name = bank.account_holder_name,
                is_verified = bank.is_verified,
                method_type = "BANK"
            };

            CurrentBankAccount = normalized;
            OnSavedBankAccountUpdated?.Invoke(normalized);
        }

        public async Task<bool> CancelWithdrawal(string withdrawalId)
        {
            LoadingManager.Instance?.Show("Cancelling withdrawal...");
            try
            {
                var (_, headers) = IdempotencyHeader.New();
                var response = await ApiClient.Instance.Post<CancelWithdrawalResponse>(
                    WithdrawalRoutes.Cancel(withdrawalId), null, headers);

                if (response != null && response.status == WithdrawalStatus.CANCELLED_BY_USER)
                {
                    var existing = cachedWithdrawals.Find(w => w.id == withdrawalId);
                    if (existing != null)
                    {
                        existing.status = response.status;
                        existing.refund_transaction_id = response.refund_transaction_id;
                    }
                    OnWithdrawalCancelled?.Invoke(existing);
                    OnBalanceCheckNeeded?.Invoke();
                    PopupManager.Instance?.ShowSuccess("Withdrawal cancelled successfully");
                    await RefreshHistory();
                    return true;
                }
                return false;
            }
            catch (ApiException e)
            {
                HandleApiError(e);
                return false;
            }
            finally
            {
                LoadingManager.Instance?.Hide();
            }
        }

        public bool CanCancelWithdrawal(WithdrawalItem withdrawal)
            => withdrawal != null && withdrawal.status == WithdrawalStatus.PENDING_REVIEW;

        public void RefreshWithdrawalPresets()
        {
            cachedPresets.Clear();
            if (BootstrapService.Instance.TryGetWithdrawalPresets(out var presets))
            {
                cachedPresets.AddRange(presets);
            }
            OnWithdrawalPresetsLoaded?.Invoke(cachedPresets);
        }

        void HandleApiError(ApiException e)
        {
            switch (e.StatusCode)
            {
                case 400:
                    PopupManager.Instance?.ShowError("Invalid request. Please check your details.");
                    break;
                case 403:
                    if (e.Message.Contains("kyc"))
                        PopupManager.Instance?.ShowError("KYC verification required before withdrawal.");
                    else if (e.Message.Contains("frozen"))
                        PopupManager.Instance?.ShowError("Your wallet is frozen. Please contact support.");
                    else
                        PopupManager.Instance?.ShowError("Not authorized for withdrawals.");
                    break;
                case 409:
                    PopupManager.Instance?.ShowError("This request was already submitted.");
                    break;
                case 422:
                    if (e.Message.Contains("insufficient"))
                        PopupManager.Instance?.ShowError("Insufficient balance for withdrawal.");
                    else if (e.Message.Contains("limit"))
                        PopupManager.Instance?.ShowError("Withdrawal limit exceeded.");
                    else
                        PopupManager.Instance?.ShowError(e.Message);
                    break;
                default:
                    PopupManager.Instance?.ShowError("Something went wrong. Please try again.");
                    break;
            }
        }
    }
}
