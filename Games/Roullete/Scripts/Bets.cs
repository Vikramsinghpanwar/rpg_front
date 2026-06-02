using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Bets : MonoBehaviour
{
    List<GameObject> myBetCoinsList = new List<GameObject>();
    // Start is called before the first frame update
    public BetManagerRoullete bManagerScript;
    public List<BetDetails> betsOnList = new List<BetDetails>();
    public List<BetDetails> betsOnList2 = new List<BetDetails>();
    public List<BetDetails> betsOnList3 = new List<BetDetails>();
    public List<BetDetails> betsOnList4 = new List<BetDetails>();
    public List<BetDetails> betsOnList6 = new List<BetDetails>();
    public List<BetDetails> betsOnList12 = new List<BetDetails>();
    public List<GameObject> coinPrefabs;
    public List<Transform> betPosObjectsList;
    public List<Transform> betPosObjectsList2H;
    public List<Transform> betPosObjectsList2V;
    public List<Transform> betPosObjectsList3;
    public List<Transform> betPosObjectsList4;
    public List<Transform> betPosObjectsList6;
    public List<Transform> betPosObjectsList12;
    public TextMeshProUGUI totalBetAmtTxt;
    public float totalBetAmt = 0;
    public ManagerRoullete managerScript;
    SocketManagerRoulette socketManagerRef;
    private void Start()
    {
        socketManagerRef = FindObjectOfType<SocketManagerRoulette>();
        managerScript = FindObjectOfType<ManagerRoullete>();
        bManagerScript = FindObjectOfType<BetManagerRoullete>();

    }
    public void newbet()
    {
        betsOnList.Clear();
        betsOnList2.Clear();
        betsOnList3.Clear();
        betsOnList4.Clear();
        betsOnList6.Clear();
        betsOnList12.Clear();
    }
    public void BetOn(int val)
    {
        if(managerScript.gamePhase != "Betting")
        {
            return;
        }
        if (bManagerScript.betVal < managerScript.walletAmount)
        {
            PlaceBet(val);

        }
    }

    public void Clear()
    {

        managerScript.walletAmount += totalBetAmt;
        managerScript.walletTxt.text = "₹" + managerScript.walletAmount.ToString("F2");

        totalBetAmt = 0;
        totalBetAmtTxt.text = "0";
        foreach (Transform g in transform)
        {
            Destroy(g.gameObject);
        }
        betsOnList.Clear();
        betsOnList2.Clear();
        betsOnList4.Clear();
        betsOnList6.Clear();
        betsOnList12.Clear();

        Clear_myBetCoins();
        managerScript._isBetPlaced = false;        
        socketManagerRef.ClearAllBets();
        PlayerPrefs.SetInt("roulette_totalBets", 0);



    }


    public void BetOn2H(int val)
    {

        if (managerScript.gamePhase != "Betting")
        {
            return;
        }

        if (bManagerScript.betVal < managerScript.walletAmount)
        {
            managerScript._isBetPlaced = true;
            totalBetAmt += bManagerScript.betVal;
            totalBetAmtTxt.text = totalBetAmt.ToString();
            GameObject gm = Instantiate(coinPrefabs[bManagerScript.betChipNum - 1], transform);
            gm.transform.position = betPosObjectsList2H[val].position;
            betsOnList2.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val });

            PlayerPrefs.SetInt("roulette_totalBets", (int)totalBetAmt);
            PlayerPrefs.SetString("roulette_roundId", socketManagerRef.currentRoundId);

            betsOnList2.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val + 3 });

            managerScript.walletAmount -= bManagerScript.betVal;
            managerScript.walletTxt.text = "₹" + managerScript.walletAmount.ToString("F2");

            socketManagerRef.SendBetDataToServer(val, bManagerScript.betVal/2f);
            socketManagerRef.SendBetDataToServer(val+3, bManagerScript.betVal/2f);
        }
    }


    public void BetOn2V(int val)
    {

        if (managerScript.gamePhase != "Betting")
        {
            return;
        }

        if (bManagerScript.betVal < managerScript.walletAmount)
        {
            managerScript._isBetPlaced = true;
            totalBetAmt += bManagerScript.betVal;
            totalBetAmtTxt.text = totalBetAmt.ToString();
            GameObject gm = Instantiate(coinPrefabs[bManagerScript.betChipNum - 1], transform);
            gm.transform.position = betPosObjectsList2V[val].position;
            betsOnList2.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val });

            PlayerPrefs.SetInt("roulette_totalBets", (int)totalBetAmt);
            PlayerPrefs.SetString("roulette_roundId", socketManagerRef.currentRoundId);

            betsOnList2.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val + 1 });

            managerScript.walletAmount -= bManagerScript.betVal;
            managerScript.walletTxt.text = "₹" + managerScript.walletAmount.ToString("F2");

            socketManagerRef.SendBetDataToServer(val, bManagerScript.betVal / 2f);
            socketManagerRef.SendBetDataToServer(val + 1, bManagerScript.betVal / 2f);
        }
    }




    public void BetOn3(int val)
    {

        if (managerScript.gamePhase != "Betting")
        {
            return;
        }

        if (bManagerScript.betVal < managerScript.walletAmount)
        {
            managerScript._isBetPlaced = true;
            totalBetAmt += bManagerScript.betVal;
            totalBetAmtTxt.text = totalBetAmt.ToString();
            GameObject gm = Instantiate(coinPrefabs[bManagerScript.betChipNum - 1], transform);
            gm.transform.position = betPosObjectsList3[(val - 1) / 3].position;


            betsOnList3.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val });


            betsOnList3.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val + 1 });
            PlayerPrefs.SetInt("roulette_totalBets", (int)totalBetAmt);
            PlayerPrefs.SetString("roulette_roundId", socketManagerRef.currentRoundId);


            betsOnList3.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val + 2 });

            managerScript.walletAmount -= bManagerScript.betVal;
            managerScript.walletTxt.text = "₹" + managerScript.walletAmount.ToString("F2");

            socketManagerRef.SendBetDataToServer(val, bManagerScript.betVal / 3f);
            socketManagerRef.SendBetDataToServer(val + 1, bManagerScript.betVal / 3f);
            socketManagerRef.SendBetDataToServer(val + 2, bManagerScript.betVal / 3f);
        }
    }

    public void ZeroTriplePair(int val)
    {

        if (managerScript.gamePhase != "Betting")
        {
            return;
        }

        if (bManagerScript.betVal < managerScript.walletAmount)
        {
            managerScript._isBetPlaced = true;
            totalBetAmt += bManagerScript.betVal;
            totalBetAmtTxt.text = totalBetAmt.ToString();
            GameObject gm = Instantiate(coinPrefabs[bManagerScript.betChipNum - 1], transform);

            gm.transform.position = betPosObjectsList3[11 + val].position;


            betsOnList3.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = 0 });


            betsOnList3.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val });


            betsOnList3.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val + 1 });
            PlayerPrefs.SetInt("roulette_totalBets", (int)totalBetAmt);
            PlayerPrefs.SetString("roulette_roundId", socketManagerRef.currentRoundId);

            managerScript.walletAmount -= bManagerScript.betVal;
            managerScript.walletTxt.text = "₹" + managerScript.walletAmount.ToString("F2");

            socketManagerRef.SendBetDataToServer(0, bManagerScript.betVal / 2f);
            socketManagerRef.SendBetDataToServer(val, bManagerScript.betVal / 2f);
            socketManagerRef.SendBetDataToServer(val + 1, bManagerScript.betVal / 2f);

        }
    }

    public void ZeroDuoPair(int val)
    {

        if (managerScript.gamePhase != "Betting")
        {
            return;
        }


        if (bManagerScript.betVal < managerScript.walletAmount)
        {
            managerScript._isBetPlaced = true;
            totalBetAmt += bManagerScript.betVal;
            totalBetAmtTxt.text = totalBetAmt.ToString();
            GameObject gm = Instantiate(coinPrefabs[bManagerScript.betChipNum - 1], transform);
            gm.transform.position = betPosObjectsList2H[33 + val].position;
            betsOnList2.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = 0 });
            PlayerPrefs.SetInt("roulette_totalBets", (int)totalBetAmt);
            PlayerPrefs.SetString("roulette_roundId", socketManagerRef.currentRoundId);


            betsOnList2.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val });

            managerScript.walletAmount -= bManagerScript.betVal;
            managerScript.walletTxt.text = "₹" + managerScript.walletAmount.ToString("F2");

            socketManagerRef.SendBetDataToServer(0, bManagerScript.betVal / 2f);
            socketManagerRef.SendBetDataToServer(val, bManagerScript.betVal / 2f);

        }
    }




    public void BetOn4(int val)
    {

        if (managerScript.gamePhase != "Betting")
        {
            return;
        }


        if (bManagerScript.betVal < managerScript.walletAmount)
        {
            managerScript._isBetPlaced = true;
            totalBetAmt += bManagerScript.betVal;
            totalBetAmtTxt.text = totalBetAmt.ToString();
            GameObject gm = Instantiate(coinPrefabs[bManagerScript.betChipNum - 1], transform);
            gm.transform.position = betPosObjectsList4[CoinPos4(val)].position;
            betsOnList4.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val });

            PlayerPrefs.SetInt("roulette_totalBets", (int)totalBetAmt);

            betsOnList4.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val + 1 });


            betsOnList4.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val + 3 });

            PlayerPrefs.SetInt("roulette_totalBets", (int)totalBetAmt);
            PlayerPrefs.SetString("roulette_roundId", socketManagerRef.currentRoundId);

            betsOnList4.Add(new BetDetails
            { betAmount = bManagerScript.betVal, betOn = val + 4 });

            managerScript.walletAmount -= bManagerScript.betVal;
            managerScript.walletTxt.text = "₹" + managerScript.walletAmount.ToString("F2");


            socketManagerRef.SendBetDataToServer(val, bManagerScript.betVal / 4f);
            socketManagerRef.SendBetDataToServer(val + 1, bManagerScript.betVal / 4f);
            socketManagerRef.SendBetDataToServer(val + 3, bManagerScript.betVal / 4f);
            socketManagerRef.SendBetDataToServer(val + 4, bManagerScript.betVal / 4f);

        }
    }






    public void BetOn6(int val)
    {

        if (managerScript.gamePhase != "Betting")
        {
            return;
        }


        if (bManagerScript.betVal < managerScript.walletAmount)
        {
            managerScript._isBetPlaced = true;
            totalBetAmt += bManagerScript.betVal;
            totalBetAmtTxt.text = totalBetAmt.ToString();
            GameObject gm = Instantiate(coinPrefabs[bManagerScript.betChipNum - 1], transform);
            gm.transform.position = betPosObjectsList6[val].position;

            PlayerPrefs.SetInt("roulette_totalBets", (int)totalBetAmt);
            PlayerPrefs.SetString("roulette_roundId", socketManagerRef.currentRoundId);
            for (int i = 0; i < 6; i++)
            {
                int v = (val * 3) + 1 + i;
                betsOnList6.Add(new BetDetails
                { betAmount = bManagerScript.betVal, betOn = v });
                socketManagerRef.SendBetDataToServer(v, bManagerScript.betVal / 6f);

            }

            managerScript.walletAmount -= bManagerScript.betVal;
            managerScript.walletTxt.text = "₹" + managerScript.walletAmount.ToString("F2");
        }
    }




    public void BetOn12(int val)
    {
        /*if (bManagerScript.betVal < managerScript.walletAmount)
        {
            managerScript._isBetPlaced = true;
            totalBetAmt += bManagerScript.betVal;
            totalBetAmtTxt.text = totalBetAmt.ToString();
            GameObject gm = Instantiate(coinPrefabs[bManagerScript.betChipNum - 1], transform);
            gm.transform.position = betPosObjectsList12[val].position;
            for (int i = 0; i < 12; i++)
            {
                betsOnList12.Add(new BetDetails
                { betAmount = bManagerScript.betVal, betOn = val + i });

            }
            managerScript.walletAmount -= bManagerScript.betVal;
            managerScript.walletTxt.text = "₹" + managerScript.walletAmount.ToString("F2");

        }*/
    }


    //0-36 to number aa gae
    /// 37 mane 00
    /// 38 mane even
    /// 39 odd
    /// 40 = red
    /// 41 = black
    /// 42 = 1st 12
    /// 43 = 2nd 12
    /// 44 = 3rd 12
    /// 45 = small
    /// 46 = big
    ///  47 = 1stRow
    ///  48 = 2nd Row
    ///  49 = 3rd Row
    ///  


    void PlaceBet(int val)
    {

        managerScript._isBetPlaced = true;
        totalBetAmt += bManagerScript.betVal;
        totalBetAmtTxt.text = totalBetAmt.ToString();
        GameObject g = Instantiate(coinPrefabs[bManagerScript.betChipNum -1], transform);
        g.transform.position = betPosObjectsList[val].position;
        myBetCoinsList.Add(g);
        betsOnList.Add(new BetDetails
        { 
            betAmount = bManagerScript.betVal,
            betOn = val
        });

        PlayerPrefs.SetString("roulette_roundId", socketManagerRef.currentRoundId);

        PlayerPrefs.SetInt("roulette_totalBets", (int)totalBetAmt);
        socketManagerRef.SendBetDataToServer(val, bManagerScript.betVal);
        managerScript.walletAmount -= bManagerScript.betVal;
        managerScript.walletTxt.text = "₹" +  managerScript.walletAmount.ToString("F2");
    }

    int CoinPos4(int val)
    {
        if (val == 1)
        {
            return 0;
        }
        else if (val == 2)
        {
            return 1;
        }
        else
        {
            return (val - val / 3 - 1);
        }
    }

    public void Clear_myBetCoins()
    {
        for(int i = 0; i<myBetCoinsList.Count; i++)
        {
            Destroy(myBetCoinsList[i].gameObject);
        }
        myBetCoinsList.Clear();
    }






}
