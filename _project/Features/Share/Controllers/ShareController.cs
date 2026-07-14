using System;
using System.Threading.Tasks;
using UnityEngine;
using Features.Share.Services;
using Core.Bootstrap;

namespace Features.Share.Controllers
{
    public class ShareController : MonoBehaviour
    {
        public string PromoCode { get; private set; }
        public string ShareUrl { get; private set; }
        public string ShareMessage { get; private set; }

        public event Action<string> OnShareDataUpdated;

        void OnEnable()
        {
            if (BootstrapService.Instance != null)
                BootstrapService.Instance.OnBootstrapUpdated += OnBootstrapUpdated;
        }

        void OnDisable()
        {
            if (BootstrapService.Instance != null)
                BootstrapService.Instance.OnBootstrapUpdated -= OnBootstrapUpdated;
        }

        void OnBootstrapUpdated(Core.Models.BootstrapResponse response)
        {
            _ = RefreshShareDataAsync();
        }

        public async Task<bool> RefreshShareDataAsync()
        {
            try
            {
                PromoCode = UserDetail.PromoCode;
                ShareUrl = ShareService.ResolveShareUrl();
                ShareMessage = ShareService.ResolveShareMessage();

                Debug.Log($"[ShareController] Refreshed: PromoCode present={!string.IsNullOrEmpty(PromoCode)}, ShareUrl present={!string.IsNullOrEmpty(ShareUrl)}, ShareMessage present={!string.IsNullOrEmpty(ShareMessage)}");

                OnShareDataUpdated?.Invoke(PromoCode);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ShareController] RefreshShareDataAsync failed: {ex.Message}");
                return false;
            }
        }

        public bool HasShareData()
        {
            return ShareService.HasShareData();
        }
    }
}
