using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettingToggle : MonoBehaviour
{
    AudioManager audioManagerRef;
    public Sprite spr_On;
    public Sprite spr_Off;
    public Image musicImg;
    public Image soundImg;

    private void Start()
    {
        audioManagerRef = FindObjectOfType<AudioManager>();
        if (PlayerPrefs.GetInt("isMusicOn") == 1)
        {
            musicImg.sprite = spr_On;
        }
        else
        {
            musicImg.sprite = spr_Off;
        }

        if (PlayerPrefs.GetInt("isSoundOn") == 1)
        {
            soundImg.sprite = spr_On;
        }
        else
        {
            soundImg.sprite = spr_Off;

        }
    }

    public void ToggleMusic()
    {
        if(musicImg.sprite == spr_On)
        {
            musicImg.sprite = spr_Off;
        }
        else
        {
            musicImg.sprite = spr_On;
        }
        audioManagerRef.Music();
    }

    public void ToggleSound()
    {
        if (soundImg.sprite == spr_On)
        {
            soundImg.sprite = spr_Off;
        }
        else
        {
            soundImg.sprite = spr_On;
        }
        audioManagerRef.Sound();
    }
}
