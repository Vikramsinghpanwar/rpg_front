using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AssetLoader : MonoBehaviour
{
    public LoadImgData[] imageDataArray;


    void Start()
    {
        AllImageLoader();
    }

    void AllImageLoader()
    {
        foreach (LoadImgData i in imageDataArray)
        {
            LoadLocalImage(i);
        }        
    }

    private void LoadLocalImage(LoadImgData imgData)
    {
        string myPath = Path.Combine(Application.persistentDataPath, imgData.path, imgData.name);
        // Check if the downloaded image exists in StreamingAssets
        if (File.Exists(myPath))
        {
            byte[] imageData = File.ReadAllBytes(myPath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false); // 'false' disables mipmaps
            texture.filterMode = FilterMode.Point;
            //texture.Compress(false); // Ensure the texture is not compressed

            texture.LoadImage(imageData); // Load image data into the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            imgData.img.sprite = sprite;
            //Debug.Log($"Loaded image from StreamingAssets: {myPath}");
        }
        else
        {
            Debug.LogError("Image not found in StreamingAssets. looking for : " + myPath);
            //StartCoroutine(DownloadImage(imgData));
        }
    }   
}

[System.Serializable]
public class LoadImgData
{
    public Image img;
    public string name;
    public string path;
}

