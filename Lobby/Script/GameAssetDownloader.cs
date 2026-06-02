using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Core.Config;

public class GameAssetDownloader : MonoBehaviour
{
    public int imagesDownloaded = 0;
    public int totalImagesToDownload = 0;
    int totalGamesDownloaded = 0;
    public ImageData[] gameData;
    public Slider progressSlider;
    public TextMeshProUGUI progressText;
    public GameObject loadingPanel;

    private float targetProgress = 0f;  // Target progress value for smooth animation

    private void Start()
    {
        foreach (ImageData i in gameData)
        {
            totalImagesToDownload += i.name.Length;
        }
        PlayerPrefs.SetInt("_isNew", 22);

        if (PlayerPrefs.GetInt("_isNew") != 21)
        {
            loadingPanel.SetActive(true);
            foreach (ImageData i in gameData)
            {
                StartCoroutine(DownloadImages(i));
            }
        }

        // Start progress bar animation loop
        StartCoroutine(UpdateProgressSmoothly());
    }

    public void DownloadGameAsset(int i)
    {
        StartCoroutine(DownloadImages(gameData[i]));
    }

    private IEnumerator DownloadImages(ImageData imgData)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, imgData.path);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        for (int i = 0; i < imgData.name.Length; i++)
        {
            string fileName = imgData.name[i];
            string path = Path.Combine(folderPath, fileName);

            if (File.Exists(path))
            {
                imagesDownloaded++;
                UpdateProgress(imagesDownloaded,  1);
                continue;
            }
            string imgUrl = ServerConfig.Downloadable_Assets_Url + imgData.path + imgData.name[i];
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imgUrl))
            {
                www.SendWebRequest();

                while (!www.isDone)
                {
                    UpdateProgress(imagesDownloaded,  www.downloadProgress);
                    yield return null; // Wait for the next frame to update
                }

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error downloading image: " + www.error);
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    byte[] imageBytes = texture.EncodeToPNG();
                    File.WriteAllBytes(path, imageBytes);
                    imagesDownloaded++;
                    UpdateProgress(imagesDownloaded, 1);
                }
            }
        }
    }

    private void UpdateProgress(int imagesDownloaded, float currentDownloadProgress)
    {
        float overallProgress = ((float)imagesDownloaded + currentDownloadProgress) / totalImagesToDownload;
        
        overallProgress = Mathf.Clamp01(overallProgress);
        
        targetProgress = overallProgress; // Set the target for smooth animation


        if (overallProgress >= 1f)
        {
            totalGamesDownloaded++;
            if (imagesDownloaded >= totalImagesToDownload)
            {
                PlayerPrefs.SetInt("_isNew", 21);
                loadingPanel.SetActive(false);
            }
        }
    }

    private IEnumerator UpdateProgressSmoothly()
    {
        while (true)
        {
            if (progressSlider != null)
            {
                progressSlider.value = Mathf.Lerp(progressSlider.value, targetProgress, Time.deltaTime * 5f);
            }

            if (progressText != null)
            {
                progressText.text = Mathf.CeilToInt(progressSlider.value * 100) + "%";
            }

            yield return null; // Wait for next frame
        }
    }
}

[System.Serializable]
public class ImageData
{
    public string[] name;
    public string path;
}
