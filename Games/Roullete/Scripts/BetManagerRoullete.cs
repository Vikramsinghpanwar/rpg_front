using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetManagerRoullete : MonoBehaviour
{
    public List<GameObject> coinsList;
    public int betVal;
    public int betChipNum;

    public void GlowOffAll()
    {
        for(int i = 0; i<coinsList.Count; i++)
        {
            coinsList[i].transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    void Start()
    {
        GlowOffAll();
        betChipNum = 0;
        Select(1);
    }
    public void PosUp(GameObject coin)
    {
        GlowOffAll();
        coin.transform.GetChild(0).gameObject.SetActive(true);

        DownAllChips();
        coin.transform.position = new Vector2(coin.transform.position.x, coin.transform.position.y + 20);
    }
     public void PosDown(GameObject coin)
     {  
        coin.transform.position = new Vector2(coin.transform.position.x, coin.transform.position.y - 20);
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
                    betVal = 50;


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
                    betVal = 100;


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
                    betVal = 500;


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
                    betVal = 1000;


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
                    betVal = 2000;


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

  

}
