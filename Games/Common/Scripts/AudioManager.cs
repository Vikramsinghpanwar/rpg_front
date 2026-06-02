using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public List<AudioSource> soundsList;
    public AudioSource BGM;
    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.GetInt("isMusicOn") == 1)
        {
            BGM.volume = 1f;
        }
        else
        {
            BGM.volume = 0f;
        }

        if(PlayerPrefs.GetInt("isSoundOn") == 1)
        {
            foreach(AudioSource aud in soundsList)
            {
                aud.volume = 1f;
            }
        }
        else
        {
            foreach (AudioSource aud in soundsList)
            {
                aud.volume = 0f;
            }
        }
    }

    public void Music()
    {
        if(PlayerPrefs.GetInt("isMusicOn") == 1)
        {
            BGM.volume = 0f;
            PlayerPrefs.SetInt("isMusicOn", 0);
        }
        else
        {
            BGM.volume = 1f;
            PlayerPrefs.SetInt("isMusicOn", 1);
        }
    
    }

    public void HandleVibration()
    {
        if(PlayerPrefs.GetInt("isVibrationOn") == 1)
        {
            PlayerPrefs.SetInt("isVibrationOn", 0);
        }
        else
        {
            PlayerPrefs.SetInt("isVibrationOn", 1);
        }
    }

     public void Sound()
    {
        if(PlayerPrefs.GetInt("isSoundOn") == 1)
        {
            foreach (AudioSource aud in soundsList)
            {
                aud.volume = 0f;
            }
            PlayerPrefs.SetInt("isSoundOn", 0);
        }
        else
        {
            foreach (AudioSource aud in soundsList)
            {
                aud.volume = 1f;
            }
            PlayerPrefs.SetInt("isSoundOn", 1);
        }
    
    }


}
