using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupOpen : MonoBehaviour
{
    public Animator refreshAnimator;
    public Sprite  musicOn,  soundOn;
    public Sprite  musicOff,  soundOff;
    public AudioSource audioBackbground;
    public AudioSource audioSound;

    public Image music;
    public Image sound;
    public Image LobbyImage;
    public Image ProfileImage;
    public Image ToggleImage;

    public bool SoundOn;
    public bool MusicOnOff;
    void Start(){
        audioBackbground.Play();
        LobbyImage.gameObject.SetActive(true);
        ProfileImage.gameObject.SetActive(false);
        SoundOn = true;
        MusicOnOff = false;
        if(PlayerPrefs.GetInt("isFirstTime") != 22)
        {
            PlayerPrefs.SetInt("isMusicOn", 1);
            PlayerPrefs.SetInt("isSoundOn", 1);
            PlayerPrefs.SetInt("isFirstTime", 22);
            music.sprite = musicOn;
            sound.sprite = soundOn;
        }
        if(PlayerPrefs.GetInt("isMusicOn") == 1)
        {
            //PlayingBackground();
            if(music != null)
            music.sprite = musicOn;
            audioBackbground.volume = 1f;
        }
        else
        {
            if (music != null)
                music.sprite = musicOff;
            audioBackbground.volume = 0f;

        }
        if (PlayerPrefs.GetInt("isSoundOn") == 1)
        {
            if (sound != null)
                sound.sprite = soundOn;
            audioSound.volume = 1f;

        }
        else
        {
            if (sound != null)

                sound.sprite = soundOff;
            audioSound.volume = 0f;

        }
    }
    
    public void OpenPopup(Image imageName){
        imageName.gameObject.SetActive(true);
        TuSound();
    }
    public void HidePopup(Image imageName){
        imageName.gameObject.SetActive(false); 
        TuSound();       
    }

    public void StopBackgroundMusic(Image NewMusic)
    {
        if (audioBackbground.isPlaying){
            NewMusic.sprite = musicOff;
            MusicOnOff = false;
            PlayerPrefs.SetInt("isMusicOn", 0);
            TuMusic();
            music.sprite = musicOff;
        }else{
            NewMusic.sprite = musicOn;
            MusicOnOff = true;
            PlayerPrefs.SetInt("isMusicOn", 1);
            TuMusic();
            music.sprite = musicOn;
        }
    }
    public void TuMusic(){
        if(MusicOnOff == true){
            audioBackbground.Play();
        }else{
            audioBackbground.Stop();
        }
    }
    public void PlayingBackground(){
        MusicOnOff = true;
        TuMusic();
    }

    public void StopSoundS()
    {
        if(SoundOn)
        {
            ToggleImage.sprite = soundOff;
            SoundOn = false;
            PlayerPrefs.SetInt("isSoundOn", 0);
            sound.sprite = soundOff;
        }
        else
        {
            ToggleImage.sprite = soundOn;
            SoundOn = true;
            PlayerPrefs.SetInt("isSoundOn", 1);
            sound.sprite = soundOn;
        }
    }
    public void TuSound(){
        if(SoundOn){
            audioSound.Play();
        }
    }


    public void MusicToggle()
    {
        if(PlayerPrefs.GetInt("isMusicOn") == 1)
        {
            //music is on set it to off
            music.sprite = musicOff;
            audioBackbground.volume = 0f;
            PlayerPrefs.SetInt("isMusicOn", 0);
        }
        else
        {
            //music is off set it to on
            music.sprite = musicOn;
            audioBackbground.volume = 1f;
            PlayerPrefs.SetInt("isMusicOn", 1);

        }
    }
    

    public void SoundToggle()
    {
        if (PlayerPrefs.GetInt("isSoundOn") == 1)
        {
            //music is on set it to off
            sound.sprite = soundOff;
            audioSound.volume = 0f;
            PlayerPrefs.SetInt("isSoundOn", 0);

        }
        else
        {
            //music is off set it to on
            sound.sprite = soundOn;
            audioSound.volume = 1f;
            PlayerPrefs.SetInt("isSoundOn", 1);

        }
    }
    
    public void Refresh()
    {
        refreshAnimator.SetBool("_is", true);
        Invoke("RefreshNormal", 2f);
    }

    void RefreshNormal()
    {
        refreshAnimator.SetBool("_is", false);
    }
}
