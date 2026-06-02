using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;



public class BetChecker : MonoBehaviour
{
    public int bet1;
    public int bet2;
    public int bet3;
    public int bet4;

    public TextMeshProUGUI betOn1;
    public TextMeshProUGUI betOn2;
    public TextMeshProUGUI betOn3;
    public TextMeshProUGUI betOn4;
    SocketManagerBo5 socketMangerRef;
    BetManagerBo5 betManagerRef;
    BestOf5 managerRef;
    private void Start()
    {
        socketMangerRef = FindObjectOfType<SocketManagerBo5>();
        betManagerRef = FindObjectOfType<BetManagerBo5>();
        managerRef = FindObjectOfType<BestOf5>();
        ResetBets();
    }
    public void ResetBets()
    {
        bet1 = 0;
        bet2 = 0;
        bet3 = 0;
        bet4 = 0;
        betOn1.text = "No Bet";
        betOn2.text = "No Bet";
        betOn3.text = "No Bet";
        betOn4.text = "No Bet";
    }
    public void Bet(int betOn)
    {
        if (managerRef.gameState != BestOf5.gameStateE.Betting) return;
        Debug.Log("Bettin");
        if(managerRef.walletAmount >= betManagerRef.betVal)
        {
            switch (betOn)
            {
                case 1:
                    bet1 += betManagerRef.betVal;
                    betOn1.text = "" + bet1;

                    break;
                case 2:
                    bet2 += betManagerRef.betVal;
                    betOn2.text = "" + bet2;

                    break;
                case 3:
                    bet3 += betManagerRef.betVal;
                    betOn3.text = "" + bet3;

                    break;
                case 4:
                    bet4 += betManagerRef.betVal;
                    betOn4.text = "" + bet4;

                    break;
            }
            managerRef.walletAmount -= betManagerRef.betVal;
            managerRef.walletText.text = "₹" + managerRef.walletAmount.ToString("F2");
            socketMangerRef.SendBetDataToServer(betOn, betManagerRef.betVal);
        }


    }

}
