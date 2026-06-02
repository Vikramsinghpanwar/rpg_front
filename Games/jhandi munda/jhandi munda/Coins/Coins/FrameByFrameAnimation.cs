using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FrameByFrameAnimation : MonoBehaviour
{
    public Sprite[] sprites;          // Array of sprites for dice frames
    public float frameDelay = 0.1f;   // Delay between frames in seconds
    private Image imageComponent;     // Reference to the UI Image component

    private void Start()
    {
        // Get the Image component attached to the GameObject
        imageComponent = GetComponent<Image>();
        RollDice();
    }

    // Method to start the dice rolling animation
    public void RollDice()
    {
        StartCoroutine(RollDiceAnimation());
    }

    private IEnumerator RollDiceAnimation()
    {
        // Loop through each sprite in the array
        for (int i = 0; i < sprites.Length; i++)
        {
            // Set the current sprite to display in the Image component
            imageComponent.sprite = sprites[i];

            // Wait for the specified frame delay before showing the next frame
            yield return new WaitForSeconds(frameDelay);
        }

        // Optionally, set a final sprite (e.g., a random sprite from the array) as the result
        imageComponent.sprite = sprites[Random.Range(0, sprites.Length)];
    }
}
