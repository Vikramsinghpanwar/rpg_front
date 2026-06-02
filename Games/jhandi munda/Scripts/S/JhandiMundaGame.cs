using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Features.Lobby.Integration;

public class JhandiMundaGame : MonoBehaviour
{
    [System.Serializable]
    public class DicePositionArrayClass
    {
        public Transform[] dicePosition_Array;
    }

    public static JhandiMundaGame Instance;

    public TextMeshProUGUI[] winXText;

    public GameObject winAmountDisplayer_Object;
    public TextMeshProUGUI winAmountDisplayer_Text;
    public DicePositionArrayClass[] dicePosition_Object;
    public Dictionary<int, int> diceResultCount;
    private Coroutine timerCoroutine;
    private bool[] isChangingSprites;
    private float spriteChangeInterval = 0.1f;
    public Transform[] central_Dice_Position_Array;
    public GameObject[] glowImagesArray;
    public AudioClip noMoreBets_Clip;
    public AudioClip placeYourBets_Clip;

    public AudioSource effect_AudioSource;
    public GameObject diceHolder;
    public float dicePos_Offset;
    public Image[] diceImage_Array;
    public Sprite[] diceSprite_Array;
    public Sprite[] angledDiceSprite_Array;
    public Transform[] diceDestination_Transform_Array;

    private System.Random random1;
    private System.Random random2;
    private System.Random random3;
    private System.Random random4;
    private System.Random random5;
    private System.Random random6;

    private float updateInterval = 0.5f;
    private int randomValue1 = 0;
    private int randomValue2 = 0;
    private int randomValue3 = 0;
    private int randomValue4 = 0;
    private int randomValue5 = 0;
    private int randomValue6 = 0;
    private bool isGenerating = false;
    public int rvi;

    public TextMeshProUGUI countdowntext;
    private int[] totalBetsForNumber = new int[6];
    private int totalBets = 0;
    private int totalWinnings = 0;
    private int lostBetsCount = 0;
    TableBotManager botManagerRef;
    SocketManagerJM SocketManagerJMRef;
    // BurstJM burst;
    BetManager betManagerRef;

    TrendHistoryJM trendHistoryJM;
    float walletAmount;
    public TextMeshProUGUI walletText;
    public Animator[] throwItemAnimators_array;
    APIs apisRef;
    public AudioSource SingleCoinSound;
    public AudioSource clockTickSound;
    public Text[] BetAmountText_Array;
    public float[] BetAmount_Array;
    public GameObject waitingForNextRound_Obj;
    public TrendHistoryJM historyManagerRef;
    public enum GameState
    {
        Betting,
        Result,
        Rest,
    }

    public GameState currentGameState;
    public Animator diceCupAnimator;


    void Start()
    {
        betManagerRef = FindObjectOfType<BetManager>();
        historyManagerRef = FindObjectOfType<TrendHistoryJM>();
        currentGameState = GameState.Rest;
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        SocketManagerJMRef = FindObjectOfType<SocketManagerJM>();
        botManagerRef = FindObjectOfType<TableBotManager>();
        // burst = FindObjectOfType<BurstJM>();
        trendHistoryJM = FindObjectOfType<TrendHistoryJM>(); // Find TrendHistoryJM
        apisRef.FetchWallet();
    }

    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }

    public void StartBetting(long startTime, string[] seeds, int remTime = 16)
    {
        Debug.Log("betting started");
        currentGameState = GameState.Betting;
        diceResultCount = new Dictionary<int, int>();
        isChangingSprites = new bool[diceImage_Array.Length]; // Initialize flags
        PlayEffect(placeYourBets_Clip);
        StartCoroutine(BettingStart(startTime, seeds, remTime));
    }
    IEnumerator BettingStart(long startTime, string[] seeds, int remTime)
    {
        waitingForNextRound_Obj.SetActive(false);
        BetAmount_Array = new float[6];
        // burst.StartCoroutine(burst.AnimStart(remTime, 16));

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(StartTimer(remTime));
        yield return new WaitForSeconds(1f);
        BetAmountIncrease(startTime, seeds);

        // yield return new WaitForSeconds(remTime);
        // burst.StopAnim();
    }

    public void EnableWaitingPanel(int remTime = 16)
    {
        waitingForNextRound_Obj.SetActive(true);
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(StartTimer(remTime));
    }

    //Result
    public void DisplayCards(int[] resultDiceArray, int[] botWinArray = null)
    {
        isGenerating = false;
        PlayEffect(noMoreBets_Clip);
        StartCoroutine(ShowDiceEnum(resultDiceArray, botWinArray));

    }

    IEnumerator ShowDiceEnum(int[] result, int[] botwinArray)
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("result Array: [" + string.Join(", ", result) + "]");
        currentGameState = GameState.Result;
        diceCupAnimator.SetBool("_is", true);
        yield return new WaitForSeconds(1f);
        diceHolder.SetActive(true);
        Dictionary<int, int> diceCount = new Dictionary<int, int>();

        for (int i = 0; i < result.Length; i++)
        {
            int diceValue = result[i];

            // Track how many times this dice value has appeared
            if (!diceCount.ContainsKey(diceValue))
            {
                diceCount[diceValue] = 0;
            }
            else
            {
                diceCount[diceValue]++;
            }

            Vector3 basePosition = diceDestination_Transform_Array[diceValue].position - new Vector3(dicePos_Offset, 0, 0);
            Vector3 offset = new Vector3(diceCount[diceValue] * 200f, 0, diceCount[diceValue] * 0.2f);

            //StartCoroutine(MoveObject(basePosition + offset, diceImage_Array[i].transform, diceValue, i));
        }

        MoveDices(diceImage_Array, result);
        // Initialize the winner array (length 6, assuming dice values are 1 to 6)
        int[] winnerArray = new int[6];

        // Find the winning dice values (values appearing at least twice)
        foreach (var pair in diceCount)
        {
            if (pair.Value >= 1) // If a dice value appears at least twice, mark it as a winner
            {
                //if (pair.Key == 0) continue;
                winnerArray[pair.Key] = 1; // Adjust index (assuming dice values are 1-based)
            }
        }

        // Debug the winner array
        yield return new WaitForSeconds(2f);

        for (int k = 0; k < winnerArray.Length; k++)
        {
            if (winnerArray[k] == 1)
            {
                glowImagesArray[k].SetActive(true);
            }
        }
        WinAmountDisplayer(result);
        yield return new WaitForSeconds(2f);
        // burst.WinnerJM(winnerArray);

        yield return new WaitForSeconds(2f);

        diceCupAnimator.SetBool("_is", false);

        diceHolder.SetActive(false);
        foreach (Image i in diceImage_Array)
        {
            i.transform.localPosition = Vector3.zero;
        }
        botManagerRef.BotWin(botwinArray);
        historyManagerRef.AddHistoryColumn(result);
        ClearMyCoins();
        // burst.MoveAllcoinsBack();
        foreach (GameObject g in glowImagesArray)
        {
            g.SetActive(false);

        }
    }

    void WinAmountDisplayer(int[] winnerArray)
    {
        Dictionary<int, int> winMultipliers = new Dictionary<int, int>
    {
        { 2, 3 },
        { 3, 5 },
        { 4, 10 },
        { 5, 20 },
        { 6, 100 },
    };

        int[] winMultiplier_Array = new int[] { 0, 0, 0, 0, 0, 0 };
        // Count occurrences of each number
        foreach (int num in winnerArray)
        {
            winMultiplier_Array[num]++;

        }


        float totalWinAmount = 0;
        int[] returnMultiplierArray = new int[] { 0, 0, 0, 0, 0, 0 };

        for (int i = 0; i < winnerArray.Length; i++)
        {
            int num = winnerArray[i];
            if (winMultiplier_Array[i] > 1)
            {
                int multiplier = winMultipliers[winMultiplier_Array[i]];
                returnMultiplierArray[i] = multiplier;
                totalWinAmount += BetAmount_Array[i] * multiplier;
            }

        }
        ShowWinText(returnMultiplierArray);

        Debug.Log("winarray : " + string.Join(", ", winMultiplier_Array));
        if (totalWinAmount > 0)
        {
            ShowWinPopUp(totalWinAmount);
            UpdateWallet(walletAmount + totalWinAmount);
        }

        Debug.Log("bets Array: [" + string.Join(", ", BetAmount_Array) + "]");
        Debug.Log("Total Win : " + totalWinAmount);
    }

    void ShowWinText(int[] multiplierArray)
    {
        for (int i = 0; i < 6; i++)
        {
            if (multiplierArray[i] > 1)
            {
                winXText[i].text = "Winx " + multiplierArray[i].ToString();
            }
        }
        Invoke("OffWinText", 2f);
    }

    void OffWinText()
    {
        foreach (TextMeshProUGUI t in winXText)
        {
            t.text = "";
        }
    }
    public void ShowWinPopUp(float winAmount)
    {
        winAmountDisplayer_Object.SetActive(true);
        winAmountDisplayer_Text.text = winAmount.ToString();
        Invoke("Offwins", 2f);
    }

    public void Offwins()
    {
        winAmountDisplayer_Object.SetActive(false);
    }
    public void MoveDices(Image[] dices, int[] reslutArray)
    {
        for (int i = 0; i < dices.Length; i++)
        {
            if (i < central_Dice_Position_Array.Length)
            {
                int diceIndex = i;
                isChangingSprites[diceIndex] = true;

                float elapsedTime = 0f; // Track time elapsed for sprite change

                // Start movement
                dices[diceIndex].transform.DOMove(central_Dice_Position_Array[diceIndex].position, 1f)
                    .SetEase(Ease.InOutSine)
                    .OnUpdate(() =>
                    {
                        elapsedTime += Time.deltaTime; // Increase time counter
                        if (elapsedTime >= spriteChangeInterval) // Change sprite at intervals
                        {
                            elapsedTime = 0f; // Reset counter
                            RandomizeSprite(diceIndex);
                        }
                    })
                     .OnComplete(() =>
                     {
                         SetFinalSprite(diceIndex, reslutArray[diceIndex]);
                         DOVirtual.DelayedCall(1f, () => MoveToFinalPosition(diceIndex, reslutArray[diceIndex]));
                     });
            }
        }
    }

    void RandomizeSprite(int index)
    {
        if (isChangingSprites[index])
        {
            int randomSpriteIndex = Random.Range(0, angledDiceSprite_Array.Length);
            diceImage_Array[index].sprite = angledDiceSprite_Array[randomSpriteIndex];
        }
    }

    void SetFinalSprite(int index, int resultIndex)
    {
        isChangingSprites[index] = false; // Stop randomization
        diceImage_Array[index].sprite = diceSprite_Array[resultIndex]; // Set final result sprite
    }

    void MoveToFinalPosition(int diceIndex, int result)
    {

        // Debug check for diceImage_Array
        if (diceImage_Array == null || diceIndex < 0 || diceIndex >= diceImage_Array.Length)
        {
            Debug.LogError($"diceImage_Array is null or diceIndex {diceIndex} is out of bounds!");
            return;
        }

        if (diceImage_Array[diceIndex] == null)
        {
            Debug.LogError($"diceImage_Array[{diceIndex}] is null!");
            return;
        }

        // Debug check for dicePosition_Array
        if (dicePosition_Object == null || result < 0 || result >= dicePosition_Object.Length)
        {
            Debug.LogError($"dicePosition_Array is null or result index {result} is out of bounds!");
            return;
        }

        if (dicePosition_Object[result] == null)
        {
            Debug.LogError($"dicePosition_Array[{result}] is null!");
            return;
        }

        if (dicePosition_Object[result].dicePosition_Array == null || dicePosition_Object[result].dicePosition_Array.Length == 0)
        {
            Debug.LogError($"dicePosition_Array[{result}].dicePosition_Array is null or empty!");
            return;
        }

        // Initialize tracking dictionary
        if (diceResultCount == null)
        {
            Debug.LogWarning("diceResultCount dictionary was null, initializing...");
            diceResultCount = new Dictionary<int, int>();
        }

        if (!diceResultCount.ContainsKey(result))
            diceResultCount[result] = 0; // Initialize tracking for this result

        int placementIndex = diceResultCount[result]; // Get the next available position
        diceResultCount[result]++; // Increment counter for this result

        // Ensure we don't go out of bounds
        if (placementIndex >= dicePosition_Object[result].dicePosition_Array.Length)
        {
            Debug.LogError($"Not enough positions for result {result}. Index {placementIndex} is out of bounds.");
            return;
        }


        // Move the dice to its final destination
        diceImage_Array[diceIndex].transform.DOMove(dicePosition_Object[result].dicePosition_Array[placementIndex].position, 0.5f)
            .SetEase(Ease.InOutSine);
    }



    public void UpdateHistory(int[][] history)
    {
        historyManagerRef.PopulateTrends(history);
    }
    IEnumerator ChangeDiceImageEnum(Image img)
    {
        while (true)
        {
            img.sprite = diceSprite_Array[Random.Range(0, diceSprite_Array.Length)];
            yield return new WaitForSeconds(0.1f);
        }

    }
    IEnumerator MoveObject(Vector3 targetPos, Transform obj, int resImg, int index)
    {
        Coroutine cor = StartCoroutine(ChangeDiceImageEnum(diceImage_Array[index]));
        while (Vector3.Distance(obj.position, targetPos) > 0.01f)
        {
            obj.position = Vector3.MoveTowards(obj.position, targetPos, 1000 * Time.deltaTime);
            yield return null;
        }
        StopCoroutine(cor);
        diceImage_Array[index].sprite = diceSprite_Array[resImg];

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



    public void CheckForPendingBets()
    {
        if (PlayerPrefs.GetString("DvT_roundId") == SocketManagerJMRef.currentRoundId)
        {
            Debug.Log("same round");
            Debug.Log(PlayerPrefs.GetInt("dt_betsOnDragon"));
            Debug.Log(PlayerPrefs.GetInt("dt_betsOnTiger"));
            Debug.Log(PlayerPrefs.GetInt("dt_betsOnTie"));
            if (PlayerPrefs.GetInt("dt_betsOnDragon") > 0)
            {
                //betAmountDragon = PlayerPrefs.GetInt("dt_betsOnDragon");
            }
            if (PlayerPrefs.GetInt("dt_betsOnTiger") > 0)
            {
                //betAmountTiger = PlayerPrefs.GetInt("dt_betsOnTiger");
            }
            if (PlayerPrefs.GetInt("dt_betsOnTie") > 0)
            {
                //betAmountTie = PlayerPrefs.GetInt("dt_betsOnTie");
            }
        }
        else
        {

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

    /// <summary>

    void BetAmountIncrease(long startTime, string[] seeds)
    {
        randomValue1 = 0;
        randomValue2 = 0;
        randomValue3 = 0;
        randomValue4 = 0;
        randomValue5 = 0;
        randomValue6 = 0;

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
            if (rvi < 50)
            {
                rvi++;
                randomValue1 += increment1;
                randomValue2 += increment2;
                randomValue3 += increment3;
                randomValue4 += increment4;
                randomValue5 += increment5;
                randomValue6 += increment6;
            }


            int k = (randomValue1 * 550);
            int l = (randomValue2 * 550);
            int m = (randomValue3 * 550);
            int n = (randomValue4 * 550);
            int o = (randomValue5 * 550);
            int p = (randomValue6 * 550);

            BetAmountText_Array[0].text = $"<color=yellow>{BetAmount_Array[0]}</color><color=#FFFFFF>/{k}</color>";
            BetAmountText_Array[1].text = $"<color=yellow>{BetAmount_Array[1]}</color><color=#FFFFFF>/{l}</color>";
            BetAmountText_Array[2].text = $"<color=yellow>{BetAmount_Array[2]}</color><color=#FFFFFF>/{m}</color>";
            BetAmountText_Array[3].text = $"<color=yellow>{BetAmount_Array[3]}</color><color=#FFFFFF>/{n}</color>";
            BetAmountText_Array[4].text = $"<color=yellow>{BetAmount_Array[4]}</color><color=#FFFFFF>/{o}</color>";
            BetAmountText_Array[5].text = $"<color=yellow>{BetAmount_Array[5]}</color><color=#FFFFFF>/{p}</color>";

            yield return new WaitForSeconds(updateInterval);
        }
    }




    private IEnumerator StartTimer(float duration)
    {
        countdowntext.text = (duration).ToString();

        for (int i = 1; i <= duration; i++)
        {
            countdowntext.text = (duration - i).ToString();
            yield return new WaitForSeconds(1f);

            if (duration - i < 4)
            {
                clockTickSound.Play();

            }

        }
        countdowntext.text = "";
        // Wait for the specified duration

    }



    public void SelectNumber(int number)
    {
        if (currentGameState != GameState.Betting)
        {
            Debug.Log("Betting is not allowed at this time.");
            return;
        }
        if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < betManagerRef.betVal)
        {
            Debug.Log("Not enough balance to place this bet.");
            return;
        }

        totalBetsForNumber[number - 1] += betManagerRef.betVal;
        totalBets += betManagerRef.betVal;

        Debug.Log($"Player placed a bet on number {number} with amount {betManagerRef.betVal}");
        DeductBetAmount(betManagerRef.betVal);
    }

    private void DeductBetAmount(int amount)
    {
        if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f >= amount)
        {
            Wallet.DeductAmount(amount);
        }
        else
        {
            Debug.Log("Not enough balance.");
        }
    }






    ////////////////////////////////////////////////////////////////////////////---------------------------------------------------------------
    ///
    /// 
    public void ClearAllBets()
    {
        if (currentGameState != GameState.Betting) return;

        float totalBets = 0;
        foreach (float val in BetAmount_Array)
        {
            totalBets += val;
        }
        if (totalBets <= 0) return;

        walletAmount += totalBets;
        walletText.text = "₹" + walletAmount.ToString("F2");
        for (int i = 0; i < BetAmount_Array.Length; i++)
        {
            BetAmount_Array[i] = 0;
        }
        SocketManagerJMRef.ClearAllBets();
        ClearMyCoins();
    }

    public List<GameObject> myCoinsList;
    public Transform myCoinHolder;
    Vector3 recentTouchPos;
    void Awake()
    {
        if (Instance != this)
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
        foreach (GameObject g in myCoinsList)
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
        Debug.Log("cr: " + currentGameState);
        if (currentGameState != GameState.Betting) return;
        int val = betManagerRef.betVal;
        if (val <= walletAmount)
        {
            walletAmount -= val;
            SingleCoinSound.Play();
            PlayerPrefs.SetString("JM_roundId", SocketManagerJMRef.currentRoundId);
            walletText.text = "₹" + walletAmount.ToString("F2");
            InstantiateCoin();
            switch (betOn)
            {
                case 0:
                    //tie case
                    BetAmount_Array[betOn] += val;
                    SocketManagerJMRef.SendBetDataToServer(betOn, val);
                    //PlayerPrefs.SetInt("dt_betsOnTie", betAmountTie);
                    break;
                case 1:
                    BetAmount_Array[betOn] += val;
                    SocketManagerJMRef.SendBetDataToServer(betOn, val);
                    //PlayerPrefs.SetInt("dt_betsOnDragon", betAmountDragon);

                    break;
                case 2:
                    BetAmount_Array[betOn] += val;
                    SocketManagerJMRef.SendBetDataToServer(betOn, val);
                    //PlayerPrefs.SetInt("dt_betsOnTiger", betAmountTiger);

                    break;

                case 3:
                    //tie case
                    BetAmount_Array[betOn] += val;
                    SocketManagerJMRef.SendBetDataToServer(betOn, val);
                    //PlayerPrefs.SetInt("dt_betsOnTie", betAmountTie);
                    break;
                case 4:
                    BetAmount_Array[betOn] += val;
                    SocketManagerJMRef.SendBetDataToServer(betOn, val);
                    //PlayerPrefs.SetInt("dt_betsOnDragon", betAmountDragon);

                    break;
                case 5:
                    BetAmount_Array[betOn] += val;
                    SocketManagerJMRef.SendBetDataToServer(betOn, val);
                    //PlayerPrefs.SetInt("dt_betsOnTiger", betAmountTiger);

                    break;
            }
        }
        else
        {
            //insufficientFundsObj.SetActive(true);
        }


    }

    public void PlayEffect(AudioClip clip)
    {
        effect_AudioSource.Stop();
        effect_AudioSource.clip = clip;
        effect_AudioSource.Play();
    }
}


