using System;
using System.Collections.Generic;

namespace Features.Lobby.Models
{
    [Serializable]
    public class BootstrapLobbyGamesSlice
    {
        public List<LobbyGame> games;
    }

    [Serializable]
    public class LobbyGame
    {
        public string id;
        public string name;
        public bool is_active;
        public List<string> tags;
        public string scene_name;
    }

    [Serializable]
    public class BootstrapLobbySectionsSlice
    {
        public List<LobbySection> sections;
    }

    [Serializable]
    public class LobbySection
    {
        public string id;
        public string title;
        public List<string> game_ids;
        public int sort_order;
    }

    public enum GameFilterType
    {
        All,
        Multiplayer,
        Skill,
        Sports
    }
}