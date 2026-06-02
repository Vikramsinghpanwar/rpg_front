using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomBetAmount : MonoBehaviour
{
    public TextMeshProUGUI amntOnBanker1;
    public TextMeshProUGUI amntOnBanker2;
    public TextMeshProUGUI amntOnBanker3;
    public TextMeshProUGUI amntOnBanker4;

    int banker1Amnt;
    int banker2Amnt;
    int banker3Amnt;
    int banker4Amnt;
    bool _is;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void StartRandomBets()
    {
        banker1Amnt = 0;
        banker2Amnt = 0;
        banker3Amnt = 0;
        banker4Amnt = 0;
        _is = true;
        StartCoroutine(RandomBetGenerator());
    }

    public IEnumerator RandomBetGenerator()
    {
        do
        {
            banker1Amnt += Random.Range(100, 1000);
            banker2Amnt += Random.Range(100, 1000);
            banker3Amnt += Random.Range(100, 1000);
            banker4Amnt += Random.Range(100, 1000);
            amntOnBanker1.text = banker1Amnt.ToString();
            amntOnBanker2.text = banker2Amnt.ToString();
            amntOnBanker3.text = banker3Amnt.ToString();
            amntOnBanker4.text = banker4Amnt.ToString();
            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
        }
        while (_is);
    }
    public void StopRandomBets()
    {
        _is = false;
    }

 
}
