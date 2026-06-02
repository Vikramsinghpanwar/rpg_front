using System;
using System.Threading.Tasks;
using UnityEngine;
using Features.Lobby.Models;
using Core.Bootstrap;

namespace Features.Lobby.Integration
{
    public class BootstrapLobbyAdapter : MonoBehaviour
    {
        [Header("Data Sync")]
        [Tooltip("Automatically sync bootstrap data to legacy static classes on session ready")]
        [SerializeField] private bool autoSyncToLegacy = true;

        void OnEnable()
        {
            Core.Session.SessionManager.Instance.OnSessionReady += OnSessionReady;
        }

        void OnDisable()
        {
            Core.Session.SessionManager.Instance.OnSessionReady -= OnSessionReady;
        }

        void OnSessionReady(Core.Models.BootstrapResponse bootstrap)
        {
            if (autoSyncToLegacy)
            {
                SyncToLegacy(bootstrap);
            }
        }

        void SyncToLegacy(Core.Models.BootstrapResponse bootstrap)
        {
            // Sync profile data
            if (bootstrap?.profile != null)
            {
                var p = bootstrap.profile;
                // UserDetail fields would be set here if needed for legacy compatibility
                // BootstrapLobbyAdapter.GetUserId() = p.id;
                // UserDetail.UserName = p.username;
                // Note: Mobile, Email, etc. are not in BootstrapProfile - would need to be added
            }

            // Sync wallet data
            if (bootstrap?.wallet != null)
            {
                var w = bootstrap.wallet;
                // Legacy compatibility - convert from paisa to rupees float
                // Games.Common.Scripts.Wallet.SetDepositWallet(w.deposit_balance / 100f);
                // Games.Common.Scripts.Wallet.SetWinWallet(w.win_balance / 100f);
                // Games.Common.Scripts.Wallet.SetBonus(w.bonus_balance / 100f);
            }
        }

        public static long GetWalletBalanceTotal()
        {
            return (BootstrapService.Instance?.Wallet?.deposit_balance ?? 0) + (BootstrapService.Instance?.Wallet?.win_balance ?? 0);
        }

        public static long GetBonusBalance()
        {
            return BootstrapService.Instance?.Wallet?.bonus_balance ?? 0;
        }

        public static string GetUserId()
        {
            return BootstrapService.Instance?.Profile?.public_id;
        }

        public static string GetUsername()
        {
            return BootstrapService.Instance?.Profile?.username;
        }
        public static string GetMobile()
        {
            return BootstrapService.Instance?.Profile?.mobile_number;
        }
    }
}