using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BetManagerBRoullete : MonoBehaviour
{
    public List<GameObject> coinsList;
    public int betVal;
    public int betChipNum;



    //dragonTiger
    public GameObject winTextObject;

    TextMeshProUGUI winText;
    public AudioSource CoinSound;
    public Animator CoverImgAnimator;
    public List<int> avoid;
    public float area;
    public List<GameObject> coinPrefabList, coinsMoved;
    public List<Vector3> destinationList;
    public float speed;
    public List<Transform> coinsToMoveList;
    public GameObject parentObject;
    public bool _isAnim;
    public Vector3 myPos;
    public TextMeshProUGUI walletAmtText;
    int wallet;

    void Start()
    {
        betChipNum = 0;
        Select(1);
    }
    public void PosUp(GameObject coin)
    {
        DownAllChips();
        coin.transform.position = new Vector2(coin.transform.position.x, coin.transform.position.y + 10);
    }
     public void PosDown(GameObject coin)
     {  
        coin.transform.position = new Vector2(coin.transform.position.x, coin.transform.position.y - 10);
     }  
    void DownAllChips()
    {
        foreach(GameObject g in coinsList)
        {
            Coin s = g.GetComponent<Coin>();
            if (s._isSelected)
            {
                s._isSelected = false;
                PosDown(g);
            }
        }
    }

    public void Select(int coinNum)
    {
        switch (coinNum)
        {
            case 1:
                Coin c = coinsList[0].GetComponent<Coin>();
                if (c._isSelected)
                {
                   /* PosDown(coinsList[0]);
                    c._isSelected = false;
                    betChipNum = 0;*/
                }
                else
                {
                    PosUp(coinsList[0]);
                    c._isSelected = true;
                    Debug.Log("is selected : " + c._isSelected);
                    betChipNum = 1;
                    betVal = 10;


                }
                break;
            case 2:
                Coin d = coinsList[1].GetComponent<Coin>();
                if (d._isSelected)
                {
                    /*PosDown(coinsList[1]);
                    d._isSelected = false;
                    betChipNum = 0;*/


                }
                else
                {
                    PosUp(coinsList[1]);
                    d._isSelected = true;
                    betChipNum = 2;
                    betVal = 50;


                }
                break;
            case 3:
                Coin e = coinsList[2].GetComponent<Coin>();
                if (e._isSelected)
                {
                    /*PosDown(coinsList[2]);
                    e._isSelected = false;
                    betChipNum = 0;*/
                }
                else
                {
                    PosUp(coinsList[2]);
                    e._isSelected = true;
                    betChipNum = 3;
                    betVal = 100;


                }
                break;
            case 4:
                Coin f = coinsList[3].GetComponent<Coin>();
                if (f._isSelected)
                {
                    /*PosDown(coinsList[3]);
                    f._isSelected = false;
                    betChipNum = 0;*/

                }
                else
                {
                    PosUp(coinsList[3]);
                    f._isSelected = true;
                    betChipNum = 4;
                    betVal = 500;


                }
                break;
            case 5:
                Coin g = coinsList[4].GetComponent<Coin>();
                if (g._isSelected)
                {
                    /*PosDown(coinsList[4]);
                    g._isSelected = false;
                    betChipNum = 0;*/

                }
                else
                {
                    PosUp(coinsList[4]);
                    g._isSelected = true;
                    betChipNum = 5;
                    betVal = 1000;


                }
                break;
            case 6:
                Coin h = coinsList[5].GetComponent<Coin>();
                if (h._isSelected)
                {
                    /*PosDown(coinsList[5]);
                    h._isSelected = false;
                    betChipNum = 0;*/

                }
                else
                {
                    PosUp(coinsList[5]);
                    h._isSelected = true;
                    betChipNum = 6;
                    betVal = 5000;

                }
                break;

        }

        
    }

    public IEnumerator AnimStart()
    {
        _isAnim = true;
        avoid.Clear();

        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Wave());
    }

    public IEnumerator Wave()
    {
        if (_isAnim)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));
            CoverImgAnimator.SetTrigger("_is");
            int k = 0, d = 0, c_num = 0;
            d = Random.Range(0, destinationList.Count);
            c_num = Random.Range(0, coinsToMoveList.Count);
            while (avoid.Contains(c_num))
            {
                c_num = Random.Range(0, coinsToMoveList.Count);
            }
            CoinSound.Play();
            StartCoroutine(MoveObject(myPos, destinationList[d], coinsToMoveList[c_num]));
            avoid.Add(c_num);
            coinsMoved.Add(coinsToMoveList[c_num].gameObject);
            k++;
            if (_isAnim && coinsToMoveList.Count != 0)
            {
                yield return new WaitForSeconds(Random.Range(2f, 5f));
                CoverImgAnimator.ResetTrigger("_is");

                StartCoroutine(Wave());
            }
        }


    }

    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {
        wallet -= Random.Range(100, 1000);
        walletAmtText.text = "Rs. " + wallet.ToString();
        Vector3 targetPosModified = targetPos + new Vector3(Random.Range(-area, area), Random.Range(-area, area), 0);

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPosModified);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.localPosition = Vector3.Lerp(initialPos, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPosModified;
    }

    IEnumerator MoveObjectBack(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPos);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / 0.1f);
            coinTransform.localPosition = Vector3.Lerp(initialPos, targetPos, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPos;



    }
}
