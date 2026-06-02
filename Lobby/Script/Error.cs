using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Error : MonoBehaviour
{
    //gameobject p lagi hai wo active ho jaega aur uska 0th child ka 0th child hamara text ban jaega
    TextMeshProUGUI textMsg;
    GameObject msgObj;
    private void Start()
    {
        msgObj = transform.GetChild(0).gameObject;
        textMsg = transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        
    }
    public void Show(string msg)
    {
        textMsg.text = msg;
        StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        msgObj.SetActive(true);
        yield return new WaitForSeconds(2f);
        msgObj.SetActive(false);
    }
}
