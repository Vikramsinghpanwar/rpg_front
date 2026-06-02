using UnityEngine;
using UnityEngine.UI; // For displaying the selected image
using System.Collections;
using System;

public class ProfileImageSelector : MonoBehaviour
{
    public Image profileImage; // UI Image to display selected profile picture
    // Login loginRef;
    // // Function to open the gallery and select an image
    // private void Start()
    // {
    //     loginRef = FindObjectOfType<Login>();
    // }
     public void SelectProfilePicture()
     {
         // Check if permission is granted
         if (NativeGallery.IsMediaPickerBusy())
             return;
         NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
         {
             if (path != null)
             {
                 // Load image from the selected path
                 StartCoroutine(LoadProfileImage(path));
             }
         }, "Select a profile picture");

         if (permission == NativeGallery.Permission.Denied)
         {
             Debug.Log("Gallery access denied");
         }
     }

     // Coroutine to load the image from the path
     private IEnumerator LoadProfileImage(string path)
     {
         // Load the image as a texture
         Texture2D texture = NativeGallery.LoadImageAtPath(path, 512, false); // 512 = maxSize, false = isReadable

         if (texture == null)
         {
             Debug.Log("Couldn't load texture from path: " + path);
             yield break;
         }

         // Check if texture is readable
         if (!texture.isReadable)
         {
             Debug.LogError("Texture is not readable!");
             yield break;
         }

         // Convert Texture2D to Sprite and display it in the UI
         Rect rect = new Rect(0, 0, texture.width, texture.height);
         Sprite newProfileSprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
         profileImage.sprite = newProfileSprite;
         SaveSpriteToPlayerPrefs(newProfileSprite, "myProfile");
     }

     public void SaveSpriteToPlayerPrefs(Sprite sprite, string key)
     {
         // Convert Sprite to Texture2D
         Texture2D texture = sprite.texture;

         // Get texture data as a byte array
         byte[] textureBytes = texture.EncodeToPNG(); // You can use JPG too

         // Convert byte array to Base64 string
         string textureBase64 = Convert.ToBase64String(textureBytes);

         // Save the Base64 string in PlayerPrefs
         PlayerPrefs.SetString(key, textureBase64);
         PlayerPrefs.Save(); // Ensures that the data is saved

         // loginRef.UpdateUploadedImg();
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
