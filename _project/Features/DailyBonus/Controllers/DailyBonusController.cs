using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Features.DailyBonus.Models;
using Core.API;
using Core.API.Endpoints;
using Core.Managers;
using Core.Utils;

namespace Features.DailyBonus.Controllers
{
    public class DailyBonusController : MonoBehaviour
    {
        private const int STREAK_GRID_SIZE = 7;
        DailyBonusStatusResponse currentStatus;
        List<DailyBonusDayData> cachedDayData = new List<DailyBonusDayData>();
        bool isClaimingInProgress;

        public event Action<DailyBonusStatusResponse> OnStatusUpdated;
        public event Action<DailyBonusClaimResponse> OnBonusClaimed;
        public event Action<string> OnError;

        public DailyBonusStatusResponse GetCurrentStatus() => currentStatus;
        public List<DailyBonusDayData> GetDayData() => cachedDayData;
        public bool IsClaimingInProgress() => isClaimingInProgress;
        public bool CanClaim() => currentStatus != null && currentStatus.can_claim && !isClaimingInProgress;

        public async Task LoadBonusStatus()
        {
        LoadingManager.Instance?.Show();
        try
        {
            var response = await ApiClient.Instance.Get<DailyBonusStatusResponse>(RewardRoutes.DailyStatus);
            if (response == null)
            {
                OnError?.Invoke("Empty bonus status response.");
                return;
            }
            currentStatus = response;
            BuildDayData();
            OnStatusUpdated?.Invoke(currentStatus);
        }
        catch (ApiException e) when (e.StatusCode == 503)
        {
            OnError?.Invoke("Daily rewards are temporarily disabled.");
            PopupManager.Instance?.ShowError("Daily rewards are temporarily unavailable.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load daily bonus status: {e.Message}");
            OnError?.Invoke("Failed to load bonus status.");
            PopupManager.Instance?.ShowError("Failed to load daily bonus. Check your connection.");
        }
        finally
        {
            LoadingManager.Instance?.Hide();
        }
    }

    public async Task ClaimBonus()
    {
        Debug.Log("[DailyBonusController] ClaimBonus called.");
        if (isClaimingInProgress)
        {
            Debug.LogWarning("[DailyBonusController] Claim already in progress.");
            return;
        }
        if (currentStatus == null)
        {
            Debug.LogWarning("[DailyBonusController] Status not loaded yet.");
            OnError?.Invoke("Status not loaded yet.");
            return;
        }
        if (!currentStatus.can_claim)
        {
            Debug.LogWarning($"[DailyBonusController] Cannot claim. can_claim={currentStatus.can_claim}, message={currentStatus.message}");
            OnError?.Invoke(currentStatus.message ?? "Bonus not available to claim yet.");
            return;
        }
        isClaimingInProgress = true;
        LoadingManager.Instance?.Show();
        try
        {
            Debug.Log("[DailyBonusController] Sending claim API request...");
            var response = await ApiClient.Instance.Post<DailyBonusClaimResponse>(RewardRoutes.DailyClaim, null);
            Debug.Log($"[DailyBonusController] Claim response received: {(response != null ? "not null" : "null")}");

            var message = response != null ? response.message : null;
            if (!string.IsNullOrEmpty(message))
            {
                Debug.Log($"[DailyBonusController] Showing info popup: {message}");
                PopupManager.Instance?.ShowInfo(message);
            }
            else if (response != null && response.amount_paisa > 0)
            {
                string formatted = MoneyFormatter.FormatPaisa(response.amount_paisa, response.currency);
                Debug.Log($"[DailyBonusController] Showing success popup: Claimed {formatted}");
                PopupManager.Instance?.ShowSuccess($"Claimed {formatted} bonus!");
            }
            else
            {
                Debug.LogWarning("[DailyBonusController] Claim response had no message or amount.");
                PopupManager.Instance?.ShowInfo("Bonus claimed successfully!");
            }

            Debug.Log("[DailyBonusController] Refreshing status after claim...");
            await RefreshStatusAfterClaim();
            Debug.Log("[DailyBonusController] Invoking OnBonusClaimed event.");
            OnBonusClaimed?.Invoke(response);
        }
        catch (ApiException e) when (e.StatusCode == 409)
        {
            Debug.LogWarning("[DailyBonusController] 409 conflict — already claimed.");
            OnError?.Invoke("Already claimed today.");
            PopupManager.Instance?.ShowInfo("You've already claimed today's bonus.");
            await RefreshStatusAfterClaim();
        }
        catch (Exception e)
        {
            Debug.LogError($"[DailyBonusController] Claim failed: {e.Message}\n{e.StackTrace}");
            OnError?.Invoke("Failed to claim bonus.");
            PopupManager.Instance?.ShowError("Failed to claim bonus. Try again later.");
        }
        finally
        {
            isClaimingInProgress = false;
            LoadingManager.Instance?.Hide();
        }
    }

    async Task RefreshStatusAfterClaim()
    {
        try
        {
            var response = await ApiClient.Instance.Get<DailyBonusStatusResponse>(RewardRoutes.DailyStatus);
            if (response == null) return;
            currentStatus = response;
            BuildDayData();
            OnStatusUpdated?.Invoke(currentStatus);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to refresh status after claim: {e.Message}");
        }
    }

        void BuildDayData()
        {
            if (currentStatus == null)
            {
                cachedDayData.Clear();
                return;
            }

            var claimed = new HashSet<int>(currentStatus.claimed_cycle_days ?? new List<int>());
            cachedDayData = new List<DailyBonusDayData>(STREAK_GRID_SIZE);
            for (int i = 0; i < STREAK_GRID_SIZE; i++)
            {
                int dayNumber = i + 1;
                long amount = currentStatus.cycle_days_paisa != null && i < currentStatus.cycle_days_paisa.Count
                    ? currentStatus.cycle_days_paisa[i] : 0;
                bool isClaimed = claimed.Contains(dayNumber);
                bool isCurrent = dayNumber == currentStatus.current_cycle_day;
                bool isAvailable = isCurrent && currentStatus.can_claim;

                cachedDayData.Add(new DailyBonusDayData
                {
                    dayNumber = dayNumber,
                    amountPaisa = amount,
                    isClaimed = isClaimed,
                    isToday = isCurrent,
                    isAvailable = isAvailable,
                    claimDate = DateTime.MinValue
                });
            }
        }
    }
}
