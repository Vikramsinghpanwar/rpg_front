using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Text;
using Features.Lobby.Integration;

public class BestOf5 : MonoBehaviour
{
    public Animator[] throwItemAnimators_array;


    private System.Random random1;
    private System.Random random2;
    private System.Random random3;
    private System.Random random4;

    private float updateInterval = 0.5f;
    private int randomValue1 = 0;
    private int randomValue2 = 0;
    private int randomValue3 = 0;
    private int randomValue4 = 0;

    private bool isGenerating = false;
    public int rvi;


    public Text walletText;
    public float walletAmount;
    public TextMeshProUGUI player1_Amount_Display_Text;
    public TextMeshProUGUI player2_Amount_Display_Text;
    public TextMeshProUGUI player3_Amount_Display_Text;
    public TextMeshProUGUI player4_Amount_Display_Text;
    public TextMeshProUGUI timer_text;
    TrendGeneratorBestOf5 historyGeneratorRef;
    CountDown321 countDownRef;
    public LuckyPlayer luckyPlayerRef;
    public GameObject winPanel;
    public Text winPanelText;
    public AudioSource winAudio;
    public AudioSource clockTickSound;
    Jugad dbRef;
    long periodId;
    public TextMeshProUGUI[] otherPlayerBets_Txt_Array;

    public int returnMultipier_pair = 3;
    public int returnMultipier_color = 5;
    public int returnMultipier_normalRun = 6;
    public int returnMultipier_straightRun = 8;
    public int returnMultipier_trio = 10;
    public int returnMultipier_highCardBig = 2;
    public int returnMultipier_highCardSmall = 1;
    public Distributor distributorRef;
    BetChecker betCheckerRef;
    public GameObject win1Img;
    public GameObject win2Img;
    public GameObject win3Img;
    public GameObject win4Img;

    public Sprite cardBgSprite;
    public List<Sprite> deck;
    public List<Sprite> gameDeck;

    public List<Image> dealerCards;

    public List<Image> banker1Cards;
    public List<Image> banker2Cards;
    public List<Image> banker3Cards;
    public List<Image> banker4Cards;

    public GameObject betStartObj;
    public GameObject betStopObj;

    public TextMeshProUGUI dealerCardsStatus;
    public TextMeshProUGUI Banker1CardsStatus;
    public TextMeshProUGUI Banker2CardsStatus;
    public TextMeshProUGUI Banker3CardsStatus;
    public TextMeshProUGUI Banker4CardsStatus;

    public BurstBo5 burstRef;
    public GameObject betRoda;
    public GameObject waitingPanel;

    int dealerCardsVal = 0;
    int banker1CardsVal = 0;
    int banker2CardsVal = 0;
    int banker3CardsVal = 0;
    int banker4CardsVal = 0;
    APIs apisRef;

    Sprite[] dealerCardsSprite = new Sprite[5];
    Sprite[] banker1CardsSprite = new Sprite[5];
    Sprite[] banker2CardsSprite = new Sprite[5];
    Sprite[] banker3CardsSprite = new Sprite[5];
    Sprite[] banker4CardsSprite = new Sprite[5];

    TableBotManager botManagerRef;
    public enum gameStateE
    {
        Betting,
        Result,
    };
    public gameStateE gameState;
    #region socket functions

    public void EnableWaitingPanel()
    {
        waitingPanel.SetActive(true);
    }

    public void StartBetting(float remTime, long roundStartTime, string[] seeds)
    {
        botManagerRef.Reset();
        StartCoroutine(StartTimer(remTime - 1f));
        gameState = gameStateE.Betting;
        Debug.Log("Betting Started");
        waitingPanel.SetActive(false);
        StartCoroutine(GameRound(remTime, roundStartTime, seeds));
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

    void BetAmountIncrease(long startTime, string[] seeds)
    {
        randomValue1 = 0;
        randomValue2 = 0;
        randomValue3 = 0;
        randomValue4 = 0;


        rvi = 0;


        long currentTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float elapsedTimeInSeconds = (currentTimestamp - startTime) / 1000f;

        // Initialize the random generators with consistent hashes
        random1 = new System.Random(GenerateConsistentHash(seeds[0]));
        random2 = new System.Random(GenerateConsistentHash(seeds[1]));
        random3 = new System.Random(GenerateConsistentHash(seeds[2]));

        random4 = new System.Random(GenerateConsistentHash(seeds[3]));


        Debug.Log("111111111111111111" + random1.Next());

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

            if (rvi < 37)
            {
                rvi++;
                randomValue1 += increment1;
                randomValue2 += increment2;
                randomValue3 += increment3;
                randomValue4 += increment4;

            }


            int k = (randomValue1 * 770);
            int l = (randomValue2 * 770);
            int m = (randomValue3 * 770);
            int n = (randomValue4 * 770);


            otherPlayerBets_Txt_Array[0].text = "₹" + k;
            otherPlayerBets_Txt_Array[1].text = "₹" + l;
            otherPlayerBets_Txt_Array[2].text = "₹" + m;
            otherPlayerBets_Txt_Array[3].text = "₹" + n;


            yield return new WaitForSeconds(updateInterval);
        }
    }


    void ResetBetAmount()
    {
        otherPlayerBets_Txt_Array[0].text = "₹" + "0";
        otherPlayerBets_Txt_Array[1].text = "₹" + "0";
        otherPlayerBets_Txt_Array[2].text = "₹" + "0";
        otherPlayerBets_Txt_Array[3].text = "₹" + "0";
    }
    public void ShowResult(int[] winner)
    {
        isGenerating = false;
        gameState = gameStateE.Result;
        StartCoroutine(Result(winner));
    }


    public void Banker_Players_CardArray_Update(string[][] nameArray)
    {

        for (int i = 0; i < nameArray[0].Length; i++)
        {
            dealerCardsSprite[i] = GetSpriteByName(nameArray[0][i]);
            banker1CardsSprite[i] = GetSpriteByName(nameArray[1][i]);
            banker2CardsSprite[i] = GetSpriteByName(nameArray[2][i]);
            banker3CardsSprite[i] = GetSpriteByName(nameArray[3][i]);
            banker4CardsSprite[i] = GetSpriteByName(nameArray[4][i]);
        }
    }


    #endregion



    private void UpdateWallet(float wAmount)
    {
        Debug.Log("updatin wallet");
        walletAmount = wAmount;
        walletText.text = "₹" + wAmount.ToString("F2");
    }

    public void LoadDeck()
    {
        System.Object[] loadedSprites = Resources.LoadAll("Deck", typeof(Sprite));

        deck = new List<Sprite>();

        // Add each loaded sprite to the list
        foreach (System.Object sprite in loadedSprites)
        {
            deck.Add(sprite as Sprite);
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


    private void Start()
    {
        LoadDeck();
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        apisRef.FetchWallet();
        player1_Amount_Display_Text.gameObject.SetActive(false);
        player2_Amount_Display_Text.gameObject.SetActive(false);
        player3_Amount_Display_Text.gameObject.SetActive(false);
        player4_Amount_Display_Text.gameObject.SetActive(false);

        historyGeneratorRef = FindObjectOfType<TrendGeneratorBestOf5>();
        botManagerRef = FindObjectOfType<TableBotManager>();
        countDownRef = FindObjectOfType<CountDown321>();
        luckyPlayerRef = FindObjectOfType<LuckyPlayer>();
        winPanel.SetActive(false);
        dbRef = FindObjectOfType<Jugad>();
        distributorRef = FindObjectOfType<Distributor>();
        burstRef = FindObjectOfType<BurstBo5>();
        betCheckerRef = FindObjectOfType<BetChecker>();
        betStartObj.SetActive(false);
        betStopObj.SetActive(false);

    }


    Sprite GetSpriteByName(string cardName)
    {
        foreach (Sprite s in deck)
        {
            if (s.name == cardName)
            {
                return s;
            }
        }
        return null;
    }
    public void Lobby()
    {
        SceneManager.LoadScene(1);
    }
    public void ShowCard(Transform card, Sprite cardImg)
    {
        card.DOScaleX(0.2f, 0.2f)
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            card.gameObject.GetComponent<Image>().sprite = cardImg;
            card.DOScaleX(1, 0.2f)
            .SetEase(Ease.Linear);

        });
    }


    public IEnumerator GameRound(float remTime, long roundStartTime, string[] seeds)
    {
        for (int i = 0; i < 5; i++)
        {
            banker1Cards[i].color = Color.white;
            banker2Cards[i].color = Color.white;
            banker3Cards[i].color = Color.white;
            banker4Cards[i].color = Color.white;
            dealerCards[i].color = Color.white;
        }
        periodId = long.Parse(System.DateTime.Now.ToString("yyyyMMddHHmmss"));

        distributorRef.coinsToDistributeList.Clear();

        betCheckerRef.ResetBets();
        win1Img.SetActive(false);
        win2Img.SetActive(false);
        win3Img.SetActive(false);
        win4Img.SetActive(false);

        dealerCardsStatus.text = "";
        Banker1CardsStatus.text = "";
        Banker2CardsStatus.text = "";
        Banker3CardsStatus.text = "";
        Banker4CardsStatus.text = "";

        for (int i = 0; i < 5; i++)
        {
            dealerCards[i].sprite = cardBgSprite;
            banker1Cards[i].sprite = cardBgSprite;
            banker2Cards[i].sprite = cardBgSprite;
            banker3Cards[i].sprite = cardBgSprite;
            banker4Cards[i].sprite = cardBgSprite;
        }
        //copy cards from deck to game deck 
        gameDeck = new List<Sprite>();
        for (int i = 0; i < deck.Count; i++)
        {
            gameDeck.Add(null);
            gameDeck[i] = deck[i];
        }
        StartCoroutine(burstRef.AnimStart());
        ResetBetAmount();
        betRoda.SetActive(false);
        yield return new WaitForSeconds(1f);

        // 2 patte bat rhe h
        for (int i = 0; i < 2; i++)
        {
            ShowCard(dealerCards[i].transform, dealerCardsSprite[i]);
            ShowCard(banker1Cards[i].transform, banker1CardsSprite[i]);
            ShowCard(banker2Cards[i].transform, banker2CardsSprite[i]);
            ShowCard(banker3Cards[i].transform, banker3CardsSprite[i]);
            ShowCard(banker4Cards[i].transform, banker4CardsSprite[i]);
        }

        if (remTime < 11)
        {
            betStartObj.SetActive(true);
            yield return new WaitForSeconds(1f);
            betStartObj.SetActive(false);
        }

        StartCoroutine(BetAnimD(roundStartTime, seeds));

        if (remTime > 5)
        {
            yield return new WaitForSeconds(remTime - 5);
        }
        for (int i = 0; i < 3; i++)
        {
            countDownRef.Hit(3 - i);
            yield return new WaitForSeconds(1f);
        }
        burstRef.StopAnim();



        betRoda.SetActive(true);
        betStopObj.SetActive(true);
        yield return new WaitForSeconds(1f);
        betStopObj.SetActive(false);
        int betAmnt = betCheckerRef.bet1 + betCheckerRef.bet2 + betCheckerRef.bet3 + betCheckerRef.bet4;
        if (betAmnt > 0)
        {
            dbRef.SaveGameHistoryDatabase(periodId, "Best of Five", betAmnt);
        }

    }

    IEnumerator BetAnimD(long roundStartTime, string[] seeds)
    {
        yield return new WaitForSeconds(1f);
        BetAmountIncrease(roundStartTime, seeds);
    }
    private IEnumerator StartTimer(float duration)
    {
        timer_text.text = (duration).ToString();

        for (int i = 1; i <= duration; i++)
        {
            yield return new WaitForSeconds(1f);
            timer_text.text = (duration - i).ToString();
            if (i > 12)
            {
                clockTickSound.Play();

            }
        }
        // Wait for the specified duration

    }


    IEnumerator Result(int[] winner)
    {

        dealerCardsVal = 0;
        banker1CardsVal = 0;
        banker2CardsVal = 0;
        banker3CardsVal = 0;
        banker4CardsVal = 0;



        // opening remaining 3 cards

        //dealer cards
        for (int i = 2; i < 5; i++)
        {
            ShowCard(dealerCards[i].transform, dealerCardsSprite[i]);
        }
        yield return new WaitForSeconds(1f);
        CheckCardsType(dealerCards, dealerCardsStatus, 0);
        for (int i = 2; i < 5; i++)
        {
            ShowCard(banker1Cards[i].transform, banker1CardsSprite[i]);
        }
        yield return new WaitForSeconds(1f);
        CheckCardsType(banker1Cards, Banker1CardsStatus, 1);

        for (int i = 2; i < 5; i++)
        {
            ShowCard(banker2Cards[i].transform, banker2CardsSprite[i]);
        }
        yield return new WaitForSeconds(1f);
        CheckCardsType(banker2Cards, Banker2CardsStatus, 2);

        for (int i = 2; i < 5; i++)
        {
            ShowCard(banker3Cards[i].transform, banker3CardsSprite[i]);
        }
        yield return new WaitForSeconds(1f);
        CheckCardsType(banker3Cards, Banker3CardsStatus, 3);

        for (int i = 2; i < 5; i++)
        {
            ShowCard(banker4Cards[i].transform, banker4CardsSprite[i]);
        }
        yield return new WaitForSeconds(1f);
        CheckCardsType(banker4Cards, Banker4CardsStatus, 4);

        StartCoroutine(WinChecker(winner));

    }

    void CheckCardsType(List<Image> cardsArray, TextMeshProUGUI text, int val)
    {
        int k = 0;

        if (CheckForTrail(cardsArray))
        {
            k = 49;
            text.text = "Trio";
        }
        else if (CheckForPureSequence(cardsArray))
        {
            k = 48;
            text.text = "Straight Run";
        }
        else if (CheckForSequence(cardsArray))
        {
            k = 47;
            text.text = "Normal Run";
        }
        else if (CheckForSameSymbol(cardsArray))
        {
            k = 46;
            text.text = "Color";
        }

        else if (CheckForPair(cardsArray))
        {
            k = 45;
            text.text = "Pair";
        }
        else
        {
            k = HighCardsValueCalculator(cardsArray);

            if (CheckForAceOrKing(cardsArray))
            {
                text.text = "High Cards (b)";
            }
            else
            {
                text.text = "High Cards (s)";
            }
            HighlightTopThreeCards(cardsArray);
        }

        switch (val)
        {
            case 0:
                dealerCardsVal = k;
                break;
            case 1:
                banker1CardsVal = k;
                break;
            case 2:
                banker2CardsVal = k;

                break;
            case 3:
                banker3CardsVal = k;

                break;
            case 4:
                banker4CardsVal = k;

                break;
        }

    }

    public IEnumerator WinChecker(int[] winner)
    {
        //0 : null
        //9 : trail
        //8 : pure sequence
        //7 : sequence
        //6 : color
        //5 : high cards

        ////////////////////////////////////////////////////////////////////////////////

        float winAmnt = 0f;
        float loseAmnt = 0f;
        int[] winAray = new int[] { 0, 0, 0, 0 };
        if (dealerCardsVal <= banker1CardsVal)
        {
            if (dealerCardsVal == banker1CardsVal)
            {
                if (CardCompareLvl2(1))
                {
                    winAray[0] = 1;
                    win1Img.SetActive(true);
                    if (betCheckerRef.bet1 > 0)
                    {
                        int returnMultipier = 1;
                        if (Banker1CardsStatus.text == "Pair")
                        {
                            returnMultipier = returnMultipier_pair;
                        }

                        else if (Banker1CardsStatus.text == "Color")
                        {
                            returnMultipier = returnMultipier_color;
                        }

                        else if (Banker1CardsStatus.text == "Straight Run")
                        {
                            returnMultipier = returnMultipier_straightRun;
                        }

                        else if (Banker1CardsStatus.text == "Normal Run")
                        {
                            returnMultipier = returnMultipier_normalRun;
                        }

                        else if (Banker1CardsStatus.text == "Trio")
                        {
                            returnMultipier = returnMultipier_trio;
                        }

                        else if (Banker1CardsStatus.text == "High Cards (b)")
                        {
                            returnMultipier = returnMultipier_highCardBig;
                        }

                        else if (Banker1CardsStatus.text == "High Cards (s)")
                        {
                            returnMultipier = returnMultipier_highCardSmall;
                        }
                        float player1_NetAmount = returnMultipier * betCheckerRef.bet1;
                        winAmnt += player1_NetAmount;
                        if (player1_NetAmount != 0)
                        {
                            player1_Amount_Display_Text.gameObject.SetActive(true);
                            string s = player1_NetAmount > 0 ? "+" : "-";
                            player1_Amount_Display_Text.text = s + player1_NetAmount.ToString("F0");
                        }

                    }
                }

                else
                {
                    //lose funda
                    if (betCheckerRef.bet1 > 0)
                    {
                        int returnMultipier = 1;
                        if (dealerCardsStatus.text == "Pair")
                        {
                            returnMultipier = returnMultipier_pair;
                        }

                        else if (dealerCardsStatus.text == "Color")
                        {
                            returnMultipier = returnMultipier_color;
                        }

                        else if (dealerCardsStatus.text == "Straight Run")
                        {
                            returnMultipier = returnMultipier_straightRun;
                        }

                        else if (dealerCardsStatus.text == "Normal Run")
                        {
                            returnMultipier = returnMultipier_normalRun;
                        }

                        else if (dealerCardsStatus.text == "Trio")
                        {
                            returnMultipier = returnMultipier_trio;
                        }

                        else if (dealerCardsStatus.text == "High Cards (b)")
                        {
                            returnMultipier = returnMultipier_highCardBig;
                        }

                        else if (dealerCardsStatus.text == "High Cards (s)")
                        {
                            returnMultipier = returnMultipier_highCardSmall;
                        }


                        float player1_NetAmount = returnMultipier * betCheckerRef.bet1;
                        loseAmnt += player1_NetAmount;
                        if (player1_NetAmount != 0)
                        {
                            player1_Amount_Display_Text.gameObject.SetActive(true);
                            player1_Amount_Display_Text.text = "-" + player1_NetAmount.ToString("F0");
                        }
                    }
                }

            }
            else
            {
                winAray[0] = 1;
                win1Img.SetActive(true);
                if (betCheckerRef.bet1 > 0)
                {
                    int returnMultipier = 1;
                    if (Banker1CardsStatus.text == "Pair")
                    {
                        returnMultipier = returnMultipier_pair;
                    }

                    else if (Banker1CardsStatus.text == "Color")
                    {
                        returnMultipier = returnMultipier_color;
                    }

                    else if (Banker1CardsStatus.text == "Straight Run")
                    {
                        returnMultipier = returnMultipier_straightRun;
                    }

                    else if (Banker1CardsStatus.text == "Normal Run")
                    {
                        returnMultipier = returnMultipier_normalRun;
                    }

                    else if (Banker1CardsStatus.text == "Trio")
                    {
                        returnMultipier = returnMultipier_trio;
                    }

                    else if (Banker1CardsStatus.text == "High Cards (b)")
                    {
                        returnMultipier = returnMultipier_highCardBig;
                    }

                    else if (Banker1CardsStatus.text == "High Cards (s)")
                    {
                        returnMultipier = returnMultipier_highCardSmall;
                    }

                    float player1_NetAmount = returnMultipier * betCheckerRef.bet1;
                    winAmnt += player1_NetAmount;
                    if (player1_NetAmount != 0)
                    {
                        player1_Amount_Display_Text.gameObject.SetActive(true);
                        string s = player1_NetAmount > 0 ? "+" : "-";
                        player1_Amount_Display_Text.text = s + player1_NetAmount.ToString("F0");
                    }
                }
            }

        }
        else
        {
            //lose funda
            if (betCheckerRef.bet1 > 0)
            {
                int returnMultipier = 1;
                if (dealerCardsStatus.text == "Pair")
                {
                    returnMultipier = returnMultipier_pair;
                }

                else if (dealerCardsStatus.text == "Color")
                {
                    returnMultipier = returnMultipier_color;
                }

                else if (dealerCardsStatus.text == "Straight Run")
                {
                    returnMultipier = returnMultipier_straightRun;
                }

                else if (dealerCardsStatus.text == "Normal Run")
                {
                    returnMultipier = returnMultipier_normalRun;
                }

                else if (dealerCardsStatus.text == "Trio")
                {
                    returnMultipier = returnMultipier_trio;
                }

                else if (dealerCardsStatus.text == "High Cards (b)")
                {
                    returnMultipier = returnMultipier_highCardBig;
                }

                else if (dealerCardsStatus.text == "High Cards (s)")
                {
                    returnMultipier = returnMultipier_highCardSmall;
                }


                float player1_NetAmount = returnMultipier * betCheckerRef.bet1;
                loseAmnt += player1_NetAmount;
                if (player1_NetAmount != 0)
                {
                    player1_Amount_Display_Text.gameObject.SetActive(true);
                    player1_Amount_Display_Text.text = "-" + player1_NetAmount.ToString("F0");
                }
            }
        }


        if (dealerCardsVal <= banker2CardsVal)
        {
            if (dealerCardsVal == banker2CardsVal)
            {
                if (CardCompareLvl2(2))
                {
                    winAray[1] = 1;

                    win2Img.SetActive(true);
                    if (betCheckerRef.bet2 > 0)
                    {
                        int returnMultipier = 1;
                        if (Banker2CardsStatus.text == "Pair")
                        {
                            returnMultipier = returnMultipier_pair;
                        }

                        if (Banker2CardsStatus.text == "Color")
                        {
                            returnMultipier = returnMultipier_color;
                        }

                        if (Banker2CardsStatus.text == "Straight Run")
                        {
                            returnMultipier = returnMultipier_straightRun;
                        }

                        if (Banker2CardsStatus.text == "Normal Run")
                        {
                            returnMultipier = returnMultipier_normalRun;
                        }

                        if (Banker2CardsStatus.text == "Trio")
                        {
                            returnMultipier = returnMultipier_trio;
                        }

                        else if (Banker2CardsStatus.text == "High Cards (b)")
                        {
                            returnMultipier = returnMultipier_highCardBig;
                        }

                        else if (Banker2CardsStatus.text == "High Cards (s)")
                        {
                            returnMultipier = returnMultipier_highCardSmall;
                        }
                        float player2_NetAmount = returnMultipier * betCheckerRef.bet2;
                        winAmnt += player2_NetAmount;
                        if (player2_NetAmount != 0)
                        {
                            player2_Amount_Display_Text.gameObject.SetActive(true);
                            player2_Amount_Display_Text.text = "+" + player2_NetAmount.ToString("F0");
                        }
                    }
                }

                else
                {
                    ///lose amount
                    if (betCheckerRef.bet2 > 0)
                    {
                        int returnMultipier = 1;
                        if (dealerCardsStatus.text == "Pair")
                        {
                            returnMultipier = returnMultipier_pair;
                        }

                        if (dealerCardsStatus.text == "Color")
                        {
                            returnMultipier = returnMultipier_color;
                        }

                        if (dealerCardsStatus.text == "Straight Run")
                        {
                            returnMultipier = returnMultipier_straightRun;
                        }

                        if (dealerCardsStatus.text == "Normal Run")
                        {
                            returnMultipier = returnMultipier_normalRun;
                        }

                        if (dealerCardsStatus.text == "Trio")
                        {
                            returnMultipier = returnMultipier_trio;
                        }

                        else if (dealerCardsStatus.text == "High Cards (b)")
                        {
                            returnMultipier = returnMultipier_highCardBig;
                        }

                        else if (dealerCardsStatus.text == "High Cards (s)")
                        {
                            returnMultipier = returnMultipier_highCardSmall;
                        }
                        float player2_NetAmount = returnMultipier * betCheckerRef.bet2;
                        loseAmnt += player2_NetAmount;
                        if (player2_NetAmount != 0)
                        {
                            player2_Amount_Display_Text.gameObject.SetActive(true);
                            player2_Amount_Display_Text.text = "-" + player2_NetAmount.ToString("F0");
                        }
                    }
                }
            }
            else
            {
                winAray[1] = 1;

                win2Img.SetActive(true);
                if (betCheckerRef.bet2 > 0)
                {
                    int returnMultipier = 1;
                    if (Banker2CardsStatus.text == "Pair")
                    {
                        returnMultipier = returnMultipier_pair;
                    }

                    if (Banker2CardsStatus.text == "Color")
                    {
                        returnMultipier = returnMultipier_color;
                    }

                    if (Banker2CardsStatus.text == "Straight Run")
                    {
                        returnMultipier = returnMultipier_straightRun;
                    }

                    if (Banker2CardsStatus.text == "Normal Run")
                    {
                        returnMultipier = returnMultipier_normalRun;
                    }

                    if (Banker2CardsStatus.text == "Trio")
                    {
                        returnMultipier = returnMultipier_trio;
                    }

                    else if (Banker2CardsStatus.text == "High Cards (b)")
                    {
                        returnMultipier = returnMultipier_highCardBig;
                    }

                    else if (Banker2CardsStatus.text == "High Cards (s)")
                    {
                        returnMultipier = returnMultipier_highCardSmall;
                    }
                    float player2_NetAmount = returnMultipier * betCheckerRef.bet2;
                    winAmnt += player2_NetAmount;
                    if (player2_NetAmount != 0)
                    {
                        player2_Amount_Display_Text.gameObject.SetActive(true);
                        player2_Amount_Display_Text.text = "+" + player2_NetAmount.ToString("F0");
                    }
                }
            }
        }
        else
        {
            ///lose amount
            if (betCheckerRef.bet2 > 0)
            {
                int returnMultipier = 1;
                if (dealerCardsStatus.text == "Pair")
                {
                    returnMultipier = returnMultipier_pair;
                }

                if (dealerCardsStatus.text == "Color")
                {
                    returnMultipier = returnMultipier_color;
                }

                if (dealerCardsStatus.text == "Straight Run")
                {
                    returnMultipier = returnMultipier_straightRun;
                }

                if (dealerCardsStatus.text == "Normal Run")
                {
                    returnMultipier = returnMultipier_normalRun;
                }

                if (dealerCardsStatus.text == "Trio")
                {
                    returnMultipier = returnMultipier_trio;
                }

                else if (dealerCardsStatus.text == "High Cards (b)")
                {
                    returnMultipier = returnMultipier_highCardBig;
                }

                else if (dealerCardsStatus.text == "High Cards (s)")
                {
                    returnMultipier = returnMultipier_highCardSmall;
                }
                float player2_NetAmount = returnMultipier * betCheckerRef.bet2;
                loseAmnt += player2_NetAmount;
                if (player2_NetAmount != 0)
                {
                    player2_Amount_Display_Text.gameObject.SetActive(true);
                    player2_Amount_Display_Text.text = "-" + player2_NetAmount.ToString("F0");
                }
            }


        }









        if (dealerCardsVal <= banker3CardsVal)
        {
            if (dealerCardsVal == banker3CardsVal)
            {
                if (CardCompareLvl2(3))
                {
                    winAray[2] = 1;

                    win3Img.SetActive(true);
                    if (betCheckerRef.bet3 > 0)
                    {
                        int returnMultipier = 1;
                        if (Banker3CardsStatus.text == "Pair")
                        {
                            returnMultipier = returnMultipier_pair;
                        }

                        if (Banker3CardsStatus.text == "Color")
                        {
                            returnMultipier = returnMultipier_color;
                        }

                        if (Banker3CardsStatus.text == "Straight Run")
                        {
                            returnMultipier = returnMultipier_straightRun;
                        }

                        if (Banker3CardsStatus.text == "Normal Run")
                        {
                            returnMultipier = returnMultipier_normalRun;
                        }

                        if (Banker3CardsStatus.text == "Trio")
                        {
                            returnMultipier = returnMultipier_trio;
                        }

                        else if (Banker3CardsStatus.text == "High Cards (b)")
                        {
                            returnMultipier = returnMultipier_highCardBig;
                        }

                        else if (Banker3CardsStatus.text == "High Cards (s)")
                        {
                            returnMultipier = returnMultipier_highCardSmall;
                        }
                        float player3_NetAmount = returnMultipier * betCheckerRef.bet3;
                        winAmnt += player3_NetAmount;
                        if (player3_NetAmount != 0)
                        {
                            player3_Amount_Display_Text.gameObject.SetActive(true);
                            player3_Amount_Display_Text.text = "+" + player3_NetAmount.ToString("F0");
                        }
                    }
                }
                else
                {
                    //lose amount 

                    if (betCheckerRef.bet3 > 0)
                    {
                        int returnMultipier = 1;
                        if (dealerCardsStatus.text == "Pair")
                        {
                            returnMultipier = returnMultipier_pair;
                        }

                        if (dealerCardsStatus.text == "Color")
                        {
                            returnMultipier = returnMultipier_color;
                        }

                        if (dealerCardsStatus.text == "Straight Run")
                        {
                            returnMultipier = returnMultipier_straightRun;
                        }

                        if (dealerCardsStatus.text == "Normal Run")
                        {
                            returnMultipier = returnMultipier_normalRun;
                        }

                        if (dealerCardsStatus.text == "Trio")
                        {
                            returnMultipier = returnMultipier_trio;
                        }

                        else if (Banker3CardsStatus.text == "High Cards (b)")
                        {
                            returnMultipier = returnMultipier_highCardBig;
                        }

                        else if (Banker3CardsStatus.text == "High Cards (s)")
                        {
                            returnMultipier = returnMultipier_highCardSmall;
                        }
                        float player3_NetAmount = returnMultipier * betCheckerRef.bet3;
                        loseAmnt += player3_NetAmount;
                        if (player3_NetAmount != 0)
                        {
                            player3_Amount_Display_Text.gameObject.SetActive(true);
                            player3_Amount_Display_Text.text = "-" + player3_NetAmount.ToString("F0");
                        }
                    }

                }

            }
            else
            {
                winAray[2] = 1;

                win3Img.SetActive(true);
                if (betCheckerRef.bet3 > 0)
                {
                    int returnMultipier = 1;
                    if (Banker3CardsStatus.text == "Pair")
                    {
                        returnMultipier = returnMultipier_pair;
                    }

                    if (Banker3CardsStatus.text == "Color")
                    {
                        returnMultipier = returnMultipier_color;
                    }

                    if (Banker3CardsStatus.text == "Straight Run")
                    {
                        returnMultipier = returnMultipier_straightRun;
                    }

                    if (Banker3CardsStatus.text == "Normal Run")
                    {
                        returnMultipier = returnMultipier_normalRun;
                    }

                    if (Banker3CardsStatus.text == "Trio")
                    {
                        returnMultipier = returnMultipier_trio;
                    }

                    else if (Banker3CardsStatus.text == "High Cards (b)")
                    {
                        returnMultipier = returnMultipier_highCardBig;
                    }

                    else if (Banker3CardsStatus.text == "High Cards (s)")
                    {
                        returnMultipier = returnMultipier_highCardSmall;
                    }
                    float player3_NetAmount = returnMultipier * betCheckerRef.bet3;
                    winAmnt += player3_NetAmount;
                    if (player3_NetAmount != 0)
                    {
                        player3_Amount_Display_Text.gameObject.SetActive(true);
                        player3_Amount_Display_Text.text = "+" + player3_NetAmount.ToString("F0");
                    }
                }
            }

        }
        else
        {
            //lose amount 

            if (betCheckerRef.bet3 > 0)
            {
                int returnMultipier = 1;
                if (dealerCardsStatus.text == "Pair")
                {
                    returnMultipier = returnMultipier_pair;
                }

                if (dealerCardsStatus.text == "Color")
                {
                    returnMultipier = returnMultipier_color;
                }

                if (dealerCardsStatus.text == "Straight Run")
                {
                    returnMultipier = returnMultipier_straightRun;
                }

                if (dealerCardsStatus.text == "Normal Run")
                {
                    returnMultipier = returnMultipier_normalRun;
                }

                if (dealerCardsStatus.text == "Trio")
                {
                    returnMultipier = returnMultipier_trio;
                }

                else if (dealerCardsStatus.text == "High Cards (b)")
                {
                    returnMultipier = returnMultipier_highCardBig;
                }

                else if (dealerCardsStatus.text == "High Cards (s)")
                {
                    returnMultipier = returnMultipier_highCardSmall;
                }
                float player3_NetAmount = returnMultipier * betCheckerRef.bet3;
                loseAmnt += player3_NetAmount;
                if (player3_NetAmount != 0)
                {
                    player3_Amount_Display_Text.gameObject.SetActive(true);
                    player3_Amount_Display_Text.text = "-" + player3_NetAmount.ToString("F0");
                }
            }

        }

        if (dealerCardsVal <= banker4CardsVal)
        {

            if (dealerCardsVal == banker4CardsVal)
            {
                if (CardCompareLvl2(4))
                {
                    winAray[3] = 1;

                    win4Img.SetActive(true);
                    if (betCheckerRef.bet4 > 0)
                    {
                        int returnMultipier = 1;
                        if (Banker4CardsStatus.text == "Pair")
                        {
                            returnMultipier = returnMultipier_pair;
                        }

                        if (Banker4CardsStatus.text == "Color")
                        {
                            returnMultipier = returnMultipier_color;
                        }

                        if (Banker4CardsStatus.text == "Straight Run")
                        {
                            returnMultipier = returnMultipier_straightRun;
                        }

                        if (Banker4CardsStatus.text == "Normal Run")
                        {
                            returnMultipier = returnMultipier_normalRun;
                        }

                        if (Banker4CardsStatus.text == "Trio")
                        {
                            returnMultipier = returnMultipier_trio;
                        }

                        else if (Banker4CardsStatus.text == "High Cards (b)")
                        {
                            returnMultipier = returnMultipier_highCardBig;
                        }

                        else if (Banker4CardsStatus.text == "High Cards (s)")
                        {
                            returnMultipier = returnMultipier_highCardSmall;
                        }
                        float player4_NetAmount = returnMultipier * betCheckerRef.bet4;
                        winAmnt += player4_NetAmount;
                        if (player4_NetAmount != 0)
                        {
                            player4_Amount_Display_Text.gameObject.SetActive(true);
                            player4_Amount_Display_Text.text = "+" + player4_NetAmount.ToString("F0");
                        }
                    }
                }
                else
                {
                    //loose funda
                    if (betCheckerRef.bet4 > 0)
                    {
                        int returnMultipier = 1;
                        if (dealerCardsStatus.text == "Pair")
                        {
                            returnMultipier = returnMultipier_pair;
                        }

                        if (dealerCardsStatus.text == "Color")
                        {
                            returnMultipier = returnMultipier_color;
                        }

                        if (dealerCardsStatus.text == "Straight Run")
                        {
                            returnMultipier = returnMultipier_straightRun;
                        }

                        if (dealerCardsStatus.text == "Normal Run")
                        {
                            returnMultipier = returnMultipier_normalRun;
                        }

                        if (dealerCardsStatus.text == "Trio")
                        {
                            returnMultipier = returnMultipier_trio;
                        }

                        else if (dealerCardsStatus.text == "High Cards (b)")
                        {
                            returnMultipier = returnMultipier_highCardBig;
                        }

                        else if (dealerCardsStatus.text == "High Cards (s)")
                        {
                            returnMultipier = returnMultipier_highCardSmall;
                        }
                        float player4_NetAmount = returnMultipier * betCheckerRef.bet4;
                        loseAmnt += player4_NetAmount;
                        if (player4_NetAmount != 0)
                        {
                            player4_Amount_Display_Text.gameObject.SetActive(true);
                            player4_Amount_Display_Text.text = "-" + player4_NetAmount.ToString("F0");
                        }
                    }
                }
            }
            else
            {
                winAray[3] = 1;

                win4Img.SetActive(true);
                if (betCheckerRef.bet4 > 0)
                {
                    int returnMultipier = 1;
                    if (Banker4CardsStatus.text == "Pair")
                    {
                        returnMultipier = returnMultipier_pair;
                    }

                    if (Banker4CardsStatus.text == "Color")
                    {
                        returnMultipier = returnMultipier_color;
                    }

                    if (Banker4CardsStatus.text == "Straight Run")
                    {
                        returnMultipier = returnMultipier_straightRun;
                    }

                    if (Banker4CardsStatus.text == "Normal Run")
                    {
                        returnMultipier = returnMultipier_normalRun;
                    }

                    if (Banker4CardsStatus.text == "Trio")
                    {
                        returnMultipier = returnMultipier_trio;
                    }

                    else if (Banker4CardsStatus.text == "High Cards (b)")
                    {
                        returnMultipier = returnMultipier_highCardBig;
                    }

                    else if (Banker4CardsStatus.text == "High Cards (s)")
                    {
                        returnMultipier = returnMultipier_highCardSmall;
                    }
                    float player4_NetAmount = returnMultipier * betCheckerRef.bet4;
                    winAmnt += player4_NetAmount;
                    if (player4_NetAmount != 0)
                    {
                        player4_Amount_Display_Text.gameObject.SetActive(true);
                        player4_Amount_Display_Text.text = "+" + player4_NetAmount.ToString("F0");
                    }
                }
            }
        }
        else
        {

            //lose
            if (betCheckerRef.bet4 > 0)
            {
                int returnMultipier = 1;
                if (dealerCardsStatus.text == "Pair")
                {
                    returnMultipier = returnMultipier_pair;
                }

                if (dealerCardsStatus.text == "Color")
                {
                    returnMultipier = returnMultipier_color;
                }

                if (dealerCardsStatus.text == "Straight Run")
                {
                    returnMultipier = returnMultipier_straightRun;
                }

                if (dealerCardsStatus.text == "Normal Run")
                {
                    returnMultipier = returnMultipier_normalRun;
                }

                if (dealerCardsStatus.text == "Trio")
                {
                    returnMultipier = returnMultipier_trio;
                }

                else if (dealerCardsStatus.text == "High Cards (b)")
                {
                    returnMultipier = returnMultipier_highCardBig;
                }

                else if (dealerCardsStatus.text == "High Cards (s)")
                {
                    returnMultipier = returnMultipier_highCardSmall;
                }
                float player4_NetAmount = returnMultipier * betCheckerRef.bet4;
                loseAmnt += player4_NetAmount;
                if (player4_NetAmount != 0)
                {
                    player4_Amount_Display_Text.gameObject.SetActive(true);
                    player4_Amount_Display_Text.text = "-" + player4_NetAmount.ToString("F0");
                }
            }

        }



        historyGeneratorRef.AddHistory(winAray);

        float netAmount = winAmnt - loseAmnt;

        if (netAmount > 0)
        {
            winPanel.SetActive(true);
            winPanelText.text = "Win : Rs." + netAmount;
            // total win hua hai
            if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < netAmount)
            {
                netAmount = BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;

            }
            Wallet.AddToWinWallet(netAmount);
            UpdateWallet(walletAmount + winAmnt);

        }
        else
        {
            if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < -netAmount)
            {
                netAmount = -BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;
            }
            Wallet.DeductAmount(-netAmount);
            UpdateWallet(walletAmount - loseAmnt);

        }



        if (walletAmount < 0) walletAmount = 0f;

        walletText.text = "₹" + walletAmount.ToString("F2");


        if (netAmount != 0)
        {

            dbRef.UpdateGameHistoryBestOf5(periodId, netAmount);

            yield return new WaitForSeconds(1f);


        }

        if (dealerCardsVal < banker1CardsVal || dealerCardsVal < banker2CardsVal || dealerCardsVal < banker3CardsVal || dealerCardsVal < banker4CardsVal)
        {
            distributorRef.Distribute(true);

        }
        else
        {
            distributorRef.Distribute(false);
        }
        yield return new WaitForSeconds(2f);
        winPanel.SetActive(false);


        player1_Amount_Display_Text.gameObject.SetActive(false);
        player2_Amount_Display_Text.gameObject.SetActive(false);
        player3_Amount_Display_Text.gameObject.SetActive(false);
        player4_Amount_Display_Text.gameObject.SetActive(false);



        burstRef.MoveAllcoinsBack();

        yield return new WaitForSeconds(2f);
        win1Img.SetActive(false);
        win2Img.SetActive(false);
        win3Img.SetActive(false);
        win4Img.SetActive(false);
    }




    bool CardCompareLvl2(int bankerNum)
    {
        List<Image> bankerCards = new List<Image>();

        if (dealerCardsStatus.text == "pair")
        {
            switch (bankerNum)
            {
                case 1:
                    for (int i = 0; i < banker1Cards.Count; i++)
                    {
                        bankerCards.Add(banker1Cards[i]);
                    }
                    break;
                case 2:
                    for (int i = 0; i < banker2Cards.Count; i++)
                    {
                        bankerCards.Add(banker2Cards[i]);
                    }
                    break;
                case 3:
                    for (int i = 0; i < banker3Cards.Count; i++)
                    {
                        bankerCards.Add(banker3Cards[i]);
                    }
                    break;
                case 4:
                    for (int i = 0; i < banker4Cards.Count; i++)
                    {
                        bankerCards.Add(banker4Cards[i]);
                    }
                    break;

            }

            List<string> dealerCardGray = new List<string>();
            List<string> bankerCardGray = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                if (dealerCards[i].color != Color.gray)
                {
                    Debug.Log("adding d : " + dealerCards[i].sprite.name);

                    dealerCardGray.Add(dealerCards[i].sprite.name);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                if (bankerCards[i].color != Color.gray)
                {
                    Debug.Log("adding b : " + bankerCards[i].sprite.name);
                    bankerCardGray.Add(bankerCards[i].sprite.name);
                }

            }

            int dealerCardsPair = 0;
            int bankerCardsPair = 0;


            List<string> dealerCardPair = new List<string>();
            List<string> bankerCardPair = new List<string>();


            if (dealerCardGray[0].Substring(1) == dealerCardGray[1].Substring(1))
            {
                dealerCardPair.Add(dealerCardGray[0]);
                dealerCardPair.Add(dealerCardGray[1]);

                dealerCardsPair = int.Parse(GetCardRank(dealerCardGray[0]));
            }
            else if (dealerCardGray[1].Substring(1) == dealerCardGray[2].Substring(1))
            {
                dealerCardPair.Add(dealerCardGray[2]);
                dealerCardPair.Add(dealerCardGray[1]);


                dealerCardsPair = int.Parse(GetCardRank(dealerCardGray[1]));

            }
            else if (dealerCardGray[0].Substring(1) == dealerCardGray[2].Substring(1))
            {
                dealerCardPair.Add(dealerCardGray[0]);
                dealerCardPair.Add(dealerCardGray[2]);

                dealerCardsPair = int.Parse(GetCardRank(dealerCardGray[0]));

            }


            if (bankerCardGray[0].Substring(1) == bankerCardGray[1].Substring(1))
            {
                bankerCardPair.Add(bankerCardGray[0]);
                bankerCardPair.Add(bankerCardGray[1]);

                bankerCardsPair = int.Parse(GetCardRank(bankerCardGray[0]));

            }
            else if (bankerCardGray[1].Substring(1) == bankerCardGray[2].Substring(1))
            {
                bankerCardPair.Add(bankerCardGray[2]);
                bankerCardPair.Add(bankerCardGray[1]);
                bankerCardsPair = int.Parse(GetCardRank(bankerCardGray[1]));

            }
            else if (bankerCardGray[0].Substring(1) == bankerCardGray[2].Substring(1))
            {
                bankerCardPair.Add(bankerCardGray[0]);
                bankerCardPair.Add(bankerCardGray[2]);
                bankerCardsPair = int.Parse(GetCardRank(bankerCardGray[0]));

            }


            if (dealerCardsPair < bankerCardsPair)
            {
                //banker bada hai 
                return true;
            }
            else if (dealerCardsPair == bankerCardsPair)
            {

                int k = Mathf.Max(GetSuitNumber(dealerCardPair[0]), GetSuitNumber(dealerCardPair[1]));
                int l = Mathf.Max(GetSuitNumber(bankerCardPair[0]), GetSuitNumber(bankerCardPair[1]));

                if (k < l)
                {
                    return true;
                }
                else return false;

            }
            return false;
        }

        switch (bankerNum)
        {
            case 1:
                for (int i = 0; i < banker1Cards.Count; i++)
                {
                    bankerCards.Add(banker1Cards[i]);
                }
                break;
            case 2:
                for (int i = 0; i < banker2Cards.Count; i++)
                {
                    bankerCards.Add(banker2Cards[i]);
                }
                break;
            case 3:
                for (int i = 0; i < banker3Cards.Count; i++)
                {
                    bankerCards.Add(banker3Cards[i]);
                }
                break;
            case 4:
                for (int i = 0; i < banker4Cards.Count; i++)
                {
                    bankerCards.Add(banker4Cards[i]);
                }
                break;

        }


        List<int> dealerCardRanks = new List<int>();
        List<int> bankerCardRanks = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            if (dealerCards[i].color != Color.gray)
            {
                dealerCardRanks.Add(int.Parse(GetCardRank(dealerCards[i].sprite.name)));
            }
        }

        for (int i = 0; i < 5; i++)
        {
            if (bankerCards[i].color != Color.gray)
            {
                bankerCardRanks.Add(int.Parse(GetCardRank(bankerCards[i].sprite.name)));
            }
        }
        dealerCardRanks.Sort();
        bankerCardRanks.Sort();

        for (int i = dealerCardRanks.Count - 1; i >= 0; i--)
        {

            if (dealerCardRanks[i] < bankerCardRanks[i])
            {
                return true;
            }
            else if (dealerCardRanks[i] > bankerCardRanks[i])
            {
                return false;
            }
        }

        List<int> dealerCardSuitRanks = new List<int>();
        List<int> bankerCardSuitRanks = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            if (dealerCards[i].color != Color.gray)
            {
                dealerCardSuitRanks.Add((GetSuitNumber(dealerCards[i].sprite.name)));
            }
        }

        for (int i = 0; i < 5; i++)
        {
            if (bankerCards[i].color != Color.gray)
            {
                bankerCardSuitRanks.Add((GetSuitNumber(bankerCards[i].sprite.name)));
            }
        }
        dealerCardSuitRanks.Sort();
        bankerCardSuitRanks.Sort();

        for (int i = 0; i < dealerCardSuitRanks.Count; i++)
        {
            if (dealerCardSuitRanks[i] < bankerCardSuitRanks[i])
            {
                return true;
            }
        }

        return false;
    }

    int GetSuitNumber(string cardSuit)
    {
        if (cardSuit.Substring(0, 1) == "s")
        {
            return 5;
        }

        else if (cardSuit.Substring(0, 1) == "h")
        {
            return 4;
        }

        else if (cardSuit.Substring(0, 1) == "d")
        {
            return 3;
        }

        else if (cardSuit.Substring(0, 1) == "c")
        {
            return 2;
        }
        else return 0;
    }


    public int HighCardsValueCalculator(List<Image> cardsImageList)
    {
        if (cardsImageList.Count != 5)
        {
            Debug.LogError("Exactly 5 card images are required.");
            return 0;
        }

        Sprite[] cardSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
        {
            cardSprites[i] = cardsImageList[i].sprite;
        }

        List<int> cardValues = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            int value = ParseCardValueForHighCard(cardSprites[i].name);
            cardValues.Add(value);
        }

        // Sort the card values in descending order
        cardValues.Sort((a, b) => b.CompareTo(a));

        // Sum the highest three values
        int sumOfHighestThree = cardValues.Take(3).Sum();

        Debug.Log("Sum of the three highest card values: " + sumOfHighestThree);
        return cardValues[0];
    }

    private int ParseCardValueForHighCard(string cardName)
    {
        if (cardName.Length < 2)
        {
            Debug.LogError("Invalid card name format.");
            return 0;
        }

        string valueStr = cardName.Substring(1);
        switch (valueStr)
        {
            case "a": return 14; // Assuming Ace is high
            case "2": return 2;
            case "3": return 3;
            case "4": return 4;
            case "5": return 5;
            case "6": return 6;
            case "7": return 7;
            case "8": return 8;
            case "9": return 9;
            case "o": return 10;
            case "j": return 11;
            case "q": return 12;
            case "k": return 13;
            default:
                Debug.LogError("Invalid card value");
                return 0;
        }
    }







    public bool CheckForTrail(List<Image> cardSpritesList)
    {
        Sprite[] cardSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
        {
            cardSprites[i] = cardSpritesList[i].sprite;
        }

        // Ensure exactly 5 sprites are passed
        if (cardSprites.Length != 5)
        {
            Debug.LogError("Exactly 5 card sprites are required.");
            return false;
        }

        // Dictionary to count occurrences of each card value
        Dictionary<string, int> cardValueCounts = new Dictionary<string, int>();

        foreach (var sprite in cardSprites)
        {
            // Assuming the card names are formatted like 's2', 'cK', etc.
            string cardName = sprite.name;
            if (cardName.Length < 2)
            {
                Debug.LogError("Invalid card name format.");
                return false;
            }

            // Extract the card value part (ignoring the first character)
            string cardValue = cardName.Substring(1);

            // Count the occurrences of each card value
            if (cardValueCounts.ContainsKey(cardValue))
            {
                cardValueCounts[cardValue]++;
            }
            else
            {
                cardValueCounts[cardValue] = 1;
            }
        }

        // Identify the card value that forms the trail (occurs at least 3 times)
        string trailValue = null;
        foreach (var kvp in cardValueCounts)
        {
            if (kvp.Value >= 3)
            {
                trailValue = kvp.Key;
                break;
            }
        }

        // If a trail is found, change the color of the images that are not part of the trail
        if (trailValue != null)
        {
            foreach (var sprite in cardSpritesList)
            {
                string cardName = sprite.sprite.name;
                string cardValue = cardName.Substring(1);
                if (cardValue != trailValue)
                {
                    sprite.color = Color.gray; // Change color to red or any other color of your choice
                }
            }
            return true; // Trail found
        }

        return false;
    }


    public bool CheckForPureSequence(List<Image> cardSpritesList)
    {
        Sprite[] cardSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
        {
            cardSprites[i] = cardSpritesList[i].sprite;
        }

        if (cardSprites.Length != 5)
        {
            Debug.LogError("Exactly 5 card sprites are required.");
            return false;
        }

        // Dictionary to map suit and value to corresponding Image
        Dictionary<(char, int), Image> suitValueToImageMap = new Dictionary<(char, int), Image>();
        Dictionary<char, List<int>> suitToValues = new Dictionary<char, List<int>>();

        foreach (var sprite in cardSprites)
        {
            string cardName = sprite.name;
            if (cardName.Length < 2)
            {
                Debug.LogError("Invalid card name format.");
                return false;
            }

            char suit = cardName[0];
            string valueStr = cardName.Substring(1);
            int value = ParseCardValueA(valueStr);

            // Map the suit and value to the Image
            suitValueToImageMap[(suit, value)] = cardSpritesList.Find(img => img.sprite == sprite);

            if (!suitToValues.ContainsKey(suit))
            {
                suitToValues[suit] = new List<int>();
            }
            suitToValues[suit].Add(value);
        }

        List<(char, int)> bestSequence = null;

        foreach (var suit in suitToValues.Keys)
        {
            List<int> suitValues = suitToValues[suit];
            suitValues.Sort();

            for (int i = 0; i < suitValues.Count - 2; i++)
            {
                List<(char, int)> currentSequence = new List<(char, int)>();

                if (suitValues[i + 1] == suitValues[i] + 1 && suitValues[i + 2] == suitValues[i] + 2)
                {
                    currentSequence.Add((suit, suitValues[i]));
                    currentSequence.Add((suit, suitValues[i + 1]));
                    currentSequence.Add((suit, suitValues[i + 2]));

                    // Continue adding to sequence if more consecutive values are found
                    for (int j = i + 3; j < suitValues.Count && suitValues[j] == suitValues[j - 1] + 1; j++)
                    {
                        currentSequence.Add((suit, suitValues[j]));
                    }

                    // Keep track of the best (highest) sequence
                    if (bestSequence == null || currentSequence[currentSequence.Count - 1].Item2 > bestSequence[bestSequence.Count - 1].Item2)
                    {
                        bestSequence = currentSequence;
                    }
                }
            }
        }

        if (bestSequence != null && bestSequence.Count >= 3)
        {
            // Select the highest 3 cards in the best sequence
            List<(char, int)> myBestSequence = new List<(char, int)>(bestSequence);

            bestSequence = bestSequence.OrderByDescending(x => x.Item2).Take(3).ToList();
            if (bestSequence.Any(card => card.Item2 == 2) && bestSequence.Any(card => card.Item2 == 3) && myBestSequence.Any(card => card.Item2 == 14))
            {
                var cardToRemove = bestSequence.FirstOrDefault(card => card.Item2 == 4);
                var tmp = bestSequence.Any(card => card.Item2 == 2);
                bestSequence.Remove(cardToRemove);


                bestSequence.Add((bestSequence.Where(card => card.Item2 == 3).Select(card => card.Item1).FirstOrDefault(), 14));
            }
            // Change the color of the selected cards
            foreach (var sprite in cardSpritesList)
            {
                string cardName = sprite.sprite.name;
                char suit = cardName[0];
                string valueStr = cardName.Substring(1);
                int value = ParseCardValueA(valueStr);

                if (bestSequence.Contains((suit, value)))
                {
                    sprite.color = Color.white; // Change color to the desired highlight color
                }
                else
                {
                    sprite.color = Color.gray; // Change color to gray or another color for non-sequence cards
                }
            }
            return true; // Pure sequence found
        }

        return false; // No pure sequence found
    }

    private int ParseCardValue(string valueStr, bool Ace = false)
    {
        switch (valueStr)
        {
            case "a": return 14;
            case "2": return 2;
            case "3": return 3;
            case "4": return 4;
            case "5": return 5;
            case "6": return 6;
            case "7": return 7;
            case "8": return 8;
            case "9": return 9;
            case "o": return 10;
            case "j": return 11;
            case "q": return 12;
            case "k": return 13;
            default: throw new System.ArgumentException("Invalid card value" + valueStr);
        }
    }

    private int ParseCardValueA(string valueStr, bool Ace = false)
    {
        switch (valueStr)
        {
            case "a": return 1;
            case "2": return 2;
            case "3": return 3;
            case "4": return 4;
            case "5": return 5;
            case "6": return 6;
            case "7": return 7;
            case "8": return 8;
            case "9": return 9;
            case "o": return 10;
            case "j": return 11;
            case "q": return 12;
            case "k": return 13;
            default: throw new System.ArgumentException("Invalid card value" + valueStr);
        }
    }

    public bool CheckForSequence(List<Image> cardSpritesList)
    {
        Sprite[] cardSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
        {
            cardSprites[i] = cardSpritesList[i].sprite;
        }

        if (cardSprites.Length != 5)
        {
            Debug.LogError("Exactly 5 card sprites are required.");
            return false;
        }

        List<int> values = new List<int>();
        Dictionary<int, List<Image>> valueToImagesMap = new Dictionary<int, List<Image>>();

        foreach (var sprite in cardSprites)
        {
            string cardName = sprite.name;
            if (cardName.Length < 2)
            {
                Debug.LogError("Invalid card name format.");
                return false;
            }

            string valueStr = cardName.Substring(1);
            int value = ParseCardValue(valueStr);
            values.Add(value);

            // Map the value to the corresponding image(s)
            if (!valueToImagesMap.ContainsKey(value))
            {
                valueToImagesMap[value] = new List<Image>();
            }
            valueToImagesMap[value].Add(cardSpritesList.Find(img => img.sprite == sprite));
        }

        // Sort values in descending order to check for highest sequence
        values.Sort((a, b) => b.CompareTo(a));

        // Track the sequence
        List<int> sequenceValues = new List<int>();
        int sequenceCount = 1; // Start with the first card

        for (int i = 1; i < values.Count; i++)
        {
            if (values[i] == values[i - 1] - 1) // Check if it's part of a sequence
            {
                sequenceCount++;
                sequenceValues.Add(values[i - 1]); // Add previous value to the sequence
            }
            else if (values[i] != values[i - 1]) // Skip duplicates in the sequence
            {
                sequenceCount = 1;
                sequenceValues.Clear(); // Reset if there's no sequence continuation
            }

            // If sequence count reaches 3, break
            if (sequenceCount == 3)
            {
                sequenceValues.Add(values[i]);
                break;
            }
        }

        // Handle Ace (value 14) being treated as 1 for sequence purposes
        if (sequenceValues.Count < 3 && values.Contains(14))
        {
            valueToImagesMap = new Dictionary<int, List<Image>>();

            foreach (var sprite in cardSprites)
            {
                string cardName = sprite.name;
                if (cardName.Length < 2)
                {
                    Debug.LogError("Invalid card name format.");
                    return false;
                }

                string valueStr = cardName.Substring(1);
                int value = ParseCardValue(valueStr);
                if (value == 14)
                {
                    value = 1;
                }
                values.Add(value);

                // Map the value to the corresponding image(s)
                if (!valueToImagesMap.ContainsKey(value))
                {
                    valueToImagesMap[value] = new List<Image>();
                }
                valueToImagesMap[value].Add(cardSpritesList.Find(img => img.sprite == sprite));
            }



            //values[values.IndexOf(14)] = 1; // Treat Ace as 1 and re-check
            values.Sort((a, b) => b.CompareTo(a)); // Re-sort after treating Ace as 1

            sequenceValues.Clear();
            sequenceCount = 1;

            for (int i = 1; i < values.Count; i++)
            {
                if (values[i] == values[i - 1] - 1)
                {
                    sequenceCount++;
                    sequenceValues.Add(values[i - 1]);
                }
                else if (values[i] != values[i - 1])
                {
                    sequenceCount = 1;
                    sequenceValues.Clear();
                }

                if (sequenceCount == 3)
                {
                    sequenceValues.Add(values[i]);
                    break;
                }
            }
        }

        // Highlight the sequence and handle duplicates
        if (sequenceValues.Count >= 3)
        {

            // Iterate through the valueToImagesMap to handle duplicates and color change
            foreach (var value in values)
            {
                if (!sequenceValues.Contains(value) || valueToImagesMap[value].Count > 1)
                {
                    if (valueToImagesMap.TryGetValue(value, out List<Image> images))
                    {
                        foreach (var image in images)
                        {
                            if (!sequenceValues.Contains(value) || valueToImagesMap[value].Count > 1)
                            {
                                // Change the color of non-sequence or duplicate images
                                image.color = Color.gray;
                                valueToImagesMap[value].Remove(image);
                                if (valueToImagesMap[value].Count == 0)
                                    valueToImagesMap.Remove(value);
                                break; // Only change one of the duplicates
                            }
                        }
                    }
                }
            }

            return true; // Sequence found
        }

        return false; // No sequence found
    }




    public bool CheckForSameSymbol(List<Image> cardSpritesList)
    {
        Sprite[] cardSprites = new Sprite[5];

        for (int i = 0; i < 5; i++)
        {
            cardSprites[i] = cardSpritesList[i].sprite;
        }

        if (cardSprites.Length != 5)
        {
            Debug.LogError("Exactly 5 card sprites are required.");
            return false;
        }

        // Dictionary to count occurrences of each card suit and store card ranks
        Dictionary<char, List<(int index, int rank)>> suitIndices = new Dictionary<char, List<(int, int)>>();

        // Iterate over the card sprites to build the dictionary of suit occurrences
        for (int i = 0; i < cardSprites.Length; i++)
        {
            string cardName = cardSprites[i].name;
            if (cardName.Length < 2)
            {
                Debug.LogError("Invalid card name format.");
                return false;
            }

            // Extract the suit part (the first character)
            char suit = cardName[0];

            // Extract the rank part (e.g., '2', 'J', 'Q', 'K', 'A')
            string rankStr = cardName.Substring(1);
            int rank = GetCardRankInt(rankStr);

            // Store the index of the card and its rank based on its suit
            if (!suitIndices.ContainsKey(suit))
            {
                suitIndices[suit] = new List<(int, int)>();
            }

            suitIndices[suit].Add((i, rank));
        }

        // Find the suit that has 3 or more cards
        char? matchingSuit = null;
        foreach (var suitEntry in suitIndices)
        {
            if (suitEntry.Value.Count >= 3)
            {
                matchingSuit = suitEntry.Key;
                break;
            }
        }

        // If no matching suit with at least 3 cards is found, return false
        if (matchingSuit == null)
        {
            return false;
        }

        // Now highlight only the top 3 highest-ranked cards from the matching suit
        List<(int index, int rank)> indicesToHighlight = suitIndices[matchingSuit.Value];

        // Sort the cards by rank in descending order (highest rank first)
        indicesToHighlight.Sort((a, b) => b.rank.CompareTo(a.rank));

        // If there are more than 3 cards of the matching suit, highlight only the highest 3
        for (int i = 0; i < indicesToHighlight.Count; i++)
        {
            if (i < 3)
            {
                // Highlight the top 3 cards (change their color or some other highlight effect)
                cardSpritesList[indicesToHighlight[i].index].color = Color.white; // Highlight color
            }
            else
            {
                // Set the rest of the cards (including the non-matching suit) to gray
                cardSpritesList[indicesToHighlight[i].index].color = Color.gray; // Non-highlighted color
            }
        }

        // Also make sure to gray out cards that do not belong to the matching suit
        for (int i = 0; i < cardSpritesList.Count; i++)
        {
            if (!indicesToHighlight.Take(3).Any(x => x.index == i))
            {
                cardSpritesList[i].color = Color.gray;
            }
        }

        return true; // At least 3 cards of the same suit found
    }

    // Function to get the rank of a card as an integer
    private int GetCardRankInt(string rankStr)
    {
        switch (rankStr)
        {
            case "2": return 2;
            case "3": return 3;
            case "4": return 4;
            case "5": return 5;
            case "6": return 6;
            case "7": return 7;
            case "8": return 8;
            case "9": return 9;
            case "0": return 10;
            case "j": return 11;
            case "q": return 12;
            case "k": return 13;
            case "a": return 14;
            default: return 0; // Invalid rank
        }
    }



    public bool CheckForPair(List<Image> cardSpritesList)
    {
        if (cardSpritesList.Count != 5)
        {
            Debug.LogError("Exactly 5 card sprites are required.");
            return false;
        }

        // Dictionary to count occurrences of each card number
        Dictionary<int, List<Image>> valueToImagesMap = new Dictionary<int, List<Image>>();

        // To keep track of the highest pair found
        int highestPairValue = -1;

        foreach (var sprite in cardSpritesList)
        {
            string cardName = sprite.sprite.name;
            if (cardName.Length < 2)
            {
                Debug.LogError("Invalid card name format.");
                return false;
            }

            // Extract the number part (everything after the first character, assuming the format is like 's2', 'ck', etc.)
            int value = ParseCardValue(cardName.Substring(1));

            // Map the value to the corresponding images
            if (!valueToImagesMap.ContainsKey(value))
            {
                valueToImagesMap[value] = new List<Image>();
            }
            valueToImagesMap[value].Add(sprite);

            // If this value has exactly 2 occurrences, check if it's the highest pair so far
            if (valueToImagesMap[value].Count == 2 && value > highestPairValue)
            {
                highestPairValue = value;
            }
        }

        // If no pairs are found, return false
        if (highestPairValue == -1)
        {
            Debug.Log("No pairs found.");
            return false;
        }

        // Highlight the highest pair and gray out the others
        List<Image> leftCardsList = new List<Image>();
        foreach (var kvp in valueToImagesMap)
        {
            foreach (var img in kvp.Value)
            {
                if (kvp.Key == highestPairValue)
                {
                    img.color = Color.white; // Highlight the highest pair
                }
                else
                {
                    img.color = Color.gray; // Gray out the others
                    leftCardsList.Add(img);
                }
            }
        }

        HighlightHighestCard(leftCardsList);

        return true; // Highest pair found and highlighted
    }



    public bool CheckForAceOrKing(List<Image> cardSpritesList)
    {
        Sprite[] cardSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
        {
            cardSprites[i] = cardSpritesList[i].sprite;
        }

        if (cardSprites.Length != 5)
        {
            Debug.LogError("Exactly 5 card sprites are required.");
            return false;
        }

        foreach (var sprite in cardSprites)
        {
            string cardName = sprite.name;
            if (cardName.Length < 2)
            {
                Debug.LogError("Invalid card name format.");
                return false;
            }

            // Extract the number/face part (everything after the first character)
            string numberOrFace = cardName.Substring(1);

            // Check if the card is an Ace or a King
            if (numberOrFace == "a" || numberOrFace == "k")
            {
                return true; // An Ace or a King found
            }
        }

        return false; // No Ace or King found
    }

    #region checking for trail formation
    public List<Sprite> FindBestTrail(Sprite card1, Sprite card2)
    {

        // Extract ranks of the fixed cards
        string rank1 = GetCardRank(card1.name);
        string rank2 = GetCardRank(card2.name);

        // Check if the fixed cards form a trail with any other card from the deck
        List<Sprite> trail = FindTrailWithFixedCards(card1, card2);
        if (trail != null)
        {
            return trail;
        }

        // If not, check for any trail of three cards in the remaining deck
        return FindAnyTrail();
    }

    private List<Sprite> FindTrailWithFixedCards(Sprite card1, Sprite card2)
    {
        // If the fixed cards have the same rank, we need a third card of the same rank
        string rank1 = GetCardRank(card1.name);
        string rank2 = GetCardRank(card2.name);

        if (rank1 == rank2)
        {
            foreach (Sprite card in gameDeck)
            {
                if (GetCardRank(card.name) == rank1 && card.name != card1.name && card.name != card2.name)
                {
                    gameDeck.Remove(card);
                    Sprite s1 = gameDeck[Random.Range(0, gameDeck.Count)];
                    gameDeck.Remove(s1);
                    Sprite s2 = gameDeck[Random.Range(0, gameDeck.Count)];
                    gameDeck.Remove(s2);
                    return new List<Sprite> { card, s1, s2 };
                }
                else return null;

            }
        }
        else
        {
            List<Sprite> matchingCards = gameDeck.Where(card => GetCardRank(card.name) == rank1).ToList();
            if (matchingCards.Count >= 2)
            {
                Sprite s1 = gameDeck[Random.Range(0, gameDeck.Count)];
                gameDeck.Remove(s1);
                Sprite s2 = matchingCards[0];
                gameDeck.Remove(s2);
                Sprite s3 = matchingCards[1];
                gameDeck.Remove(s3);
                return new List<Sprite> { s1, s2, s3 };
            }

            // Try to form a trail with card2
            matchingCards = gameDeck.Where(card => GetCardRank(card.name) == rank2).ToList();
            if (matchingCards.Count >= 2)
            {

                Sprite s2 = matchingCards[0];
                gameDeck.Remove(s2);
                Sprite s3 = matchingCards[1];
                gameDeck.Remove(s3);

                Sprite s1 = gameDeck[Random.Range(0, gameDeck.Count)];
                gameDeck.Remove(s1);
                return new List<Sprite> { s1, s2, s3 };
            }
        }
        Debug.Log("nothing is in fixed cards trail");
        // If no trail is found with the fixed cards, return an empty list
        return null;
    }

    private List<Sprite> FindAnyTrail()
    {
        // Generate all combinations of three cards from the remaining deck
        foreach (var combo in GetCombinations(gameDeck, 3))
        {
            if (IsTrail(combo))
            {
                return combo.ToList();
            }
        }
        Debug.Log("nothing is in any trail");

        // If no trail is found, return an empty list
        return null;
    }

    private IEnumerable<IEnumerable<T>> GetCombinations<T>(IEnumerable<T> list, int length)
    {
        return length == 0 ? new[] { Enumerable.Empty<T>() } :
            list.SelectMany((e, i) => GetCombinations(list.Skip(i + 1), length - 1).Select(c => (new[] { e }).Concat(c)));
    }

    private bool IsTrail(IEnumerable<Sprite> cards)
    {
        var ranks = cards.Select(card => GetCardRank(card.name)).Distinct().ToList();
        return ranks.Count == 1;  // A trail requires all ranks to be the same
    }

    // Helper function to extract the rank from the card name (e.g., "s2" -> "2")
    private string GetCardRank(string cardName)
    {
        if (cardName.Substring(1) == "a")
        {
            return "14";
        }

        if (cardName.Substring(1) == "k")
        {
            return "13";
        }

        if (cardName.Substring(1) == "q")
        {
            return "12";
        }

        if (cardName.Substring(1) == "j")
        {
            return "11";
        }
        if (cardName.Substring(1) == "o")
        {
            return "10";
        }
        Debug.Log("card name is : " + cardName);
        return cardName.Substring(1);  // Get the rank from the card's name (skip the first character)
    }
    #endregion

    public List<Sprite> FindThreeCardsOfSameSuit(Sprite card1, Sprite card2, List<Sprite> gameDeck)
    {
        // Extract suits of the fixed cards
        string suit1 = GetCardSuit(card1.name);
        string suit2 = GetCardSuit(card2.name);

        // If the two input cards have the same suit, find one more card of the same suit
        if (suit1 == suit2)
        {
            Sprite matchingCard = gameDeck.FirstOrDefault(card => GetCardSuit(card.name) == suit1);
            if (matchingCard != null)
            {
                Sprite s = matchingCard;
                gameDeck.Remove(s);

                Sprite s1 = gameDeck[Random.Range(0, gameDeck.Count)];
                gameDeck.Remove(s1);

                Sprite s2 = gameDeck[Random.Range(0, gameDeck.Count)];
                gameDeck.Remove(s2);

                // Return the three matching cards
                return new List<Sprite> { s1, s2, s };
            }
        }
        else
        {
            // If the two input cards have different suits, try to find two matching cards with the same suit

            // Find all matching cards for suit1
            List<Sprite> matchingCardsForSuit1 = gameDeck.Where(card => GetCardSuit(card.name) == suit1).ToList();

            // Find all matching cards for suit2
            List<Sprite> matchingCardsForSuit2 = gameDeck.Where(card => GetCardSuit(card.name) == suit2).ToList();

            // Check if there are at least two cards in matchingCardsForSuit1
            if (matchingCardsForSuit1.Count >= 2)
            {


                Sprite s1 = matchingCardsForSuit1[0];
                gameDeck.Remove(s1);

                Sprite s2 = matchingCardsForSuit1[1];
                gameDeck.Remove(s2);

                Sprite s3 = gameDeck[Random.Range(0, gameDeck.Count)];
                gameDeck.Remove(s3);

                // Return the three matching cards
                return new List<Sprite> { s3, s1, s2 };
            }
            // Otherwise, check if there are at least two cards in matchingCardsForSuit2
            else if (matchingCardsForSuit2.Count >= 2)
            {
                Sprite s1 = matchingCardsForSuit2[0];
                gameDeck.Remove(s1);

                Sprite s2 = matchingCardsForSuit2[1];
                gameDeck.Remove(s2);

                Sprite s3 = gameDeck[Random.Range(0, gameDeck.Count)];
                gameDeck.Remove(s3);

                // Return the three matching cards
                return new List<Sprite> { s3, s1, s2 };
            }
            else
            {
                // If no matching pair is found, return null or handle accordingly
                return null;
            }
        }


        Debug.Log("No matching suit found with fixed cards");
        // If no suitable cards are found, return null
        return null;
    }

    // Helper function to extract the suit from the card name (e.g., "s2" -> "s")
    private string GetCardSuit(string cardName)
    {
        return cardName.Substring(0, 1);  // Get the suit from the card's name (first character)
    }





    public List<Sprite> FindThreeCardSequence(Sprite card1, Sprite card2, List<Sprite> gameDeck)
    {
        // Try to find a sequence with both fixed cards
        List<Sprite> sequence = FindSequenceWithFixedCards(card1, card2, gameDeck);
        if (sequence != null)
        {
            return sequence;
        }

        // If no sequence is found, try to find a sequence with card1 only
        sequence = FindSequenceWithOneFixedCard(card1, gameDeck);
        if (sequence != null)
        {
            return sequence;
        }

        // If no sequence is found, try to find a sequence with card2 only
        sequence = FindSequenceWithOneFixedCard(card2, gameDeck);
        if (sequence != null)
        {
            return sequence;
        }

        // If still no sequence is found, try to find any sequence from the deck
        return FindAnySequence(gameDeck);
    }

    private List<Sprite> FindSequenceWithFixedCards(Sprite card1, Sprite card2, List<Sprite> gameDeck)
    {
        int rank1 = int.Parse(GetCardRank(card1.name));
        int rank2 = int.Parse(GetCardRank(card2.name));
        string suit1 = GetCardSuit(card1.name);
        string suit2 = GetCardSuit(card2.name);

        if (suit1 == suit2)
        {
            int minRank = Mathf.Min(rank1, rank2);
            int maxRank = Mathf.Max(rank1, rank2);

            // Check for the card that would complete the sequence
            Sprite matchingCard = gameDeck.FirstOrDefault(card =>
                GetCardSuit(card.name) == suit1 &&
                (int.Parse(GetCardRank(card.name)) == minRank - 1 || int.Parse(GetCardRank(card.name)) == maxRank + 1));

            if (matchingCard != null)
            {
                Sprite s1 = matchingCard;
                gameDeck.Remove(s1);

                Sprite s2 = gameDeck[Random.Range(0, gameDeck.Count)];
                gameDeck.Remove(s2);

                Sprite s3 = gameDeck[Random.Range(0, gameDeck.Count)];
                gameDeck.Remove(s3);

                // Return the three matching cards
                return new List<Sprite> { s3, s1, s2 };
            }
        }

        return null;
    }

    private List<Sprite> FindSequenceWithOneFixedCard(Sprite fixedCard, List<Sprite> gameDeck)
    {
        int fixedRank = int.Parse(GetCardRank(fixedCard.name));
        string fixedSuit = GetCardSuit(fixedCard.name);
        var possibleSequences = gameDeck
            .Where(card => GetCardSuit(card.name) == fixedSuit)
            .Select(card => int.Parse(GetCardRank(card.name)))
            .OrderBy(rank => rank)
            .ToList();

        for (int i = 0; i < possibleSequences.Count - 1; i++)
        {
            int rank1 = possibleSequences[i];
            int rank2 = possibleSequences[i + 1];

            // Check if the fixed card can be part of this sequence
            if (Mathf.Abs(fixedRank - rank1) == 1 || Mathf.Abs(fixedRank - rank2) == 1)
            {
                Sprite card1 = gameDeck.First(card => int.Parse(GetCardRank(card.name)) == rank1 && GetCardSuit(card.name) == fixedSuit);
                gameDeck.Remove(card1);
                Sprite card2 = gameDeck.First(card => int.Parse(GetCardRank(card.name)) == rank2 && GetCardSuit(card.name) == fixedSuit);
                gameDeck.Remove(card2);

                Sprite s = gameDeck[Random.Range(0, gameDeck.Count)];
                gameDeck.Remove(s);



                // Return the three matching cards
                return new List<Sprite> { s, card1, card2 };
            }
        }

        return null;
    }

    private List<Sprite> FindAnySequence(List<Sprite> gameDeck)
    {
        // Group the cards by their suits
        var groupedBySuit = gameDeck.GroupBy(card => GetCardSuit(card.name));

        foreach (var group in groupedBySuit)
        {
            // Extract the ranks and sort them
            var sortedRanks = group.Select(card => int.Parse(GetCardRank(card.name))).OrderBy(rank => rank).ToList();

            // Iterate over the sorted ranks to find a sequence of 3 consecutive cards
            for (int i = 0; i < sortedRanks.Count - 2; i++)
            {
                if (sortedRanks[i + 1] == sortedRanks[i] + 1 && sortedRanks[i + 2] == sortedRanks[i] + 2)
                {
                    // Find the corresponding cards in the original group
                    Sprite card1 = group.First(card => int.Parse(GetCardRank(card.name)) == sortedRanks[i]);
                    gameDeck.Remove(card1);
                    Sprite card2 = group.First(card => int.Parse(GetCardRank(card.name)) == sortedRanks[i + 1]);
                    gameDeck.Remove(card2);

                    Sprite card3 = group.First(card => int.Parse(GetCardRank(card.name)) == sortedRanks[i + 2]);
                    gameDeck.Remove(card3);


                    return new List<Sprite> { card1, card2, card3 };
                }
            }
        }

        // If no sequence is found, return null
        return null;
    }



    public void HighlightTopThreeCards(List<Image> cardSpritesList)
    {
        if (cardSpritesList.Count != 5)
        {
            Debug.LogError("Exactly 5 card sprites are required.");
            return;
        }

        // List to store the card information (suit, value, and associated image)
        List<(char suit, int value, Image image)> cardInfoList = new List<(char, int, Image)>();

        foreach (var sprite in cardSpritesList)
        {
            string cardName = sprite.sprite.name;
            if (cardName.Length < 2)
            {
                Debug.LogError("Invalid card name format.");
                return;
            }

            char suit = cardName[0];
            string valueStr = cardName.Substring(1);
            int value = ParseCardValue(valueStr);

            cardInfoList.Add((suit, value, sprite));
        }

        // Sort the list based on card values in descending order
        cardInfoList = cardInfoList.OrderByDescending(card => card.value).ToList();

        // Change the color of the highest 3 cards
        for (int i = 0; i < cardInfoList.Count; i++)
        {
            if (i < 3)
            {
            }
            else
            {
                cardInfoList[i].image.color = Color.gray;

            }

        }
    }



    public void HighlightHighestCard(List<Image> cardSpritesList)
    {
        if (cardSpritesList.Count != 3)
        {
            Debug.LogError("Exactly 3 card sprites are required.");
            return;
        }

        // List to store the card information (suit, value, and associated image)
        List<(char suit, int value, Image image)> cardInfoList = new List<(char, int, Image)>();

        foreach (var sprite in cardSpritesList)
        {
            string cardName = sprite.sprite.name;
            if (cardName.Length < 2)
            {
                Debug.LogError("Invalid card name format.");
                return;
            }

            char suit = cardName[0];
            string valueStr = cardName.Substring(1);
            int value = ParseCardValue(valueStr);

            cardInfoList.Add((suit, value, sprite));
        }

        // Find the card with the highest value
        var highestCard = cardInfoList.OrderByDescending(card => card.value).First();

        // Change the color of the highest card
        foreach (var card in cardInfoList)
        {
            if (card.Equals(highestCard))
            {
                card.image.color = Color.white; // Gray out the other cards

            }
            else
            {
                card.image.color = Color.gray; // Gray out the other cards
            }
        }
    }




}
