using System;
using System.Threading.Tasks;
using UnityEngine;
using Features.Rewards.Models;
using Core.API;
using Core.API.Endpoints;
using Core.Managers;
using Core.Utils;
using System.Collections.Generic;

namespace Features.Rewards.Controllers
{
    public class ReferralController : MonoBehaviour
    {
        public string ReferralCode { get; private set; }
        public List<ReferredUser> ReferredUsers { get; private set; } = new List<ReferredUser>();

        public event Action<string> OnReferralDataUpdated;
        public event Action<List<ReferredUser>> OnReferredUsersUpdated;
        public event Action<string> OnError;

        bool isLoading;

        public async Task<bool> LoadReferralData()
        {
            if (isLoading) return false;

            isLoading = true;
            LoadingManager.Instance?.Show("Loading referral data...");
            try
            {
                var response = await ApiClient.Instance.Get<ReferralMeResponse>(RewardRoutes.ReferralMe);
                if (response == null)
                {
                    OnError?.Invoke("Empty referral data response.");
                    return false;
                }

                ReferralCode = response.referralCode ?? string.Empty;
                ReferredUsers = response.referredUsers ?? new List<ReferredUser>();

                if (Core.Bootstrap.BootstrapService.Instance != null && Core.Bootstrap.BootstrapService.Instance.Current != null)
                {
                    Core.Bootstrap.BootstrapService.Instance.Current.referral_code = ReferralCode;
                    Core.Bootstrap.BootstrapService.Instance.Current.referred_users = ReferredUsers;
                }

                OnReferralDataUpdated?.Invoke(ReferralCode);
                OnReferredUsersUpdated?.Invoke(ReferredUsers);
                return true;
            }
            catch (ApiException e)
            {
                Debug.LogError($"Load referral data failed: {e.Message}");
                OnError?.Invoke(e.ErrorCode ?? "REFERRAL_FAILED");
                Toast.Instance.ShowError("Failed to load referral data.");
                return false;
            }
            finally
            {
                isLoading = false;
                LoadingManager.Instance?.Hide();
            }
        }

        public async Task<bool> RefreshReferralData()
        {
            return await LoadReferralData();
        }
    }
}
