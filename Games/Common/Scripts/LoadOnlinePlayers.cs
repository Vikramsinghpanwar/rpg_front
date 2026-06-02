using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadOnlinePlayers : MonoBehaviour
{
    public static List<Sprite> onlinePlayerSpritesList;
    void OnEnable()
    {
        LoadAllSpritesFromResources();
    }

    public void LoadAllSpritesFromResources()
    {
        // Load all sprites from the Profile folder within the Resources directory
        Object[] loadedSprites = Resources.LoadAll("Profiles", typeof(Sprite));

        // Initialize the list of sprites
        onlinePlayerSpritesList = new List<Sprite>();

        // Add each loaded sprite to the list
        foreach (Object sprite in loadedSprites)
        {
            onlinePlayerSpritesList.Add(sprite as Sprite);
        }

    }
}
