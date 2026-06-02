using UnityEngine;
using UnityEngine.UI;

public class AudioFX : MonoBehaviour
{
	public AudioClip[] audioFxClips;
	AudioSource oneShotAudioSource;

    //public Image[] buttonImages;
    //public Sprite[] soundOnOffSprites;
    public Slider soundSlider;

	void Awake()
	{
		oneShotAudioSource = GetComponent<AudioSource>();
		InitializeSounds();
	}

	public void PieceMove()
	{
		oneShotAudioSource.PlayOneShot(audioFxClips[0]);
	}

	public void DiceUnRoll()
	{
		oneShotAudioSource.PlayOneShot(audioFxClips[1]);
	}

	public void DiceRoll()
	{
		oneShotAudioSource.PlayOneShot(audioFxClips[2]);
	}

	public void PieceAtFinalHome()
	{
		oneShotAudioSource.PlayOneShot(audioFxClips[3]);
	}

	public void PieceCaptured()
	{
		oneShotAudioSource.PlayOneShot(audioFxClips[4]);
	}

	public void ToggleSelectionChange()
	{
		oneShotAudioSource.PlayOneShot(audioFxClips[5]);
	}

	public void ButtonClicked()
	{
		oneShotAudioSource.PlayOneShot(audioFxClips[6]);
	}

	public void GameOver()
	{
		oneShotAudioSource.PlayOneShot(audioFxClips[7]);
	}

	void InitializeSounds()
	{
        AudioListener.volume = PlayerPrefs.GetFloat("soundvolume",1);
        soundSlider.value = PlayerPrefs.GetFloat("soundvolume",1);
    }

	public void SoundSliderValueChanged(float _value)
	{		
			AudioListener.volume = soundSlider.value;
			PlayerPrefs.SetFloat("soundvolume", soundSlider.value);		
	}
}
