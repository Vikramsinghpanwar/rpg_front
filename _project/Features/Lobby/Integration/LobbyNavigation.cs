using System;
using System.Threading.Tasks;
using UnityEngine;
using Features.Lobby.Controllers;
using Features.Lobby.UI;
using Features.Lobby.Models;
using Core.Bootstrap;
using Core.Session;
using Core.Managers;
using UnityEngine.SceneManagement;

namespace Features.Lobby.Integration
{
    public class LobbyNavigation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LobbyController lobbyController;
        [SerializeField] private LobbyScreenUI lobbyScreenUI;

        [Header("Lobby GameObject")]
        [SerializeField] private GameObject lobbyRoot;

        void Start()
        {
            if (lobbyController == null)
                lobbyController = GetComponent<LobbyController>();
            if (lobbyScreenUI == null)
                lobbyScreenUI = GetComponent<LobbyScreenUI>();

            SessionManager.Instance.OnSessionReady += OnSessionReady;
            SessionManager.Instance.OnUnauthenticated += HideLobby;

            lobbyController.OnGamesLoaded += lobbyScreenUI.SetGames;
            lobbyController.OnGamesLoadFailed += reason => Debug.LogWarning("[LobbyNavigation] Games load failed: " + reason);
            lobbyScreenUI.SetOnFilterRequested(lobbyController.FilterGames);
        }

        private void OnSessionReady(Core.Models.BootstrapResponse response)
        {
            _ = InitializeLobby();
        }

        private async Task InitializeLobby()
        {
            Debug.Log("[LobbyNavigation] Initializing lobby...");
            var bootstrap = BootstrapService.Instance;
            if (bootstrap != null && bootstrap.HasData != true)
            {
                await bootstrap.Refresh();
            }

            lobbyScreenUI.SetOnGameSelected(HandleGameSelected);
            lobbyScreenUI.ShowLoading();

            try
            {
                Debug.Log("[LobbyNavigation] Loading games from bootstrap...");
                await lobbyController.InitializeFromBootstrap();
                lobbyScreenUI.RefreshHeader();
                lobbyScreenUI.HideLoading();

                if (lobbyRoot != null) lobbyRoot.SetActive(true);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LobbyNavigation] Bootstrap failed: " + e.Message);
                lobbyScreenUI.HideLoading();
                LoadingManager.Instance?.Show("Failed to load lobby. Please try again.");
            }
        }

        void HandleGameSelected(LobbyGame game)
        {
            if (game == null) return;

            Debug.Log("[LobbyNavigation] Selected game: " + game.name + " (scene: " + game.scene_name + ")");

            if (!string.IsNullOrEmpty(game.scene_name))
            {
                SceneManager.LoadScene(game.scene_name);
            }
            else
            {
                Debug.LogWarning("[LobbyNavigation] No scene_name defined for game: " + game.name);
            }
        }

        void HideLobby()
        {
            if (lobbyRoot != null) lobbyRoot.SetActive(false);
        }

        void OnDestroy()
        {
            if (lobbyController != null)
            {
                lobbyController.OnGamesLoaded -= lobbyScreenUI.SetGames;
                lobbyController.OnGamesLoadFailed -= HandleGameLoadFailed;
            }
        }

        private void HandleGameLoadFailed(string reason)
        {
            Debug.LogWarning("[LobbyNavigation] Games load failed: " + reason);
        }
    }
}
