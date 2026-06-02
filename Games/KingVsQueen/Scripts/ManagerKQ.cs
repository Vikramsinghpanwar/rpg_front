using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Security.Cryptography;
using System.Text;

public class ManagerKQ : MonoBehaviour
{

    public Animator[] throwItemAnimators_array;
    public static ManagerKQ Instance;

    private System.Random random1;
    private System.Random random2;
    private System.Random random3;
    private float updateInterval = 0.5f; // 5 times per second
    private int randomValue1 = 0;
    private int randomValue2 = 0;
    private int randomValue3 = 0;
    private bool isGenerating = false;
    public int rvi;

    SocketManagerKQ socketManager;
    public GameObject waitingForNextRound_Obj;
    public TextMeshProUGUI queenCardqypeTxt;
    public TextMeshProUGUI kingCardqypeTxt;
    public CountDown321 countDownRef;
    public GameObject kingGlowObj;
    public GameObject queenGlowObj;
    public GameObject tieGlowObj;
    public List<Sprite> qCardList;
    public List<Sprite> kCardList;

    public GameObject winPanel;
    public Text winPanelAmntTxt;
    public AudioSource winAudio;
    RandomHistoryKQ historyGeneratorRef;
    public AudioSource clockTickSound;
    public GameObject insufficientFundsObj;
    public BetManager betManagerRef;
    public AudioSource cashCollectSound;
    public AudioSource betStopSound;
    public AudioSource SingleCoinSound;
    public AudioSource SwordSlashSoundEffectAudioSource;
    public Animator cardAnimator1;
    public Animator cardAnimator2;
    public Animator cardAnimator3;
    public Animator cardAnimator4;
    public Animator cardAnimator5;
    public Animator cardAnimator6;
    public GameObject betRoda;
    public Text KingBetAmountText, QueenBetAmountText, tieBetAmountText;
    public GameObject target1 ,target2;
    public GameObject betStopsPanel, betStartPanel;
    public Text timer_text;
    public Image clockVsImg;
    public Sprite clockSpr;
    public Sprite vsSpr;
    public Text walletText;
    float walletAmount;
    public Sprite cardImg, cardImgRed;
    public int p1po, p2po;
    public Image kingCards_1;
    public Image kingCards_2;
    public Image kingCards_3;
    public Image queenCard_1;
    public Image queenCard_2;
    public Image queenCard_3; 
    public Sprite KingCardDetail, QueenCardDetail;
    public List<Sprite> cardsList;
    public int totalBidAmount;
    public int betNumber, betAmountKing, betAmountQueen, betAmountShot;
    public GameObject winnerPannel;
    public TextMeshProUGUI winnerText;
    // Start is called before the first frame update
    public bool _StopAmount;
    int k, l, m;
    // public BurstKQ burstScript;
    APIs apisRef;
    TableBotManager botManagerRef;
    SocketManagerKQ SocketManagerKQRef;
    string gamePhase = "";
    private void Start()
    {
        SocketManagerKQRef = FindObjectOfType<SocketManagerKQ>();
        botManagerRef = FindObjectOfType<TableBotManager>();
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        socketManager = FindObjectOfType<SocketManagerKQ>();
        countDownRef = FindObjectOfType<CountDown321>();
        historyGeneratorRef = FindObjectOfType<RandomHistoryKQ>();
        betManagerRef = FindObjectOfType<BetManager>();
        betStartPanel.SetActive(false);
        // burstScript = FindObjectOfType<BurstKQ>();
        betRoda.SetActive(true);
        insufficientFundsObj.SetActive(false);
        apisRef.FetchWallet();
    }

    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }

    private void OnEnable()
    {
        // deck se cards utha le re baba
        cardsList.Clear();
        Sprite[] cardsObject = Resources.LoadAll<Sprite>("Deck");
        for(int s = 0; s<cardsObject.Length; s++)
        {
            cardsList.Add(cardsObject[s]);
        }
    }

    public void CheckForPendingBets()
    {
        if (PlayerPrefs.GetString("kq_roundId") == SocketManagerKQRef.currentRoundId)
        {
            if (PlayerPrefs.GetInt("kq_betsOnKing") > 0)
            {
                betAmountKing = PlayerPrefs.GetInt("kq_betsOnKing");
            }
            if (PlayerPrefs.GetInt("kq_betsOnQueen") > 0)
            {
                betAmountQueen = PlayerPrefs.GetInt("kq_betsOnQueen");
            }
            if (PlayerPrefs.GetInt("kq_betsOnShot") > 0)
            {
                betAmountShot = PlayerPrefs.GetInt("kq_betsOnShot");
            }

        }
        else
        {
            PlayerPrefs.SetInt("kq_betsOnKing", 0);
            PlayerPrefs.SetInt("kq_betsOnQueen", 0);
            PlayerPrefs.SetInt("kq_betsOnShot", 0);
  
        }
    }


    #region Socket Functions

    public void EnableWaitingPanel()
    {
        waitingForNextRound_Obj.SetActive(true);
    }

    public void StartBetting(int val, long serverStartTime, string[] seeds)
    {
        gamePhase = "Betting";
        botManagerRef.Reset();

        StartCoroutine(GameStartEnum(val, serverStartTime, seeds));
        StartTimerFun(val);
    }




    public void ThrowItemAnim(int player, int item)
    {
        StartCoroutine(ThrowItemAnimEnum(player, item));
    }

    IEnumerator ThrowItemAnimEnum(int player, int item)
    {
        Debug.Log("player : " + player);
        throwItemAnimators_array[player].SetInteger("val", item);
        yield return new WaitForSeconds(0.5f);
        throwItemAnimators_array[player].SetInteger("val", 0);
    }


    public void Result(List<string> KingCard, List<string> QueenCard, char winner, string kingCardsType, string queenCardsType)
    {
        gamePhase = "Result";
        isGenerating = false;
        PlayerPrefs.SetInt("kq_betsOnKing", 0);
        PlayerPrefs.SetInt("kq_betsOnQueen", 0);
        PlayerPrefs.SetInt("kq_betsOnShot", 0);

        StartCoroutine(ShowCardsEnum(KingCard, QueenCard, winner, kingCardsType, queenCardsType));
        BetCheck();

    }

    #endregion

    Coroutine timerEnumRef;

    public void StartTimerFun(float val)
    {
        if (timerEnumRef != null)
        {
            StopCoroutine(timerEnumRef);

        }
        timerEnumRef = StartCoroutine(StartTimer(val));
    }


    public void InitializeBots(List<BotData> data, long roundStartTime)
    {
        if (botManagerRef != null)
        {
            botManagerRef.InitializeBots(data, roundStartTime);
        }
    }
    
    public IEnumerator GameStartEnum(int val, long startTime, string[] seeds)
    {
        waitingForNextRound_Obj.SetActive(false);
        queenCardqypeTxt.text = "";
        kingCardqypeTxt.text = "";
        qCardList.Clear();
        kCardList.Clear();
        List<int> excludedValues = new List<int>();
    

        if (val > 18)
        {
            betStartPanel.SetActive(true);
            SwordSlashSoundEffectAudioSource.Play();

            yield return new WaitForSeconds(2f);
            betStartPanel.SetActive(false);
        }
        
        _StopAmount = false;
        isGenerating = true;
        k = l = m = 0;
        BetAmountIncrease(startTime, seeds);


        betRoda.SetActive(false);
        // StartCoroutine(burstScript.AnimStart(val, 20));

        winnerPannel.SetActive(false);
        foreach (Transform child in target1.transform)
        {
            // Destroy the child GameObject
            if(child.gameObject.tag != "Value")
            {
                Destroy(child.gameObject);

            }
        }

        foreach (Transform child in target2.transform)
        {
            // Destroy the child GameObject
            if (child.gameObject.tag != "Value")
            {
                Destroy(child.gameObject);

            }
        }
        KingBetAmountText.text = "0/0";
        QueenBetAmountText.text = "0/0";
        betAmountKing = 0;
        betAmountQueen = 0;
        betAmountShot = 0;
        betNumber = -1;
        betStopsPanel.SetActive(false);
        totalBidAmount = 0;
        walletText.text = "₹" + walletAmount.ToString("F2");


       
        clockVsImg.transform.DOScaleX(0, 0.3f)
           .SetEase(Ease.Linear)
           .OnComplete(() =>
           {

               clockVsImg.sprite = clockSpr;
               timer_text.text = "";
               clockVsImg.transform.DOScaleX(1, 0.3f)
           .SetEase(Ease.Linear)
           .OnComplete(() =>
           {

           });
           });
    }


    public IEnumerator GameStopEnum()
    {

        betStopsPanel.SetActive(true);
        betStopSound.Play();
        BetCheck();
        yield return new WaitForSeconds(1);
        betStopsPanel.SetActive(false);

        clockVsImg.transform.DOScaleX(0, 0.3f)
           .SetEase(Ease.Linear)
           .OnComplete(() =>
           {

               clockVsImg.sprite = vsSpr;
               timer_text.text = "";
               clockVsImg.transform.DOScaleX(1, 0.3f)
           .SetEase(Ease.Linear)
           .OnComplete(() =>
           {

           });
           });
    }
    void BetAmountIncrease(long startTime, string[] seeds)
    {
        Debug.Log("start time :  " + startTime);
        randomValue1 = 0;
        randomValue2 = 0;
        randomValue3 = 0;

        rvi = 0;


        long currentTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float elapsekqimeInSeconds = (currentTimestamp - startTime) / 1000f;

        // Initialize the random generators with consistent hashes
        random1 = new System.Random(GenerateConsistentHash(seeds[0]));
        random2 = new System.Random(GenerateConsistentHash(seeds[1]));
        random3 = new System.Random(GenerateConsistentHash(seeds[2]));


        // Skip ahead in the random sequences based on elapsed time
        int stepsToSkip = Mathf.FloorToInt(elapsekqimeInSeconds * 2); // 5 steps per second
        Debug.Log("steps to skip : " + stepsToSkip);    
        for (int i = 0; i < stepsToSkip; i++)
        {
            if (rvi < 37)
            {
                rvi++;
                randomValue1 += random1.Next(1, 10) * random1.Next(1, 10);
                randomValue2 += random2.Next(1, 10) * random1.Next(1, 10);
                randomValue3 += random3.Next(1, 10) * random1.Next(1, 10);
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
            int increment1 = random1.Next(1, 10) * random1.Next(1, 10);
            int increment2 = random2.Next(1, 10) * random1.Next(1, 10);
            int increment3 = random3.Next(1, 10) * random1.Next(1, 10);
            if (rvi < 37)
            {
                rvi++;
                randomValue1 += increment1;
                randomValue2 += increment2;
                randomValue3 += increment3;
            }


            int k = (randomValue1 * 120);
            int l = (randomValue2 * 120);
            int m = (randomValue3 * 20);

            KingBetAmountText.text = $"<color=yellow>{betAmountKing}</color><color=#02ccfe>/{k}</color>";
            QueenBetAmountText.text = $"<color=yellow>{betAmountQueen}</color><color=#02ccfe>/{l}</color>";
            tieBetAmountText.text = $"<color=yellow>{betAmountShot}</color><color=#02ccfe>/{m}</color>";

            yield return new WaitForSeconds(updateInterval);
        }
    }


    private IEnumerator StartTimer(float duration)
    {
        timer_text.text = (duration).ToString();

        for (int i = 1; i <= duration; i++)
        {
            yield return new WaitForSeconds(1f);
            float remTime = duration - i;
            timer_text.text = (remTime).ToString();
            if (remTime < 4 && remTime > 0)
            {
                countDownRef.Hit((int)remTime);
            }
        }      
    }

    public void BetCheck()
    {
        Debug.Log("stopping coins");
        betRoda.SetActive(true);
        // burstScript.StopAnim();
        _StopAmount = true;
    }

    public void ClearAllBets()
    {
        if(gamePhase != "Betting")
        {
            return;
        }
        if(betAmountKing <= 0 && betAmountQueen <= 0 && betAmountShot <= 0) return;

        walletAmount += betAmountKing + betAmountQueen + betAmountShot;
        walletText.text = "₹" + walletAmount.ToString("F2");

        betAmountKing = betAmountQueen = betAmountShot = 0;
        m = l = k = 0;
        PlayerPrefs.SetInt("kq_betsOnShot", betAmountShot);
        PlayerPrefs.SetInt("kq_betsOnKing", betAmountShot);
        PlayerPrefs.SetInt("kq_betsOnQueen", betAmountShot);

        socketManager.ClearAllBets();
        ClearMyCoins();

    }

    public List<GameObject> myCoinsList;
    public Transform myCoinHolder;
    Vector3 recentTouchPos;
    void Awake()
    {
        if(Instance != this)
        {
            Instance = this;
        }
    }    
    public void RegisterTouch(Vector3 touch)
    {
        recentTouchPos = touch;
    }
    void ClearMyCoins()
    {
        foreach(GameObject g in myCoinsList)
        {
            Destroy(g);
        }
        myCoinsList.Clear();
    }

    void InstantiateCoin()
    {        
        GameObject coin = Instantiate(TableBotManager.Instance.coinPrefabList[betManagerRef.betChipNum - 1], myCoinHolder);
        coin.transform.localScale = Vector3.one;
        coin.transform.position = recentTouchPos;
        myCoinsList.Add(coin);
        coin.transform.SetParent(myCoinHolder);
    }
     
   
    public void Bet(int betOn)
    {
        if(gamePhase != "Betting")
        {
            return;
        }
        int val = betManagerRef.betVal;
        betNumber = 0;
        if(val <= walletAmount)
        {
            walletAmount -= val;
            SingleCoinSound.Play();
            PlayerPrefs.SetString("kq_roundId", SocketManagerKQRef.currentRoundId);
            InstantiateCoin();
            walletText.text = "₹" + walletAmount.ToString("F2");
            switch (betOn){
                case 0:
                    //Shot case
                    betAmountShot += val;
                    m += betManagerRef.betVal;
                    PlayerPrefs.SetInt("kq_betsOnShot", betAmountShot);

                    break;
                case 1:
                    betAmountKing += val;
                    k += betManagerRef.betVal;
                    PlayerPrefs.SetInt("kq_betsOnKing", betAmountShot);

                    break;
                case 2:
                    betAmountQueen += val;
                    l += betManagerRef.betVal;
                    PlayerPrefs.SetInt("kq_betsOnQueen", betAmountShot);

                    break;
            }

            totalBidAmount += val;
            socketManager.SendBetDataToServer(betOn, val);

        }
        else { 
            insufficientFundsObj.SetActive(true);
        }


    }



   
    public IEnumerator ShowCardsEnum(List<string> kingCardsList, List<string> queenCardsList, char winner, string kingCardsType, string queenCardsType)
    {
        int k = Random.Range(0, 1000);  
        yield return new WaitForSeconds(3f);
        cardAnimator1.SetBool("_flip", true);
        cardAnimator2.SetBool("_flip", true);
        cardAnimator3.SetBool("_flip", true);
        cardAnimator4.SetBool("_flip", true);
        cardAnimator5.SetBool("_flip", true);
        cardAnimator6.SetBool("_flip", true);
        yield return new WaitForSeconds(0.3f);




        kingCards_1.sprite = GetCardSpriite(kingCardsList[0]);
        kingCards_2.sprite = GetCardSpriite(kingCardsList[1]);
        kingCards_3.sprite = GetCardSpriite(kingCardsList[2]);

        queenCard_1.sprite = GetCardSpriite(queenCardsList[0]);
        queenCard_2.sprite = GetCardSpriite(queenCardsList[1]);
        queenCard_3.sprite = GetCardSpriite(queenCardsList[2]);

        yield return new WaitForSeconds(2f);
        cardAnimator1.SetBool("_flip", false);
        cardAnimator2.SetBool("_flip", false);
        cardAnimator3.SetBool("_flip", false);
        cardAnimator4.SetBool("_flip", false);
        cardAnimator5.SetBool("_flip", false);
        cardAnimator6.SetBool("_flip", false);

        kingCardqypeTxt.text = kingCardsType;
        queenCardqypeTxt.text = queenCardsType;
        historyGeneratorRef.TrendGenerator(winner);
        if(winner == 'k')
        {
            StartCoroutine(WinnerDisplay(winner, kingCardsList));
        }
        else if (winner == 'q')
        {
            StartCoroutine(WinnerDisplay(winner, queenCardsList));
        }
    }

    Sprite GetCardSpriite(string val)
    {
        foreach(Sprite s in cardsList)
        {
            if(s.name == val)
            {
                return s;
            }
        }
        return null;
    }
   

    public void WinPanelOff()
    {
        winPanel.SetActive(false);

    }


    int GetCardRank(string cardName)
    {
        string s = cardName.Substring(1);
    
        if (int.TryParse(s, out int res))
        {
            return res;
        }
        else
        {
            if(s == "a")
            {
                return 14;
            }
            else if (s == "k")
            {
                return 13;
            }
            else if (s == "q")
            {
                return 12;
            }
            else if (s == "j")
            {
                return 11;
            }
            else if (s == "o")
            {
                return 10;
            }
            else
            {
                return 0;
            }

        }

    }
    public void WinnerCoinMoveAnimation(int val)
    {
        // burstScript.Winner(val);
    }

    bool HighPairCheck(List<string> WinningCardsList)
    {
        int pairVal = 0;
        int r1 = GetCardRank(WinningCardsList[0]);
        int r2 = GetCardRank(WinningCardsList[1]);
        int r3 = GetCardRank(WinningCardsList[2]);
        if ( r1 == r2 || r1 == r3){
            pairVal = r1;
        }
        else if(r2 == r3)
        {
            pairVal = r2;
        }
        return pairVal > 8 ? true : false;
    }
    private IEnumerator WinnerDisplay(char val, List<string> WinningCardsList)
    {
        float winAmount = 0;
        float winMultiplier = 1f;
        
        if (val == 'k')
        {
            string cType = kingCardqypeTxt.text;
            switch (cType)
            {
                case "Trail":
                    winMultiplier = 11;
                    break;
                case "Pure Sequence":
                    winMultiplier = 7;
                    break;
                case "Sequence":
                    winMultiplier = 5;
                    break;
                case "Color":
                    winMultiplier = 4;
                    break;
                case "Suit":
                    winMultiplier = 4;
                    break;
                case "Pair":
                    if (HighPairCheck(WinningCardsList))
                    {
                        winMultiplier = 3;
                    }
                    else
                    {
                        winMultiplier = 0;
                    }
                    break;
                case "High Card":
                    winMultiplier = 0;
                    break;
            }

            winAmount += 2 * betAmountKing;
            if (winMultiplier > 0)
            {
                winAmount += winMultiplier * betAmountShot;
                tieGlowObj.SetActive(true);
            }
            kingGlowObj.SetActive(true);
            yield return new WaitForSeconds(2f);
            kingGlowObj.SetActive(false);
        }
        else if (val == 'q')
        {
            string cType = queenCardqypeTxt.text;
            switch (cType)
            {
                case "Trail":
                    winMultiplier = 10;
                    break;
                case "Pure Sequence":
                    winMultiplier = 8;
                    break;
                case "Sequence":
                    winMultiplier = 4;
                    break;
                case "Color":
                    winMultiplier = 3;
                    break;
                case "Suit":
                    winMultiplier = 3;
                    break;
                case "Pair":
                    if (HighPairCheck(WinningCardsList))
                    {
                        winMultiplier = 1;
                    }
                    else
                    {
                        winMultiplier = 0;
                    }
                    break;
                case "High Card":
                    winMultiplier = 0;
                    break;
            }

            winAmount += 2 * betAmountQueen;
            if (winMultiplier > 0)
            {
                winAmount += winMultiplier * betAmountShot;
                tieGlowObj.SetActive(true);
            }
            queenGlowObj.SetActive(true);
            yield return new WaitForSeconds(2f);
            queenGlowObj.SetActive(false);
        }
        tieGlowObj.SetActive(false);

        yield return new WaitForSeconds(1f);


        winnerPannel.SetActive(true);
        if(val == 'k')
        {
            winnerText.text = "King Wins";
            // burstScript.Winnerr(0, null);

        }
        else if( val == 'q')
        {
            winnerText.text = "Queen Wins";
            // burstScript.Winnerr(2, null);

        }
        else if(val == 't')
        {
            winnerText.text = "Tie";
            // burstScript.Winnerr(1, null);

        }
        yield return new WaitForSeconds(2f);
        winnerPannel.SetActive(false);
ClearMyCoins();


        yield return new WaitForSeconds(1f);
        if(winAmount > 0)
        {
            winAudio.Play();
            winPanel.SetActive(true);
            winPanelAmntTxt.text = "<size=36>You Win</size>\n₹" + winAmount.ToString("F1");
            yield return new WaitForSeconds(1.5f);
            winPanel.SetActive(false);
        }
        if (betAmountKing + betAmountQueen + betAmountShot > 0)
        {
            apisRef.FetchWallet();
        }

        yield return new WaitForSeconds(0.5f);
        // burstScript.MoveAllcoinsBack();
        yield return new WaitForSeconds(1);
        yield return new WaitForSeconds(0.3f);
        kingCards_1.sprite = cardImg;
        kingCards_2.sprite = cardImg;
        kingCards_3.sprite = cardImg;
        queenCard_1.sprite = cardImg;
        queenCard_2.sprite = cardImg;
        queenCard_3.sprite = cardImg;


        yield return new WaitForSeconds(0.5f);
        cardAnimator1.SetBool("_flip", false);
        cardAnimator2.SetBool("_flip", false);
        cardAnimator3.SetBool("_flip", false);
        cardAnimator4.SetBool("_flip", false);
        cardAnimator5.SetBool("_flip", false);
        cardAnimator6.SetBool("_flip", false);
        yield return new WaitForSeconds(0.5f);
    }
}
