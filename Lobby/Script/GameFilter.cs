using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class GameFilter : MonoBehaviour
{

    [System.Serializable]
    public enum GameType
    {
        All,
        Multiplayer,
        Skill,
        Sports
    }

    [System.Serializable]
    public class GameContainer
    {
        public GameType[] tags;
        public string name;
        public GameObject icon;
        public bool active;
    }
    public GameContainer[] games;
    public static GameFilter Instance;

    private void Awake() {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    
    // Start is called before the first frame update
    public void PopulateGames()
    {
        foreach(var game in GameActive.Instance.games)
        {
            if(game.active == 1)
            {
                foreach(var icon in games)
                {
                    if(icon.name == game.gamename.ToLower())
                    {
                        icon.active = true;
                        icon.icon.SetActive(true);
                    }
                }
            }
        }
        All();
    }
    public void All()
    {
        Filter(GameType.All);
    }
    public void Multiplayer()
    {
        Filter(GameType.Multiplayer);
    }
    public void Skill()
    {
        Filter(GameType.Skill);
    }
    public void Sports()
    {
        Filter(GameType.Sports);
    }


    void Filter(GameType type)
    {
        foreach (var game in games)
        {
            if (game.tags.Contains(type) && game.active)
            {   
                game.icon.SetActive(true);
            }
            else game.icon.SetActive(false);
        }
    }

}


