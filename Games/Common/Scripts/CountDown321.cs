using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountDown321 : MonoBehaviour
{
    public AudioSource countDownAudio;
    public GameObject numObj;
    TextMeshProUGUI numTxt;
    // Start is called before the first frame update
    private void Start()
    {
        numTxt = numObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }
    public void Hit(int val)
    {
        countDownAudio.Play();
        numObj.SetActive(true);
        numTxt.text = val.ToString();
        Invoke("Deactive", 0.5f);
    }

    void Deactive()
    {
        numObj.SetActive(false);
    }
    
}
