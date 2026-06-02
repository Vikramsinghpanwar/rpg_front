using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class OptionsRedBlack : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject rulesPanel;
    public AudioSource bgmMusicPlayer;
    public bool _isMenuPanel, _isSound, _isMusic, _isRules;
    public GameObject musicOffimg, soundOffimg;

   
    public void MenuBtn()
    {
        if (_isMenuPanel)
        {
            menuPanel.SetActive(false);
        }
        else
        {
            menuPanel.SetActive(true);

        }
        _isMenuPanel = !_isMenuPanel;
    }

        void Start()
        {

            musicOffimg.SetActive(false);
            soundOffimg.SetActive(false);
            bgmMusicPlayer.Play();
            _isMenuPanel = false;
            _isRules = false;
            _isMusic = true;
            _isSound = true;
            rulesPanel.SetActive(false);
            menuPanel.SetActive(false);
        }
    /*
        // Update is called once per frame
        void Update()
        {

        }*/

    public void GoHome()
    {
        bgmMusicPlayer.Stop();
        Debug.Log("Application Quit");

        Application.Quit();
        Debug.Log("Application Quit2");
    }

    public void Music()
    {
        if (_isMusic)
        {
            bgmMusicPlayer.Stop();
            musicOffimg.SetActive(true);

        }
        else
        {
            bgmMusicPlayer.Play();
            musicOffimg.SetActive(false);

        }
        _isMusic = !_isMusic;

    }
    public void Sound()
    {
        if (_isSound)
        {
            soundOffimg.SetActive(true);
        }
        else
        {
            soundOffimg.SetActive(false);

        }
        _isSound = !_isSound;

    }

    public void Rules()
    {
        if (_isRules)
        {
            rulesPanel.SetActive(false);

        }
        else
        {
            rulesPanel.SetActive(true);


        }
        _isRules = !_isRules;

    }

}
