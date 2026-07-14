using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Bootstrap;
using Core.Models;
using Features.Lobby.Models;
using Features.Lobby.UI;
using TMPro;

namespace Features.Lobby.Controllers
{
    public class LobbyGameVisibilityController : MonoBehaviour
    {
        [Serializable]
        public class GameButtonBinding
        {
            [Tooltip("Lobby game button GameObject to show/hide.")]
            public GameObject buttonRoot;

            [Tooltip("Exact gameCode matching the backend (e.g. 'teenpatti', 'dragon_tiger').")]
            public string gameCode;

            [Tooltip("Optional TitleText to update from backend displayName.")]
            public TMP_Text titleText;
        }

        [Header("Game Buttons")]
        [SerializeField] private List<GameButtonBinding> gameButtons = new List<GameButtonBinding>();

        [Header("Options")]
        [SerializeField] private bool refreshOnBootstrapUpdate = true;
        [SerializeField] private bool logVisibilityChanges = false;

        [Header("Debug")]
        [SerializeField] private bool autoFindButtonsInChildren = true;

        readonly Dictionary<string, GameButtonBinding> buttonMap = new Dictionary<string, GameButtonBinding>();

        public event Action OnVisibilityRefreshed;

        void OnEnable()
        {
            if (BootstrapService.Instance != null && refreshOnBootstrapUpdate)
            {
                BootstrapService.Instance.OnBootstrapUpdated += OnBootstrapUpdated;
            }

            if (autoFindButtonsInChildren)
            {
                CacheButtonsFromChildren();
            }
            else
            {
                CacheButtonsFromInspector();
            }

            RefreshGameVisibility();
        }

        void OnDisable()
        {
            if (BootstrapService.Instance != null && refreshOnBootstrapUpdate)
            {
                BootstrapService.Instance.OnBootstrapUpdated -= OnBootstrapUpdated;
            }
        }

        void OnBootstrapUpdated(BootstrapResponse response)
        {
            RefreshGameVisibility();
        }

        public void CacheButtonsFromChildren()
        {
            buttonMap.Clear();

            var items = GetComponentsInChildren<LobbyGameItemUI>(true);
            foreach (var item in items)
            {
                if (item == null) continue;

                var code = item.GameCode;
                if (string.IsNullOrEmpty(code)) continue;

                if (!buttonMap.ContainsKey(code))
                {
                    buttonMap.Add(code, new GameButtonBinding
                    {
                        gameCode = code,
                        buttonRoot = item.gameObject,
                        // titleText = item.GetComponentInChildren<TMP_Text>(true)
                    });
                }
            }
        }

        void CacheButtonsFromInspector()
        {
            buttonMap.Clear();
            foreach (var binding in gameButtons)
            {
                if (binding.buttonRoot == null || string.IsNullOrEmpty(binding.gameCode)) continue;
                if (!buttonMap.ContainsKey(binding.gameCode))
                {
                    buttonMap.Add(binding.gameCode, binding);
                }
            }
        }

        public void RefreshGameVisibility()
        {
            var gameConfigs = GetActiveGameConfigs();
            var enabledCodes = new HashSet<string>();
            var displayNameMap = new Dictionary<string, string>();

            if (gameConfigs != null)
            {
                foreach (var game in gameConfigs)
                {
                    if (game == null) continue;
                    if (string.IsNullOrEmpty(game.gameCode)) continue;
                    if (game.enabled && !game.maintenanceMode)
                    {
                        enabledCodes.Add(game.gameCode);
                    }
                    if (!string.IsNullOrEmpty(game.displayName))
                    {
                        displayNameMap[game.gameCode] = game.displayName;
                    }
                }
            }

            foreach (var kvp in buttonMap)
            {
                var gameCode = kvp.Key;
                var binding = kvp.Value;

                bool shouldBeVisible = enabledCodes.Contains(gameCode);
                bool isCurrentlyActive = binding.buttonRoot != null && binding.buttonRoot.activeSelf;

                if (shouldBeVisible != isCurrentlyActive)
                {
                    if (binding.buttonRoot != null)
                    {
                        binding.buttonRoot.SetActive(shouldBeVisible);
                    }
                    if (logVisibilityChanges)
                    {
                        // Debug.Log($"[LobbyGameVisibilityController] {gameCode}: {(shouldBeVisible ? "VISIBLE" : "HIDDEN")}");
                    }
                }

                if (binding.titleText != null && displayNameMap.TryGetValue(gameCode, out var displayName))
                {
                    binding.titleText.text = displayName;
                }
            }

            OnVisibilityRefreshed?.Invoke();
        }

        public bool IsGameVisible(string gameCode)
        {
            if (string.IsNullOrEmpty(gameCode)) return false;
            var configs = GetActiveGameConfigs();
            if (configs == null) return false;
            return configs.Any(g => g != null && g.gameCode == gameCode && g.enabled && !g.maintenanceMode);
        }

        public string GetGameDisplayName(string gameCode)
        {
            if (string.IsNullOrEmpty(gameCode)) return null;
            var configs = GetActiveGameConfigs();
            if (configs == null) return null;
            return configs.FirstOrDefault(g => g != null && g.gameCode == gameCode)?.displayName;
        }

        public IReadOnlyList<ActiveGameConfig> GetActiveGameConfigs()
        {
            try
            {
                var response = BootstrapService.Instance?.Current;
                if (response?.games == null || response.games.data == null)
                {
                    Debug.LogWarning("[LobbyGameVisibilityController] Game configuration unavailable. Hiding lobby games.");
                    return Array.Empty<ActiveGameConfig>();
                }

                return response.games.data.Where(d => d != null).ToList();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LobbyGameVisibilityController] Error reading game config: {ex.Message}");
                return Array.Empty<ActiveGameConfig>();
            }
        }
    }
}
