using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BetSystemCrash : MonoBehaviour
{
    public GameObject winPopUP;
    public GameObject[] coinPrefabs;
    public int lastBet;
    public OtherPlayerBets otherPlayerBetsRef;
    public GameObject cashOutBTN;
    public BetManager betManagerRef;
    public AudioSource cashOutSound;
    public int betAmount;
    public float cashOutAmt;
    public Text cashOutAmtText;
    public ControllerCrash controller;
    public bool _crah;
    public bool _betted;
    public bool _inBet;
    public bool _cashOut;
    public float betPlacedForAmount;
    SocketManagerCrash socketManagerRef;

    public GameObject myBetDataPrefab;
    public Transform myCoinsHolder;
    GetTouchPosition getTouchPosRef;

    // Start is called before the first frame update
    void Start()
    {

        getTouchPosRef = FindObjectOfType<GetTouchPosition>();
        socketManagerRef = FindObjectOfType<SocketManagerCrash>();
        lastBet = 0;
        betManagerRef = FindObjectOfType<BetManager>();

        _betted = false;
        _inBet = false;
        controller = FindAnyObjectByType<ControllerCrash>();
        _crah = false;
        betAmount = 0;
        cashOutBTN.SetActive(false);



    }

    private void UpdateWallet(float wAmount)
    {
        controller.walletAmount = wAmount;
        controller.walletText.text = "₹" + wAmount.ToString("F2");
    }

    // Update is called once per frame



    public void latestTouchPos()
    {

    }
    public void ClearAllBets()
    {
        _betted = false;
        lastBet = 0;
        controller.walletAmount += betAmount;
        otherPlayerBetsRef.betAmount -= betAmount;
        otherPlayerBetsRef.myBet -= betAmount;
        betAmount = 0;
        controller.walletText.text = controller.walletAmount.ToString("F2");
        PlayerPrefs.SetFloat("betInCrash", 0);
        socketManagerRef.ClearAllBets();
    }

    public void Bet()
    {
        int val = betManagerRef.betVal;
        if (controller.walletAmount >= val)
        {
            Debug.Log("Bet pressed");
            otherPlayerBetsRef.betAmount += val;
            otherPlayerBetsRef.myBet += val;
            controller.walletAmount -= val;
            controller.walletText.text = "₹" + controller.walletAmount.ToString("F2");

            _betted = true;
            betAmount += val;
            lastBet = betAmount;
            // Instantiate the GameObject at the touch position
            GameObject coin = Instantiate(coinPrefabs[betManagerRef.betChipNum - 1], myCoinsHolder);
            coin.transform.localScale = Vector3.one;
            coin.transform.position = getTouchPosRef.touchPos;
            coin.transform.SetParent(myCoinsHolder);
            socketManagerRef.SendBetData(val);
            PlayerPrefs.SetFloat("betInCrash", betAmount);
            PlayerPrefs.SetString("roundId_Crash", controller.roundId.ToString());
        }
        else { Debug.LogWarning("Insufficient Funds"); }


    }
    public bool _isRebetted = false;
    public void Rebet()
    {
        if (_isRebetted)
        {
            return;
        }
        _isRebetted = true;
        int k = betManagerRef.betVal;
        betManagerRef.betVal = lastBet;
        Bet();
        betManagerRef.betVal = k;
    }

    public void CashOut()
    {
        if (!_cashOut)
        {
            Debug.Log("cashout :" + controller.s);
            socketManagerRef.Cashout(betAmount);
        }

    }

    public void DelayedCashOut(float val)
    {
        Debug.Log("Delayed cashout " + val);
        if (cashOutBTN.activeInHierarchy)
        {
            if (!_cashOut)
            {
                _cashOut = true;
                cashOutAmt = betAmount * val;
                if (cashOutAmt > 0)
                {
                    cashOutSound.Play();
                    PlayerPrefs.SetFloat("betInCrash", 0f);

                    winPopUP.SetActive(true);
                    winPopUP.transform.GetChild(0).GetComponent<Text>().text = "Win " + val;
                    winPopUP.transform.GetChild(1).GetComponent<Text>().text = "₹" + (betAmount * val);
                    Invoke("OffWinPopUp", 2);
                }
                controller.walletAmount += cashOutAmt;
                //controller.walletText.text = "₹" + controller.walletAmount.ToString("F2");
                cashOutBTN.SetActive(false);
                _betted = false;
            }
            else
            {
                Debug.LogWarning("cash out not available");

            }

        }
        Invoke("WalletChk", 0.5f);
    }


    void WalletChk()
    {
        controller.apisRef.FetchWallet();

    }
    void OffWinPopUp()
    {
        winPopUP.SetActive(false);
    }

    public IEnumerator CashOutCal()
    {
        _inBet = true;
        _cashOut = false;
        cashOutBTN.SetActive(true);
        do
        {
            cashOutAmtText.text = (betAmount * controller.s).ToString("F2");
            yield return new WaitForSeconds(0.05f);


        }
        while (!controller._crash && !_cashOut);
        if (controller._crash)
        {
            cashOutAmt = 0f;
            cashOutBTN.SetActive(false);
            cashOutAmtText.text = "";
            _betted = false;
        }
        _inBet = false;
        yield return null;
    }

    public void CancelBet()
    {
        _betted = false;
        controller.walletAmount += betAmount;
        controller.walletText.text = controller.walletAmount.ToString("F2");

    }



}
