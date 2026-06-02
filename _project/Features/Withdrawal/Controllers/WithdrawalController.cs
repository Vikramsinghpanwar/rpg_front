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
        // Client-side UX hints only — the server is authoritative on limits and will 422
        // any out-of-range amount. These exist so we don't roundtrip for an obviously bad amount.
        [Header("Limits (client UX hints)")]
        [SerializeField] private long minWithdrawalAmount = 10000;
        [SerializeField] private long maxWithdrawalAmount = 10000000;

        // Optional per-user/KYC tags forwarded to the server. Defaults match server defaults.
        [Header("Optional Headers")]
        [SerializeField] private string userTier = "standard";
        [SerializeField] private string kycStatus = "verified";
        [SerializeField] private bool panVerified = true;
        [SerializeField] private bool bankVerified = true;

        public long MinWithdrawalAmount => minWithdrawalAmount;
        public long MaxWithdrawalAmount => maxWithdrawalAmount;

        readonly List<WithdrawalItem> cachedWithdrawals = new List<WithdrawalItem>();
        int currentOffset;
        int pageSize = 20;
        bool hasMore = true;
        bool isLoadingMore;

        public event Action<List<WithdrawalItem>> OnHistoryUpdated;
        public event Action<WithdrawalItem> OnWithdrawalCreated;
        public event Action<WithdrawalItem> OnWithdrawalCancelled;
        public event Action OnBalanceCheckNeeded;

        public async Task<long> GetWithdrawableBalance()
        {
            LoadingManager.Instance?.Show("Fetching balance...");
            try
            {
                var data = await BootstrapService.Instance.Refresh();
                if (data?.wallet != null) return data.wallet.withdrawable_amount;

                var reason = BootstrapService.Instance.GetWalletErrorReason();
                PopupManager.Instance?.ShowError(
                    $"Wallet balance is temporarily unavailable ({BootstrapErrorReason.Friendly(reason)}). Please try again.");
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
            if (amount < minWithdrawalAmount)
            {
                PopupManager.Instance?.ShowError($"Minimum withdrawal is {MoneyFormatter.FormatPaisa(minWithdrawalAmount)}");
                return null;
            }
            if (amount > maxWithdrawalAmount)
            {
                PopupManager.Instance?.ShowError($"Maximum withdrawal is {MoneyFormatter.FormatPaisa(maxWithdrawalAmount)}");
                return null;
            }

            var balance = await GetWithdrawableBalance();
            if (balance < 0) return null;
            if (amount > balance)
            {
                PopupManager.Instance?.ShowError($"Insufficient balance. Available: {MoneyFormatter.FormatPaisa(balance)}");
                return null;
            }

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
                    payout_method = payoutMethod,
                    account_details = accountDetails
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

        public async Task<List<WithdrawalItem>> GetWithdrawalHistory(int limit = 20, int offset = 0, bool refresh = false)
        {
            if (refresh)
            {
                currentOffset = 0;
                hasMore = true;
                cachedWithdrawals.Clear();
            }
            if (!hasMore && !refresh) return cachedWithdrawals;
            if (isLoadingMore) return cachedWithdrawals;

            isLoadingMore = true;
            try
            {
                var response = await ApiClient.Instance.Get<List<WithdrawalItem>>(WithdrawalRoutes.List(limit, offset));
                var items = response ?? new List<WithdrawalItem>();

                if (offset == 0) { cachedWithdrawals.Clear(); cachedWithdrawals.AddRange(items); }
                else cachedWithdrawals.AddRange(items);

                currentOffset = offset + items.Count;
                hasMore = items.Count >= limit;
                OnHistoryUpdated?.Invoke(cachedWithdrawals);
                return cachedWithdrawals;
            }
            catch (ApiException e)
            {
                Debug.LogError($"Failed to get withdrawal history: {e.Message}");
            }
            finally
            {
                isLoadingMore = false;
            }
            return cachedWithdrawals;
        }

        public Task RefreshHistory() => GetWithdrawalHistory(pageSize, 0, true);

        public Task LoadMoreHistory()
        {
            if (hasMore && !isLoadingMore) return GetWithdrawalHistory(pageSize, currentOffset);
            return Task.CompletedTask;
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
                return null;
            }
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

        // Per contract: cancel is only valid while status == pending_review.
        // The server 409s any other state, so this guard must match exactly.
        public bool CanCancelWithdrawal(WithdrawalItem withdrawal)
            => withdrawal != null && withdrawal.status == WithdrawalStatus.PENDING_REVIEW;

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
