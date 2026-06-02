using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerDataUpdater : MonoBehaviour
{
    public Image profileImg;
    public TextMeshProUGUI playerNameTMP;
    // Start is called before the first frame update
    void Start()
    {
        playerNameTMP.text = UserDetail.UserName;
        LoadSprite(UserDetail.profileImageIndex);
    }

    void LoadSprite(int k)
    {
        if (PlayerPrefs.HasKey("myProfile"))
        {
            profileImg.sprite = LoadSpriteFromPlayerPrefs("myProfile");
        }
        else
        {
            Sprite sprite = Resources.Load<Sprite>("Avatar/" + k.ToString());

            // Check if the sprite was loaded successfully
            if (sprite != null)
            {
                // Assign the loaded sprite to the target Image component
                profileImg.sprite = sprite;
            }
            else
            {
                Debug.LogError("Sprite not found: " + k);
            }
        }
   
    }



    public Sprite LoadSpriteFromPlayerPrefs(string key)
    {
        // Get the Base64 string from PlayerPrefs
        string textureBase64 = PlayerPrefs.GetString(key, null);

        if (!string.IsNullOrEmpty(textureBase64))
        {
            // Convert the Base64 string back to a byte array
            byte[] textureBytes = Convert.FromBase64String(textureBase64);

            // Create a Texture2D from the byte array
            Texture2D texture = new Texture2D(1, 1); // Create a placeholder texture
            texture.LoadImage(textureBytes); // Load the image data into the texture

            // Create and return a new Sprite from the Texture2D
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        }

        return null; // Return null if no sprite was saved in PlayerPrefs
    }

}
