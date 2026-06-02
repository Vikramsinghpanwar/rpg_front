using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ManagerRoullete : MonoBehaviour
{
    public GameObject waitingForNextRoundPanel;
    float winAmnt;
    AnimBallPos animBallPosRef;
    public Animator ballAnim;
    public GameObject ballAnimObj;
    public bool _isAmerican;
    public List<GameObject> number_glow_animation;
    public GameObject even_glow_animation;
    public GameObject odd_glow_animation;
    public GameObject small_glow_animation;
    public GameObject big_glow_animation;
    public GameObject red_glow_animation;
    public GameObject black_glow_animation;
    public GameObject first12_glow_animation;
    public GameObject second12_glow_animation;
    public GameObject third12_glow_animation;
    public GameObject first_game_glow_animation;
    public GameObject second_game_glow_animation;
    public GameObject third_game_glow_animation;

    public float glow_anim_duration = 2f;
    public Transform initialBallHolder;
    public Transform initialBallPos;
    bool _Spinned;
    public Transform num_Wheel;
    public int initialgotiForce = 2000;
    public Rigidbody2D goti;
    bool _isHistoryOpen;
    public GameObject historyPanel;
    RandomHIstoryRoullete historyRef;
    public Transform resultWheel;
    public GameObject resultSideImg;
    public AutoRoullete autoRoulleteRef;
    public GameObject showNumberObj;
    public AudioSource roulletSpinAudio;
    public AudioSource touckSound;
    public bool _isBetPlaced;
    public GameObject obstaclePanel;
    public bool _isSpinning;
    public GameObject totalWinOrLoseShowPanel;
    public TextMeshProUGUI totalWinLoseText;
    public Animator wheelAnimator;
    public Animator slideAnimator;
    public GameObject whiteBall;
    public GameObject ballPosParent;
    public List<GameObject> whiteBallPosObjects;
    private int luckyNum;
    public Text walletTxt;
    public float numbersMultiplier = 2;
    public float dualNumbersMultiplier = 2;
    public float tripleNumberMultiplier = 2;
    public float FourNumbersMultiplier = 2;
    public float sixNumbersMultiplier = 2;
    public float twelveNumbersMultiplier = 2;
    public float twoMultiplier = 2;
    public float rowMultiplier = 2;
    public float twoColumnMultiplier = 2;
    public float fourColumnMultiplier = 2;
    int[] red = new int[] { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
    int[] black = new int[] { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35 };
    BetManager bManager;
    Bets betsScript;
    APIs apisRef;

    public RouletteSpin s4;

    public float walletAmount;

    TableBotManager botManagerRef;
    int[] botWinArray;
    public string gamePhase = "";
    SocketManagerRoulette socketManagerRef;
    #region socket function
    public void HistoryUpdate(string[] hisArray)
    {
        historyRef.InitialHistoryCreator(hisArray);
    }

    public void StartBetting(int remTime, long serverStartTime, string seed)
    {
        Debug.Log("Betting Sttart");
        gamePhase = "Betting";
        waitingForNextRoundPanel.SetActive(false);
        StartCoroutine(autoRoulleteRef.Auto(remTime, serverStartTime, seed));
    }

    public void StopBetting()
    {
        autoRoulleteRef.isGenerating = false;
        gamePhase = "";
    }

    public void EnableWaitingPanel()
    {
        //StartCoroutine(WaitinPanelCountDown());
        waitingForNextRoundPanel.SetActive(true);

    }

    public void DisplayResult(int winNum, int[] botWinA)
    {
        autoRoulleteRef.isGenerating = false;
        gamePhase = "Result";
        PlayerPrefs.SetInt("roulette_totalBets", 0);
        botWinArray = new int[3];
        for(int i = 0; i< botWinA.Length; i++)
        {
            botWinArray[i] = botWinA[i];
        }
        Spin(winNum);
    }


    #endregion



    public void CheckForPendingBets()
    {
        if (PlayerPrefs.GetString("roulette_roundId") == socketManagerRef.currentRoundId)
        {
            Debug.Log("same round ");
            Debug.Log("roulette total bets : " + PlayerPrefs.GetInt("roulette_totalBets"));
            if (PlayerPrefs.GetInt("roulette_totalBets") > 0)
            {
                betsScript.totalBetAmt = PlayerPrefs.GetInt("roulette_totalBets");
                betsScript.totalBetAmtTxt.text = betsScript.totalBetAmt.ToString();
            }
          
        }
        else
        {
            PlayerPrefs.SetInt("roulette_totalBets", 0);

        }
    }


    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletTxt.text = "₹" + walletAmount.ToString("F2");
    }

    void Start()
    {
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        botManagerRef = FindObjectOfType<TableBotManager>();
        animBallPosRef = FindObjectOfType<AnimBallPos>();
        walletTxt.text = "₹" + walletAmount.ToString("F2");
        socketManagerRef = FindObjectOfType<SocketManagerRoulette>();
        goti.constraints = RigidbodyConstraints2D.FreezeAll;
        _Spinned = false;
        historyPanel.SetActive(false);
        _isHistoryOpen = false;
        historyRef = FindObjectOfType<RandomHIstoryRoullete>();
        resultSideImg.SetActive(false);
        showNumberObj.SetActive(false);
        _isBetPlaced = false;
        obstaclePanel.SetActive(false);
        _isSpinning = false;
        totalWinOrLoseShowPanel.SetActive(false);
        bManager = FindObjectOfType<BetManager>();
        betsScript = FindObjectOfType<Bets>();
        whiteBall.SetActive(false);
        whiteBallPosObjects.Clear();
        foreach (Transform child in ballPosParent.transform)
        {
            whiteBallPosObjects.Add(child.gameObject);
        }

        autoRoulleteRef = FindObjectOfType<AutoRoullete>();
        apisRef.FetchWallet();
    }



    public void Spin(int winNum)
    {
        if (!_isSpinning)
        {
            winAmnt = 0f;
            
            roulletSpinAudio.Play();
            obstaclePanel.SetActive(true);

            StartCoroutine(SpinAnim(winNum));
            walletTxt.text = "₹" + walletAmount.ToString("F2");
            _isSpinning = true;
        }

    }

    public void InitializeBots(List<BotData> data, long roundStartTime)
    {


        if (botManagerRef != null)
        {
            botManagerRef.InitializeBots(data, roundStartTime);
        }
        else
        {
            Debug.Log("null pada hai table");
            botManagerRef = FindObjectOfType<TableBotManager>();
            if (botManagerRef != null)
            {
                botManagerRef.InitializeBots(data, roundStartTime);

            }
            else
            {
                Debug.Log("Abhi bhi null pada hai table");
            }
        }

    }

    public IEnumerator SpinAnim(int winNum)
    {
        //Coroutine timerCoroutine = StartCoroutine(autoRoulleteRef.Timer(18));
        ballAnimObj.SetActive(true);
        ballAnim.SetBool("_is", true);
        
        whiteBall.SetActive(false);
        slideAnimator.SetBool("_slide", true);
        yield return new WaitForSeconds(4f);
        do
        {
            yield return new WaitForSeconds(0.01f);
        }
        while(animBallPosRef.latestCollisionObj.name != winNum.ToString());
        ballAnimObj.SetActive(false);
        whiteBall.SetActive(true);
        ballAnim.SetBool("_is", false);

        whiteBall.transform.localPosition = animBallPosRef.latestCollisionObj.transform.localPosition;
        touckSound.Play();
        ballAnimObj.SetActive(false);
        luckyNum = winNum;
        whiteBall.SetActive(true);
        yield return new WaitForSeconds(1f);
        showNumberObj.SetActive(true);
        showNumberObj.GetComponent<Image>().color = Color.black;

        for (int i = 0; i < red.Length; i++)
        {
            if (red[i] == luckyNum)
            {
                showNumberObj.GetComponent<Image>().color = Color.red;
            }
        }

        showNumberObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = luckyNum.ToString();
        yield return new WaitForSeconds(1f);
        ShowResult(luckyNum);
        s4.gameObject.SetActive(false);
        WinAmountChk();
    
        yield return new WaitForSeconds(2);
        historyRef.HistoryUpdate(luckyNum);
        yield return new WaitForSeconds(3);
        wheelAnimator.SetBool("_spin", true);
        slideAnimator.SetBool("_slide", false);


        yield return new WaitForSeconds(0.5f);
        WinChk(luckyNum);
        botManagerRef.BotWin(botWinArray);
        yield return new WaitForSeconds(glow_anim_duration + 0.5f);
        if (winAmnt> 0)
        {
            walletAmount += winAmnt;
            totalWinOrLoseShowPanel.SetActive(true);
            totalWinLoseText.text = "<size=48><color=yellow>Win</color></size>\n<color=green>" + "Rs. " + winAmnt + "</color>";
            walletTxt.text = "₹" + walletAmount.ToString("F2");

        }
        _isBetPlaced = false;


        yield return new WaitForSeconds(3);
        showNumberObj.SetActive(false);
        resultSideImg.SetActive(false);
        wheelAnimator.SetBool("_spin", false);

        ///////////////
        ///spin pura hua sabko reset karo
        totalWinOrLoseShowPanel.SetActive(false);
        betsScript.betsOnList.Clear();
        betsScript.betsOnList2.Clear();
        betsScript.betsOnList4.Clear();
        betsScript.betsOnList6.Clear();
        betsScript.betsOnList12.Clear();

        betsScript.Clear_myBetCoins();


        Clear();
        _isSpinning = false;

        /*timerCoroutine = StartCoroutine(autoRoulleteRef.Timer(5));
        yield return new WaitForSeconds(5);*/

        // StopCoroutine(timerCoroutine);
        obstaclePanel.SetActive(false);


    }

    public void Clear()
    {
        betsScript.betsOnList.Clear();
        betsScript.betsOnList2.Clear();
        betsScript.betsOnList4.Clear();
        _isBetPlaced = false;
        betsScript.totalBetAmt = 0;
        betsScript.totalBetAmtTxt.text = "0";
        foreach (Transform g in betsScript.transform)
        {
            Destroy(g.gameObject);
        }
    }

    public void TwoX()
    {
        if (walletAmount > 2 * betsScript.totalBetAmt)
        {
            foreach (BetDetails bDetail in betsScript.betsOnList)
            {
                bDetail.betAmount *= 2;
                betsScript.totalBetAmt *= 2;
                betsScript.totalBetAmtTxt.text = betsScript.totalBetAmt.ToString();
            }


            foreach (BetDetails bDetail in betsScript.betsOnList2)
            {
                bDetail.betAmount *= 2;
                betsScript.totalBetAmt *= 2;
                betsScript.totalBetAmtTxt.text = betsScript.totalBetAmt.ToString();
            }


            foreach (BetDetails bDetail in betsScript.betsOnList3)
            {
                bDetail.betAmount *= 2;
                betsScript.totalBetAmt *= 2;
                betsScript.totalBetAmtTxt.text = betsScript.totalBetAmt.ToString();
            }

            foreach (BetDetails bDetail in betsScript.betsOnList4)
            {
                bDetail.betAmount *= 2;
                betsScript.totalBetAmt *= 2;
                betsScript.totalBetAmtTxt.text = betsScript.totalBetAmt.ToString();
            }

            foreach (BetDetails bDetail in betsScript.betsOnList6)
            {
                bDetail.betAmount *= 2;
                betsScript.totalBetAmt *= 2;
                betsScript.totalBetAmtTxt.text = betsScript.totalBetAmt.ToString();
            }
            foreach (BetDetails bDetail in betsScript.betsOnList12)
            {
                bDetail.betAmount *= 2;
                betsScript.totalBetAmt *= 2;
                betsScript.totalBetAmtTxt.text = betsScript.totalBetAmt.ToString();
            }
        }
        else
        {


        }
    }

    public void WinChk(int val)
    {
        if (val != 0)
        {
            if (val % 2 == 0)
            {
                //even
                StartCoroutine(GlowAnimation(even_glow_animation));
            }
            else if (val % 2 == 1)
            {
                StartCoroutine(GlowAnimation(odd_glow_animation));

            }

            for (int i = 0; i < red.Length; i++)
            {
                if (red[i] == val)
                {
                    StartCoroutine(GlowAnimation(red_glow_animation));
                    break;
                }
            }

            for (int i = 0; i < black.Length; i++)
            {
                if (black[i] == val)
                {
                    StartCoroutine(GlowAnimation(black_glow_animation));
                    break;
                }
            }

            if (val <= 18)
            {
                StartCoroutine(GlowAnimation(small_glow_animation));

            }
            else
            {
                StartCoroutine(GlowAnimation(big_glow_animation));

            }
            if (val <= 12)
            {
                StartCoroutine(GlowAnimation(first12_glow_animation));
            }

            else if (val <= 24)
            {
                StartCoroutine(GlowAnimation(second12_glow_animation));
            }

            else if (val <= 36)
            {
                StartCoroutine(GlowAnimation(third12_glow_animation));
            }

            if (luckyNum % 3 == 0)
            {
                StartCoroutine(GlowAnimation(first_game_glow_animation));
            }
            if ((luckyNum + 1) % 3 == 0)
            {
                StartCoroutine(GlowAnimation(second_game_glow_animation));

            }

            if ((luckyNum + 2) % 3 == 0)
            {
                StartCoroutine(GlowAnimation(third_game_glow_animation));
            }


        }

        StartCoroutine(GlowAnimation(number_glow_animation[val]));

    }

    public IEnumerator GlowAnimation(GameObject g)
    {
        g.SetActive(true);
        yield return new WaitForSeconds(glow_anim_duration);
        g.SetActive(false);
    }
    public void WinAmountChk()
    {
        winAmnt = 0;
        foreach (BetDetails bDetail in betsScript.betsOnList)
        {
            Debug.Log("bet on : " + bDetail.betOn + "\n bet amount : " + bDetail.betAmount);
            Debug.Log("initial win amount : " + winAmnt);
            /// number p koi bet h kya
            if (luckyNum == bDetail.betOn)
            {
                winAmnt += (bDetail.betAmount * numbersMultiplier);
            }

            if(luckyNum != 0)
            {
                /// odd or even
                /// even hai

                if (bDetail.betOn == 38)
                {
                    if (luckyNum % 2 == 0)
                    {
                        winAmnt += (bDetail.betAmount * twoMultiplier);
                    }
                }
                //odd hai
                else if (bDetail.betOn == 39)
                {
                    if (luckyNum % 2 == 1)
                    {
                        winAmnt += (bDetail.betAmount * twoMultiplier);
                    }
                }

                if (bDetail.betOn == 40)
                {
                    for (int i = 0; i < red.Length; i++)
                    {
                        if (red[i] == luckyNum)
                        {
                            winAmnt += (bDetail.betAmount * twoMultiplier);                          
                        }
                    }
                }

                else if (bDetail.betOn == 41)
                {
                    for (int i = 0; i < black.Length; i++)
                    {
                        if (black[i] == luckyNum)
                        {
                            winAmnt += (bDetail.betAmount * twoMultiplier);
                        }
                    }
                }

            }

            if (bDetail.betOn == 42)
            {
                if (luckyNum > 0 && luckyNum <= 12)
                {
                    winAmnt += (bDetail.betAmount * fourColumnMultiplier);
                }
            }

            if (bDetail.betOn == 43)
            {
                if (luckyNum > 12 && luckyNum <= 24)
                {
                    winAmnt += (bDetail.betAmount * fourColumnMultiplier);
                }
            }

            if (bDetail.betOn == 44)
            {
                if (luckyNum > 24 && luckyNum <= 36)
                {
                    winAmnt += (bDetail.betAmount * fourColumnMultiplier);
                }
            }

            if (bDetail.betOn == 45)
            {
                if (luckyNum > 0 && luckyNum <= 18)
                {
                    winAmnt += (bDetail.betAmount * twoMultiplier);
                }
            }

            if (bDetail.betOn == 46)
            {
                if (luckyNum > 18 && luckyNum <= 36)
                {
                    winAmnt += (bDetail.betAmount * twoMultiplier);
                }
            }

            if(luckyNum != 0)
            {
                if (bDetail.betOn == 47)
                {
                    if (luckyNum % 3 == 0)
                    {
                        winAmnt += (bDetail.betAmount * rowMultiplier);
                    }
                }

                if (bDetail.betOn == 48)
                {
                    if ((luckyNum + 1) % 3 == 0)
                    {
                        winAmnt += (bDetail.betAmount * rowMultiplier);
                    }
                }

                if (bDetail.betOn == 49)
                {
                    if ((luckyNum + 2) % 3 == 0)
                    {
                        winAmnt += (bDetail.betAmount * rowMultiplier);
                    }
                }
            }
           
        }















        ////////////////bets on double number
        ///


        foreach (BetDetails bDetail in betsScript.betsOnList2)
        {


            /// number p koi bet h kya
            if (luckyNum == bDetail.betOn)
            {
                winAmnt += (bDetail.betAmount * dualNumbersMultiplier);
            }


        }



        foreach (BetDetails bDetail in betsScript.betsOnList3)
        {


            /// number p koi bet h kya
            if (luckyNum == bDetail.betOn)
            {
                winAmnt += (bDetail.betAmount * tripleNumberMultiplier);
            }


        }





        foreach (BetDetails bDetail in betsScript.betsOnList4)
        {


            /// number p koi bet h kya
            if (luckyNum == bDetail.betOn)
            {
                winAmnt += (bDetail.betAmount * FourNumbersMultiplier);
            }


        }

        foreach (BetDetails bDetail in betsScript.betsOnList6)
        {


            /// number p koi bet h kya
            if (luckyNum == bDetail.betOn)
            {
                winAmnt += (bDetail.betAmount * sixNumbersMultiplier);
            }


        }


        foreach (BetDetails bDetail in betsScript.betsOnList12)
        {
            /// number p koi bet h kya
            if (luckyNum == bDetail.betOn)
            {
                winAmnt += (bDetail.betAmount * twelveNumbersMultiplier);
            }
        }
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
    /// 45 = 1-6
    /// 46 = 31-36
    ///  47 = 1stRow
    ///  48 = 2nd Row
    ///  49 = 3rd Row
    ///  
    // Update is called once per frame

    public void ShowResult(int val)
    {
        int[] numberSeq = new int[] {13, 36, 11, 30, 8, 23, 10, 5
            , 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35
            , 3, 26, 0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27 };

        if (_isAmerican)
        {
            numberSeq = new int[] {33, 16, 4, 23, 35, 14, 2, 0, 28
            , 9, 26, 30, 11, 7, 20, 32, 17, 5, 22, 34, 15, 3, 24, 36
            , 13, 1, 00, 27, 10, 25, 29, 12, 8, 19, 31, 18, 6, 21 };
        }



        int pos = 0;
        float z;


        for (int i = 0; i < numberSeq.Length; i++)
        {
            if (numberSeq[i] == val)
            {
                pos = i;
                break;
            }
        }

        resultSideImg.SetActive(true);
        if (_isAmerican)
        {
            z = (pos + 3) * (360f / 38f) - 7f;

        }
        else
        {
            z = pos * (360f / 37f) - 1f;

        }

        resultWheel.localRotation = Quaternion.Euler(0, 0, z);

    }

    public void History()
    {
        if (_isHistoryOpen)
        {
            historyPanel.SetActive(false);
        }
        else
        {
            historyPanel.SetActive(true);
        }
        _isHistoryOpen = !_isHistoryOpen;
    }

    public void Lobby()
    {
        SceneManager.LoadScene(1);
    }
}
