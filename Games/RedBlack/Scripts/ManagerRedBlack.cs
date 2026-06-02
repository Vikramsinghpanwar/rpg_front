
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;
using System.Text;

public class ManagerRedBlack : MonoBehaviour
{

    private System.Random random1;
    private System.Random random2;
    private System.Random random3;
    private System.Random random4;
    private System.Random random5;
    private System.Random random6;
    private System.Random random7;
    private float updateInterval = 0.5f; // 5 times per second
    private int randomValue1 = 0;
    private int randomValue2 = 0;
    private int randomValue3 = 0;
    private int randomValue4 = 0;
    private int randomValue5 = 0;
    private int randomValue6 = 0;
    private int randomValue7 = 0;
    private bool isGenerating = false;
    public int rvi;

    public TextMeshProUGUI betOn1;
    public TextMeshProUGUI betOn2;
    public TextMeshProUGUI betOn3;
    public TextMeshProUGUI betOn4;
    public TextMeshProUGUI betOn5;
    public TextMeshProUGUI betOn6;
    public TextMeshProUGUI betOn7;

    public GameObject waitingPanel;
    public long tmp_periodIdChk;
    public TextMeshProUGUI spadeCount;
    public TextMeshProUGUI heartCount;
    public TextMeshProUGUI clubCount;
    public TextMeshProUGUI diamondCount;
    public TextMeshProUGUI kCount;
    public TextMeshProUGUI blackCount;
    public TextMeshProUGUI redCount;

    public int val_spadeCount;
    public int val_heartCount;
    public int val_clubCount;
    public int val_diamondCount;
    public int val_kCount;
    public int val_blackCount;
    public int val_redCount;


    public bool _isAuto;
    public int[] pastBets = new int[7];
    public GameObject insufficientFunds;
    public long periodId;
    float totalBets;
    float totalWinnings;
    public GameObject winPanel;
    public AudioSource winAudio;
    public Text winAmountTxt;
    public float betPercentClub;
    public float betPercentKing;
    public float betPercentRed;
    public float betPercentBlack;
    public float betPercentHeart;
    public float betPercentDiamond;
    public float betPercentSpade;

    public AudioSource clickSoundA;
    public GameObject w1, w2, w3, w4, w5, w6, w7;
    public int k;
    bool _canBet;
    public GameObject timerCard;
    public GameObject Card;
    public Image cardImage;
    public Animator cardAnimator;
    public GameObject startBetPanel, stopBetPanel;
    public Image timerImg;
    public TextMeshProUGUI timerText;
    public Sprite transparentSprite, cardBackSprite;
    public List<CardDetailRB> cardsList = new List<CardDetailRB>(52);
    public List<CardDetailRB> cardsListVisible = new List<CardDetailRB>(32);
    public List<Image> cardImagesList = new List<Image>(32);
    int chancecount;
    public int time;
    public bool _timer;
    public float currentValue;
    public bool bet1, bet1t, bet2t, bet3t, bet4t, bet5t, bet6t, bet7t, bet2, bet3, bet4, bet5, bet6, bet7, _isBet;
    public List<Sprite> betBTNIsprites;
    public List<Image> betBTNImages;
    public int betOnSpade, betOnClub, betOnDiamond, betOnHeart, betOnRed, betOnBlack, betOnKing;
    public TextMeshProUGUI spadeBetText, clubBetText, diamondBetText, heartBetText, redBetText, blackBetText, kingBetText;
    public BetAmount betAmount;
    public RandomBoli randBet;
    float walletAmount;
    public Text walletText;
    public Options op;
    SocketManagerRVS socketRef;
    APIs apisRef;

    public void ToggleAuto()
    {
        _isAuto = !_isAuto;
    }

   

    private void Start()
    {
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        socketRef = FindObjectOfType<SocketManagerRVS>();
        waitingPanel.SetActive(false);
     


        _isAuto = false;
        insufficientFunds.SetActive(false);
        op = FindObjectOfType<Options>();
        w1.SetActive(false);
        w2.SetActive(false);
        w3.SetActive(false);
        w4.SetActive(false);
        w5.SetActive(false);
        w6.SetActive(false);
        w7.SetActive(false);
        betAmount = FindObjectOfType<BetAmount>();
        randBet = FindObjectOfType<RandomBoli>();

        _canBet = false;
        bet1 = false;
        bet2 = false;
        bet3 = false;
        bet4 = false;
        bet5 = false;
        bet6 = false;
        bet7 = false;
        bet1t = false;
        bet2t = false;
        bet3t = false;
        bet4t = false;
        bet5t = false;
        bet6t = false;
        bet7t = false;
        cardImage = Card.GetComponent<Image>();
        cardAnimator = Card.GetComponent<Animator>();
        startBetPanel.SetActive(false);
        stopBetPanel.SetActive(false);
        chancecount = 24;
        apisRef.FetchWallet();


    }

    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }

    CardDetailRB GetCardObj(string val)
    {
        foreach (CardDetailRB s in cardsList)
        {
            if (s.cardName == val)
            {
                return s;
            }
        }
        return null;
    }


    #region socket function

    public void EnableWaitingPanel()
    {
        waitingPanel.SetActive(true);
    }

    public void HistoryUpdate(string[] hisArray)
    {
        val_spadeCount = 0;
        val_heartCount = 0;
        val_clubCount= 0;
        val_diamondCount = 0;
        val_redCount = 0;
        val_blackCount = 0;
        val_kCount = 0;

        for(int i = 0; i< hisArray.Length; i++)
        {
            char c = char.Parse(hisArray[i].Substring(0, 1));
            switch (c)
            {
                case 's':
                    val_spadeCount++;
                    val_blackCount++;
                    break;
                case 'h':
                    val_heartCount++;
                    val_redCount++;
                    break;
                case 'c':
                    val_clubCount++;
                    val_blackCount++;
                    break;
                case 'd':
                    val_diamondCount++;
                    val_redCount++;
                    break;
                case 'J':
                    val_kCount++;
                    break;
            }
        }


        spadeCount.text = val_spadeCount.ToString();
        heartCount.text = val_heartCount.ToString();
        clubCount.text = val_clubCount.ToString();
        diamondCount.text = val_diamondCount.ToString();

        redCount.text = val_diamondCount + val_heartCount + "";
        blackCount.text = val_spadeCount + val_clubCount + "";
        kCount.text = val_kCount.ToString();

        for (int i = 24; i < 32; i++)
        {

            CardDetailRB visibleCard = new CardDetailRB
            {
                cardImgL = transparentSprite
            };

            cardsListVisible[i] = visibleCard;
            cardImagesList[i].sprite = visibleCard.cardImgL;
        }
        int k = 24;
        if(hisArray.Length < 24)
        {
            k = hisArray.Length;
        }
        for (int i = 0; i < k; i++)
        {
            int randomIndex = Random.Range(0, cardsList.Count);
            CardDetailRB originalCard = GetCardObj(hisArray[i]);

            CardDetailRB visibleCard = new CardDetailRB
            {
                cardImgL = originalCard.cardImgL
            };

            cardsListVisible[i] = visibleCard;
            cardImagesList[i].sprite = visibleCard.cardImgL;
        }
    }

    public void StartBetting(int val, long serverStartTime, string[] seeds)
    {
        BetAmountIncrease(serverStartTime, seeds);

        StartCoroutine(Timer(val));
    }

    public void DeclareResult()
    {
    }

    public void DisplayCard(string resultCard)
    {
        if (waitingPanel.activeInHierarchy)
        {
            return;
        }
        StartCoroutine(AddCard(resultCard));
        isGenerating = false;

    }
    #endregion
    private IEnumerator Timer(int time)
    {
        periodId = long.Parse(System.DateTime.Now.ToString("yyyyMMddHHmmss"));
        waitingPanel.SetActive(false);
        totalWinnings = 0;
        totalBets = 0;
        w1.SetActive(false);
        w2.SetActive(false);
        w3.SetActive(false);
        w4.SetActive(false);
        w5.SetActive(false);
        w6.SetActive(false);
        w7.SetActive(false);
        randBet._isBet = true;
        randBet.BetStarter();
        Card.GetComponent<Image>().sprite = cardBackSprite;
        betOnBlack = 0;
        betOnRed = 0;
        betOnDiamond = 0;
        betOnClub = 0;
        betOnHeart = 0;
        betOnSpade = 0;
        betOnKing = 0;

        blackBetText.text = betOnBlack.ToString();
        redBetText.text = betOnRed.ToString();
        diamondBetText.text = betOnDiamond.ToString();
        clubBetText.text = betOnClub.ToString();
        heartBetText.text = betOnHeart.ToString();
        spadeBetText.text = betOnSpade.ToString();
        kingBetText.text = betOnKing.ToString();


        startBetPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        startBetPanel.SetActive(false);

        _canBet = true;
        BetButtonGray(0);

        timerCard.SetActive(true);

        _timer = true;
        currentValue = time;
        int k = time;
        StartCoroutine(DecreaseOverTime());
        if (_isAuto)
        {
            RepeatBets();
        }
        for (int i = 0; i <= time; i++)
        {
            timerText.text = k.ToString();

            yield return new WaitForSeconds(1);
            k--;

        }
        timerCard.SetActive(false);


        // timer completed now....



    }



    void BetAmountIncrease(long startTime, string[] seeds)
    {
        randomValue1 = 0;
        randomValue2 = 0;
        randomValue3 = 0;
        randomValue4 = 0;
        randomValue5 = 0;
        randomValue6 = 0;
        randomValue7 = 0;

        rvi = 0;


        long currentTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float elapsedTimeInSeconds = (currentTimestamp - startTime) / 1000f;

        // Initialize the random generators with consistent hashes
        random1 = new System.Random(GenerateConsistentHash(seeds[0]));
        random2 = new System.Random(GenerateConsistentHash(seeds[1]));
        random3 = new System.Random(GenerateConsistentHash(seeds[2]));
        random4 = new System.Random(GenerateConsistentHash(seeds[3]));
        random5 = new System.Random(GenerateConsistentHash(seeds[4]));
        random6 = new System.Random(GenerateConsistentHash(seeds[5]));
        random7 = new System.Random(GenerateConsistentHash(seeds[6]));

        Debug.Log("111111111111111111" + random1.Next());

        // Skip ahead in the random sequences based on elapsed time
        int stepsToSkip = Mathf.FloorToInt(elapsedTimeInSeconds * 2); // 5 steps per second
        for (int i = 0; i < stepsToSkip; i++)
        {
            if (rvi < 37)
            {
                rvi++;
                randomValue1 += random1.Next(1, 10);
                randomValue2 += random2.Next(1, 10);
                randomValue3 += random3.Next(1, 10);
                randomValue4 += random4.Next(1, 10);
                randomValue5 += random5.Next(1, 10);
                randomValue6 += random6.Next(1, 10);
                randomValue7 += random7.Next(1, 10);
            }

        }

        // Start generating random values
        isGenerating = true;
        StartCoroutine(IncreaseRandomValues());
    }

    private int GenerateConsistentHash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return System.BitConverter.ToInt32(hashBytes, 0);
        }
    }

    private IEnumerator IncreaseRandomValues()
    {

        while (isGenerating)
        {

            int increment1 = random1.Next(1, 10);
            int increment2 = random2.Next(1, 10);
            int increment3 = random3.Next(1, 10);
            int increment4 = random4.Next(1, 10);
            int increment5 = random5.Next(1, 10);
            int increment6 = random6.Next(1, 10);
            int increment7 = random7.Next(1, 10);
            if (rvi < 37)
            {
                rvi++;
                randomValue1 += increment1;
                randomValue2 += increment2;
                randomValue3 += increment3;
                randomValue4 += increment4;
                randomValue5 += increment5;
                randomValue6 += increment6;
                randomValue7 += increment7;
            }


            int k1 = (randomValue1 * 10);
            int l1 = (randomValue2 * 10);
            int m1 = (randomValue3 * 10);
            int n1 = (randomValue1 * 10);
            int o1 = ((int)(randomValue2/5) * 10);
            int p1 = (randomValue3 * 30);
            int q1 = (randomValue3 * 30);

            betOn1.text = k1.ToString();
            betOn2.text = l1.ToString();
            betOn3.text = m1.ToString();
            betOn4.text = n1.ToString();
            betOn5.text = o1.ToString();
            betOn6.text = p1.ToString();
            betOn7.text = q1.ToString();

            yield return new WaitForSeconds(updateInterval);
        }
    }




    IEnumerator BetStartPanelCoroutine(string cName)
    {

        stopBetPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        stopBetPanel.SetActive(false);
        _canBet = false;
        randBet._isBet = false;

        BetButtonGray(1);

        yield return new WaitForSeconds(1);
        //card animation Logic
        cardAnimator.SetBool("_flip", true);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(0.5f);
        if (totalBets > 0)
        {
            WinnerDisplayer(cName);
        }
        cardImage.sprite = GetCardObj(cName).cardImg;
        cardAnimator.SetBool("_flip", false);
        WinnerCheck(cName);






    }
    IEnumerator DecreaseOverTime()
    {
        float timer = 0.0f;

        while (timer <= time)
        {
            // Calculate the normalized time (0 to 1)
            float normalizedTime = timer / time;

            // Use Mathf.Lerp to smoothly interpolate between 1 and 0
            currentValue = Mathf.Lerp(1.0f, 0.0f, normalizedTime);
            timerImg.fillAmount = currentValue;
            // Increment the timer
            timer += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }


    }

    public IEnumerator AddCard(string cName)
    {
        StartCoroutine(BetStartPanelCoroutine(cName));
        if (cName.ToString().Substring(0, 1) == "s")
        {
            val_spadeCount++;
        }
        else if (cName.ToString().Substring(0, 1) == "h")
        {
            val_heartCount++;
        }

        else if (cName.ToString().Substring(0, 1) == "c")
        {
            val_clubCount++;
        }

        else if (cName.ToString().Substring(0, 1) == "d")
        {
            val_diamondCount++;
        }

        if (cName.ToString().Substring(1) == "k")
        {
            val_kCount++;
        }

        if (val_kCount > Random.Range(5, 15))
        {
            val_kCount--;
        }

        int rn = Random.Range(0, 4);
        switch (rn)
        {
            case 0:
                val_spadeCount--;

                break;
            case 1:
                val_heartCount--;

                break;
            case 2:
                val_clubCount--;
                break;
            case 3:
                val_diamondCount--;
                break;
        }


        spadeCount.text = val_spadeCount.ToString();
        heartCount.text = val_heartCount.ToString();
        clubCount.text = val_clubCount.ToString();
        diamondCount.text = val_diamondCount.ToString();

        redCount.text = val_diamondCount + val_heartCount + "";
        blackCount.text = val_spadeCount + val_clubCount + "";
        kCount.text = val_kCount.ToString();

        yield return new WaitForSeconds(2f);
        if (chancecount == 32)
        {
            AdjustCards();
            chancecount = 24;
        }
        Debug.Log(chancecount);
        Debug.Log(cardsListVisible[chancecount].cardImgL.name + " : radhey");
        Debug.Log(cName);
        Debug.Log(GetCardObj(cName).cardImgL.name);
        cardsListVisible[chancecount].cardImgL = GetCardObj(cName).cardImgL;
        cardImagesList[chancecount].sprite = GetCardObj(cName).cardImgL;

        chancecount++;
    }

    private void AdjustCards()
    {
        for (int i = 0; i < 24; i++)
        {
            CardDetailRB p = cardsListVisible[i + 8];
            cardsListVisible[i].cardImg = p.cardImgL;
            cardImagesList[i].sprite = p.cardImgL;
        }

        for (int i = 24; i < 32; i++)
        {
            cardsListVisible[i].cardImg = null;
            cardImagesList[i].sprite = transparentSprite;
        }
    }




    public void BetButtonGray(int k)
    {
        if (k == 0)
        {
            betBTNImages[0].sprite = betBTNIsprites[0];
            betBTNImages[1].sprite = betBTNIsprites[2];
            betBTNImages[2].sprite = betBTNIsprites[4];
            betBTNImages[3].sprite = betBTNIsprites[6];
            betBTNImages[4].sprite = betBTNIsprites[8];
            betBTNImages[5].sprite = betBTNIsprites[10];
            betBTNImages[6].sprite = betBTNIsprites[12];
        }
        else
        {
            betBTNImages[0].sprite = betBTNIsprites[1];
            betBTNImages[1].sprite = betBTNIsprites[3];
            betBTNImages[2].sprite = betBTNIsprites[5];
            betBTNImages[3].sprite = betBTNIsprites[7];
            betBTNImages[4].sprite = betBTNIsprites[9];
            betBTNImages[5].sprite = betBTNIsprites[11];
            betBTNImages[6].sprite = betBTNIsprites[13];
        }


    }



    public void CheckForNewRoundBet()
    {
        if (tmp_periodIdChk != periodId)
        {
            pastBets = new int[7];
            tmp_periodIdChk = periodId;
        }
    }

    public void BetOnSpade(bool _isClicked)
    {
        if (_canBet)
        {
            if (betAmount.amount <= walletAmount)
            {
                betOnSpade += betAmount.amount;
                walletAmount -= betAmount.amount;
                walletText.text = walletAmount.ToString("F2");
                spadeBetText.text = betOnSpade.ToString();
                totalBets += betAmount.amount;
                if (!_isClicked)
                {
                    CheckForNewRoundBet();
                }
                pastBets[0] = betOnSpade;
                randBet.i1 += betAmount.amount;
                randBet.BetUpdate(1);
                socketRef.SendBetDataToServer('s', betAmount.amount);

            }
            else
            {
                insufficientFunds.SetActive(true);
            }
        }

    }


    public void BetOnClub(bool _isClicked)
    {
        if (_canBet)
        {
            if (betAmount.amount <= walletAmount)
            {
                betOnClub += betAmount.amount;
                walletAmount -= betAmount.amount;
                walletText.text = walletAmount.ToString("F2");
                clubBetText.text = betOnClub.ToString();
                totalBets += betAmount.amount;
                if (!_isClicked)
                {
                    CheckForNewRoundBet();
                }
                pastBets[2] = betOnClub;
                socketRef.SendBetDataToServer('c', betAmount.amount);

                randBet.i3 += betAmount.amount;
                randBet.BetUpdate(3);
            }

            else
            {
                insufficientFunds.SetActive(true);
            }
        }


    }

    public void BetOnHeart(bool _isClicked)
    {
        if (_canBet)
        {
            if (betAmount.amount <= walletAmount)
            {
                betOnHeart += betAmount.amount;
                walletAmount -= betAmount.amount;
                walletText.text = walletAmount.ToString("F2");
                heartBetText.text = betOnHeart.ToString();
                totalBets += betAmount.amount;
                if (!_isClicked)
                {
                    CheckForNewRoundBet();
                }
                pastBets[1] = betOnHeart;
                randBet.i2 += betAmount.amount;
                randBet.BetUpdate(2);
                socketRef.SendBetDataToServer('h', betAmount.amount);

            }
            else
            {
                insufficientFunds.SetActive(true);
            }

        }


    }



    public void BetOnDiamond(bool _isClicked)
    {
        if (_canBet)
        {
            if (betAmount.amount <= walletAmount)
            {
                betOnDiamond += betAmount.amount;
                walletAmount -= betAmount.amount;
                walletText.text = walletAmount.ToString("F2");
                diamondBetText.text = betOnDiamond.ToString();
                totalBets += betAmount.amount;
                if (!_isClicked)
                {
                    CheckForNewRoundBet();
                }
                pastBets[3] = betOnDiamond;
                randBet.i4 += betAmount.amount;
                randBet.BetUpdate(4);
                socketRef.SendBetDataToServer('d', betAmount.amount);

            }
            else
            {
                insufficientFunds.SetActive(true);
            }

        }


    }


    public void BetOnKing(bool _isClicked)
    {
        if (_canBet)
        {
            if (betAmount.amount <= walletAmount)
            {
                betOnKing += betAmount.amount;
                walletAmount -= betAmount.amount;
                walletText.text = walletAmount.ToString("F2");
                kingBetText.text = betOnKing.ToString();
                totalBets += betAmount.amount;
                if (!_isClicked)
                {
                    CheckForNewRoundBet();
                }
                pastBets[4] = betOnKing;
                randBet.i5 += betAmount.amount;
                randBet.BetUpdate(5);
                socketRef.SendBetDataToServer('j', betAmount.amount);

            }
            else
            {
                insufficientFunds.SetActive(true);
            }

        }


    }

    public void BetOnRed(bool _isClicked)
    {
        if (_canBet)
        {
            if (betAmount.amount <= walletAmount)
            {
                betOnRed += betAmount.amount;
                walletAmount -= betAmount.amount;
                walletText.text = walletAmount.ToString("F2");
                redBetText.text = betOnRed.ToString();
                totalBets += betAmount.amount;
                if (!_isClicked)
                {
                    CheckForNewRoundBet();
                }
                pastBets[6] = betOnRed;
                randBet.i7 += betAmount.amount;
                randBet.BetUpdate(7);
                socketRef.SendBetDataToServer('r', betAmount.amount);

            }

            else
            {
                insufficientFunds.SetActive(true);
            }
        }


    }

    public void BetOnBlack(bool _isClicked)
    {
        if (_canBet)
        {
            if (betAmount.amount <= walletAmount)
            {
                betOnBlack += betAmount.amount;
                walletAmount -= betAmount.amount;
                walletText.text = walletAmount.ToString("F2");
                blackBetText.text = betOnBlack.ToString();
                totalBets += betAmount.amount;
                if (!_isClicked)
                {
                    CheckForNewRoundBet();
                }
                pastBets[5] = betOnBlack;

                randBet.i6 += betAmount.amount;
                randBet.BetUpdate(6);
                socketRef.SendBetDataToServer('b', betAmount.amount);

            }

            else
            {
                insufficientFunds.SetActive(true);
            }

        }

    }

    public void RepeatBets()
    {
        int k = betAmount.amount;
        int totalBets = 0;
        for (int i = 0; i < 7; i++)
        {
            totalBets += pastBets[i];
        }
        if (totalBets == 0)
        {
            return;
        }
        for (int i = 0; i < 7; i++)
        {
            if (pastBets[i] > 0)
            {
                betAmount.amount = pastBets[i];

                switch (i)
                {
                    case 0:
                        BetOnSpade(true);
                        break;
                    case 1:
                        BetOnHeart(true);
                        break;
                    case 2:
                        BetOnClub(true);
                        break;
                    case 3:
                        BetOnDiamond(true);
                        break;
                    case 4:
                        BetOnKing(true);
                        break;
                    case 5:
                        BetOnBlack(true);
                        break;
                    case 6:
                        BetOnRed(true);
                        break;
                }

            }
        }

        betAmount.amount = k;

    }

    public void WinnerCheck(string cName)
    {
        if (betOnBlack > 0)
        {
            if (cName.Substring(0, 1) == "s" || cName.Substring(0, 1) == "c")
            {

                totalWinnings += betPercentBlack * betOnBlack;
                walletAmount += betPercentBlack * betOnBlack;
                walletText.text = walletAmount.ToString("F2");
            }
        }

        if (betOnRed > 0)
        {
            if (cName.Substring(0, 1) == "d" || cName.Substring(0, 1) == "h")
            {
                totalWinnings += betPercentRed * betOnRed;
                walletAmount += betPercentRed * betOnRed;
                walletText.text = walletAmount.ToString("F2");
            }
        }



        if (betOnClub > 0)
        {
            if (cName.Substring(0, 1) == "c")
            {
                totalWinnings += betPercentClub * betOnClub;
                walletAmount += betPercentClub * betOnClub;
                walletText.text = walletAmount.ToString("F2");
            }
        }

        if (betOnSpade > 0)
        {
            if (cName.Substring(0, 1) == "s")
            {
                totalWinnings += betPercentSpade * betOnSpade;
                walletAmount += betPercentSpade * betOnSpade;
                walletText.text = walletAmount.ToString("F2");
            }
        }

        if (betOnHeart > 0)
        {
            if (cName.Substring(0, 1) == "h")
            {
                totalWinnings += betPercentHeart * betOnHeart;
                walletAmount += betPercentHeart * betOnHeart;
                walletText.text = walletAmount.ToString("F2");
            }
        }

        if (betOnDiamond > 0)
        {
            if (cName.Substring(0, 1) == "d")
            {
                totalWinnings += betPercentDiamond * betOnDiamond;
                walletAmount += betPercentDiamond * betOnDiamond;
                walletText.text = walletAmount.ToString("F2");
            }
        }

        if (totalWinnings > 0)
        {
            winPanel.SetActive(true);
            Invoke("DeactivateWinPanel", 2);
            winAudio.Play();
            winAmountTxt.text = "₹ " + totalWinnings.ToString("F2");
        }

    }

    public void DeactivateWinPanel()
    {
        winPanel.SetActive(false);
    }
    public void WinnerDisplayer(string cName)
    {
        if (cName.Substring(0, 1) == "c")
        {
            w3.SetActive(true);
            w6.SetActive(true);

        }
        else if (cName.Substring(0, 1) == "s")
        {
            w1.SetActive(true);
            w6.SetActive(true);


        }
        else if (cName.Substring(0, 1) == "h")
        {
            w2.SetActive(true);
            w7.SetActive(true);


        }
        else if (cName.Substring(0, 1) == "d")
        {
            w4.SetActive(true);
            w7.SetActive(true);


        }

    }

}
