using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Lobby.Models;

namespace Features.Lobby.UI
{
    public class LobbyGameItemUI : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField]
        [Tooltip("Exact backend gameCode (e.g. 'teenpatti', 'dragon_tiger'). Set in Inspector.")]
        private string gameCode = string.Empty;

        [Header("UI References")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button playButton;

        public string GameCode => gameCode;

        private Action<string> onSelectWithGameCode;
        private LobbyGame boundGame;
        private Action<LobbyGame> onSelect;

        public void Initialize(LobbyGame game, Action<LobbyGame> onSelectCallback)
        {
            boundGame = game;
            onSelect = onSelectCallback;

            if (game != null && titleText != null)
            {
                titleText.text = game.name;
            }

            if (playButton != null)
            {
                playButton.onClick.AddListener(() => onSelect?.Invoke(boundGame));
            }
        }

        public void Initialize(string gameCodeValue, string displayName, Action<string> onSelectCallback)
        {
            gameCode = gameCodeValue;
            if (!string.IsNullOrEmpty(displayName) && titleText != null)
            {
                titleText.text = displayName;
            }
            onSelectWithGameCode = onSelectCallback;
            if (playButton != null)
            {
                playButton.onClick.AddListener(() => onSelectWithGameCode?.Invoke(gameCode));
            }
        }

        public void SetTitle(string title)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void Reset()
        {
            if (titleText == null)
            {
                titleText = GetComponentInChildren<TMP_Text>(true);
            }
            if (icon == null)
            {
                icon = GetComponentInChildren<Image>(true);
            }
            if (playButton == null)
            {
                playButton = GetComponentInChildren<Button>(true);
            }
        }
    }
}
