using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MatchmakingAnimation : MonoBehaviour
{
    public RectTransform wavePrefab;
    public RectTransform profilePrefab;
    public Transform waveContainer;
    public Transform profileContainer;
    public Sprite[] profileImages;
    public RectTransform centerImage;

    [Header("Settings")]
    public float waveInterval = 1f;
    public float waveDuration = 2.5f;
    public float waveMaxScale = 5f;
    public float profileInterval = 1.2f;

    private bool isRunning = false;
private Coroutine waveRoutine;
private Coroutine profileRoutine;



    public void StartAnim()
    {
        if (isRunning) return;

    isRunning = true;
    waveRoutine = StartCoroutine(SpawnWaves());
    profileRoutine = StartCoroutine(SpawnProfiles());
    }

  public void StopAnimation()
{
    if (!isRunning) return;
    isRunning = false;

    // Stop coroutines
    if (waveRoutine != null) StopCoroutine(waveRoutine);
    if (profileRoutine != null) StopCoroutine(profileRoutine);

    // Stop all LeanTweens in this UI
    LeanTween.cancelAll();

    // Optional: reset center image scale
    LeanTween.scale(centerImage, Vector3.one, 0.3f).setEaseOutBack();

    // Clear any active waves/profiles
    foreach (Transform child in waveContainer)
        Destroy(child.gameObject);
    foreach (Transform child in profileContainer)
        Destroy(child.gameObject);
}

    IEnumerator SpawnWaves()
    {
        while (true)
        {
            RectTransform wave = Instantiate(wavePrefab, waveContainer);
            wave.anchoredPosition = centerImage.anchoredPosition;
            wave.localScale = Vector3.one * 0.2f;
            Image waveImg = wave.GetComponent<Image>();
            Color startColor = new Color(1, 1, 1, 0.5f);
            Color endColor = new Color(1, 1, 1, 0);

            LeanTween.scale(wave, Vector3.one * waveMaxScale, waveDuration).setEaseOutCubic();
            LeanTween.value(wave.gameObject, 0.5f, 0f, waveDuration)
                .setOnUpdate((float val) =>
                {
                    waveImg.color = new Color(1, 1, 1, val);
                })
                .setOnComplete(() => Destroy(wave.gameObject));

            yield return new WaitForSeconds(waveInterval);
        }
    }

    IEnumerator SpawnProfiles()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(profileInterval, profileInterval + 1f));

            RectTransform profile = Instantiate(profilePrefab, profileContainer);
            profile.anchoredPosition = centerImage.anchoredPosition;

            // Random position around center
            float radius = Random.Range(200f, 400f);
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            profile.anchoredPosition += offset;

            // Assign random sprite
            Image img = profile.transform.GetChild(0).GetComponent<Image>();
            img.sprite = profileImages[Random.Range(0, profileImages.Length)];
            img.color = new Color(1, 1, 1, 0);

            // Animate in-out
            LeanTween.scale(profile, Vector3.one * 1.2f, 0.5f).setEaseOutBack();
            LeanTween.value(profile.gameObject, 0f, 1f, 0.5f).setOnUpdate((float v) =>
            {
                img.color = new Color(1, 1, 1, v);
            });

            yield return new WaitForSeconds(1.5f);

            LeanTween.scale(profile, Vector3.zero, 0.4f).setEaseInBack()
                .setOnComplete(() => Destroy(profile.gameObject));

        }
    }
}
