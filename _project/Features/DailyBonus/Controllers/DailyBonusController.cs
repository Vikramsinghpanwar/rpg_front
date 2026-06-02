using System;
using System.Collections.Generic;
using System.Globalization;
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
        // Visualization grid size only. The contract returns ONE value (bonus_amount_paisa)
        // for the next claim — we MUST NOT fabricate per-day amounts. The grid here is purely
        // a streak visualization; today's actual amount comes from the server.
        [SerializeField] private int streakGridSize = 7;

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
            if (isClaimingInProgress) return;

            if (currentStatus == null)
            {
                OnError?.Invoke("Status not loaded yet.");
                return;
            }
            if (!currentStatus.can_claim)
            {
                OnError?.Invoke(currentStatus.message ?? "Bonus not available to claim yet.");
                return;
            }

            isClaimingInProgress = true;
            LoadingManager.Instance?.Show();
            try
            {
                var response = await ApiClient.Instance.Post<DailyBonusClaimResponse>(RewardRoutes.DailyClaim, null);

                if (response.replayed)
                {
                    PopupManager.Instance?.ShowInfo(response.message ?? "Already claimed today!");
                }
                else
                {
                    PopupManager.Instance?.ShowSuccess(
                        $"Claimed {MoneyFormatter.FormatPaisa(response.amount_paisa)} bonus!");
                }

                await RefreshStatusAfterClaim();
                OnBonusClaimed?.Invoke(response);
            }
            catch (ApiException e) when (e.StatusCode == 409)
            {
                OnError?.Invoke("Already claimed today.");
                PopupManager.Instance?.ShowInfo("You've already claimed today's bonus.");
                await RefreshStatusAfterClaim();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to claim bonus: {e.Message}");
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
            cachedDayData = new List<DailyBonusDayData>();
            if (currentStatus == null) return;

            DateTime lastClaimDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(currentStatus.last_claim_at))
            {
                DateTime.TryParse(currentStatus.last_claim_at,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out lastClaimDate);
            }

            int currentStreak = currentStatus.current_streak;
            bool hasClaimedToday = !currentStatus.can_claim && currentStreak > 0;
            int activeIndex = currentStatus.can_claim ? currentStreak : currentStreak - 1;

            for (int i = 0; i < streakGridSize; i++)
            {
                bool isActive = (i == activeIndex);
                cachedDayData.Add(new DailyBonusDayData
                {
                    dayNumber = i + 1,
                    // Only the active day gets the contract-backed amount. Other slots are 0
                    // so the UI hides the figure (no fabricated per-day rewards).
                    amountPaisa = isActive ? currentStatus.bonus_amount_paisa : 0,
                    isClaimed = i < currentStreak,
                    isToday = (i == currentStreak - 1 && hasClaimedToday) || (i == currentStreak && currentStatus.can_claim),
                    isAvailable = (i == currentStreak && currentStatus.can_claim),
                    claimDate = lastClaimDate.AddDays(i - (currentStreak - 1))
                });
            }
        }
    }
}
