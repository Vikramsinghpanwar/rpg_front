using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using DG.Tweening;
using System.Security.Cryptography;
using System.Text;
using System;
using Features.Lobby.Integration;

public class BettingSystem : MonoBehaviour
{

    private System.Random random1;
    private System.Random random2;
    private System.Random random3;
    private System.Random random4;
    private System.Random random5;
    private System.Random random6;
    private System.Random random7;
    private System.Random random8;


    private float updateInterval = 0.5f; // 2 times per second
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


    public GameObject setWin, pureSeqWin, seqWin, colorWin, pairWin, highCardWin;
    public GameObject[] glowCard;
    public GameObject[] betNotAllow;
    public TextMeshProUGUI countdownText;
    public int randomResult;

    public List<Sprite> cardSprites;
    public Image[] cardImage; // show result card 1
    public Sprite reteunCardSprite;
    public GameObject betStop, betAgain;

    private int[] totalBetsForNumber = new int[6];  // Array to store bet amounts for each number
    public TextMeshProUGUI[] otherPeopleBets_Txt_Array;  // Array to store bet amounts for each number
    private int totalBets = 0;
    private int totalWinnings = 0;

    public GameObject waitForNext, addEnoughCash;
    public GameObject showWinPanel, newHistorySpawn;
    public TextMeshProUGUI winAmountText;
    private bool bettingAllow = false;

    public TextMeshProUGUI[] betAmountText;
    SocketManagerJackpot socketManagerJackpot;
    float walletAmount;
    public Text walletText;
    APIs apisRef;
    BetManager betManagerRef;

    public int[] payoutArray;

    void Start()
    {
        betManagerRef = FindObjectOfType<BetManager>();
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        apisRef.FetchWallet();
        socketManagerJackpot = FindObjectOfType<SocketManagerJackpot>();
    }

    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }

    IEnumerator RoundStart(float timeRem)
    {
        ResetBets();
        setWin.SetActive(false);
        pureSeqWin.SetActive(false);
        seqWin.SetActive(false);
        colorWin.SetActive(false);
        pairWin.SetActive(false);
        highCardWin.SetActive(false);
        newHistorySpawn.SetActive(false);

        for (int i = 0; i < cardImage.Length; i++)
        {
            cardImage[i].sprite = reteunCardSprite;
        }
        foreach (var card in glowCard)
        {
            card.SetActive(false);
        }

        foreach (var card in betNotAllow)
        {
            card.SetActive(false);
        }
        StartCoroutine(StatrBet());
        yield return StartCoroutine(StartCountdown(timeRem));
    }

    IEnumerator StatrBet()
    {
        betAgain.SetActive(true);
        yield return new WaitForSeconds(1f);
        betAgain.SetActive(false);
    }
    IEnumerator StopBet()
    {
        betStop.SetActive(true);
        yield return new WaitForSeconds(1f);
        betStop.SetActive(false);
    }

    IEnumerator StartCountdown(float timeRemaining)
    {
        bettingAllow = true;



        while (timeRemaining > 0)
        {

            countdownText.text = Mathf.Ceil(timeRemaining).ToString("00") + "s";
            timeRemaining -= Time.deltaTime;
            yield return null;

        }
        countdownText.text = "00";

        bettingAllow = false;
        StartCoroutine(StopBet());



    }

    public void SelectNumber(int number)
    {
        if (!bettingAllow)
        {
            return;
        }
        if (walletAmount < betManagerRef.betVal)
        {
            addEnoughCash.SetActive(true);
            return;
        }

        totalBetsForNumber[number] += betManagerRef.betVal;
        totalBets += betManagerRef.betVal;

        DeductBetAmount(betManagerRef.betVal);
        betAmountText[number].text = totalBetsForNumber[number].ToString();
        socketManagerJackpot.SendBetDataToServer(number, betManagerRef.betVal, int.Parse(BootstrapLobbyAdapter.GetUserId()));
    }

    public void ClearAllBets()
    {
        int total = 0;
        for (int i = 0; i < totalBetsForNumber.Length; i++)
        {
            total += totalBetsForNumber[i];
            totalBetsForNumber[i] = 0;
            betAmountText[i].text = totalBetsForNumber[i].ToString();
        }

        walletAmount += total;
        UpdateWallet(walletAmount);
        socketManagerJackpot.ClearAllBets();

    }



    private void UpdateBetAmountTextForAllNumbers()
    {
        for (int i = 0; i < betAmountText.Length; i++)
        {
            betAmountText[i].text = "0"; // Reset bet amount to 0 for all numbers
        }
    }

    private void DeductBetAmount(int amount)
    {
        if (walletAmount >= amount)
        {
            walletAmount -= amount;
            UpdateWallet(walletAmount);
        }
        else
        {
            Debug.Log("Not enough balance.");
        }
    }

    public void ShowWinner(int winner)
    {


        Debug.Log("Winner: " + winner);

        randomResult = winner;

        foreach (var card in glowCard)
        {
            card.SetActive(false);
        }

        // Activate the glow card for the winning number
        if (winner >= 1 && winner <= glowCard.Length)
        {
            glowCard[winner].SetActive(true);  // Winner number should be 1-based index, hence winner-1
        }


        foreach (var card in betNotAllow)
        {
            card.SetActive(true);
        }

        if (winner == 0) setWin.SetActive(true);
        else if (winner == 1) pureSeqWin.SetActive(true);
        else if (winner == 2) seqWin.SetActive(true);
        else if (winner == 3) colorWin.SetActive(true);
        else if (winner == 4) pairWin.SetActive(true);
        else if (winner == 5) highCardWin.SetActive(true);

        if (totalBetsForNumber[winner] > 0)
        {
            youWinPanel.SetActive(true);
            Invoke("DisableYouWin", 3f);
            youWinTxt.text = (totalBetsForNumber[winner] * payoutArray[winner]).ToString("F2");
        }
        newHistorySpawn.SetActive(true);
    }

    public void DisableYouWin()
    {
        youWinPanel.SetActive(false);
    }

    public GameObject youWinPanel;
    public TextMeshProUGUI youWinTxt;

    public void ShowCards(List<string> cards)
    {
        Debug.Log("Cards: " + string.Join(", ", cards));
        isGenerating = false;
        // Assign card sprites
        for (int i = 0; i < cards.Count && i < cardImage.Length; i++)
        {
            string cardName = cards[i];
            Sprite cardSprite = cardSprites.FirstOrDefault(sprite => sprite.name == cardName);

            if (cardSprite != null)
            {
                cardImage[i].sprite = cardSprite;
            }
            else
            {
                Debug.LogWarning($"Card sprite not found for {cardName}");
            }
        }

        ShowResult();
    }

    public void ShowResult()
    {
        totalWinnings = 0;
        Sequence cardFlipSequence = DOTween.Sequence();
        foreach (var cardObj in cardImage)
        {
            cardObj.transform.DORotate(new Vector3(0, 180, 0), 0.15f, RotateMode.Fast)
                .SetEase(Ease.InOutSine);
        }

        cardFlipSequence.AppendCallback(() =>
        {
            foreach (var cardObj in cardImage)
            {
                cardObj.transform.DORotate(new Vector3(0, 0, 0), 0.15f, RotateMode.Fast)
                    .SetEase(Ease.InOutSine);
            }
        });

        cardFlipSequence.Play();

        for (int i = 0; i < totalBetsForNumber.Length; i++)
        {
            if (totalBetsForNumber[i] > 0)
            {
                int betNumber = i + 1;
                if (betNumber == randomResult)
                {
                    Debug.Log("You win!");
                    int payoutMultiplier = GetPayoutMultiplier(randomResult);
                    int winnings = totalBetsForNumber[i] * payoutMultiplier;
                    totalWinnings += winnings;  // Add winnings to total
                    walletAmount += winnings;
                    Debug.Log($"You won {winnings}. New balance: {walletAmount}");
                    StartCoroutine(ShowWinAmount());
                    Debug.Log("Total Winnings: " + totalWinnings);
                }
                else
                {
                    Debug.Log($"You lose on number {betNumber}.");
                }
            }
        }

        UpdateWallet(walletAmount);
    }

    IEnumerator ShowWinAmount()
    {
        showWinPanel.SetActive(true);
        winAmountText.text = totalWinnings.ToString();
        yield return new WaitForSeconds(3f);
        showWinPanel.SetActive(false);

    }

    private int GetPayoutMultiplier(int result)
    {
        switch (result)
        {
            case 1: return 2;
            case 2: return 10;
            case 3: return 6;
            case 4: return 5;
            case 5: return 4;
            case 6: return 3;
            default: return 0;
        }
    }

    void ResetBets()
    {
        for (int i = 0; i < totalBetsForNumber.Length; i++)
        {
            totalBetsForNumber[i] = 0;
        }
        totalBets = 0;
        totalWinnings = 0;
        UpdateBetAmountTextForAllNumbers();
    }

    public void EnableWaitingPanel()
    {
        waitForNext.SetActive(true);
    }

    public void StartBetting(float remainingTime, long roundStartTime, string[] seeds)
    {
        waitForNext.SetActive(false);
        StartCoroutine(RoundStart(remainingTime));

        BetAmountIncrease(roundStartTime, seeds);

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
        //random7 = new System.Random(GenerateConsistentHash(seeds[6]));

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
                //randomValue7 += random7.Next(1, 10);
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
            //int increment7 = random7.Next(1, 10);
            if (rvi < 37)
            {
                rvi++;
                randomValue1 += increment1;
                randomValue2 += increment2;
                randomValue3 += increment3;
                randomValue4 += increment4;
                randomValue5 += increment5;
                randomValue6 += increment6;
                //randomValue7 += increment7;
            }


            int k1 = (randomValue1 * 10);
            int l1 = (randomValue2 * 10);
            int m1 = (randomValue3 * 10);
            int n1 = (randomValue4 * 10);
            int o1 = ((int)(randomValue5 / 5) * 10);
            int p1 = (randomValue6 * 30);
            //int q1 = (randomValue3 * 30);

            otherPeopleBets_Txt_Array[0].text = k1.ToString();
            otherPeopleBets_Txt_Array[1].text = l1.ToString();
            otherPeopleBets_Txt_Array[2].text = m1.ToString();
            otherPeopleBets_Txt_Array[3].text = n1.ToString();
            otherPeopleBets_Txt_Array[4].text = o1.ToString();
            otherPeopleBets_Txt_Array[5].text = p1.ToString();
            //otherPeopleBets_Txt_Array[0].text = q1.ToString();

            yield return new WaitForSeconds(updateInterval);
        }
    }



}
