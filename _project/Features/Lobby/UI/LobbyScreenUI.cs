using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Lobby.Models;
using Core.Bootstrap;
using Core.Utils;

namespace Features.Lobby.UI
{
    public class LobbyScreenUI : MonoBehaviour
    {
        [Header("Header Section")]
        [SerializeField] private TMP_Text usernameTMP;
        [SerializeField] private TMP_Text userIdTMP;
        [SerializeField] private TMP_Text walletBalanceTMP;
        [SerializeField] private TMP_Text bonusBalanceTMP;
        [SerializeField] private Image profileIcon;

        [Header("Game Grid")]
        [SerializeField] private Transform gameGridParent;
        [SerializeField] private GameObject gameItemPrefab;

        [Header("Filter Buttons")]
        [SerializeField] private Button allButton;
        [SerializeField] private Button multiplayerButton;
        [SerializeField] private Button skillButton;
        [SerializeField] private Button sportsButton;

        [Header("Loading")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private Image loadingSpinner;

        Action<LobbyGame> onGameSelected;
        Action<GameFilterType> onFilterRequested;

        void Start()
        {
            SetupFilterButtons();
            RefreshHeader();
        }

        void SetupFilterButtons()
        {
            if (allButton != null) allButton.onClick.AddListener(() => RequestFilter(GameFilterType.All));
            if (multiplayerButton != null) multiplayerButton.onClick.AddListener(() => RequestFilter(GameFilterType.Multiplayer));
            if (skillButton != null) skillButton.onClick.AddListener(() => RequestFilter(GameFilterType.Skill));
            if (sportsButton != null) sportsButton.onClick.AddListener(() => RequestFilter(GameFilterType.Sports));
        }

        public void SetOnFilterRequested(Action<GameFilterType> callback)
        {
            onFilterRequested = callback;
        }

        void RequestFilter(GameFilterType filter)
        {
            onFilterRequested?.Invoke(filter);
        }

        public void RefreshHeader()
        {
            Debug.Log("[LobbyScreenUI] Refreshing header with latest profile and wallet data");
            if (BootstrapService.Instance?.HasData == true)
            {
                Debug.Log("[LobbyScreenUI] Bootstrap data is available. Updating header.");
                var profile = BootstrapService.Instance.Profile;
                var wallet = BootstrapService.Instance.Wallet;

                if (profile != null)
                {
                    if (usernameTMP != null) usernameTMP.SetText(profile.username);
                    if (userIdTMP != null) userIdTMP.SetText(profile.public_id);
                }

                if (wallet != null)
                {
                    if (walletBalanceTMP != null) walletBalanceTMP.SetText(MoneyFormatter.FormatPaisa(wallet.deposit_balance + wallet.win_balance));
                    if (bonusBalanceTMP != null) bonusBalanceTMP.SetText(MoneyFormatter.FormatPaisa(wallet.bonus_balance));
                }
            }
        }

        public void SetGames(List<LobbyGame> games)
        {
            PopulateGameGrid(games);
        }

        public void SetOnGameSelected(Action<LobbyGame> callback)
        {
            onGameSelected = callback;
        }

        void PopulateGameGrid(List<LobbyGame> games)
        {
            if (gameGridParent == null || gameItemPrefab == null) return;

            foreach (Transform child in gameGridParent)
            {
                Destroy(child.gameObject);
            }

            if (games == null) return;

            foreach (var game in games)
            {
                var instance = Instantiate(gameItemPrefab, gameGridParent);
                var itemUI = instance.GetComponent<LobbyGameItemUI>();
                if (itemUI != null)
                {
                    itemUI.Initialize(game, onGameSelected);
                }
            }
        }

        public void ShowLoading()
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(true);
        }

        public void HideLoading()
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(false);
        }

        public void Clear()
        {
            foreach (Transform child in gameGridParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
}