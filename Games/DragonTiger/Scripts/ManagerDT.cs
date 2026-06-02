using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Security.Cryptography;
using System.Text;

public class ManagerDT : MonoBehaviour
{
    public static ManagerDT Instance;
    public Animator[] throwItemAnimators_array;
    public AudioClip noMoreBets_Clip;
    public AudioClip placeYourBets_Clip;
    public AudioClip dragonWins_Clip;
    public AudioClip tie_Clip;
    public AudioClip tigerWins_Clip;
    public AudioSource effect_AudioSource;
    private System.Random random1;
    private System.Random random2;
    private System.Random random3;
    private float updateInterval = 0.5f;
    private int randomValue1 = 0;
    private int randomValue2 = 0;
    private int randomValue3 = 0;
    private bool isGenerating = false;
    public int rvi;

    public TableBotManager botManagerRef;
    public GameObject waitingForNextRound_Obj;
    public GameObject elephantAnim;
    public AudioSource elephantSound;
    public GameObject winPanel;
    public TextMeshProUGUI winPanelAmntTxt;
    RandomHistoryDT historyGeneratorRef;
    public AudioSource clockTickSound;
    public GameObject insufficientFundsObj;
    public AudioSource tigerRoar;
    public AudioSource dragonRoar;
    public AudioSource clawSoundEffect;
    public AudioSource thunderSoundEffect;
    public BetManager betManagerRef;

    public Animator dragonAnimator;
    public Animator tigerAnimator;
    public AudioSource cashCollectSound;
    public Animator thunderAnimator;
    public Animator clawAnimator;
    public AudioSource SingleCoinSound;
    public AudioSource SwordSlashSoundEffectAudioSource;
    public GameObject fire1Object, fire2Object;
    public Animator cardAnimator1;
    public Animator cardAnimator2;
    public GameObject betRoda;
    public Text dragonBetAmountText, tigerBetAmountText, tieBetAmountText;
    public GameObject target1 ,target2;
    public GameObject betStopsPanel, betStartPanel;
    public Text timer_text;
    public Image clockVsImg;
    public Sprite clockSpr;
    public Sprite vsSpr;
    float walletAmount;
    public Text walletText;
    public Sprite cardImg, cardImgRed;
    public int p1po, p2po;
    public Image dragonCard, tigerCard;
    public string dragonCardName, tigerCardName;
    List<Sprite> cardsList;
    public int betAmountDragon, betAmountTiger, betAmountTie;

    public bool _StopAmount;
    // public BurstDT burstScript;
    public SocketManagerDT SocketManagerDTRef;

    public List<GameObject> myCoinsList;
    public Transform myCoinHolder;
    Vector3 recentTouchPos;
    APIs apisRef;
    public enum GameState
    {
        Betting,
        Result,
        Rest,
    }

    void Awake()
    {
        if(Instance != this)
        {
            Instance = this;
        }
    }

    public GameState currentGameState;

    public void RegisterTouch(Vector3 touch)
    {
        recentTouchPos = touch;
    }
    public void LoadDeck()
    {
        System.Object[] loadedSprites = Resources.LoadAll("Deck", typeof(Sprite));

        cardsList = new List<Sprite>();

        // Add each loaded sprite to the list
        foreach (System.Object sprite in loadedSprites)
        {
            cardsList.Add(sprite as Sprite);
        }

    }

    private void Start()
    {
        Debug.Log("apath: " + Application.persistentDataPath);
        LoadDeck();
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        apisRef.FetchWallet();

        Screen.orientation = ScreenOrientation.LandscapeLeft;
        SocketManagerDTRef = FindObjectOfType<SocketManagerDT>();

        historyGeneratorRef = FindObjectOfType<RandomHistoryDT>();
        betManagerRef = FindObjectOfType<BetManager>();
        betStartPanel.SetActive(false);
        // burstScript = FindObjectOfType<BurstDT>();
        betRoda.SetActive(true);
        insufficientFundsObj.SetActive(false);
        dragonCard.sprite = cardImg;
        tigerCard.sprite = cardImgRed;
    }

    public void InitializeBots(List<BotData> data, long roundStartTime)
    {
        if(botManagerRef != null)
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

    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }
    public void CheckForPendingBets()
    {

        if(PlayerPrefs.GetString("DvT_roundId") == SocketManagerDTRef.currentRoundId)
        {
           
            if(PlayerPrefs.GetInt("dt_betsOnDragon") > 0)
            {
                betAmountDragon = PlayerPrefs.GetInt("dt_betsOnDragon");
            }
            if (PlayerPrefs.GetInt("dt_betsOnTiger") > 0)
            {
                betAmountTiger = PlayerPrefs.GetInt("dt_betsOnTiger");
            }
            if (PlayerPrefs.GetInt("dt_betsOnTie") > 0)
            {
                betAmountTie = PlayerPrefs.GetInt("dt_betsOnTie");
            }
           
        }
        else
        {
            PlayerPrefs.SetInt("dt_betsOnDragon", 0);
            PlayerPrefs.SetInt("dt_betsOnTiger", 0);
            PlayerPrefs.SetInt("dt_betsOnTie", 0);

        }
    }

    public void StartBetting(int val, long serverStartTime, string[] seeds)
    {

        PlayEffect(placeYourBets_Clip);
        currentGameState = GameState.Betting;
        botManagerRef.Reset();
        StartCoroutine(GameStartEnum(val, serverStartTime, seeds));
        StartTimerFun(val);
    }



    public void EnableWaitingPanel()
    {
        waitingForNextRound_Obj.SetActive(true);
    }
   
    public IEnumerator GameStartEnum(int remTime, long startTime, string[] seeds)
    {
        Debug.Log("Hurray");
        waitingForNextRound_Obj.SetActive(false);
        fire1Object.SetActive(true);
        fire2Object.SetActive(true);
        betAmountDragon = 0;
        betAmountTiger = 0;
        betAmountTie = 0;
        if (remTime > 10)
        {
      
            betStartPanel.SetActive(true);
        }
        SwordSlashSoundEffectAudioSource.Play();
        
        yield return new WaitForSeconds(2f);
        betStartPanel.SetActive(false);
        _StopAmount = false;
        isGenerating = true;
        BetAmountIncrease(startTime, seeds);
        betRoda.SetActive(false);
        // StartCoroutine(burstScript.AnimStart(remTime, 15));
        

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



        dragonBetAmountText.text = "0/0";
        tigerBetAmountText.text = "0/0";

        betStopsPanel.SetActive(false);


        int t;
        List<int> excludedValues = new List<int>();
        for(int i = 0; i<3; i++)
        {
            do
            {
                t = UnityEngine.Random.Range(0, 52);
            } while (excludedValues.Contains(t));
            excludedValues.Add(t);
        }

        for (int i = 0; i < 3; i++)
        {
            do
            {
                t = UnityEngine.Random.Range(0, 52);
            } while (excludedValues.Contains(t));
            excludedValues.Add(t);
        }
        if(clockVsImg.sprite != clockSpr)
        {
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
      
    }
    Coroutine timerEnumRef;
    public void StartTimerFun(float val)
    {
        if(timerEnumRef != null)
        {
            StopCoroutine(timerEnumRef);
        }
        timerEnumRef = StartCoroutine(StartTimer(val));
    }



    void BetAmountIncrease(long startTime, string[] seeds)
    {
        randomValue1 = 0;
        randomValue2 = 0;
        randomValue3 = 0;

        rvi = 0;
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float elapsedTimeInSeconds = (currentTimestamp - startTime) / 1000f;

        // Initialize the random generators with consistent hashes
        random1 = new System.Random(GenerateConsistentHash(seeds[0]));
        random2 = new System.Random(GenerateConsistentHash(seeds[1]));
        random3 = new System.Random(GenerateConsistentHash(seeds[2]));

        // Skip ahead in the random sequences based on elapsed time
        int stepsToSkip = Mathf.FloorToInt(elapsedTimeInSeconds * 2); // 5 steps per second
        for (int i = 0; i < stepsToSkip; i++)
        {
            if(rvi < 37)
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
            return BitConverter.ToInt32(hashBytes, 0);
        }
    }

    private IEnumerator IncreaseRandomValues()
    {        
        while (isGenerating)
        {

            int increment1 = random1.Next(1, 10);
            int increment2 = random2.Next(1, 10);
            int increment3 = random3.Next(1, 10);
            if(rvi < 37)
            {
                rvi++;
                randomValue1 += increment1;
                randomValue2 += increment2;
                randomValue3 += increment3;
            }
          

            int k = (randomValue1 * 990);
            int l = (randomValue2 * 990);
            int m = (randomValue3 * 90);

            dragonBetAmountText.text = $"<color=yellow>{betAmountDragon}</color><color=#02ccfe>/{k}</color>";
            tigerBetAmountText.text = $"<color=yellow>{betAmountTiger}</color><color=#02ccfe>/{l}</color>";
            tieBetAmountText.text = $"<color=yellow>{betAmountTie}</color><color=#02ccfe>/{m}</color>";

            yield return new WaitForSeconds(updateInterval);
        }
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

    public void DisplayCards(string dragonCard, string tigerCard, char winner, int[] botWinArray)
    {
        Debug.Log("Displaying Cards");
        // burstScript.StopAnim();
        isGenerating = false;
        currentGameState = GameState.Result;
        PlayEffect(noMoreBets_Clip);

        StartCoroutine(ShowCardsEnum(dragonCard, tigerCard, winner, botWinArray));
    }
    public void ThrowItemAnim(int player, int item)
    {
        StartCoroutine(ThrowItemAnimEnum(player, item));
    }

    IEnumerator ThrowItemAnimEnum(int player, int item)
    {
        throwItemAnimators_array[player].SetInteger("val", item );
        yield return new WaitForSeconds(0.5f);
        throwItemAnimators_array[player].SetInteger("val", 0);
    }

    public void ClearAllBets()
    {
        Debug.Log("radhey");
        if(betAmountDragon <= 0 && betAmountTie <= 0 && betAmountTiger <= 0) return;
        walletAmount += betAmountDragon + betAmountTie + betAmountTiger;
        walletText.text = "₹" + walletAmount.ToString("F2");

        betAmountTie = 0; 
        betAmountDragon = 0; 
        betAmountTiger = 0;
        PlayerPrefs.SetInt("dt_betsOnDragon", betAmountDragon);
        PlayerPrefs.SetInt("dt_betsOnTie", betAmountTie);
        PlayerPrefs.SetInt("dt_betsOnTiger", betAmountTiger);
        SocketManagerDTRef.ClearAllBets();
        ClearMyCoins();
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

        if (currentGameState != GameState.Betting) return;
        int val = betManagerRef.betVal;
        if(val <= walletAmount)
        {
            walletAmount -= val;
            SingleCoinSound.Play();
            PlayerPrefs.SetString("DvT_roundId", SocketManagerDTRef.currentRoundId);
            walletText.text = "₹" + walletAmount.ToString("F2");
            InstantiateCoin();

            switch (betOn){
                case 0:
                    //tie case
                    betAmountTie += val;
                    SocketManagerDTRef.SendBetDataToServer(betOn, val);
                    PlayerPrefs.SetInt("dt_betsOnTie", betAmountTie);
                    break;
                case 1:
                    betAmountDragon += val;
                    SocketManagerDTRef.SendBetDataToServer(betOn, val);
                    PlayerPrefs.SetInt("dt_betsOnDragon", betAmountDragon);

                    break;
                case 2:
                    betAmountTiger += val;
                    SocketManagerDTRef.SendBetDataToServer(betOn, val);
                    PlayerPrefs.SetInt("dt_betsOnTiger", betAmountTiger);
                    break;
            }
        }
        else { 
            insufficientFundsObj.SetActive(true);
        }


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


    public IEnumerator ShowCardsEnum(string dragon_card, string tiger_card, char winner, int[] winArray)
    {

        int k = UnityEngine.Random.Range(0, 1000);

        yield return new WaitForSeconds(1f);
        fire1Object.SetActive(false);
        fire2Object.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        cardAnimator1.SetBool("_flip", true);
        cardAnimator2.SetBool("_flip", true);
        yield return new WaitForSeconds(0.3f);

        dragonCard.sprite = GetSpriteByName(dragon_card);
        tigerCard.sprite = GetSpriteByName(tiger_card);
        
        yield return new WaitForSeconds(0.5f);
        cardAnimator1.SetBool("_flip", false);
        cardAnimator2.SetBool("_flip", false);
   
        StartCoroutine(WinnerDisplay(winner, winArray));
        Debug.Log("WinnerDisplassssssy");


    }


    public IEnumerator WinnerDisplay(char val, int[] winArray)
    {
        float winAmnt = 0;
        historyGeneratorRef.TrendGenerator(val);
        switch (val)
        {
            case 'd':
                PlayEffect(dragonWins_Clip);
                dragonAnimator.SetBool("_isWin", true);
                dragonRoar.Play();
                yield return new WaitForSeconds(0.5f);
                thunderAnimator.SetBool("_thunder", true);
                thunderSoundEffect.Play();
                winAmnt += betAmountDragon * 2f;
                break;
            case 't':
                PlayEffect(tigerWins_Clip);
                tigerAnimator.SetBool("_isWin", true);
                tigerRoar.Play();
                yield return new WaitForSeconds(0.5f);
                clawAnimator.SetBool("_claw", true);
                clawSoundEffect.Play();
                winAmnt += betAmountTiger * 2f;
                break;
            case 'o':
                PlayEffect(tie_Clip);
                elephantAnim.SetActive(true);
                elephantSound.Play();
                yield return new WaitForSeconds(2f);
                elephantAnim.SetActive(false);
                winAmnt += betAmountTie * 9f;
                winAmnt += betAmountDragon * 0.5f;
                winAmnt += betAmountTiger * 0.5f;
                break;

        }
  
        yield return new WaitForSeconds(1);



        tigerAnimator.SetBool("_isWin", false);
        dragonAnimator.SetBool("_isWin", false);

        yield return new WaitForSeconds(0.5f);

        switch (val)
        {
            case 'd':
                thunderAnimator.SetBool("_thunder", false);
                // burstScript.Winnerr(0, null);
                break;
            case 't':
                clawAnimator.SetBool("_claw", false);
                // burstScript.Winnerr(2, null);
                break;
            case 'o':
                // burstScript.Winnerr(1, null);
                break;

        }
       ClearMyCoins();
        yield return new WaitForSeconds(2f);

        botManagerRef.BotWin(winArray);


        if (winAmnt > 0)
        {
            Debug.Log("win amnt : " + winAmnt);
            winPanel.SetActive(true);
            winPanelAmntTxt.text = "+" + winAmnt.ToString("F1");
            yield return new WaitForSeconds(0.5f);
            apisRef.FetchWallet();
            yield return new WaitForSeconds(1f);
            winPanel.SetActive(false);
        }


        yield return new WaitForSeconds(2);
        botManagerRef.OffWins();
        dragonCard.sprite = cardImg;
        tigerCard.sprite = cardImgRed;




        yield return new WaitForSeconds(0.5f);
        cardAnimator1.SetBool("_flip", false);
        cardAnimator2.SetBool("_flip", false);
        yield return new WaitForSeconds(0.5f);
    }


    public void PlayEffect(AudioClip clip)
    {
        effect_AudioSource.Stop();
        effect_AudioSource.clip = clip;
        effect_AudioSource.Play();
    }
  
}
