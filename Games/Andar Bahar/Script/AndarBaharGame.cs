
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Card
{
    public string name;
    public Sprite image;

}
public class AndarBaharGame : MonoBehaviour
{

public static AndarBaharGame Instance;
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

    private bool isGenerating = false;
    public int rvi;


    public TextMeshProUGUI slot1;
    public TextMeshProUGUI slot2;
    public TextMeshProUGUI slot3;
    public TextMeshProUGUI slot4;
    public TextMeshProUGUI slot5;
    public TextMeshProUGUI slot6;
    public TextMeshProUGUI slot7;
    public TextMeshProUGUI slot8;
    public TextMeshProUGUI slot9;
    public TextMeshProUGUI slot10;

    public Animator[] throwItemAnimators_array;
    public GameObject waitingforNextPanel;
    public GameObject redScreen;
    public GameObject countingflipingCard;
    private int flipCount = 0;

    public GameObject andarL, baharL, bet1L, bet2L, bet3L, bet4L, bet5L, bet6L, bet7L, bet8L;
    // BurstAB burstRef;
    public TextMeshProUGUI countdownText;
    public GameObject countdownPanel;
    private bool isBetting;

    [SerializeField] List<Card>  andarCards, baharCards;
    public List<Sprite> deck;
    public Image jokerImg;
    [SerializeField] List<Image> andarCardImg, baharCardImg;
    public GameObject stopBetPanel, startBetPanel, betRoda;
    public Sprite trp, cardPNG;

    private float[] playerBets = new float[10];
    public bool userPalceBet = false;

    float totalWinnings = 0;

    public GameObject addCashPanel;


    public float walletAmount;
    public Text walletText;
    SocketManagerAB socketManagerAB;
    TableBotManager botManagerRef;
    BetManager betManagerRef;





    public GameObject showWinPanel;
    public TextMeshProUGUI showWinText;
    APIs apisRef;
    public AudioClip noMoreBets_Clip;
    public AudioClip placeYourBets_Clip;

    public AudioSource effect_AudioSource;




    void Start()
    {

        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        apisRef.FetchWallet();
        betManagerRef = FindFirstObjectByType<BetManager>();

        botManagerRef = FindObjectOfType<TableBotManager>();

        socketManagerAB = FindObjectOfType<SocketManagerAB>();

        // burstRef = FindObjectOfType<BurstAB>();
        stopBetPanel.SetActive(false);
        startBetPanel.SetActive(false);

        andarCards.Clear();
        baharCards.Clear();

    }

    public void StartBetting(float timeRem, long startTime, string[] seeds)
    {
        ResetBetsTxt();

        PlayEffect(placeYourBets_Clip);

        isBetting = true;
        waitingforNextPanel.SetActive(false);
        StartCoroutine(StartCountdown(timeRem, startTime, seeds));
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
          
        }

    }

    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + wAmount.ToString("F2");
    }

    

    IEnumerator StartCountdown(float timeRem, long startTime, string[] seeds)
    {
        betRoda.SetActive(false);
        startBetPanel.SetActive(true);

        yield return new WaitForSeconds(1f);
        startBetPanel.SetActive(false);
        BetAmountIncrease(startTime, seeds);
        float timeRemaning = timeRem;
        // StartCoroutine(burstRef.AnimStart());
        RoundStart();
        countdownPanel.SetActive(true);
        isBetting = true;

        while (timeRemaning > -1)
        {
            countdownText.text = Mathf.Ceil(timeRemaning).ToString();
            timeRemaning -= Time.deltaTime;
            yield return null;

        }
        isBetting = false;

        countdownPanel.SetActive(false);


    }


    void RoundStart()
    {
        ResetBets();
        ResetTable();

        countingflipingCard.SetActive(false);
        flipCount = 0;


        for (int i = 0; i < andarCardImg.Count; i++)
        {
            baharCardImg[i].sprite = trp;
            andarCardImg[i].sprite = trp;
        }






    }

    public void ThrowItemAnim(string token, int player, int item)
    {
        StartCoroutine(ThrowItemAnimEnum(token, player, item));
    }

    IEnumerator ThrowItemAnimEnum(string token, int player, int item)
    {
        throwItemAnimators_array[player].SetInteger("val", item);
        yield return new WaitForSeconds(0.5f);
        throwItemAnimators_array[player].SetInteger("val", 0);
    }

    public void ResetBets()
    {
        for (int i = 0; i < playerBets.Length; i++)
        {
            playerBets[i] = 0;
        }
        totalWinnings = 0;

        userPalceBet = false;
    }


    public void SetJokerCard(string jokerCardId)
    {
        // if (string.IsNullOrEmpty(jokerCardId))
        // {
        //     Debug.LogError("Joker card ID is empty!");
        //     return;
        // }
        // Invoke()
        StartCoroutine(JokerCardFlipAnm());
        Card jokerCard = new Card
        {
            name = deck.Find(card => card.name == jokerCardId).name,
            image = deck.Find(card => card.name == jokerCardId)
        };
        if (jokerCard != null)
        {


            jokerImg.sprite = jokerCard.image;
            Debug.Log("Joker card set to: " + jokerCard.name);
        }
        else
        {
            Debug.LogError("Joker card not found: " + jokerCardId);
        }
    }


    IEnumerator JokerCardFlipAnm()
    {


        jokerImg.GetComponent<Animator>().SetBool("flip", true);
        yield return new WaitForSeconds(0.2f);

        jokerImg.GetComponent<Animator>().SetBool("flip", false);
        yield return new WaitForSeconds(0.2f);
    }


    IEnumerator GameShuffle(int cardsDealt, int[] botsWinArray)
    {

        stopBetPanel.SetActive(true);
        yield return new WaitForSeconds(1f);
        stopBetPanel.SetActive(false);
        betRoda.SetActive(true);

        yield return new WaitForSeconds(1f);
        int a = 0, b = 0;

        // Ensure we do not exceed the size of the image lists
        int maxAndarCount = Mathf.Min(andarCards.Count, andarCardImg.Count);
        int maxBaharCount = Mathf.Min(baharCards.Count, baharCardImg.Count);

        countingflipingCard.SetActive(true);
        RectTransform rt = countingflipingCard.GetComponent<RectTransform>();


        float initialPositionX = -254f;
        rt.anchoredPosition = new Vector2(initialPositionX, rt.anchoredPosition.y);
        TextMeshProUGUI Text = countingflipingCard.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        Vector3 rVec1 = new Vector3(0, 90, 0);
        Vector3 rVec2 = new Vector3(0, 0, 0);
        for (int i = 0; i < Mathf.Max(andarCards.Count, baharCards.Count); i++)
        {
            if (a < maxAndarCount)
            {
                andarCardImg[a].sprite = cardPNG;
                yield return andarCardImg[a].transform.DORotate(rVec1, 0.2f)
                .SetEase(Ease.InOutSine)
                 .SetAutoKill(false)
                 .SetLoops(1, LoopType.Restart)
                .OnComplete(() =>
                {
                    andarCardImg[a].transform.DOKill();
                })
                .WaitForCompletion();

                andarCardImg[a].sprite = andarCards[a].image;

                yield return andarCardImg[a].transform.DORotate(rVec2, 0.2f)
                .SetEase(Ease.InOutSine)
                 .SetAutoKill(false)
                 .SetLoops(1, LoopType.Restart)
                .OnComplete(() =>
                {
                    andarCardImg[a].transform.DOKill();
                })
                .WaitForCompletion();

                a++;
                flipCount++;

                Text.text = flipCount.ToString();


                if (a + b > 25)
                {
                    redScreen.SetActive(true);
                }
            }


            if (b < maxBaharCount)
            {
                baharCardImg[b].sprite = cardPNG;
                yield return baharCardImg[b].transform.DORotate(rVec1, 0.2f)
                .SetEase(Ease.InOutSine)
                 .SetAutoKill(false)
                 .SetLoops(1, LoopType.Restart)
                .OnComplete(() =>
                {
                    baharCardImg[b].transform.DOKill();
                })
                .WaitForCompletion();

                baharCardImg[b].sprite = baharCards[b].image;

                yield return baharCardImg[b].transform.DORotate(rVec2, 0.2f)
                .SetEase(Ease.InOutSine)
                 .SetAutoKill(false)
                 .SetLoops(1, LoopType.Restart)
                .OnComplete(() =>
                {
                    baharCardImg[b].transform.DOKill();
                })
                .WaitForCompletion();

                b++;
                flipCount++;

                Text.text = flipCount.ToString();
                if (flipCount % 2 == 0)  // Every two flips
                {
                    // Increment position by 20 for every 2 cards flipped
                    float newPosX = initialPositionX + 20 * (flipCount / 2);
                    rt.anchoredPosition = new Vector2(newPosX, rt.anchoredPosition.y);
                }


            }
        }


        andarCards.Clear();
        baharCards.Clear();


        yield return new WaitForSeconds(1f);
        WinSystem(cardsDealt, botsWinArray);
    }

    public void UpdateGameCards(List<string> andarCards, List<string> baharCards, int cardsDealt, int[] botWinArray)
    {
        isGenerating = false;
        PlayEffect(noMoreBets_Clip);
        // burstRef.StopAnim();
        List<Card> andarCardObjects = new List<Card>();
        List<Card> baharCardObjects = new List<Card>();

        foreach (string cardName in andarCards)
        {
            Card card = new Card
            {
                name = deck.Find(c => c.name == cardName).name,
                image = deck.Find(c => c.name == cardName)
            };
            andarCardObjects.Add(card);
        }

        foreach (string cardName in baharCards)
        {
            Card card = new Card
            {
                name = deck.Find(c => c.name == cardName).name,
                image = deck.Find(c => c.name == cardName)
            };
            baharCardObjects.Add(card);

        }

        this.andarCards = andarCardObjects;
        this.baharCards = baharCardObjects;

        for (int i = 0; i < andarCardImg.Count; i++)
        {
            andarCardImg[i].sprite = trp;
            baharCardImg[i].sprite = trp;
        }

        StartCoroutine(GameShuffle(cardsDealt, botWinArray));
    }

    public void ClearAllBets()
    {
        if(!isBetting) return;
        userPalceBet = false;
        float totalBets = 0;
        foreach(float val in playerBets)
        {
            totalBets += val;
        }
        if(totalBets <= 0) return;

        walletAmount += totalBets;
        walletText.text = "₹" + walletAmount.ToString("F2");
        for(int i = 0; i< playerBets.Length; i++)
        {
            playerBets[i] = 0;
        }
        socketManagerAB.ClearAllBets();
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
         


    public void PlaceBet(int betOn)
    {
        userPalceBet = true;
        if (!isBetting)
        {
            Debug.Log("Betting is not allowed right now.");

            return;
        }


        if (walletAmount < betManagerRef.betVal)
        {
            Debug.Log("Not enough balance to place the bet.");
            addCashPanel.SetActive(true);
            return;
        }


        playerBets[betOn] += betManagerRef.betVal; // Add the bet amount to the current bet level
        InstantiateCoin();
        Debug.Log($"Player placed {betManagerRef.betVal} on level {betOn}. Total bet on this level: {playerBets[betOn]}");
        walletAmount -= betManagerRef.betVal;
        UpdateWallet(walletAmount);
        // Update the UI with the new bet amount (this part depends on your specific UI setup)

        socketManagerAB.SendBetDataToServer(betOn, betManagerRef.betVal);


    }

    public void WinSystem(int cardsShuffled, int[] botsWinArray)
    {
        bool winAndarBahar = cardsShuffled % 2 == 0; // Even numbers are Andar (0), odd numbers are Bahar (1)
        int winNum = winAndarBahar ? 1 : 0; // If winAndarBahar is true, Bahar (1) wins, else Andar (0) wins

        if (winAndarBahar)
        {
            baharL.SetActive(true);
        }
        else
        {
            andarL.SetActive(true);
        }

        // Add the win result to the history


        // Check if the player has placed any bets
        if (userPalceBet)
        {
            // Calculate winnings for Andar/Bahar


            for (int i = 0; i < playerBets.Length; i++)
            {
                if (playerBets[i] > 0) // If the player has placed a bet on Andar/Bahar
                {
                    float winnings = 0;

                    // Check for Andar/Bahar win
                    if (winNum == 0 && i == 0) // Andar wins
                    {
                        winnings = playerBets[i] * 1.9f; // 1.9x multiplier for Andar
                    }
                    else if (winNum == 1 && i == 1) // Bahar wins
                    {
                        winnings = playerBets[i] * 2f; // 2x multiplier for Bahar
                    }

                    // If the winnings are greater than 0, add them to the player's balance
                    if (winnings > 0)
                    {
                        walletAmount += winnings;
                        Debug.Log($"Player won {winnings} on Andar/Bahar level {i + 1}!");
                        totalWinnings += winnings;
                        StartCoroutine(ShowWinAmount());
                    }
                }

                // Now check for Shuffle Range Wins
                float shuffleMultiplier = CardShuffledMultiplier(cardsShuffled, i);
                if (shuffleMultiplier > 1)
                {
                    // If a shuffle multiplier was applied, the player wins with that multiplier
                    float shuffleWinnings = playerBets[i] * shuffleMultiplier;
                    walletAmount += shuffleWinnings;
                    Debug.Log($"Player won {shuffleWinnings} on Shuffle Range level {i + 1} with multiplier {shuffleMultiplier}!");
                    totalWinnings += shuffleWinnings;
                    StartCoroutine(ShowWinAmount());
                }
            }

            // Update the UI with the new balance
            Debug.Log("Total Winnings: " + totalWinnings);




        }



        // StartCoroutine(burstRef.WinEnum(winNum, CardsWinCalculator(cardsShuffled), botsWinArray));

        // Show the bet range UI based on the number of cards shuffled
        ShowBetRange(cardsShuffled);
        ClearMyCoins();
        botManagerRef.BotWin(botsWinArray);
        UpdateWallet(walletAmount);
        Invoke("ResetTable", 4);

    }

    int CardsWinCalculator(int cardsShuffled)
    {
        if (cardsShuffled <= 5)
        {
            return 0;
        }
        else if (cardsShuffled > 5 && cardsShuffled <= 10)
        {
            return 1;
        }
        else if (cardsShuffled > 10 && cardsShuffled <= 15)
        {
            return 2;
        }
        else if (cardsShuffled > 15 && cardsShuffled <= 25)
        {
            return 3;
        }
        else if (cardsShuffled > 25 && cardsShuffled <= 30)
        {
            return 4;
        }
        else if (cardsShuffled > 30 && cardsShuffled <= 35)
        {
            return 5;
        }
        else if (cardsShuffled > 35 && cardsShuffled <= 40)
        {
            return 6;
        }
        else if (cardsShuffled > 45)
        {
            return 7;
        }
        return 99;
    }

    void ResetTable()
    {
        redScreen.SetActive(false);

        andarL.SetActive(false);
        baharL.SetActive(false);
        bet1L.SetActive(false);
        bet2L.SetActive(false);
        bet3L.SetActive(false);
        bet4L.SetActive(false);
        bet5L.SetActive(false);
        bet6L.SetActive(false);
        bet7L.SetActive(false);
        bet8L.SetActive(false);

    }


    IEnumerator ShowWinAmount()
    {

        showWinPanel.SetActive(true);
        showWinText.text = totalWinnings.ToString();
        yield return new WaitForSeconds(2f);
        showWinPanel.SetActive(false);


    }




    private void ShowBetRange(int cardsShuffled)
    {

        if (cardsShuffled <= 5)
        {
            bet1L.SetActive(true);
        }
        else if (cardsShuffled > 5 && cardsShuffled <= 10)
        {
            bet2L.SetActive(true);
        }
        else if (cardsShuffled > 10 && cardsShuffled <= 15)
        {
            bet3L.SetActive(true);
        }
        else if (cardsShuffled > 15 && cardsShuffled <= 25)
        {
            bet4L.SetActive(true);
        }
        else if (cardsShuffled > 25 && cardsShuffled <= 30)
        {
            bet5L.SetActive(true);
        }
        else if (cardsShuffled > 30 && cardsShuffled <= 35)
        {
            bet6L.SetActive(true);
        }
        else if (cardsShuffled > 35 && cardsShuffled <= 40)
        {
            bet7L.SetActive(true);
        }
        else if (cardsShuffled > 45)
        {
            bet8L.SetActive(true);
        }
    }

    private float CardShuffledMultiplier(int cardsShuffled, int betOn)
    {
        // For Shuffle Range bets, check if the bet level matches the shuffled card range
        if (playerBets[betOn] > 0)
        {
            if (cardsShuffled >= 1 && cardsShuffled <= 5 && betOn == 2)
                return 3.5f; // 1-5 cards shuffled
            else if (cardsShuffled >= 6 && cardsShuffled <= 10 && betOn == 3)
                return 4.5f; // 6-10 cards shuffled
            else if (cardsShuffled >= 11 && cardsShuffled <= 15 && betOn == 4)
                return 5.5f; // 11-15 cards shuffled
            else if (cardsShuffled >= 16 && cardsShuffled <= 25 && betOn == 5)
                return 4.5f; // 16-20 cards shuffled
            else if (cardsShuffled >= 26 && cardsShuffled <= 30 && betOn == 6)
                return 15f; // 21-25 cards shuffled
            else if (cardsShuffled >= 31 && cardsShuffled <= 35 && betOn == 7)
                return 25f; // 26-30 cards shuffled
            else if (cardsShuffled >= 36 && cardsShuffled <= 40 && betOn == 8)
                return 50f; // 31-35 cards shuffled
            else if (cardsShuffled > 41 && betOn == 9)
                return 120f; // 36+ cards shuffled
        }

        return 1f; // Default multiplier
    }



    public void EnableWaitingPanel()
    {

        waitingforNextPanel.SetActive(true);

    }
  
    public void PlayEffect(AudioClip clip)
    {
        Debug.Log("clip " + clip);
        effect_AudioSource.Stop();
        effect_AudioSource.clip = clip;
        effect_AudioSource.Play();
    }


    void ResetBetsTxt()
    {
        Debug.Log("bets clear");
        slot1.text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        slot2.text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        slot3.text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        slot4.text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        slot5.text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        slot6.text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        slot7.text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        slot8.text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        slot9.text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        slot10.text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
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


            int k = (randomValue1 * 440);
            int l = (randomValue2 * 440);
            int m = (randomValue3 * 50);
            int n = (randomValue4 * 50);
            int o = (randomValue5 * 50);
            int p = (randomValue6 * 50);
            int q = (randomValue7 * 50);
            int r = (randomValue8 * 50);
            int s = (randomValue8 * 50);
            int t = (randomValue8 * 50);


            slot1.text = $"<color=yellow>{playerBets[0]}</color><color=#02ccfe>/{k}</color>";
            slot2.text = $"<color=yellow>{playerBets[1]}</color><color=#02ccfe>/{l}</color>";
            slot3.text = $"<color=yellow>{playerBets[2]}</color><color=#02ccfe>/{m}</color>";
            slot4.text = $"<color=yellow>{playerBets[3]}</color><color=#02ccfe>/{n}</color>";
            slot5.text = $"<color=yellow>{playerBets[4]}</color><color=#02ccfe>/{o}</color>";
            slot6.text = $"<color=yellow>{playerBets[5]}</color><color=#02ccfe>/{p}</color>";
            slot7.text = $"<color=yellow>{playerBets[6]}</color><color=#02ccfe>/{q}</color>";
            slot8.text = $"<color=yellow>{playerBets[7]}</color><color=#02ccfe>/{r}</color>";
            slot9.text = $"<color=yellow>{playerBets[8]}</color><color=#02ccfe>/{s}</color>";
            slot10.text = $"<color=yellow>{playerBets[9]}</color><color=#02ccfe>/{t}</color>";


            yield return new WaitForSeconds(updateInterval);
        }
    }




}



