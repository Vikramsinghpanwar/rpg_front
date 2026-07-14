using System;
using System.Threading.Tasks;
using UnityEngine;
using Features.Lobby.Models;
using Core.Bootstrap;
using Core.API;
using Core.API.Endpoints;

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
            _ = Core.Bootstrap.BootstrapService.Instance.HydrateReferralCodeIfMissing();
        }

        void SyncToLegacy(Core.Models.BootstrapResponse bootstrap)
        {
            // Legacy sync hubbed through BootstrapService only.
            // Direct writes to Assets/Lobby user classes are kept out of _project to avoid
            // tight coupling; legacy screens that still need UserDetail should be migrated.
        }

        public static long GetWalletBalanceTotal()
        {
            return (BootstrapService.Instance?.Wallet?.available_balance ?? 0) - (BootstrapService.Instance?.Wallet?.locked_balance ?? 0);
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