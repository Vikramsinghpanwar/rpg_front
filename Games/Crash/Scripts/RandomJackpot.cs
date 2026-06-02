using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomJackpot : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI jackpotTxt;
    public int jackpotAmount;
    public bool _isStop;
    void Start()
    {
        
    }
    public IEnumerator StartJackpot()
    {
        jackpotAmount = Random.Range(100000, 1000000);
        do
        {
            jackpotTxt.text = "Jackpot : " + jackpotAmount;
            jackpotAmount += Random.Range(1000, 99999);
            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
        }
        while (!_isStop);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
