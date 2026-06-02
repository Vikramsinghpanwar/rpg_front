using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetSystemAviator : MonoBehaviour
{
    public InputField autoCashOutMultiplier;
    public Transform winPanelPos;
    public Transform canvasRef;
    public GameObject winPanelPrefab;

    SocketManagerAviator socketManagerRef;
    public GameObject rodaToBetChange;
    public bool _isAutoBet;
    public int pastBetAmnt = 10;
    public Image autoBetImg;
    public Text betBtnAmntTxt;
    public int betRally = -1;
    public Sprite on;
    public Sprite off;
    public GameObject Roda;
    public AudioSource cashOutSound;
    public int betAmount = 10;
    public float betPlacedForAmount;
    public float cashOutAmt;
    public Text betAmtText;
    public Text cashOutAmtText;
    public GameObject cashOutBTN;
    public GameObject betBTN;
    public GameObject cancelBetBTN;
    public Controller controller;
    public bool _crah;
    public bool _betted;
    public bool _inBet;
    public bool _cashOut;
    public GameObject slideStopper;
    public Image autoCahoutBTN_img;

    public InputField autoCahOutInput;
    public bool _isAutoCashOut;
    float autoCahoutAmt;
    public GameObject myBetDataPrefab;
    public Color dGreen, Orange;
    public GameObject myBetsContent;
    public int sys_Num = 0;
    // Start is called before the first frame update
    void Start()
    {
        socketManagerRef = FindObjectOfType<SocketManagerAviator>();
        rodaToBetChange.SetActive(false);
        _isAutoBet = false;
        pastBetAmnt = 10;
        Roda.SetActive(false);
        slideStopper.SetActive(false);
        autoCahoutBTN_img.sprite = off;
        _betted = false;
        _inBet = false;
        controller = FindAnyObjectByType<Controller>();
        _crah = false;
        cashOutBTN.SetActive(false);
        cancelBetBTN.SetActive(false);
        _isAutoCashOut = false;
        betPlacedForAmount = 0;
        betManagerRef = FindObjectOfType<BetManager>();
        autoCahOutInput.text = "1.1";
        Debug.Log(" p : " + PlayerPrefs.GetInt("aviator_betAmount0"));
        Debug.Log("bet in aviator : " + PlayerPrefs.GetFloat("bet1InAviator"));
        if (sys_Num == 0)
        {

            if (PlayerPrefs.HasKey("aviator_betAmount0") && PlayerPrefs.GetInt("aviator_betAmount0") != 0)
            {
                betAmount = PlayerPrefs.GetInt("aviator_betAmount0");
                Debug.Log("bet amount updated 1");
            }
            else
            {
                Debug.Log("aeeeeeee");
                betAmount = 10;
            }
        }
        else
        {
            if (PlayerPrefs.HasKey("aviator_betAmount1") && PlayerPrefs.GetInt("aviator_betAmount1") != 0)
            {
                betAmount = PlayerPrefs.GetInt("aviator_betAmount1");
                Debug.Log("bet amount updated 2");

            }
            else
            {
                Debug.Log("aeeeeeee");

                betAmount = 10;
            }
        }
        betAmtText.text = betAmount.ToString("F0");
        betBtnAmntTxt.text = "₹" + betAmount;

        /*if(sys_Num == 0)
        {
            if (PlayerPrefs.GetInt("Aviator_AutoBet1") == 1)
            {
                _isAutoBet = true;
                rodaToBetChange.SetActive(true);
                autoBetImg.sprite = on;
                if(PlayerPrefs.GetFloat("bet1InAviator") < 0)
                {
                    Invoke("Bet", 0.5f);

                }
            }
        }
        
        else if(sys_Num == 1)
        {
            if (PlayerPrefs.GetInt("Aviator_AutoBet2") == 1)
            {
                _isAutoBet = true;
                rodaToBetChange.SetActive(true);
                autoBetImg.sprite = on;
                if (PlayerPrefs.GetFloat("bet2InAviator") < 0)
                {
                    Invoke("Bet", 0.5f);

                }
            }
        }*/

    }


    public void AutoBet()
    {
        _isAutoBet = !_isAutoBet;

        if (_isAutoBet)
        {
            Bet();
            rodaToBetChange.SetActive(true);

            autoBetImg.sprite = on;
            if (sys_Num == 0)
            {
                PlayerPrefs.SetInt("Aviator_AutoBet1", 1);

            }
            else if (sys_Num == 1)
            {
                PlayerPrefs.SetInt("Aviator_AutoBet2", 1);

            }
        }
        else
        {
            CancelBet();


        }
    }
    public void Plus()
    {
        betAmount += 10;
        betAmtText.text = betAmount.ToString("F0");
        betBtnAmntTxt.text = "₹" + betAmount;
        if (sys_Num == 0)
        {
            PlayerPrefs.SetInt("aviator_betAmount0", betAmount);
        }
        else
        {
            PlayerPrefs.SetInt("aviator_betAmount1", betAmount);
        }

    }

    public void Minus()
    {
        if (betAmount > 10f)
        {
            betAmount -= 10;
            betAmtText.text = betAmount.ToString("F0");
            betBtnAmntTxt.text = "₹" + betAmount;
            if (sys_Num == 0)
            {
                PlayerPrefs.SetInt("aviator_betAmount0", betAmount);
            }
            else
            {
                PlayerPrefs.SetInt("aviator_betAmount1", betAmount);
            }
        }
    }

    public void Add100()
    {
        if (betRally != 1)
        {
            betAmount = 100;
            betRally = 1;
        }
        else
        {
            betAmount += 100;
        }
        betAmtText.text = betAmount.ToString("F0");
        betBtnAmntTxt.text = "₹" + betAmount;
        if (sys_Num == 0)
        {
            PlayerPrefs.SetInt("aviator_betAmount0", betAmount);
        }
        else
        {
            PlayerPrefs.SetInt("aviator_betAmount1", betAmount);
        }
    }
    public void Add500()
    {
        if (betRally != 2)
        {
            betAmount = 500;
            betRally = 2;
        }
        else
        {
            betAmount += 500;
        }
        betAmtText.text = betAmount.ToString("F0");
        betBtnAmntTxt.text = "₹" + betAmount;
        if (sys_Num == 0)
        {
            PlayerPrefs.SetInt("aviator_betAmount0", betAmount);
        }
        else
        {
            PlayerPrefs.SetInt("aviator_betAmount1", betAmount);
        }
    }
    public void Add2000()
    {
        if (betRally != 3)
        {
            betAmount = 2000;
            betRally = 3;
        }
        else
        {
            betAmount += 2000;
        }
        betAmtText.text = betAmount.ToString("F0");
        betBtnAmntTxt.text = "₹" + betAmount;
        if (sys_Num == 0)
        {
            PlayerPrefs.SetInt("aviator_betAmount0", betAmount);
        }
        else
        {
            PlayerPrefs.SetInt("aviator_betAmount1", betAmount);
        }
    }
    public void Add10000()
    {
        if (betRally != 4)
        {
            betAmount = 10000;
            betRally = 4;
        }
        else
        {
            betAmount += 10000;
        }
        betAmtText.text = betAmount.ToString("F0");
        betBtnAmntTxt.text = "₹" + betAmount;
        if (sys_Num == 0)
        {
            PlayerPrefs.SetInt("aviator_betAmount0", betAmount);
        }
        else
        {
            PlayerPrefs.SetInt("aviator_betAmount1", betAmount);
        }
    }

    public BetManager betManagerRef;

    public void Bet()
    {
        Debug.Log("bet for : " + betAmount);
        Debug.Log("betted");
        if (cancelBetBTN.activeInHierarchy)
        {
            Debug.Log("huii");
            return;
        }
        if (controller.walletAmount >= betAmount)
        {
            autoCahoutBTN_img.gameObject.GetComponent<Button>().interactable = false;
            pastBetAmnt = betAmount;
            betPlacedForAmount = betAmount;
            betBTN.SetActive(false);
            cancelBetBTN.SetActive(true);
            _betted = true;
            Roda.SetActive(true);
            if (controller.gamePhase == "Betting")
            {
                controller.walletAmount -= betAmount;
                controller.walletText.text = "₹" + controller.walletAmount.ToString("F2");
                if (_isAutoCashOut)
                {
                    socketManagerRef.SendBetData(betAmount, sys_Num, cashOutAmt);
                }
                else
                {
                    socketManagerRef.SendBetData(betAmount, sys_Num);
                }

                if (sys_Num == 0)
                {
                    PlayerPrefs.SetFloat("bet1InAviator", betAmount);
                }
                else
                {
                    PlayerPrefs.SetFloat("bet2InAviator", betAmount);
                }
                Debug.Log("saving round Id : " + controller.roundId.ToString());
                PlayerPrefs.SetString("roundId_Aviator", controller.roundId.ToString());
            }

        }
        else { Debug.LogWarning("Insufficient Funds"); }
    }

    public void CancelBet()
    {
        _isAutoBet = false;
        rodaToBetChange.SetActive(false);
        autoBetImg.sprite = off;
        if (sys_Num == 0)
        {
            PlayerPrefs.SetInt("Aviator_AutoBet1", 0);

        }
        else if (sys_Num == 1)
        {
            PlayerPrefs.SetInt("Aviator_AutoBet2", 0);

        }

        autoCahoutBTN_img.gameObject.GetComponent<Button>().interactable = true;
        if (sys_Num == 0)
        {
            PlayerPrefs.SetFloat("bet1InAviator", 0);
        }
        else
        {
            PlayerPrefs.SetFloat("bet2InAviator", 0);
        }
        betPlacedForAmount = 0f;
        _betted = false;
        if (controller.gamePhase == "Betting")
        {
            controller.walletAmount += betAmount;
            controller.walletText.text = "₹" + controller.walletAmount.ToString("F2");
            socketManagerRef.CancelBet(betAmount, sys_Num);

        }
        betBTN.SetActive(true);
        cancelBetBTN.SetActive(false);
        Roda.SetActive(false);

    }


    public void CashOut(int _isAuto = 0)
    {
        if (!_cashOut)
        {
            Debug.Log("Send Cashout Req");
            _cashOut = true;
            if (_isAuto == 1)
            {
                socketManagerRef.Cashout(betAmount, sys_Num, controller.s, autoCahoutAmt);
            }
            else
            {
                socketManagerRef.Cashout(betAmount, sys_Num, controller.s);
            }
        }
    }

    public void DelayedCashOut(float multiplier)
    {

        float cashOutAmnt = betAmount * multiplier;
        Debug.Log("Multiplier from server : " + multiplier);
        Debug.Log("delayed cashout :" + controller.roundEndedAt);
        if (cashOutBTN.activeInHierarchy)
        {
            cashOutSound.Play();
            BetDataUpdate(multiplier, true);
            autoCahoutBTN_img.gameObject.GetComponent<Button>().interactable = true;

            controller.walletAmount += cashOutAmnt;
            controller.walletText.text = "₹" + controller.walletAmount.ToString("F2");

            cashOutBTN.SetActive(false);
            betBTN.SetActive(true);
            _betted = false;
            Roda.SetActive(false);

            GameObject winPanel = Instantiate(winPanelPrefab, canvasRef);
            winPanel.transform.position = winPanelPos.position;
            winPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = multiplier.ToString("F2");
            winPanel.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = cashOutAmnt.ToString("F2");
            StartCoroutine(DisableGameObject(winPanel));
            if (sys_Num == 0)
            {
                PlayerPrefs.SetFloat("bet1InAviator", 0);
            }
            if (sys_Num == 1)
            {
                PlayerPrefs.SetFloat("bet2InAviator", 0);
            }

        }

        if (_isAutoBet)
        {
            Bet();
        }
    }
    public IEnumerator DisableGameObject(GameObject g)
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("disabling Obj");
        g.SetActive(false);
    }
    public IEnumerator CashOutCal()
    {
        _inBet = true;
        _cashOut = false;
        cancelBetBTN.SetActive(false);
        cashOutBTN.SetActive(true);
        do
        {
            cashOutAmt = betAmount * controller.s;
            cashOutAmtText.text = "₹" + cashOutAmt.ToString("F2");
            yield return new WaitForSeconds(0.05f);
            if (_isAutoCashOut)
            {
                if (controller.s >= autoCahoutAmt)
                {
                    CashOut(1);
                    break;
                }
            }
            else { }

        }
        while (!controller._crash && !_cashOut);
        if (controller._crash)
        {
            betBTN.SetActive(true);
            cashOutAmt = 0f;
            cashOutAmtText.text = "";
            cashOutBTN.SetActive(false);
            _betted = false;
            Roda.SetActive(false);
        }
        _inBet = false;
    }



    public void AutoCashOut()
    {
        if (autoCahOutInput.text != null)
        {
            autoCahoutAmt = float.Parse(autoCahOutInput.text);
            if (autoCahoutAmt < 1.1f)
            {
                autoCahOutInput.text = "1.1";
                autoCahoutAmt = 1.1f;
            }
            if (autoCahoutAmt >= 1.1f)
            {
                if (_isAutoCashOut)
                {
                    // cashout button is already red and active now we are disabling it
                    _isAutoCashOut = false;
                    autoCahoutBTN_img.sprite = off;
                    slideStopper.SetActive(false);
                    autoCashOutMultiplier.interactable = true;
                }
                else
                {
                    _isAutoCashOut = true;
                    autoCahoutBTN_img.sprite = on;
                    slideStopper.SetActive(true);
                    autoCashOutMultiplier.interactable = false;
                }
            }
        }


    }


    public void BetDataUpdate(float betX, bool _isWin)
    {
        MyBetUpdate(betAmount, _isWin, betX);
        betPlacedForAmount = 0f;
    }

    public void MyBetUpdate(float betAmt, bool _isProfit, float multx)
    {



        RectTransform contentRectTransform = myBetsContent.GetComponent<RectTransform>();
        VerticalLayoutGroup layoutGroup = myBetsContent.GetComponent<VerticalLayoutGroup>();

        List<int> exclude = new List<int>();
        List<int> presentPlayers = new List<int>();
        GameObject imageGO = Instantiate(myBetDataPrefab, myBetsContent.transform);




        Transform text1Transform = imageGO.transform.Find("Text1");
        Transform text2Transform = imageGO.transform.Find("Text2");
        Transform text3Transform = imageGO.transform.Find("Text3");
        Transform text4Transform = imageGO.transform.Find("Text4");
        Transform greenImg = imageGO.transform.Find("GreenImg");

        // Change the values of the text objects
        Text text1 = text1Transform.GetComponent<Text>();
        text1.text = System.DateTime.Now.ToString("mm:ss");

        Text text2 = text2Transform.GetComponent<Text>();
        //text2.text = playerDetailList[k].wallet;            // Optionally, you can set the image's size, sprite, etc.
        text2.text = betAmt.ToString();            // Optionally, you can set the image's size, sprite, etc.

        Text text3 = text3Transform.GetComponent<Text>();
        text3.text = multx.ToString("F2") + "x";

        if (_isProfit)
        {
            greenImg.GetComponent<Image>().enabled = true;
            Text text4 = text4Transform.GetComponent<Text>();
            text4.text = (multx * betAmt).ToString("F2");
        }



    }

    public void ClearBet()
    {
        Debug.Log("hhhhhhhhhh");
        betAmount = 10;
        betAmtText.text = betAmount.ToString("F0");
        betBtnAmntTxt.text = "₹" + betAmount;

    }
}
