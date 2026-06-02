using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    AudioSource clickAudioPlayer;
    float strength = 1.1f;


    private void Start()
    {
        clickAudioPlayer = GameObject.FindWithTag("ClickAudioPlayer").GetComponent<AudioSource>();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if(clickAudioPlayer != null)
        clickAudioPlayer.Play();
        transform.localScale = new Vector3(strength, strength, 1);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }
}
