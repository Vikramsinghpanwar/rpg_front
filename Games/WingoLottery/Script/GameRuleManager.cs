using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

public class GameRuleManager : MonoBehaviour
{
        public static GameRuleManager Instance;

    private System.Random random1;
    private System.Random random2;
    private System.Random random3;
    private System.Random random4;
    private System.Random random5;
    private System.Random random6;
    private System.Random random7;
    private System.Random random8;
    private System.Random random9;
    private System.Random random10;
    private System.Random random11;
    private System.Random random12;
    private System.Random random13;


    private float updateInterval = 0.5f; // 5 times per second
    private int randomValue1 = 0;
    private int randomValue2 = 0;
    private int randomValue3 = 0;
    private int randomValue4 = 0;
    private int randomValue5 = 0;
    private int randomValue6 = 0;
    private int randomValue7 = 0;
    private int randomValue8 = 0;
    private int randomValue9 = 0;
    private int randomValue10 = 0;
    private int randomValue11 = 0;
    private int randomValue12 = 0;
    private int randomValue13 = 0;

    private bool isGenerating = false;
    public int rvi;

    public Animator animator;
    public Animator[] throwItemAnimators_array;
    public GameObject bet0, bet1, bet2, bet3, bet4, bet5, bet6, bet7, bet8, bet9, betGreen, betVoilet, betRed;
    public TextMeshProUGUI countdownText;

    public TextMeshProUGUI showWinText;
    public TextMeshProUGUI walletText;
    public float walletAmount;
    public GameObject waitForNext, showWinPanel, betStart, betStop, addCashPanel; // Reference to the MoneyManager
    
    TableBotManager botManagerRef;


    //BurstWJ burstWJ;
    private RandomHistoryWL randomHistoryWL;

    private bool bettingAllow = false;

    private int[] totalBets_Array = new int[13];  // Array to store bet amounts for each number
    float totalWinnings = 0;

    public TextMeshProUGUI[] betAmountText;

    SocketManagerWL socketManagerWL;
    LotteryBallAnimation lotteryBallAnimation;

    public Image resBall1_Img;
    public Image resBall2_Img;
    public Sprite[] ballImages_Array;
    APIs apisRef;
    BetManager betManagerRef;


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

    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
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


    void Start()
    {
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        apisRef.FetchWallet();
        betManagerRef = FindObjectOfType<BetManager>();

        // burstWJ = FindObjectOfType<BurstWJ>();
        randomHistoryWL = FindObjectOfType<RandomHistoryWL>();
        socketManagerWL = FindObjectOfType<SocketManagerWL>();
        lotteryBallAnimation = FindObjectOfType<LotteryBallAnimation>();
        botManagerRef = FindObjectOfType<TableBotManager>();
    }






    IEnumerator GameLoop(float resultTime, long serverStartTime, string[] seeds)
    {
        ResetGame();

        bet0.SetActive(false);
        bet1.SetActive(false);
        bet2.SetActive(false);
        bet3.SetActive(false);
        bet4.SetActive(false);
        bet5.SetActive(false);
        bet6.SetActive(false);
        bet7.SetActive(false);
        bet8.SetActive(false);
        bet9.SetActive(false);
        betGreen.SetActive(false);
        betRed.SetActive(false);
        betVoilet.SetActive(false);

        StartCoroutine(StatrBet(serverStartTime, seeds));
        yield return StartCoroutine(StartCountdown(resultTime));  // Fixed betting time
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
        randomValue9 = 0;
        randomValue10 = 0;
        randomValue11 = 0;
        randomValue12 = 0;
        randomValue13 = 0;
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
        random9 = new System.Random(GenerateConsistentHash(seeds[8]));
        random10 = new System.Random(GenerateConsistentHash(seeds[9]));
        random11 = new System.Random(GenerateConsistentHash(seeds[10]));
        random12 = new System.Random(GenerateConsistentHash(seeds[11]));
        random13 = new System.Random(GenerateConsistentHash(seeds[12]));


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
                randomValue9 += random8.Next(1, 10);
                randomValue10 += random8.Next(1, 10);

                randomValue11 += 7 * random8.Next(1, 10);
                randomValue12 += 4 * random8.Next(1, 10);
                randomValue13 += 7 * random8.Next(1, 10);

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
            int increment9 = random9.Next(1, 10);
            int increment10 = random10.Next(1, 10);
            int increment11 = random11.Next(1, 10);
            int increment12 = random12.Next(1, 10);
            int increment13 = random13.Next(1, 10);


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
                randomValue9 += increment9;
                randomValue10 += increment10;
                randomValue11 += increment11;
                randomValue12 += increment12;
                randomValue13 += increment13;

            }

            int k = (randomValue1 * 10);
            int l = (randomValue2 * 20);
            int m = (randomValue3 * 30);
            int n = (randomValue4 * 40);
            int o = (randomValue5 * 50);
            int p = (randomValue6 * 50);
            int q = (randomValue7 * 50);
            int r = (randomValue8 * 50);
            int s = (randomValue9 * 50);
            int t = (randomValue10 * 50);
            int u = (randomValue11 * 50);
            int v = (randomValue12 * 50);
            int w = (randomValue13 * 50);


            betAmountText[0].text = $"<color=yellow>{totalBets_Array[0]}</color><color=#02ccfe>/{k}</color>";
            betAmountText[1].text = $"<color=yellow>{totalBets_Array[1]}</color><color=#02ccfe>/{l}</color>";
            betAmountText[2].text = $"<color=yellow>{totalBets_Array[2]}</color><color=#02ccfe>/{m}</color>";
            betAmountText[3].text = $"<color=yellow>{totalBets_Array[3]}</color><color=#02ccfe>/{n}</color>";
            betAmountText[4].text = $"<color=yellow>{totalBets_Array[4]}</color><color=#02ccfe>/{o}</color>";
            betAmountText[5].text = $"<color=yellow>{totalBets_Array[5]}</color><color=#02ccfe>/{p}</color>";
            betAmountText[6].text = $"<color=yellow>{totalBets_Array[6]}</color><color=#02ccfe>/{q}</color>";
            betAmountText[7].text = $"<color=yellow>{totalBets_Array[7]}</color><color=#02ccfe>/{r}</color>";
            betAmountText[8].text = $"<color=yellow>{totalBets_Array[8]}</color><color=#02ccfe>/{s}</color>";
            betAmountText[9].text = $"<color=yellow>{totalBets_Array[9]}</color><color=#02ccfe>/{t}</color>";
            betAmountText[10].text = $"<color=yellow>{totalBets_Array[10]}</color><color=#02ccfe>/{u}</color>";
            betAmountText[11].text = $"<color=yellow>{totalBets_Array[11]}</color><color=#02ccfe>/{v}</color>";
            betAmountText[12].text = $"<color=yellow>{totalBets_Array[12]}</color><color=#02ccfe>/{w}</color>";


            yield return new WaitForSeconds(updateInterval);
        }
    }




    IEnumerator StartCountdown(float remainingTime)
    {
        bettingAllow = true;
        float timeRemaining = remainingTime;
        // StartCoroutine(burstWJ.AnimStart());
        while (timeRemaining > 0)
        {
            countdownText.text = Mathf.Ceil(timeRemaining).ToString("");
            timeRemaining -= Time.deltaTime;
            yield return null;
        }


        countdownText.text = "0";
        bettingAllow = false;
        StartCoroutine(StopBet());

    }

    IEnumerator StatrBet(long serverStartTime, string[] seeds)
    {
        betStart.SetActive(true);
        yield return new WaitForSeconds(1f);
        betStart.SetActive(false);
        BetAmountIncrease(serverStartTime, seeds);

    }
    IEnumerator StopBet()
    {
        betStop.SetActive(true);
        yield return new WaitForSeconds(1f);
        betStop.SetActive(false);
    }




    public void ClearAllBets()
    {
        Debug.Log("radhey");
        int totalBet = 0;
        for (int i = 0; i< totalBets_Array.Length; i++)
        {
            totalBet += totalBets_Array[i]; 
        }
        if(totalBet <= 0) return;
        if(!bettingAllow) return;
        
        walletAmount += totalBet;
        walletText.text = "₹" + walletAmount.ToString("F2");

 for (int i = 0; i< totalBets_Array.Length; i++)
        {
            totalBets_Array[i] = 0; 
        }
        
                socketManagerWL.ClearAllBets();
        ClearMyCoins();
    }

    public List<GameObject> myCoinsList;
    public Transform myCoinHolder;
        Vector3 recentTouchPos;


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


    public void SelectNumber(int number)
    {
        if (!bettingAllow)
        {
            Debug.Log("Betting is not allowed at this time.");
            return;
        }

        if (walletAmount < betManagerRef.betVal)
        {
            Debug.Log("Not enough balance to place this bet.");
            addCashPanel.SetActive(true);
            return;
        }

        walletAmount -= betManagerRef.betVal;
        UpdateWallet(walletAmount);
        totalBets_Array[number] += betManagerRef.betVal;
        Debug.Log("Total bets on number " + number + ": " + totalBets_Array[number]);
            InstantiateCoin();

        DeductBetAmount(betManagerRef.betVal);
        socketManagerWL.SendBetDataToServer(number, betManagerRef.betVal);

    }


    private void DeductBetAmount(int amount)
    {
        if (walletAmount >= amount)
        {
            Wallet.DeductAmount(amount);
        }
        else
        {
            Debug.Log("Not enough balance.");
        }
    }

    private void WinSystem(int winner)
    {

        if (winner == 0)
        {
            bet0.SetActive(true);
            betRed.SetActive(true);
            betVoilet.SetActive(true);

        }
        else if (winner == 1)
        {
            bet1.SetActive(true);
            betGreen.SetActive(true);
        }
        else if (winner == 2)
        {
            bet2.SetActive(true);
            betRed.SetActive(true);
        }
        else if (winner == 3)
        {
            bet3.SetActive(true);
            betGreen.SetActive(true);
        }
        else if (winner == 4)
        {
            bet4.SetActive(true);
            betRed.SetActive(true);

        }
        else if (winner == 5)
        {
            bet5.SetActive(true);
            betGreen.SetActive(true);
            betVoilet.SetActive(true);

        }
        else if (winner == 6)
        {
            bet6.SetActive(true);
            betRed.SetActive(true);

        }
        else if (winner == 7)
        {
            bet7.SetActive(true);
            betGreen.SetActive(true);
        }
        else if (winner == 8)
        {
            bet8.SetActive(true);
            betRed.SetActive(true);

        }
        else if (winner == 9)
        {
            bet9.SetActive(true);
            betGreen.SetActive(true);
        }
               ClearMyCoins();


        UserWinnigAmount(winner);

    }

    private void UserWinnigAmount(int winner)
    {
        totalWinnings = 0;  // Reset total winnings at the start of the function

        // Check if the player has placed a bet on the selected winning number
        if (totalBets_Array[winner] > 0)
        {
            Debug.Log("You win on number " + winner + "!");
            int winnings = totalBets_Array[winner] * 9;  // Select Number win (9x multiplier)
            totalWinnings += winnings;  // Add winnings to total

        }
        if (totalBets_Array[10] > 0 || totalBets_Array[11] > 0 || totalBets_Array[12] > 0)
        {
            List<int> redArray = new List<int> { 2, 4, 6, 8 };
            List<int> greenArray = new List<int> { 1, 3, 7, 9 };

            if (redArray.Contains(winner))
            {
                //red wins
                totalWinnings += totalBets_Array[12] * 2;
            }
            else if (greenArray.Contains(winner))
            {
                totalWinnings += totalBets_Array[10] * 2;
            }
            else if ( winner == 0) totalWinnings += totalBets_Array[12] * 1.5f;
            else if (winner == 5) totalWinnings += totalBets_Array[10] * 1.5f;

        }

Debug.Log("total user winning:  " + totalWinnings);
        if (totalWinnings > 0)
        {
            UpdateWallet(walletAmount + totalWinnings);
            Wallet.AddToWinWallet(totalWinnings);
            StartCoroutine(ShowWinAmount());
        }

        CheckBonusNumbers(winner);
    }

    IEnumerator ShowWinAmount()
    {
        showWinPanel.SetActive(true);
        showWinText.text = totalWinnings.ToString();  // Display total winnings
        yield return new WaitForSeconds(2f);
        showWinPanel.SetActive(false);
    }

    private void CheckBonusNumbers(int winner)
    {
        // JOIN RED: Number 10 wins if the result is 1, 3, 7, or 9 (2x multiplier)
        if (IsBonusForNumber(winner, new int[] { 1, 3, 7, 9 }) && totalBets_Array[10] > 0)
        {
            int bonusWinnings = totalBets_Array[10] * 2;  // 2x multiplier for number 10
            totalWinnings += bonusWinnings;
            Wallet.AddToWinWallet(bonusWinnings);
        }

        if (IsBonusForNumber(winner, new int[] { 0, 5 }) && totalBets_Array[11] > 0)
        {
            int bonusWinnings = Mathf.RoundToInt(totalBets_Array[11] * 4.5f);  // 4.5x multiplier for number 11
            totalWinnings += bonusWinnings;
            Wallet.AddToWinWallet(bonusWinnings);
        }

        // JOIN RED: Number 12 wins if the result is 2, 4, 6, or 8 (2x multiplier)
        if (IsBonusForNumber(winner, new int[] { 2, 4, 6, 8 }) && totalBets_Array[12] > 0)
        {
            int bonusWinnings = totalBets_Array[12] * 2;  // 2x multiplier for number 12
            totalWinnings += bonusWinnings;
            Wallet.AddToWinWallet(bonusWinnings);
        }

        // Additional special win conditions based on the result
        if (winner == 0)
        {
            // JOIN VIOLET: Win if result is 0 (1.5x multiplier for number 0)
            if (totalBets_Array[0] > 0)
            {
                int bonusWinnings = Mathf.RoundToInt(totalBets_Array[0] * 1.5f);  // 1.5x multiplier for number 0
                totalWinnings += bonusWinnings;
                Wallet.AddToWinWallet(bonusWinnings);
            }
        }
        else if (winner == 5)
        {
            // JOIN VIOLET: Win if result is 5 (1.5x multiplier for number 5)
            if (totalBets_Array[5] > 0)
            {
                int bonusWinnings = Mathf.RoundToInt(totalBets_Array[5] * 1.5f);  // 1.5x multiplier for number 5
                totalWinnings += bonusWinnings;
                Wallet.AddToWinWallet(bonusWinnings);
            }
        }
    }

    private bool IsBonusForNumber(int winningNumber, int[] validBonusNumbers)
    {
        foreach (int validNumber in validBonusNumbers)
        {
            if (winningNumber == validNumber)
                return true;
        }
        return false;
    }

    public void GameResult(int result)
    {
        isGenerating = false;
        // burstWJ.StopAnim();
        StartCoroutine(MachineAnimation(result));
    }

    public IEnumerator MachineAnimation(int result)
    {
        resBall1_Img.sprite = ballImages_Array[result];
        resBall2_Img.sprite = ballImages_Array[result];

        ResetAnimatorBools();

        animator.SetBool("isScalingUp", true);
        yield return new WaitForSeconds(3f);

        animator.SetBool("isScalingUp", false);

        StartCoroutine(lotteryBallAnimation.RandomBallMovement());
        yield return new WaitForSeconds(4f);

        animator.SetBool("isWinBall", true);
        yield return new WaitForSeconds(1f);

        animator.SetBool("isWinBall", false);
        animator.SetBool("appearBall", true);
        yield return new WaitForSeconds(3f);
        animator.SetBool("appearBall", false);
        animator.SetBool("isScalingBack", true);

        WinSystem(result);
        randomHistoryWL.AddSingleHistoryResult(result);

        TableBotManager.Instance.ClearBets();

        // burstWJ.MoveAllcoinsBack();

    }

    private void ResetAnimatorBools()
    {
        animator.SetBool("isScalingUp", false);
        animator.SetBool("isWinBall", false);
        animator.SetBool("appearBall", false);
        animator.SetBool("isScalingBack", false);
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

    void ResetGame()
    {
        for (int i = 0; i < totalBets_Array.Length; i++)
        {
            totalBets_Array[i] = 0;
            betAmountText[i].text = "0";
        }
        totalWinnings = 0;
    }

    public void EnableWaitingPanel()
    {
        if (!waitForNext.activeSelf)
        {
            waitForNext.SetActive(true);
        }


    }

    public void StartBetting(float remainingTime, long roundStartTime, string[] seeds)
    {

        if (waitForNext.activeSelf)
        {
            waitForNext.SetActive(false);
        }


        StartCoroutine(GameLoop(remainingTime, roundStartTime, seeds));  // Use dynamic remaining time
    }



}








