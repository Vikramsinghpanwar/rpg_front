using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Security.Cryptography;

public class BetManager : MonoBehaviour
{
    public List<GameObject> coinsList;
    public int betVal;
    public int betChipNum;



    public float displacement = 20f;
    public AudioSource CoinSound;


    void GlowOffAll()
    {
        for (int i = 0; i < coinsList.Count; i++)
        {
            coinsList[i].transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    void Start()
    {
        betChipNum = 0;
        Select(1);
    }
    public void PosUp(GameObject coin)
    {
        DownAllChips();
        coin.transform.position = new Vector2(coin.transform.position.x, coin.transform.position.y + displacement);
    }
     public void PosDown(GameObject coin)
     {  
        coin.transform.position = new Vector2(coin.transform.position.x, coin.transform.position.y - displacement);
     }  
    void DownAllChips()
    {
        foreach(GameObject g in coinsList)
        {
            Coin s = g.transform.GetChild(1).GetComponent<Coin>();
            if (s._isSelected)
            {
                s._isSelected = false;
                PosDown(g);
            }
            g.transform.GetChild(0).gameObject.SetActive(false);
        }
    }



    public void Select(int coinNum)
    {




        switch (coinNum)
        {
            
            case 1:
                Coin c = coinsList[0].transform.GetChild(1).GetComponent<Coin>();
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
                    betChipNum = 1;
                    betVal = 50;
                    GlowOffAll();

                    coinsList[0].transform.GetChild(0).gameObject.SetActive(true);

                }
                break;
            case 2:
                Coin d = coinsList[1].transform.GetChild(1).GetComponent<Coin>();
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
                    betVal = 100;
                    GlowOffAll();

                    coinsList[1].transform.GetChild(0).gameObject.SetActive(true);


                }
                break;
            case 3:
                Coin e = coinsList[2].transform.GetChild(1).GetComponent<Coin>();
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
                    betVal = 500;
                    GlowOffAll();

                    coinsList[2].transform.GetChild(0).gameObject.SetActive(true);


                }
                break;
            case 4:
                Coin f = coinsList[3].transform.GetChild(1).GetComponent<Coin>();
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
                    betVal = 1000;
                    GlowOffAll();

                    coinsList[3].transform.GetChild(0).gameObject.SetActive(true);


                }
                break;
            case 5:
                Coin g = coinsList[4].transform.GetChild(1).GetComponent<Coin>();
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
                    betVal = 2000;
                    GlowOffAll();

                    coinsList[4].transform.GetChild(0).gameObject.SetActive(true);
                }
                break;
            case 6:
                Coin h = coinsList[5].transform.GetChild(1).GetComponent<Coin>();
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
                    GlowOffAll();

                    coinsList[5].transform.GetChild(0).gameObject.SetActive(true);

                }
                break;

        }

        
    }


}
