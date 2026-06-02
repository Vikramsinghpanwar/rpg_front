using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Lobby.Models;

namespace Features.Lobby.UI
{
    public class LobbyGameItemUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameTMP;
        [SerializeField] private Button playButton;

        LobbyGame boundGame;

        Action<LobbyGame> onSelect;

        public void Initialize(LobbyGame game, Action<LobbyGame> onSelectCallback)
        {
            boundGame = game;
            onSelect = onSelectCallback;

            if (nameTMP != null) nameTMP.text = game.name;
            if (playButton != null) playButton.onClick.AddListener(() => onSelect?.Invoke(boundGame));

            // Icon would be loaded based on game.id or game.scene_name
            // For now, the icon is assigned via inspector or left as default
        }
    }
}