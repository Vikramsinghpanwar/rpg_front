using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscapeManager : MonoBehaviour
{
    public GameObject mainScreen;
    public Image confirmationUIBK;
    public RectTransform confirmationUI;
    public RectTransform confirmationUI_RefStart;
    public RectTransform confirmationUI_RefStop;

    public void OnClickExit()
    {
        confirmationUIBK.gameObject.SetActive(true);
        LeanTween.move(confirmationUI, confirmationUI_RefStop.anchoredPosition, 0.5F).setEaseOutCirc();
    }

    public void ConfirmationYes()
    {
        LeanTween.move(confirmationUI, confirmationUI_RefStart.anchoredPosition, 0.5F).setEaseOutCirc().setOnComplete(() =>
        {
            confirmationUIBK.gameObject.SetActive(false);
            Application.Quit();
        });
    }

    public void ConfirmationNo()
    {
        LeanTween.move(confirmationUI, confirmationUI_RefStart.anchoredPosition, 0.5F).setEaseOutCirc();
        confirmationUIBK.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (mainScreen.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !confirmationUIBK.gameObject.activeSelf)
            {
                OnClickExit();
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && confirmationUIBK.gameObject.activeSelf)
            {
                ConfirmationYes();
            }
        }
    }
}
