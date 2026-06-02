using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupAnimator : MonoBehaviour
{
    public enum PopupAnimation
    {
        ScaleUp,
        Bounce,
        Punch,
        ShrinkDisappear
    }

    Image targetImage; // assign in Inspector
    void Start()
    {
        targetImage = transform.GetChild(0).GetComponent<Image>();
    }

    public void Play(PopupAnimation animationType, Sprite sprite)
    {
        targetImage.gameObject.SetActive(true);
        targetImage.sprite = sprite;

        switch (animationType)
        {
            case PopupAnimation.ScaleUp:
                targetImage.transform.localScale = Vector3.zero;
                LeanTween.scale(targetImage.rectTransform, Vector3.one, 0.3f)
                         .setEaseOutBack();
                break;

            case PopupAnimation.Bounce:
                targetImage.transform.localScale = Vector3.zero;
                LeanTween.scale(targetImage.rectTransform, Vector3.one, 0.4f)
                         .setEaseOutElastic();
                break;

            case PopupAnimation.Punch:
                targetImage.transform.localScale = Vector3.one;
                LeanTween.scale(targetImage.rectTransform, Vector3.one * 1.2f, 1.2f)
                         .setEaseOutQuad()
                         .setOnComplete(() =>
                         {
                             LeanTween.scale(targetImage.rectTransform, Vector3.one, 0.2f);
                             targetImage.gameObject.SetActive(false);
                         });

                break;

            case PopupAnimation.ShrinkDisappear:
                targetImage.transform.localScale = Vector3.one;
                LeanTween.scale(targetImage.rectTransform, Vector3.zero, 0.3f)
                         .setEaseInBack()
                         .setOnComplete(() =>
                         {
                             targetImage.gameObject.SetActive(false);
                         });
                break;
        }
    }
}
