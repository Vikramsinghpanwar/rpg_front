/*using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class SpriteSheetSlicer : MonoBehaviour
{
    public int columns = 4; // Number of columns in the sprite sheet
    public int rows = 2;    // Number of rows in the sprite sheet
    public float frameRate = 10f; // Animation speed

    public Texture2D downloadedTexture;
    public Image targetImage; // Assign this in the Inspector

    private Sprite[] slicedSprites;
    private Animator animator;

    private void Start()
    {
        if (targetImage == null)
        {
            Debug.LogError("No UI Image assigned!");
            return;
        }

        SliceSpriteSheet();
        CreateAndAssignAnimation(targetImage.gameObject);
    }

    void SliceSpriteSheet()
    {
        int width = downloadedTexture.width / columns;
        int height = downloadedTexture.height / rows;
        slicedSprites = new Sprite[columns * rows];

        int index = 0;
        for (int y = rows - 1; y >= 0; y--) // Unity slices bottom-to-top
        {
            for (int x = 0; x < columns; x++)
            {
                Rect rect = new Rect(x * width, y * height, width, height);
                slicedSprites[index] = Sprite.Create(downloadedTexture, rect, new Vector2(0.5f, 0.5f));
                index++;
            }
        }
    }

    void CreateAndAssignAnimation(GameObject imageObj)
    {
        // Add Animator component if not already present
        animator = imageObj.GetComponent<Animator>();
        if (animator == null)
        {
            animator = imageObj.AddComponent<Animator>();
        }

        // Create and assign AnimatorController
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath("Assets/RuntimeAnimator.controller");
        animator.runtimeAnimatorController = animatorController;

        // Create animation clip
        AnimationClip clip = new AnimationClip();
        clip.frameRate = frameRate;

        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(Image),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[slicedSprites.Length];
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / frameRate,
                value = slicedSprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);
        AssetDatabase.CreateAsset(clip, "Assets/RuntimeAnimation.anim");

        // Assign animation to AnimatorController
        animatorController.AddMotion(clip);
        Debug.Log("Animation successfully assigned to UI Image.");

        clip.wrapMode = WrapMode.Loop;
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

    }
}
*/