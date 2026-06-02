
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;
using DG.Tweening;
using Features.Lobby.Integration;


public class ManagerBac : MonoBehaviour
{
    public static ManagerBac Instance;
    public Animator[] throwItemAnimators_array;

    private System.Random random1;
    private System.Random random2;
    private System.Random random3;
    private System.Random random4;
    private System.Random random5;



    private float updateInterval = 0.5f; // 5 times per second
    private int randomValue1 = 0;
    private int randomValue2 = 0;
    private int randomValue3 = 0;
    private int randomValue4 = 0;
    private int randomValue5 = 0;


    private bool isGenerating = false;
    public int rvi;

    public GameObject waitingForNextRound;
    SocketManagerBac socketRef;
    public GameObject winPanel;
    public Text winPanelText;
    public AudioSource winAudio;

    public AudioSource coinSound;
    public TextMeshProUGUI randomBetText1;
    public TextMeshProUGUI randomBetText2;
    public TextMeshProUGUI randomBetText3;
    public TextMeshProUGUI randomBetText4;
    public TextMeshProUGUI randomBetText5;
    public GameObject betstartPanel;
    public BurstBaccarat burstRef;
    BetManagerBac betManagerRef;
    public GameObject winnerAPImg;
    public GameObject winnerAImg;
    public GameObject winnerBPImg;
    public GameObject winnerBImg;
    public GameObject winnerTieImg;
    public TextMeshProUGUI playerSumText, bankerSumText;
    public int playerSum, bankerSum;
    public GameObject betRoda;
    //public Text player1BetAmount, player2BetAmount;
    public GameObject target1, target2;
    public GameObject betStopsPanel;
    public Text timer_text;
    public Text walletText;
    public float walletAmount;
    public Sprite cardImg;
    public int p1po, p2po;
    public Image p1i1, p1i2, p1i3, p2i1, p2i2, p2i3;
    public List<Sprite> cardsList;
    List<Sprite> p1CardList;
    List<Sprite> p2CardList;
    public int totalBidAmount;
    public int betNumber, betAmountPlayerPair, betAmntTie, betAmountBankerPair, betAmountPlayer, betAmountBanker;
    public GameObject winnerPannel;
    public Text winnerText;
    // Start is called before the first frame update
    TableBotManager botManagerRef;
    public int[] botsWinArray;
    APIs apisRef;
    string gamePhase = "";


    public AudioClip noMoreBets_Clip;
    public AudioClip placeYourBets_Clip;

    public AudioSource effect_AudioSource;
    
    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }

    public void Lobby()
    {
        SceneManager.LoadScene(1);
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



    private void Start()
    {
        botsWinArray = new int[] { 0, 0, 0, 0, 0, 0 };
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;

        botManagerRef = FindObjectOfType<TableBotManager>();
        socketRef = FindObjectOfType<SocketManagerBac>();
        LoadDeck();
        winPanel.SetActive(false);
        betManagerRef = FindObjectOfType<BetManagerBac>();
        betRoda.SetActive(true);
        walletAmount = BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;
        walletText.text = "₹" + walletAmount.ToString("F2");
        apisRef.FetchWallet();

    }

    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }

    public void LoadDeck()
    {
        Object[] loadedSprites = Resources.LoadAll("Deck", typeof(Sprite));

        cardsList = new List<Sprite>();

        // Add each loaded sprite to the list
        foreach (Object sprite in loadedSprites)
        {
            cardsList.Add(sprite as Sprite);
        }

    }


    #region Socket functions 

    public void EnableWaitingPanel()
    {
        waitingForNextRound.SetActive(true);
    }

    public void StartBetting(int val, long startTime, string[] seeds)
    {
        PlayEffect(placeYourBets_Clip);

        waitingForNextRound.SetActive(false);
        BetAmountIncrease(startTime, seeds);
        gamePhase = "Betting";
        GameStart(val);
        ResetCardsPos();
    }
    public void StopBetting()
    {
        isGenerating = false;
        gamePhase = "";
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


    public void DisplayCards(List<string> playerCards, List<string> dealerCards, char winner,int playerPair, int bankerPair, int[] botWinArray)
    {
        for(int i = 0; i<6; i++)
        {
            botsWinArray[i] = botWinArray[i];
        }
        PlayEffect(noMoreBets_Clip);

        gamePhase = "Result";
        botsWinArray = new int[6];
        for(int i = 0; i< botWinArray.Length; i++)
        {
            botsWinArray[i] = botWinArray[i];
        }
        if(p1CardList == null)
        {
            p1CardList = new List<Sprite>();
        }

        if (p2CardList == null)
        {
            p2CardList = new List<Sprite>();
        }

        for (int i = 0; i < playerCards.Count; i++)
        {
            p1CardList.Add(GetSpriteByName(playerCards[i]));
        }
        
        for (int i = 0; i < dealerCards.Count; i++)
        {
            p2CardList.Add(GetSpriteByName(dealerCards[i]));
        }
        BetCheck(playerPair, bankerPair);

    }

    public Sprite GetSpriteByName(string spriteName)
    {
        foreach (Sprite sprite in cardsList)
        {
            if (sprite.name == spriteName)
            {
                return sprite; // Return the matching sprite
            }
        }

        return null; // Return null if no sprite matches
    }

    #endregion
    public void GameStart(int val)
    {
        p1CardList = new List<Sprite>();
        p2CardList = new List<Sprite>();

        winnerAImg.SetActive(false);
        winnerAPImg.SetActive(false);
        winnerBPImg.SetActive(false);
        winnerBImg.SetActive(false);
        winnerTieImg.SetActive(false);
        playerSum = 0;
        playerSumText.text = "";
        bankerSum = 0;
        bankerSumText.text = "";


        botManagerRef.Reset();
        betRoda.SetActive(false);
        winnerPannel.SetActive(false);
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
        //player1BetAmount.text = "0/0";
        //player2BetAmount.text = "0/0";
        betAmountBankerPair = 0;
        betAmntTie = 0;
        betAmountPlayerPair = 0;
        betAmountPlayer = 0;
        betAmountBanker = 0;
        betNumber = -1;
        betStopsPanel.SetActive(false);
        totalBidAmount = 0;
        p1CardList.Clear();
        p2CardList.Clear();
        p1i1.sprite = cardImg;
        p1i2.sprite = cardImg;
        p1i3.sprite = cardImg;
        p2i1.sprite = cardImg;
        p2i2.sprite = cardImg;
        p2i3.sprite = cardImg;
       
      
        StartCoroutine(StartTimer(val -3f));
    }


    private IEnumerator StartTimer(float duration)
    {
        if (duration > 15)
        {
            betstartPanel.SetActive(true);
        }
        yield return new WaitForSeconds(1f);
        betstartPanel.SetActive(false);
        StartCoroutine(burstRef.AnimStart(((int)duration), 18));



        for (int i = 0; i <= duration; i++)
        {
            yield return new WaitForSeconds(1f);
            timer_text.text = (duration - i).ToString();
        }



        // Wait for the specified duration

        burstRef.StopAnim();
        betStopsPanel.SetActive(true);
        yield return new WaitForSeconds(1);
        betStopsPanel.SetActive(false);
        int k = betAmntTie + betAmountBanker + betAmountBankerPair + betAmountPlayer + betAmountPlayerPair;
  
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
        random4 = new System.Random(GenerateConsistentHash(seeds[3]));
        random5 = new System.Random(GenerateConsistentHash(seeds[4]));
 


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



            if (rvi < 37)
            {
                rvi++;
                randomValue1 += increment1;
                randomValue2 += increment2;
                randomValue3 += increment3;
                randomValue4 += increment4;
                randomValue5 += increment5;
            }

            int k = (randomValue1 * 440);
            int l = (randomValue2 * 440);
            int m = (randomValue3 * 770);
            int n = (randomValue4 * 40);
            int o = (randomValue5 * 770);

            randomBetText1.text = $"<color=yellow>{betAmountBankerPair}</color><color=#02ccfe>/{k}</color>";
            randomBetText2.text = $"<color=yellow>{betAmountPlayerPair}</color><color=#02ccfe>/{l}</color>";
            randomBetText3.text = $"<color=yellow>{betAmountPlayer}</color><color=#02ccfe>/{m}</color>";
            randomBetText4.text = $"<color=yellow>{betAmntTie}</color><color=#02ccfe>/{n}</color>";
            randomBetText5.text = $"<color=yellow>{betAmountBanker}</color><color=#02ccfe>/{o}</color>";

            yield return new WaitForSeconds(updateInterval);
        }
    }


    public void BetCheck(int playerPair, int bankerPair)
    {
        betRoda.SetActive(true);
        StartCoroutine(ShowCards(playerPair, bankerPair));
    }

    public void Check1()
    {
        p1i1.sprite = p1CardList[0];
        p1i2.sprite = p1CardList[1];
        p1i3.sprite = p1CardList[2];

    }
    public void Check2()
    {
        p2i1.sprite = p2CardList[0];
        p2i2.sprite = p2CardList[1];
        p2i3.sprite = p2CardList[2];


    }

    public void ClearAllBets()
    {
                if(gamePhase != "Betting")return;

        if (betAmntTie <= 0 && betAmountBanker <= 0 && betAmountBankerPair <=0 && betAmountPlayer <= 0 && betAmountPlayerPair <= 0) return;
        walletAmount += betAmountBankerPair + betAmntTie + betAmountBankerPair + betAmountPlayer + betAmountPlayerPair;
        walletText.text = "₹" + walletAmount.ToString("F2");

        betAmountBanker = betAmountBankerPair = betAmntTie = betAmountPlayer = betAmountPlayerPair = 0;
        socketRef.ClearAllBets();
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
        if (val <= walletAmount)
        {
            coinSound.Play();
            walletAmount -= val;
            walletText.text = "₹" + walletAmount.ToString("F2");
            InstantiateCoin();

            switch (betOn)
            {
                case 1:
                    betAmountBankerPair += val;
                  
                    break;
                case 3:
                    betAmntTie += val;
                   

                    break;
                case 2:
                    betAmountPlayerPair += val;
                    
                    break;
                case 4:
                    betAmountPlayer += val;
                   
                    break;
                case 5:
                    betAmountBanker += val;
                   

                    break;
            }

            socketRef.SendBetDataToServer(betOn, val);


        }
    }



public IEnumerator ShowCards(int playerPair, int bankerPair)
{
    yield return new WaitForSeconds(0.5f);

    // ===== PLAYER CARD 1 =====
    yield return DealAndFlip(p1i1, p1CardList[0], p1Slot1);

    playerSum += GetCardValue(p1CardList[0].name.Substring(1));
    playerSumText.text = playerSum.ToString();

    yield return new WaitForSeconds(0.4f);

    // ===== BANKER CARD 1 =====
    yield return DealAndFlip(p2i1, p2CardList[0], p2Slot1);

    bankerSum += GetCardValue(p2CardList[0].name.Substring(1));
    bankerSumText.text = bankerSum.ToString();

    yield return new WaitForSeconds(0.4f);

    // ===== PLAYER CARD 2 =====
    yield return DealAndFlip(p1i2, p1CardList[1], p1Slot2);

    playerSum += GetCardValue(p1CardList[1].name.Substring(1));
    if (playerSum >= 10) playerSum -= 10;
    playerSumText.text = playerSum.ToString();

    yield return new WaitForSeconds(0.4f);

    // ===== BANKER CARD 2 =====
    yield return DealAndFlip(p2i2, p2CardList[1], p2Slot2);

    bankerSum += GetCardValue(p2CardList[1].name.Substring(1));
    if (bankerSum >= 10) bankerSum -= 10;
    bankerSumText.text = bankerSum.ToString();

    yield return new WaitForSeconds(0.4f);

    // ===== OPTIONAL THIRD CARDS =====
    if (p1CardList.Count > 2)
    {
        yield return DealAndFlip(p1i3, p1CardList[2], p1Slot3);

        playerSum += GetCardValue(p1CardList[2].name.Substring(1));
        if (playerSum >= 10) playerSum -= 10;
        playerSumText.text = playerSum.ToString();

        yield return new WaitForSeconds(0.4f);
    }

    if (p2CardList.Count > 2)
    {
        yield return DealAndFlip(p2i3, p2CardList[2], p2Slot3);

        bankerSum += GetCardValue(p2CardList[2].name.Substring(1));
        if (bankerSum >= 10) bankerSum -= 10;
        bankerSumText.text = bankerSum.ToString();

        yield return new WaitForSeconds(0.4f);
    }

    WinnerChk(playerPair, bankerPair);
}


    int GetCardValue(string value)
    {
        if (value == "o")
            return 0;
        else if (value == "j")
            return 0;
        else if (value == "q")
            return 0;
        else if (value == "k")
            return 0;
        else if (value == "a")
            return 1;
        else
        {
            int parsedValue;
            if (int.TryParse(value, out parsedValue))
            {
                return parsedValue;
            }
            else
            {
                // Handle the case where the value is not a valid integer
                Debug.LogError("Invalid card value: " + value);
                return 0; // or any default value
            }
        }
    }





    public void WinnerChk(int playerPair, int bankerPair)
    {
        float winAmount = 0;
        if(playerPair == 1)
        {
            winnerAPImg.SetActive(true);
            winAmount += 12 * betAmountPlayerPair;
        }
        if (bankerPair == 1)
        {
            winnerBPImg.SetActive(true);
            winAmount += 12 * betAmountBankerPair;
        }
        if (playerSum > bankerSum)
        {
            winnerAImg.SetActive(true);
            winAmount += 2 * betAmountPlayer;
            walletText.text = "₹" + walletAmount.ToString("F2");


        }
        else if (playerSum < bankerSum)
        {
            winnerBImg.SetActive(true);
            winAmount += betAmountBanker * 2;
            walletText.text = "₹" + walletAmount.ToString("F2");
        }
        else
        {
            winnerTieImg.SetActive(true);
            winAmount += betAmntTie * 9;
            winAmount += betAmountBanker;
            winAmount += betAmountPlayer;
            walletText.text = "₹" + walletAmount.ToString("F2");
        }
        if (p1CardList[0].name == p1CardList[1].name)
        {
            winnerAPImg.SetActive(true);
            winAmount += 3 * betAmountPlayerPair;
            walletText.text = "₹" + walletAmount.ToString("F2");
        }
        if (p2CardList[0].name == p2CardList[1].name)
        {
            winnerBPImg.SetActive(true);
            winAmount += 3 * betAmountBankerPair;
            walletText.text = "₹" + walletAmount.ToString("F2");
        }

        if (winAmount > 0)
        {
            walletAmount += winAmount;
            winPanel.SetActive(true);
            winPanelText.text = "You Win\n" + winAmount.ToString("F2");
            winAudio.Play();
            walletText.text = walletAmount.ToString("F2");
        }
        if(betAmntTie + betAmountBanker + betAmountBankerPair + betAmountPlayer + betAmountPlayerPair > 0)
        {
            apisRef.FetchWallet();
        }
        StartCoroutine(WinnerDisplaly());
    }

    private IEnumerator WinnerDisplaly()
    {

        yield return new WaitForSeconds(2);
        botManagerRef.BotWin(botsWinArray);
        winPanel.SetActive(false);

        yield return new WaitForSeconds(2f);
       ClearMyCoins();
        winnerAImg.SetActive(false);
        winnerBImg.SetActive(false);
        winnerTieImg.SetActive(false);
        winnerAPImg.SetActive(false);
        winnerBPImg.SetActive(false);
        burstRef.MoveAllcoinsBack();
    }

    public void PlayEffect(AudioClip clip)
    {
        Debug.Log("clip " + clip);
        effect_AudioSource.Stop();
        effect_AudioSource.clip = clip;
        effect_AudioSource.Play();
    }











    // ADD THESE IN ManagerBac
public Transform dealerPoint;

public Transform p1Slot1;
public Transform p1Slot2;
public Transform p1Slot3;

public Transform p2Slot1;
public Transform p2Slot2;
public Transform p2Slot3;




private IEnumerator DealAndFlip(Image cardImage, Sprite newSprite, Transform target)
{
    // 1. Start from dealer
    cardImage.transform.position = dealerPoint.position;
    // cardImage.transform.rotation = Quaternion.Euler(0, 0, 0);
    cardImage.sprite = cardImg;   // back card

    // 2. Move to target
    yield return cardImage.transform
        .DOMove(target.position, 0.35f)
        .SetEase(Ease.OutCubic)
        .WaitForCompletion();

    // 3. Flip animation by code
    yield return cardImage.transform
        .DORotate(new Vector3(0, 90, 0), 0.12f)
        .WaitForCompletion();

    // change sprite at mid
    cardImage.sprite = newSprite;

    yield return cardImage.transform
        .DORotate(new Vector3(0, 0, 0), 0.12f)
        .WaitForCompletion();
}

public void ResetCardsPos()
    {
        p1i1.transform.position = dealerPoint.position;
        p1i2.transform.position = dealerPoint.position;
        p1i3.transform.position = dealerPoint.position;
        p2i1.transform.position = dealerPoint.position;
        p2i2.transform.position = dealerPoint.position;
        p2i3.transform.position = dealerPoint.position;
    }










}




