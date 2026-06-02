using System.Collections;
using UnityEngine;

public class Dice : MonoBehaviour
{
    // Define the boundaries for the dice movement on the table
    public Vector2 minBounds;
    public Vector2 maxBounds;
    private float delayTime = 15f; // 15-second delay
    public bool gameStarted = false;
    public RectTransform panel; // Reference to the panel's RectTransform
    public float duration = 1f; // Duration of the movement

    void Start()
    {
        if (gameStarted)
        {
            Invoke("MoveDiceToRandomPosition", delayTime);
        }
        MovePanel();
    }

    public void MovePanel()
    {
        StartCoroutine(PlayMoveV());
    }

    private IEnumerator PlayMoveV()
    {
        Vector2 startPosition = new Vector2(162, 140);      // Starting position
        Vector2 endPosition = new Vector2(-170, 72);    // End position
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector2 currentPosition = Vector2.Lerp(startPosition, endPosition, elapsed / duration);
            panel.anchoredPosition = currentPosition;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the panel reaches the exact end position
        panel.anchoredPosition = endPosition;
        Debug.Log("Panel movement complete.");
    }

    public void MoveDiceToRandomPosition()
    {
        if (gameStarted)
        {
            float randomX = Random.Range(minBounds.x, maxBounds.x);
            float randomY = Random.Range(minBounds.y, maxBounds.y);
            Vector2 randomPosition = new Vector2(randomX, randomY);

            // Move the dice to the new random position
            transform.localPosition = randomPosition;
            Debug.Log("Dice moved to new random position: " + randomPosition);
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        Invoke("MoveDiceToRandomPosition", delayTime);
    }
}
