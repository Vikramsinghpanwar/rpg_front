using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class OptionsBac : MonoBehaviour
{
     

    public AudioSource bgmMusicPlayer;

    /*public GameObject menuPanel;
    public GameObject rulesPanel;
    //public bool _isMenuPanel, _isSound, _isMusic, _isRules;
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
        PlayerPrefs.SetInt("_isSound", 1);

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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoHome()
    {
        bgmMusicPlayer.Stop();
        Debug.Log("Application Quit");
        GameStatusExit(true);

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
            PlayerPrefs.SetInt("_isSound", 0);
        }
        else
        {
            soundOffimg.SetActive(false);
            PlayerPrefs.SetInt("_isSound", 1);


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
*/

    public void Lobby()
    {
        SceneManager.LoadScene(1);
        bgmMusicPlayer.Stop();
        Application.Quit();

    }
}
