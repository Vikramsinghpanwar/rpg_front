using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OtherPlayerBets : MonoBehaviour
{
    public TextMeshProUGUI amountTxt;
    public int betAmount;
    public int min = 100;
    public int max = 1000;
    public bool _is;
    public int myBet;

    private void Start()
    {
        amountTxt = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        
    }

    public IEnumerator CountingStart()
    {
        betAmount = 0;
        myBet = 0;
        _is = true;
        do
        {
            betAmount += Random.Range(min, max);
            amountTxt.text = "<color=#ADE8FF>" + myBet + "</color><color=yellow>/" + (betAmount + myBet) + "</color>";
            yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));
        }
        while (_is);
    }
}
