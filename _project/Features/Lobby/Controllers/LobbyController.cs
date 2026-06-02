using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Features.Lobby.Models;
using Core.Bootstrap;
using Core.API;
using Core.API.Endpoints;

namespace Features.Lobby.Controllers
{
    public class LobbyController : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("If true, fetches from public endpoint when no bootstrap data is present.")]
        [SerializeField] private bool fetchPublicWhenEmpty = false;

        readonly List<LobbyGame> allGames = new List<LobbyGame>();

        public bool HasGames => allGames.Count > 0;
        public IReadOnlyList<LobbyGame> AllGames => allGames;

        public event Action<List<LobbyGame>> OnGamesLoaded;
        public event Action<string> OnGamesLoadFailed;
        public event Action<GameFilterType> OnFilterChanged;

        public LobbyGame FindGameById(string id) => allGames.Find(g => g.id == id);
        public LobbyGame FindGameByName(string name) => allGames.Find(g => g.name.Equals(name, StringComparison.OrdinalIgnoreCase));

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
            _ = InitializeFromBootstrap();
        }

        public async Task<bool> InitializeFromBootstrap()
        {
            var slice = BootstrapService.Instance?.GetLobbySlice<BootstrapLobbyGamesSlice>("games");

            if (slice?.games != null && slice.games.Count > 0)
            {
                SetGames(slice.games);
                return true;
            }

            if (fetchPublicWhenEmpty)
            {
                return await FetchGamesPublic();
            }

            allGames.Clear();
            OnGamesLoadFailed?.Invoke("no_games_in_bootstrap");
            return false;
        }

        public async Task<bool> RefreshGames()
        {
            await BootstrapService.Instance?.Refresh();
            var slice = BootstrapService.Instance?.GetLobbySlice<BootstrapLobbyGamesSlice>("games");

            if (slice?.games != null)
            {
                SetGames(slice.games);
                return true;
            }

            return false;
        }

        public void FilterGames(GameFilterType filterType)
        {
            List<LobbyGame> filtered;

            switch (filterType)
            {
                case GameFilterType.All:
                    filtered = allGames.ToList();
                    break;
                case GameFilterType.Multiplayer:
                case GameFilterType.Skill:
                case GameFilterType.Sports:
                    filtered = allGames.Where(g => g.tags != null && g.tags.Contains(filterType.ToString())).ToList();
                    break;
                default:
                    filtered = allGames.ToList();
                    break;
            }

            OnFilterChanged?.Invoke(filterType);
            OnGamesLoaded?.Invoke(filtered);
        }

        async Task<bool> FetchGamesPublic()
        {
            try
            {
                var result = await ApiClient.Instance.Get<BootstrapLobbyGamesSlice>(LobbyRoutes.PublicGames);
                if (result?.games != null && result.games.Count > 0)
                {
                    SetGames(result.games);
                    return true;
                }
                OnGamesLoadFailed?.Invoke("no_public_games");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LobbyController] public games fetch failed: {e.Message}");
                OnGamesLoadFailed?.Invoke(e.Message);
                return false;
            }
        }

        void SetGames(List<LobbyGame> games)
        {
            allGames.Clear();
            foreach (var game in games.Where(g => g.is_active))
            {
                allGames.Add(game);
            }
            OnGamesLoaded?.Invoke(allGames);
        }

        public void Clear()
        {
            allGames.Clear();
            OnGamesLoaded?.Invoke(null);
        }
    }
}