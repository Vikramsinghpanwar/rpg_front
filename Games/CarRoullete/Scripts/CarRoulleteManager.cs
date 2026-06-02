using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;
using System.Text;

public class CarRoulleteManager : MonoBehaviour
{
    public static CarRoulleteManager Instance { get; set;}

    public AudioClip noMoreBets_Clip;
    public AudioClip placeYourBets_Clip;

    public AudioSource effect_AudioSource;

    public Animator[] throwItemAnimators_array;

    public List<GameObject> myCoinsList;

    private System.Random random1;
    private System.Random random2;
    private System.Random random3;
    private System.Random random4;
    private System.Random random5;
    private System.Random random6;
    private System.Random random7;
    private System.Random random8;


    private float updateInterval = 0.5f; // 5 times per second
    private int randomValue1 = 0;
    private int randomValue2 = 0;
    private int randomValue3 = 0;
    private int randomValue4 = 0;
    private int randomValue5 = 0;
    private int randomValue6 = 0;
    private int randomValue7 = 0;
    private int randomValue8 = 0;

    private bool isGenerating = false;
    public int rvi;



    public GameObject waitingPanel;
    public TextMeshProUGUI timeTowaitTxt;
    public AudioSource bellSound;
    public List<int> otherPlayerWinList = new List<int>();
    public GameObject[] winImgArray;
    public int totalBetAmount;
    public HistoryGeneratorCarRoullete historyRef;
    public GameObject winPanel;
    public Text winPanelAmntText;
    public AudioSource myWin;
    float winAmount;
    public GameObject insufficientFundsPanel;
    public AudioSource coinSound;
    // public Burst burstManager;
    public AudioSource winnerAudio;
    public List<Sprite> items;
    public GameObject winnerImgDisplayer;
    public Image winnerImgDisplayerImage;
    public Animator target1A, target2A, target3A, target4A, target5A, target6A, target7A, target8A;
    public GameObject stopBetPanel, startBetPanel, betRoda;
    [SerializeField] int cardsShuffled;
    public Sprite trp, cardPNG;
    public Image JokerImg;
    public Text walletText;
    float walletAmount;
    TileGlowController tileScript;
    public bool _bet1, _bet2, _bet3, _bet4, _bet5, _bet6, _bet7, _bet8;
    public int bet1BetVal, bet2BetVal, bet3BetVal, bet4BetVal, bet5BetVal, bet6BetVal, bet7BetVal, bet8BetVal;
    public Transform bet1T, bet2T, bet3T, bet4T, bet5T, bet6T, bet7T, bet8T;
    // Start is called before the first frame update

    public TextMeshProUGUI slot1;
    public TextMeshProUGUI slot2;
    public TextMeshProUGUI slot3;
    public TextMeshProUGUI slot4;
    public TextMeshProUGUI slot5;
    public TextMeshProUGUI slot6;
    public TextMeshProUGUI slot7;
    public TextMeshProUGUI slot8;


    public BetManager betManagerRef;
    SocketManagerCarRoulette  socketManager;
    public Transform myCoinHolder;
    Vector3 recentTouchPos;


    APIs apisRef;
    TableBotManager botManagerRef;
    int[] botWinArray;

    #region Socket Fun 
    public void HistoryUpdate(string[] hisArray)
    {
        historyRef.InitialHistoryGenerator(hisArray);
    }

    void Awake()
    {
        if(Instance != this)
        {
            Instance = this;
        }
    }

    public void RegisterTouch(Vector3 pos)
    {
        recentTouchPos = pos;
    }


    public void CheckForPendingBets()
    {
        Debug.Log("Checking");
        if (PlayerPrefs.GetString("car_r_roundId") == socketManager.currentRoundId)
        {
            Debug.Log("same round");
            Debug.Log(PlayerPrefs.GetInt("car_r_betsOnDragon"));
            Debug.Log(PlayerPrefs.GetInt("car_r_betsOnTiger"));
            Debug.Log(PlayerPrefs.GetInt("car_r_betsOnTie"));
            if (PlayerPrefs.GetInt("car_r_betsOn1") > 0)
            {
                bet1BetVal = PlayerPrefs.GetInt("car_r_betsOn1");
            }
            if (PlayerPrefs.GetInt("car_r_betsOn2") > 0)
            {
                bet2BetVal = PlayerPrefs.GetInt("car_r_betsOn2");
            }
            if (PlayerPrefs.GetInt("car_r_betsOn3") > 0)
            {
                bet3BetVal = PlayerPrefs.GetInt("car_r_betsOn3");
            }


            if (PlayerPrefs.GetInt("car_r_betsOn4") > 0)
            {
                bet4BetVal = PlayerPrefs.GetInt("car_r_betsOn4");
            }
            if (PlayerPrefs.GetInt("car_r_betsOn5") > 0)
            {
                bet5BetVal = PlayerPrefs.GetInt("car_r_betsOn5");
            }
            if (PlayerPrefs.GetInt("car_r_betsOn6") > 0)
            {
                bet6BetVal = PlayerPrefs.GetInt("car_r_betsOn6");
            }


            if (PlayerPrefs.GetInt("car_r_betsOn7") > 0)
            {
                bet7BetVal = PlayerPrefs.GetInt("car_r_betsOn7");
            }
            if (PlayerPrefs.GetInt("car_r_betsOn8") > 0)
            {
                bet8BetVal = PlayerPrefs.GetInt("car_r_betsOn8");
            }
      

        }
        else
        {
            PlayerPrefs.SetInt("car_r_betsOnDragon", 0);
            PlayerPrefs.SetInt("car_r_betsOnTiger", 0);
            PlayerPrefs.SetInt("car_r_betsOnTie", 0);

            Debug.Log("p.r : " + PlayerPrefs.GetString("car_r_roundId"));
            Debug.Log("c.r : " + socketManager.currentRoundId);
        }
    }

    public void StartBetting(int remTime, long startTime, string[] seeds)
    {
        PlayEffect(placeYourBets_Clip);

        waitingPanel.SetActive(false);
        Debug.Log("Betting started");
        RoundStart(remTime);
        BetAmountIncrease(startTime, seeds);


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
    public void EnableWaitingPanel()
    {
        //StartCoroutine(WaitinPanelCountDown());
        waitingPanel.SetActive(true);
    }

    public void ShowResult(int winIndex, int[] botWinIndexArray)
    {
        PlayEffect(noMoreBets_Clip);

        StartCoroutine(ShowResultEnum(winIndex));
        botWinArray = new int[6];
        for(int i = 0; i< botWinIndexArray.Length; i++)
        {
            botWinArray[i] = botWinIndexArray[i];
        }
    }
    #endregion
    IEnumerator WaitinPanelCountDown()
    {
        int k = Random.Range(2, 10);
        for(int i = 0; i<=k; i++)
        {
            timeTowaitTxt.text = (k - i).ToString();
            yield return new WaitForSeconds(1f);
        }
        waitingPanel.SetActive(false);

    }



    void Start()
    {
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        botManagerRef = FindObjectOfType<TableBotManager>();
        socketManager = FindObjectOfType<SocketManagerCarRoulette>();
        historyRef = FindObjectOfType<HistoryGeneratorCarRoullete>();
        betManagerRef = FindObjectOfType<BetManager>();
        winnerImgDisplayer.SetActive(false);

        tileScript = FindObjectOfType<TileGlowController>();
        stopBetPanel.SetActive(false);
        startBetPanel.SetActive(false);
        //deck = new List<Card>();
        waitingPanel.SetActive(true);
        apisRef.FetchWallet();

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
        Debug.Log("rasafsdafasdfasdfnkdjsl222");

    }

    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }

    void DeleteChildren(Transform parent)
    {
        // Loop through each child of the parent transform
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            // Destroy the child GameObject
            Destroy(parent.GetChild(i).gameObject);

        }

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
        randomValue8 = 0;

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
        random8 = new System.Random(GenerateConsistentHash(seeds[7]));


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
                randomValue8 += random8.Next(1, 10);

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
            int increment8 = random8.Next(1, 10);
    

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
                randomValue8 += increment8;
       
            }


            int k = (randomValue1 * 10);
            int l = (randomValue2 * 20);
            int m = (randomValue3 * 30);
            int n = (randomValue4 * 40);
            int o = (randomValue5 * 50);
            int p = (randomValue6 * 50);
            int q = (randomValue7 * 50);
            int r = (randomValue8 * 50);
         

            slot1.text = $"<color=yellow>{bet1BetVal}</color><color=#02ccfe>/{k}</color>";
            slot2.text = $"<color=yellow>{bet2BetVal}</color><color=#02ccfe>/{l}</color>";
            slot3.text = $"<color=yellow>{bet3BetVal}</color><color=#02ccfe>/{m}</color>";
            slot4.text = $"<color=yellow>{bet4BetVal}</color><color=#02ccfe>/{n}</color>";
            slot5.text = $"<color=yellow>{bet5BetVal}</color><color=#02ccfe>/{o}</color>";
            slot6.text = $"<color=yellow>{bet6BetVal}</color><color=#02ccfe>/{p}</color>";
            slot7.text = $"<color=yellow>{bet7BetVal}</color><color=#02ccfe>/{q}</color>";
            slot8.text = $"<color=yellow>{bet8BetVal}</color><color=#02ccfe>/{r}</color>";
     

            yield return new WaitForSeconds(updateInterval);
        }
    }






    void RoundStart(int remTime)
    {
        otherPlayerWinList.Clear();
        winAmount = 0f;
        DeleteAllCoins();
        bet1BetVal = 0;
        bet2BetVal = 0;
        bet3BetVal = 0;
        bet4BetVal = 0;
        bet5BetVal = 0;
        bet6BetVal = 0;
        bet7BetVal = 0;
        bet8BetVal = 0;

        _bet1 = false;
        _bet2 = false;
        _bet3 = false;
        _bet4 = false;
        _bet5 = false;
        _bet6 = false;
        _bet7 = false;
        _bet8 = false;



        //CardDistributor();
        StartCoroutine(GameShuffle(remTime));

    }

    public void DeleteAllCoins()
    {

        DeleteChildren(bet1T);
        DeleteChildren(bet2T);
        DeleteChildren(bet3T);
        DeleteChildren(bet4T);
        DeleteChildren(bet5T);
        DeleteChildren(bet6T);
        DeleteChildren(bet7T);
        DeleteChildren(bet8T);
    }
    Coroutine betAmountCoroutineRef;
    IEnumerator GameShuffle(int remTime = 12)
    {
        totalBetAmount = 0;
        ///Game Bet time here
        ///
        betRoda.SetActive(false);

        // StartCoroutine(burstManager.AnimStart(remTime, 12));
        startBetPanel.SetActive(true);
        bellSound.Play();
        yield return new WaitForSeconds(1f);
        startBetPanel.SetActive(false);
        StartCoroutine(tileScript.CountDown(remTime));
       
    }

    IEnumerator ShowResultEnum(int stopAt)
    {
        isGenerating = false;
        // burstManager.StopAnim();
        stopBetPanel.SetActive(true);

        betRoda.SetActive(true);
     
        if(betAmountCoroutineRef != null)
        {
            StopCoroutine(betAmountCoroutineRef);
        }
        yield return new WaitForSeconds(1);
        stopBetPanel.SetActive(false);
        tileScript.TileStart(stopAt);
    }

    public IEnumerator WinnerDeclare(int index)
    {
        Debug.Log("Declaring Winner");
        //resutl show wala panel;

        yield return new WaitForSeconds(2);
        ClearMyCoins();
        winnerAudio.Play();
        winnerImgDisplayer.SetActive(true);
        Transform childTransform = tileScript.tiles[index].transform.GetChild(0);
        Debug.Log(childTransform.name);
        string winner = "";
        if (childTransform != null)
        {
            Image image = childTransform.GetComponent<Image>();
            if (image != null)
            {
                winner = image.sprite.name;
                // Use the 'winner' variable as needed
            }
        }
        print(winner);

        #region for players random win
        int k;
        k = Random.Range(0, 10);
        if (k > 5)
        {
            otherPlayerWinList.Add(1);

            k = Random.Range(0, 10);
            if (k > 5)
            {
                otherPlayerWinList.Add(2);
            }
        }
        else
        {
            otherPlayerWinList.Add(2);
        }



        k = Random.Range(0, 10);
        if (k > 5)
        {
            otherPlayerWinList.Add(3);
            k = Random.Range(0, 10);
            if (k > 5)
            {
                otherPlayerWinList.Add(4);
            }

        }
        else
        {
            otherPlayerWinList.Add(4);
        }


        k = Random.Range(0, 10);
        if (k > 5)
        {
            otherPlayerWinList.Add(5);

            k = Random.Range(0, 10);
            if (k > 5)
            {
                otherPlayerWinList.Add(6);
            }
        }
        else
        {
            otherPlayerWinList.Add(6);
        }



        #endregion








        if (winner == "one")
        {
            //heros
            winnerImgDisplayerImage.sprite = items[0];
            target1A.SetBool("_is", true);
            if (_bet1)
            {
                winAmount += 40 * bet1BetVal;
            }

            // burstManager.Winnerr(0, otherPlayerWinList);

            historyRef.AddHistory(0);
            winImgArray[0].SetActive(true);
        }


        if (winner == "two")
        {
            //Trophy
            winnerImgDisplayerImage.sprite = items[1];

            target2A.SetBool("_is", true);

            if (_bet2)
            {
                winAmount += 25 * bet2BetVal;
            }
            // burstManager.Winnerr(1, otherPlayerWinList);

            historyRef.AddHistory(1);
            winImgArray[1].SetActive(true);

        }

        if (winner == "three")
        {
            //helmet
            winnerImgDisplayerImage.sprite = items[2];
            target3A.SetBool("_is", true);
            if (_bet3)
            {
                winAmount += 15 * bet3BetVal;
            }
            // burstManager.Winnerr(2, otherPlayerWinList);

            historyRef.AddHistory(2);
            winImgArray[2].SetActive(true);

        }

        if (winner == "four")
        {
            //ball
            winnerImgDisplayerImage.sprite = items[3];

            target4A.SetBool("_is", true);

            if (_bet4)
            {
                winAmount += 10 * bet4BetVal;
            }

            // burstManager.Winnerr(3, otherPlayerWinList);

            historyRef.AddHistory(3);
            winImgArray[3].SetActive(true);

        }



        if (winner == "five")
        {
            //red ball
            winnerImgDisplayerImage.sprite = items[4];

            target5A.SetBool("_is", true);

            if (_bet5)
            {
                winAmount += 5 * bet5BetVal;
            }

            // burstManager.Winnerr(4, otherPlayerWinList);

            historyRef.AddHistory(4);
            winImgArray[4].SetActive(true);

        }

        if (winner == "six")
        {
            //green ball
            winnerImgDisplayerImage.sprite = items[5];

            target6A.SetBool("_is", true);

            if (_bet6)
            {
                winAmount += 5 * bet6BetVal;
            }

            // burstManager.Winnerr(5, otherPlayerWinList);

            historyRef.AddHistory(5);
            winImgArray[5].SetActive(true);

        }




        if (winner == "seven")
        {
            //blue ball
            winnerImgDisplayerImage.sprite = items[6];
            target7A.SetBool("_is", true);
            if (_bet7)
            {
                winAmount += 5 * bet7BetVal;
            }

            // burstManager.Winnerr(6, otherPlayerWinList);

            historyRef.AddHistory(6);
            winImgArray[6].SetActive(true);

        }

        if (winner == "eight")
        {
            //white ball
            winnerImgDisplayerImage.sprite = items[7];

            target8A.SetBool("_is", true);

            if (_bet8)
            {
                winAmount += 5 * bet8BetVal;
            }

            // burstManager.Winnerr(7, otherPlayerWinList);

            historyRef.AddHistory(7);
            winImgArray[7].SetActive(true);

        }
        if(winAmount > 0)
        {
            walletAmount += winAmount;
        }

        botManagerRef.BotWin(botWinArray);
        yield return new WaitForSeconds(2);
       
        walletText.text = "₹" + walletAmount.ToString("F2");
        if (winAmount > 0)
        {
            winPanel.SetActive(true);
            winPanelAmntText.text = "₹ " + winAmount;
            myWin.Play();
            yield return new WaitForSeconds(2);
            winPanel.SetActive(false);


        }


        winnerImgDisplayer.SetActive(false);

       
        yield return new WaitForSeconds(2);
        target1A.SetBool("_is",false);
        target2A.SetBool("_is",false);
        target3A.SetBool("_is",false);
        target4A.SetBool("_is",false);
        target5A.SetBool("_is",false);
        target6A.SetBool("_is",false);
        target7A.SetBool("_is",false);
        target8A.SetBool("_is",false);
        for(int i= 0; i<winImgArray.Length; i++)
        {
            winImgArray[i].SetActive(false);
        }
        yield return new WaitForSeconds(1f);
        // burstManager.MoveAllcoinsBack(true);
        tileScript.tiles[index].color = new Color(1, 1, 1, 0);
  
        winnerImgDisplayerImage.sprite = null;
        yield return new WaitForSeconds(1f);
    }


    public void Munim(int val)
    {
        totalBetAmount += val;
        walletAmount -= val;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }

    public void ClearAllBets()
    {
                if (betRoda.activeInHierarchy) return;

        if(bet1BetVal <=0 && bet2BetVal <=0 && bet3BetVal <=0 && bet4BetVal <=0 && bet5BetVal <=0 && bet6BetVal <=0 && bet7BetVal <=0 && bet8BetVal <= 0) return;
        _bet1 = _bet2 = _bet3 = _bet4 = _bet5 = _bet6 = _bet7 = _bet8 = false;

        totalBetAmount = 0;
        walletAmount += bet1BetVal + bet2BetVal + bet3BetVal + bet4BetVal + bet5BetVal + bet6BetVal + bet7BetVal + bet8BetVal;
        walletText.text = "₹" + walletAmount.ToString("F2");

        bet1BetVal = bet2BetVal = bet3BetVal = bet4BetVal = bet5BetVal = bet6BetVal = bet7BetVal = bet8BetVal = 0;
        socketManager.ClearlAllBets();
        ClearMyCoins();
    }


    public void Bet(int betOn)
    {
        if (!betRoda.activeInHierarchy)
        {
            int val = betManagerRef.betVal;
            if (walletAmount >= val)
            {
                Munim(val);
                InstantiateCoin();
                socketManager.SendBetDataToServer(betOn, val);
                switch (betOn)
                {
                    case 1:
                        _bet1 = true;
                        bet1BetVal += val;
                        break;
                        
                    case 2:
                        _bet2 = true;
                        bet2BetVal += val;
                        break;
                    case 3:
                        _bet3 = true;
                        bet3BetVal += val;
                        break;
                    case 4:
                        _bet4 = true;
                        bet4BetVal += val;
                        break;
                    case 5:
                        _bet5 = true;
                        bet5BetVal += val;
                        break;
                    case 6:
                        _bet6 = true;
                        bet6BetVal += val;
                        break;
                    case 7:
                        _bet7 = true;
                        bet7BetVal += val;
                        break;
                    case 8:
                        _bet8 = true;
                        bet8BetVal += val;
                        break;

                            
                }
            }
            else
            {
                insufficientFundsPanel.SetActive(true);
            }
        }

    }

    void InstantiateCoin()
    {        
        coinSound.Play();
        GameObject coin = Instantiate(TableBotManager.Instance.coinPrefabList[betManagerRef.betChipNum - 1], myCoinHolder);
        coin.transform.localScale = Vector3.one;
        coin.transform.position = recentTouchPos;
        myCoinsList.Add(coin);
        coin.transform.SetParent(myCoinHolder);
    }

    void ClearMyCoins()
    {
        foreach(GameObject g in myCoinsList)
        {
            Destroy(g);
        }
        myCoinsList.Clear();
    }

    public void PlayEffect(AudioClip clip)
    {
        Debug.Log("clip " + clip);
        effect_AudioSource.Stop();
        effect_AudioSource.clip = clip;
        effect_AudioSource.Play();
    }
}