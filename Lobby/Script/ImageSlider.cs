using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSlider : MonoBehaviour
{
   public List<Sprite> images; 
    public Image displayImage;   
    // public Button nextButton;    
    // public Button prevButton;   
    public float autoPlayInterval = 2f;  

    private int currentIndex = 0;
    private Coroutine autoPlayCoroutine;

    private void Start()
    {
        // nextButton.onClick.AddListener(NextImage);
        // prevButton.onClick.AddListener(PreviousImage);
        UpdateImage();
        StartAutoPlay();
    }

    private void OnDestroy()
    {
        StopAutoPlay();
    }

    public void NextImage()
    {
        currentIndex = (currentIndex + 1) % images.Count;
        UpdateImage();
    }

    public void PreviousImage()
    {
        currentIndex = (currentIndex - 1 + images.Count) % images.Count;
        UpdateImage();
    }

    private void UpdateImage()
    {
        displayImage.sprite = images[currentIndex];
    }

    private void StartAutoPlay()
    {
        autoPlayCoroutine = StartCoroutine(AutoPlay());
    }

    private void StopAutoPlay()
    {
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }
    }

    private IEnumerator AutoPlay()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoPlayInterval);
            NextImage();
        }
    }
}
