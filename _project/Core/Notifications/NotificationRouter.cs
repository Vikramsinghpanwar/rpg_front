namespace Core.Notifications
{
    using System;
    using Features.Lobby.Integration;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class NotificationRouter
    {
        static NotificationType _pendingType;
        static NotificationData _pendingData;
        static bool _hasPending;

        public static void Enqueue(NotificationType type, NotificationData data = null)
        {
            _pendingType = type;
            _pendingData = data;
            _hasPending = true;
            Debug.Log($"[NotificationRouter] Queued route: type={type}, screen={data?.screen}");
        }

        public static void TryFlush()
        {
            if (!_hasPending) return;

            var type = _pendingType;
            var data = _pendingData;
            _pendingType = NotificationType.Unknown;
            _pendingData = null;
            _hasPending = false;

            Debug.Log($"[NotificationRouter] Flushing route: type={type}");
            Route(type, data);
        }

        static void Route(NotificationType type, NotificationData data)
        {
            if (type == NotificationType.Admin)
            {
                GoToLobby();
                return;
            }

            if (type == NotificationType.RechargeSuccess)
            {
                GoToWallet();
                return;
            }

            if (type == NotificationType.WithdrawalSuccess)
            {
                GoToWithdrawalHistory();
                return;
            }

            if (type == NotificationType.ReferralBonus)
            {
                GoToReferral();
                return;
            }

            GoToLobby();
        }

        static void GoToLobby()
        {
            if (SceneManager.GetActiveScene().name == "Lobby")
            {
                Debug.Log("[NotificationRouter] Already in Lobby");
                return;
            }

            SceneManager.LoadScene("Lobby");
        }

        static void GoToWallet()
        {
            if (SceneManager.GetActiveScene().name == "Lobby")
            {
                LobbyNavigation lobby = UnityEngine.Object.FindObjectOfType<LobbyNavigation>();
                if (lobby == null)
                {
                    Debug.LogWarning("[NotificationRouter] LobbyNavigation not found for wallet route");
                    return;
                }

                var wallet = lobby.GetComponent<Features.Withdrawal.UI.WithdrawalScreen>();
                if (wallet != null)
                {
                    wallet.gameObject.SetActive(true);
                    return;
                }
            }

            SceneManager.LoadScene("Lobby");
        }

        static void GoToWithdrawalHistory()
        {
            if (SceneManager.GetActiveScene().name == "Lobby")
            {
                LobbyNavigation lobby = UnityEngine.Object.FindObjectOfType<LobbyNavigation>();
                if (lobby == null)
                {
                    Debug.LogWarning("[NotificationRouter] LobbyNavigation not found for withdrawal history route");
                    return;
                }

                var history = UnityEngine.Object.FindObjectOfType<Features.Rewards.UI.RewardHistoryScreen>();
                if (history != null)
                {
                    history.gameObject.SetActive(true);
                    return;
                }
            }

            SceneManager.LoadScene("Lobby");
        }

        static void GoToReferral()
        {
            if (SceneManager.GetActiveScene().name == "Lobby")
            {
                LobbyNavigation lobby = UnityEngine.Object.FindObjectOfType<LobbyNavigation>();
                if (lobby == null)
                {
                    Debug.LogWarning("[NotificationRouter] LobbyNavigation not found for referral route");
                    return;
                }

                var history = UnityEngine.Object.FindObjectOfType<Features.Rewards.UI.RewardHistoryScreen>();
                if (history != null)
                {
                    history.gameObject.SetActive(true);
                    return;
                }
            }

            SceneManager.LoadScene("Lobby");
        }
    }
}
