using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class Options : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject rulesPanel;
    public bool _isSound, _isMusic, _isRules;
    public AudioManager audioManagerRef;
    public GameObject musicOff;
    public GameObject soundOff;
    public GameObject vibrationOff;
    public AudioSource tapAudio;

    
    public void MenuBtn()
    {
        tapAudio.Play();
        if (menuPanel.activeInHierarchy)
        {
            menuPanel.SetActive(false);
        }
        else
        {
            menuPanel.SetActive(true);

        }
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("isMusicOn") == 1)
        {
            musicOff.SetActive(false);
        }
        else
        {
            musicOff.SetActive(true);
        }

        if (PlayerPrefs.GetInt("isSoundOn") == 1)
        {
            soundOff.SetActive(false);
        }
        else
        {
            soundOff.SetActive(true);
        }

        audioManagerRef = FindObjectOfType<AudioManager>();
    
        _isRules = false;
        _isMusic = true;
        _isSound = true;
        rulesPanel.SetActive(false);
        menuPanel.SetActive(false);
    }


    public void GoHome()
    {
        tapAudio.Play();

        SceneManager.LoadScene(1);
    }

 
    public void ToggleMusic()
    {
        tapAudio.Play();

        if (PlayerPrefs.GetInt("isMusicOn") == 1)
        {
            musicOff.SetActive(true);
        }
        else
        {
            musicOff.SetActive(false);
        }

        audioManagerRef.Music();

    }
    public void ToggleSound()
    {
        tapAudio.Play();

        if (PlayerPrefs.GetInt("isSoundOn") == 1)
        {
            soundOff.SetActive(true);
        }
        else
        {
            soundOff.SetActive(false);
        }
        audioManagerRef.Sound();

    }
    public void ToggleVibration()
    {
        tapAudio.Play();

        if (PlayerPrefs.GetInt("isVibrationOn") == 1)
        {
            vibrationOff.SetActive(true);
        }
        else
        {
            vibrationOff.SetActive(false);
        }
        audioManagerRef.HandleVibration();

    }

    

    public void Rules()
    {
        tapAudio.Play();

        if (_isRules)
        {
            rulesPanel.SetActive(false);

        }
        else
        {
            rulesPanel.SetActive(true);
        }
        _isRules = !_isRules;
        menuPanel.SetActive(false);
    }

}
