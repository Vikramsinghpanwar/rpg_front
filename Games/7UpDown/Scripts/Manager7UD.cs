using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Security.Cryptography;

public class Manager7UD : MonoBehaviour
{

    public static Manager7UD Instance;
    public Animator[] throwItemAnimators_array;

    private System.Random random1;
    private System.Random random2;
    private System.Random random3;
    private float updateInterval = 0.5f; // 5 times per second
    private int randomValue1 = 0;
    private int randomValue2 = 0;
    private int randomValue3 = 0;
    private bool isGenerating = false;
    public int rvi;

    public GameObject waitingForNextRound_Panel;
    public Transform myCoinsHolder;
    public GetTouchPosition getTouchPosRef1;
    public GetTouchPosition getTouchPosRef2;
    public GetTouchPosition getTouchPosRef3;

    public GameObject[] coinPrefabs;
    RandomHistory7UD historyRef;
    public GameObject countDown;
    public TextMeshProUGUI countDownText;
    public List<Sprite> glassDice1List;
    public List<Sprite> glassDice2List;
    public List<Sprite> glassDice3List;
    public List<Sprite> glassDice4List;
    public List<Sprite> glassDice5List;
    public List<Sprite> glassDice6List;

    public GameObject winPanel;
    public Text winPanelTxt;
    public AudioSource winAudio;
    public TextMeshProUGUI betAmount1Text;
    public TextMeshProUGUI betAmount2Text;
    public TextMeshProUGUI betAmount3Text;

    public GameObject notEnoughCashPanel;

    public DistributorSingleWin distributorRef;
    public TextMeshProUGUI watchTxt;
    public AudioSource watchCountDownAudio;
    public AudioSource diceSound;
    public BetManager betManagerRef;
    // Burst7UpDown burstScript;
    public GameObject winner1, winner2, winner3;
    public Image DiceBox;
    public List<Sprite> diceSprites;//A
    public Image dice1Img, dice2Img;
    public Image dice1Img_inBox, dice2Img_inBox;
    public Animator DiceAnimator;
    public GameObject betRoda;
    public GameObject target1, target2, target3;
    public GameObject betStopsPanel;
    public GameObject betStartPanel;
    public Text walletText;
    float walletAmount;
    public int totalBidAmount;
    public int betNumber, betAmount1, betAmount2, betAmount3;
    public Text winnerText;
    public int d1, d2;
    public bool betOn1, betOn2, betOn3;
    // Start is called before the first frame update
    public RandomHistory7UD historyGeneratorRef;
    APIs apisRef;

    SocketManager7UD socketManagerRef;
    TableBotManager botManagerRef;

    public AudioClip noMoreBets_Clip;
    public AudioClip placeYourBets_Clip;

    public AudioSource effect_AudioSource;
    public void ExitGame()
    {
        SceneManager.LoadScene(1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            walletAmount = +1000f;
        }
    }
    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }

    private void Start()
    {
        botManagerRef = FindObjectOfType<TableBotManager>();
        apisRef = FindObjectOfType<APIs>();
        socketManagerRef = FindObjectOfType<SocketManager7UD>();
        apisRef.OnWalletFetched += UpdateWallet;
        waitingForNextRound_Panel.SetActive(false);
        countDown.SetActive(false);
        winPanel.SetActive(false);
        historyGeneratorRef = FindObjectOfType<RandomHistory7UD>();
        historyRef = FindObjectOfType<RandomHistory7UD>();
        notEnoughCashPanel.SetActive(false);
        betManagerRef = FindObjectOfType<BetManager>();
        // burstScript = FindObjectOfType<Burst7UpDown>();
        distributorRef = FindObjectOfType<DistributorSingleWin>();
        apisRef.FetchWallet();

    }


    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }


    public void CheckForPendingBets()
    {
        Debug.Log("Checking for pending bets");
        Debug.Log("current round id : " + socketManagerRef.currentRoundId);
        if (PlayerPrefs.GetString("7ud_roundId") == socketManagerRef.currentRoundId)
        {
            Debug.Log("same round");
            Debug.Log(PlayerPrefs.GetInt("7ud_betsOnUp"));
            Debug.Log(PlayerPrefs.GetInt("7ud_betsOnDown"));
            Debug.Log(PlayerPrefs.GetInt("7ud_betsOnTie"));
            if (PlayerPrefs.GetInt("7ud_betsOnUp") > 0)
            {
                betAmount1 = PlayerPrefs.GetInt("7ud_betsOnUp");
            }
            if (PlayerPrefs.GetInt("7ud_betsOnDown") > 0)
            {
                betAmount2 = PlayerPrefs.GetInt("7ud_betsOnDown");
            }
            if (PlayerPrefs.GetInt("7ud_betsOnTie") > 0)
            {
                betAmount3 = PlayerPrefs.GetInt("7ud_betsOnTie");
            }
            Debug.Log(betAmount1 + " " + betAmount2 + " " + betAmount3);

        }
        else
        {
            PlayerPrefs.SetInt("7ud_betsOnDragon", 0);
            PlayerPrefs.SetInt("7ud_betsOnTiger", 0);
            PlayerPrefs.SetInt("7ud_betsOnTie", 0);

            Debug.Log("p.r : " + PlayerPrefs.GetString("7ud_roundId"));
            Debug.Log("c.r : " + socketManagerRef.currentRoundId);
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




    #region Socket functions




    public void EnableWaitingPanel()
    {
        waitingForNextRound_Panel.SetActive(true);
    }


    public void StartBetting(int val, long serverStartTime, string[] seeds)
    {
        PlayEffect(placeYourBets_Clip);


        waitingForNextRound_Panel.SetActive(false);
        GameStart(val, serverStartTime, seeds);
    }


    public void DisplayWinner(int dice1Val, int dice2Val, char winner, int[] botWinArray)
    {
        PlayEffect(noMoreBets_Clip);

        d1 = dice1Val;
        d2 = dice2Val;
        WinnerChk(winner, botWinArray);
    }
    #endregion
    public void GameStart(int val, long startTime, string[] seeds)
    {
        distributorRef.coinsToDistributeList.Clear();

        dice1Img.color = new Color(0, 0, 0, 0);
        dice2Img.color = new Color(0, 0, 0, 0);
        winner1.SetActive(false);
        winner2.SetActive(false);
        winner3.SetActive(false);
        dice1Img.sprite = null;
        dice2Img.sprite = null;
        betOn1 = false;
        betOn2 = false;
        betOn3 = false;

        foreach (Transform child in target1.transform)
        {
            // Destroy the child GameObject
            if (child.gameObject.tag != "Value")
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

        foreach (Transform child in target3.transform)
        {
            // Destroy the child GameObject
            if (child.gameObject.tag != "Value")
            {
                Destroy(child.gameObject);

            }
        }


        betAmount1 = 0;
        betAmount2 = 0;
        betAmount3 = 0;
        betNumber = -1;
        betStopsPanel.SetActive(false);
        betStartPanel.SetActive(false);
        totalBidAmount = 0;
        walletText.text = "₹" + walletAmount.ToString("F2");
        BetAmountIncrease(startTime, seeds);

        StartCoroutine(DiceChaal(val));

        // StartCoroutine(burstScript.AnimStart(val, 15));
        Debug.Log("Anim Started ");
    }
    public IEnumerator DiceChaal(int val)
    {
        betStartPanel.SetActive(true);
        betRoda.SetActive(false);


        watchTxt.transform.parent.gameObject.SetActive(true);

        StartCoroutine(Clock(val));

        yield return new WaitForSeconds(1);
        betStartPanel.SetActive(false);
        yield return new WaitForSeconds(val - 1);
        isGenerating = false;

        betStopsPanel.SetActive(true);
        yield return new WaitForSeconds(1f);
        betStopsPanel.SetActive(false);

        // burstScript.StopAnim();
        betRoda.SetActive(true);

        //StartCoroutine(Clock(12, true));


    }

    public void ThrowItemAnim(int player, int item)
    {
        StartCoroutine(ThrowItemAnimEnum(player, item));
    }

    IEnumerator ThrowItemAnimEnum(int player, int item)
    {
        throwItemAnimators_array[player].SetInteger("val", item);
        yield return new WaitForSeconds(0.5f);
        throwItemAnimators_array[player].SetInteger("val", 0);
    }


    void BetAmountIncrease(long startTime, string[] seeds)
    {
        randomValue1 = 0;
        randomValue2 = 0;
        randomValue3 = 0;

        rvi = 0;


        long currentTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float elapsedTimeInSeconds = (currentTimestamp - startTime) / 1000f;

        // Initialize the random generators with consistent hashes
        random1 = new System.Random(GenerateConsistentHash(seeds[0]));
        random2 = new System.Random(GenerateConsistentHash(seeds[1]));
        random3 = new System.Random(GenerateConsistentHash(seeds[2]));

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
            if (rvi < 37)
            {
                rvi++;
                randomValue1 += increment1;
                randomValue2 += increment2;
                randomValue3 += increment3;
            }

            int k = (randomValue1 * 990);
            int l = (randomValue2 * 70);
            int m = (randomValue3 * 990);

            betAmount1Text.text = $"<color=yellow>{betAmount1}</color><color=#02ccfe>/{k}</color>";
            betAmount2Text.text = $"<color=yellow>{betAmount3}</color><color=#02ccfe>/{l}</color>";
            betAmount3Text.text = $"<color=yellow>{betAmount2}</color><color=#02ccfe>/{m}</color>";

            yield return new WaitForSeconds(updateInterval);
        }
    }



    Sprite GlassDiceImageSelector(int val1, int val2)
    {
        switch (val1)
        {
            case 1:
                return glassDice1List[val2 - 1];
            case 2:
                return glassDice2List[val2 - 1];

            case 3:
                return glassDice3List[val2 - 1];

            case 4:
                return glassDice4List[val2 - 1];

            case 5:
                return glassDice5List[val2 - 1];

            case 6:
                return glassDice6List[val2 - 1];


            default: return null;
        }
    }

    public void WinnerChk(char winner, int[] botWinArray)
    {
        float winAmount = 0;
        if (winner == 'u')
        {
            if (betOn1)
            {
                winAmount += (2 * betAmount1);

            }
        }
        else if (winner == 'd')

        {
            if (betOn2)
            {
                winAmount += (2 * betAmount2);


            }
        }
        else if (winner == '7')
        {
            if (betOn3)
            {
                winAmount += (5 * betAmount3);


            }
        }



        StartCoroutine(WinnerDisplay(winner, winAmount, botWinArray));
    }
    public void ClearAllBets()
    {
        if(betAmount1 <= 0 && betAmount2 <= 0 && betAmount3 <= 0) return;

        walletAmount += betAmount1 + betAmount2 + betAmount3;
        walletText.text = "₹" + walletAmount.ToString("F2");

        betAmount1 = betAmount2 = betAmount3 = 0;
        PlayerPrefs.SetInt("7ud_betsOnTie", betAmount3);
        PlayerPrefs.SetInt("7ud_betsOnUp", betAmount1);
        PlayerPrefs.SetInt("7ud_betsOnDown", betAmount2);

        socketManagerRef.ClearAllBets();
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
        int val = betManagerRef.betVal;

        betNumber = 2;
        if (val <= walletAmount)
        {
            walletAmount -= val;
            walletText.text = "₹" + walletAmount.ToString();
            GameObject coin = null;
            socketManagerRef.SendBetDataToServer(betOn, val);
            PlayerPrefs.SetString("7ud_roundId", socketManagerRef.currentRoundId);

            switch (betOn)
            {
                case 0:
                    betAmount1 += val;
                    betOn1 = true;
                    InstantiateCoin();
                    PlayerPrefs.SetInt("7ud_betsOnUp", betAmount1);

                    break;
                case 1:
                    betAmount2 += val;

                    betOn2 = true;
                    InstantiateCoin();
                    PlayerPrefs.SetInt("7ud_betsOnDown", betAmount2);

                    break;
                case 2:
                    betAmount3 += val;
                    betOn3 = true;
                    InstantiateCoin();
                    PlayerPrefs.SetInt("7ud_betsOnTie", betAmount3);

                    break;
            }


            StartCoroutine(Destroyer(coin, 1.5f));


        }
        else
        {
            notEnoughCashPanel.SetActive(true);
        }

    }



    public IEnumerator Destroyer(GameObject g, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(g);
    }

    void WinPanelOff()
    {
        winPanel.SetActive(false);
    }
    private IEnumerator WinnerDisplay(char winner, float winAmount, int[] botWinArray)
    {
        Sprite s = GlassDiceImageSelector(d1, d2);
        yield return new WaitForSeconds(2);

        DiceAnimator.enabled = true;

        DiceAnimator.SetBool("_roll", true);



        yield return new WaitForSeconds(1.5f);

        diceSound.Play();

        dice1Img.sprite = diceSprites[d1 - 1];
        dice2Img.sprite = diceSprites[d2 - 1];
        dice1Img.color = new Color(1, 1, 1, 1);
        dice2Img.color = new Color(1, 1, 1, 1);
        yield return new WaitForSeconds(2);
        DiceAnimator.SetBool("_roll", false);


        if (winAmount > 0)
        {
            winPanel.SetActive(true);
            winPanelTxt.text = "<size=36>You Win</size>\n₹" + winAmount.ToString("F2");
            winAudio.Play();
            apisRef.FetchWallet();
        }

        Invoke("WinPanelOff", 2);

        int k = 0;
        if (winner == 'u')
        {
            winner1.SetActive(true);
            k = 0;
            historyRef.TrendGenerator('u');

        }
        else if (winner == 'd')
        {
            winner2.SetActive(true);
            k = 1;
            historyRef.TrendGenerator('d');

        }
        else if (winner == '7')
        {
            winner3.SetActive(true);
            k = 2;
            historyRef.TrendGenerator('7');

        }

        ///yha winner wale dabbe ko chamkana hai
        distributorRef.Distribute(botWinArray);
        botManagerRef.BotWin(botWinArray);

        yield return new WaitForSeconds(2f);

        yield return new WaitForSeconds(0.5f);
        // burstScript.MoveAllcoinsBack();
        ClearMyCoins();
        winner1.SetActive(false);
        winner2.SetActive(false);
        winner3.SetActive(false);
        yield return new WaitForSeconds(1f);

        //StartCoroutine(Clock(4, true));
    }


    public IEnumerator Clock(int val, bool _is = false)
    {
        for (int i = val; i >= 0; i--)
        {
            watchTxt.text = i.ToString();
            if (i < 6)
            {
                if (!_is)
                {
                    watchCountDownAudio.Play();
                    if (i < 4)
                    {
                        countDown.SetActive(true);
                        countDownText.text = i.ToString();
                    }
                }
            }

            yield return new WaitForSeconds(1f);

        }

        countDownText.text = "";
        countDown.SetActive(false);
    }

    public void PlayEffect(AudioClip clip)
    {
        Debug.Log("clip " + clip);
        effect_AudioSource.Stop();
        effect_AudioSource.clip = clip;
        effect_AudioSource.Play();
    }
}
