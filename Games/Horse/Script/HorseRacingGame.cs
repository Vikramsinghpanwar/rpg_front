using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public class HorseRacingGame : MonoBehaviour
{
    public enum GameState
    {
        Betting,
        Result,
        Rest,
    }

    GameState gamePhase;
    public TextMeshProUGUI jackpotTxt;
    int[] botWinArray;
    
    public Animator[] throwItemAnimators_array;
    private System.Random random1;
    private System.Random random2;
    private System.Random random3;
    private System.Random random4;
    private System.Random random5;
    private System.Random random6;

    private float updateInterval = 0.5f; // 5 times per second
    private int randomValue1 = 0;
    private int randomValue2 = 0;
    private int randomValue3 = 0;
    private int randomValue4 = 0;
    private int randomValue5 = 0;
    private int randomValue6 = 0; 
    
    private bool isGenerating = false;
    public int rvi;
    public GameObject[] winGlow_ObjArray;
    public TableBotManager botManagerRef;
    public Animator[] horseAnimation;
    public GameObject winhorse1;
    public GameObject winhorse2;
    public GameObject winhorse3;
    public GameObject winhorse4;
    public GameObject winhorse5;
    public GameObject winhorse6;
    public Image backgroundImage; 
    public float bgMoveFactor = 0.5f;
    public float raceDistance = 800f; 
    public float raceDuration = 8f;

    public TextMeshProUGUI[] betAmountStatus_Txt_Array;
    public Image[] horseImages;  
    public TextMeshProUGUI[] horseMultiplierText ,horseMultiplierTextOnGround , winMultiplierText; 
    public GameObject leading_HorseLine_IMG;
    public TextMeshProUGUI leadingHorseNumber_TMP;
    public GameObject startPointImage , secoundHorseMultiplierPanelOnGround;

    private Vector3 startExtraPosition; 
    private Vector3 bgStartPos; 

    

    public TextMeshProUGUI timer_text; 
    public GameObject countdownPanel;
   

    public bool isBettingAllowed = false; 

   
    private float[] horseMultipliers  = new float[6];
  
    private Vector3[] originalPositions; // Array to store original positions of horses

    private float[] betsAmount_Array; 
    private  int horseCount = 6;

    public Resulthistroy resulthistroy;

    public GameObject waitingForNext;

    BurstHR burstRef;

    SocketManagerHR socketManagerHR;

    public GameObject showWinPanel;
    public TextMeshProUGUI showWinText;
    
    float totalWinnings = 0;

    bool isRaceRunning = false;
    BetManager betManagerRef;

    float walletAmount;
    public Text walletText;
    APIs apisRef;

    public AudioSource winAudioSource;
    public float horseStartPos_X;
    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    void Start()
    {
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        apisRef.FetchWallet();
        betManagerRef = FindFirstObjectByType<BetManager>();
        socketManagerHR = FindObjectOfType<SocketManagerHR>();
        botManagerRef = FindObjectOfType<TableBotManager>();
        burstRef = FindObjectOfType<BurstHR>();
        betManagerRef = FindObjectOfType<BetManager>();
        StoreOriginalPositions();
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

    #region socket 
    public void StartBetting(float remTime,long startTime, string[] seeds, long jakcpotVal)
    {
        ResetBetAmount();
        gamePhase = GameState.Betting;
        jackpotTxt.text = "Jackpot : " + jakcpotVal;
        betsAmount_Array = new float[6];
        waitingForNext.SetActive(false);
        StartCoroutine(GameLoop(startTime, seeds));
        StartCoroutine(StartTimer(remTime));

        StartCoroutine(StartCountdown(remTime));
        secoundHorseMultiplierPanelOnGround.SetActive(true);

        isBettingAllowed = true;
        countdownPanel.SetActive(true);
        StartCoroutine(burstRef.AnimStart(remTime + 1, 15));
        HorseIdleAnimation();
        for (int i = 0; i < horseImages.Length; i++)
        {
            horseImages[i].transform.localPosition = new Vector3(horseStartPos_X, horseImages[i].transform.localPosition.y, horseImages[i].transform.localPosition.y);
        }
    }


    public void GameResult(int winIndex, int[] botWA)
    {
        if (waitingForNext.activeInHierarchy) return;
        
        botWinArray = new int[6];
        for (int i = 0; i < botWA.Length; i++)
        {
            botWinArray[i] = botWA[i];
        }
        Debug.Log("successfully bot winArray initialized");
        isGenerating = false;
        StartCoroutine(MoveHorses(winIndex));
    }

    #endregion
    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }

    public void UpdateHistory(int[] history)
{
    
    resulthistroy.InitializeResults(history.Length); 
        for(int i = history.Length -1 ; i>0; i--)
        {
            resulthistroy.AddResultToHistory(history[i]);
        }
  
}

    private void StoreOriginalPositions()
    {
        originalPositions = new Vector3[horseImages.Length];
        for (int i = 0; i < horseImages.Length; i++)
        {
            originalPositions[i] = horseImages[i].transform.localPosition;
              startExtraPosition = leading_HorseLine_IMG.transform.localPosition;
               bgStartPos = backgroundImage.transform.localPosition;
        }
        

      
    }

    IEnumerator GameLoop(long startTime, string[] seeds)
    {
          winhorse1.SetActive(false);  
                    winhorse2.SetActive(false);
                    winhorse3.SetActive(false);  
                    winhorse4.SetActive(false);
                    winhorse5.SetActive(false);  
                    winhorse6.SetActive(false);


        yield return new WaitForSeconds(1f);
        BetAmountIncrease(startTime, seeds);

        yield return null;                   
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
                //----------------------------
                //countDownRef.Hit((int)remTime);
            }
        }
    }


    IEnumerator StartCountdown(float timeRem )
    {
       
   

        yield return new WaitForSeconds(timeRem + 1);
        timer_text.text = "0";
        countdownPanel.SetActive(false);


        isBettingAllowed = false;
  
        burstRef.StopAnim();
    }

    public void HorseIdleAnimation()
    {
        if (!isRaceRunning) // Only play the idle animation if the race is not running
        {
            foreach (var animator in horseAnimation)
            {
                StartCoroutine(HorseIdleAnimationEnum(animator));                
            }
        }
    }



    public IEnumerator HorseIdleAnimationEnum(Animator animator)
    {
        animator.speed = 1f;
        animator.SetBool("_run", false);
        yield return new WaitForSeconds(Random.Range(0, 3f));
        animator.SetBool("_idle", true);
    }
    private void playRunAnm(){
       if (isRaceRunning) // Only play the run animation if the race is running
        {
            foreach (var animator in horseAnimation)
            {
                StartCoroutine(HorseRunAnimationEnum(animator));
            }
        }
    }

    public IEnumerator HorseRunAnimationEnum(Animator animator)
    {
        animator.SetBool("_idle", false);
        animator.speed = Random.Range(0.8f, 1.2f);
        animator.SetBool("_run", true);
        yield return null;

    }

    IEnumerator MoveHorses(int winIndex)
    {
        isRaceRunning = true;
        playRunAnm();
        startPointImage.SetActive(false);
        leading_HorseLine_IMG.SetActive(true);
        secoundHorseMultiplierPanelOnGround.SetActive(false);

        float elapsedTime = 0f;
        Vector3[] startPositions = new Vector3[horseImages.Length];

        // Save start positions
        for (int i = 0; i < horseImages.Length; i++)
        {
            startPositions[i] = horseImages[i].transform.localPosition;
        }

        int leadingHorseID = Random.Range(0, horseImages.Length); // Start with a random horse as the leader

        while (elapsedTime < raceDuration)
        {
            int previousLeadingHorseID = leadingHorseID;
            float remainingTime = raceDuration - elapsedTime;

            for (int i = 0; i < horseImages.Length; i++)
            {
                float t = elapsedTime / raceDuration;
                float randomOffset = Mathf.Sin(elapsedTime * 1f + i) * 20f; // Random movement to simulate racing

                if (elapsedTime >= 1f && i == winIndex)
                {
                    // Boost the leading horse
                    float surgeFactor = Mathf.Lerp(1f, 1.1f, (elapsedTime - 1f) / (raceDuration - 1f));
                    horseImages[i].transform.localPosition = new Vector3(
                        startPositions[i].x + raceDistance * t * surgeFactor + randomOffset,
                        startPositions[i].y,
                        startPositions[i].z
                    );
                    leading_HorseLine_IMG.transform.localPosition = new Vector3(
                        startExtraPosition.x + raceDistance * t * surgeFactor + randomOffset,
                        startExtraPosition.y,
                        startExtraPosition.z
                    );
                }
                else
                {
                    horseImages[i].transform.localPosition = new Vector3(
                        startPositions[i].x + raceDistance * t + randomOffset,
                        startPositions[i].y,
                        startPositions[i].z
                    );
                }
            }

         
            // Determine new leader based on progress or random chance
            leadingHorseID = GetLeadingHorseID();
            UpdateLeadingHorseVisibility(leadingHorseID, previousLeadingHorseID);

            // Update the background position to simulate movement
            backgroundImage.transform.localPosition = new Vector3(
                bgStartPos.x - raceDistance * (elapsedTime / raceDuration) * bgMoveFactor,
                bgStartPos.y,
                bgStartPos.z
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach(Animator a in horseAnimation)
        {
            a.speed = 0.01f;
        }
        //winLine.SetActive(true);
        yield return new WaitForSeconds(2f);
     
        foreach (Animator a in horseAnimation)
        {
            a.speed = 1f;
        }

        FinalizeHorsePositions(startPositions);
        List<int> emptyList = new List<int>();
        burstRef.Winnerr(winIndex, emptyList);
        resulthistroy.AddResultToHistory(winIndex);
        WinHorseNumberShow(winIndex);
        HorseIdleAnimation();
        CalculatePayout(winIndex);
        ResetGame();
        for (int i = 0; i< horseImages.Length; i++)
        {
            horseImages[i].transform.localPosition = new Vector3(1000f, startPositions[i].y, startPositions[i].z);
        }

        yield return new WaitForSeconds(1f);
        winGlow_ObjArray[winIndex].SetActive(true);

        yield return new WaitForSeconds(1f);
        botManagerRef.BotWin(botWinArray);
        yield return new WaitForSeconds(2f);
        foreach (GameObject g in winGlow_ObjArray)
        {
            g.SetActive(false);
        }
    }



    private int GetLeadingHorseID()
    {
        int leadingHorseID = -1;
        float maxPositionX = -Mathf.Infinity;

        for (int i = 0; i < horseImages.Length; i++)
        {
            float horsePositionX = horseImages[i].transform.localPosition.x;
            if (horsePositionX > maxPositionX)
            {
                maxPositionX = horsePositionX;
                leadingHorseID = i;
            }
        }

        return leadingHorseID;
    }

    private void UpdateLeadingHorseVisibility(int leadingHorseID, int previousLeadingHorseID)
    {
        leadingHorseNumber_TMP.text = (leadingHorseID + 1).ToString();
        if (leadingHorseID != previousLeadingHorseID)
        {
         

        }
    }

    private void FinalizeHorsePositions(Vector3[] startPositions)
    {
        for (int i = 0; i < horseImages.Length; i++)
        {
            horseImages[i].transform.localPosition = new Vector3(startPositions[i].x + raceDistance, startPositions[i].y, startPositions[i].z);
        }

        startPointImage.SetActive(true);
        leading_HorseLine_IMG.SetActive(false);
         isRaceRunning = false;
    }



    public void SelectHorse(int horseID)
    {
        if (!isBettingAllowed)
        {
            Debug.Log("Betting is not allowed at this time.");
            return; 
        }

        
        if (walletAmount < betManagerRef.betVal)
        {
            Debug.Log("Not enough balance to place this bet."); 
           // payoutText.text = "Not enough balance to place this bet."; 
            return; 
        }

     
        betsAmount_Array[horseID] += betManagerRef.betVal; 
        DeductBetAmount(); 
        Debug.Log("Horse selected for betting: " + (horseID + 1) + ", Total Bet: " + betsAmount_Array[horseID]);
         socketManagerHR.SendBetDataToServer(horseID , betManagerRef.betVal);
    }


   
    private void DeductBetAmount()
    {
        if (walletAmount >= betManagerRef.betVal)
        {
            walletAmount -= betManagerRef.betVal;
            UpdateWallet(walletAmount);
        }
        else
        {
           // payoutText.text = "Not enough balance to place this bet."; // Update UI message
        }
    }

    private void CalculatePayout(int winIndex)
    {
        totalWinnings = 0;

        foreach (float f in betsAmount_Array)
        {
            if (f > 0)
            {
                apisRef.FetchWallet();
            }
        }
        for (int i = 0; i < horseCount; i++)
        {
            if (betsAmount_Array[i] > 0 && i == winIndex)
            {
                // The player won the bet on this horse
                Debug.Log("bet amount " + betsAmount_Array[i]);
                Debug.Log("horse multiplier " + horseMultipliers[i]);
                float payout = betsAmount_Array[i] * horseMultipliers[i];
                walletAmount += payout;
                Debug.Log("payoout : " + payout);
                totalWinnings += payout;
                UpdateWallet(walletAmount);
                StartCoroutine(ShowWinAmount());
            }
            else if (betsAmount_Array[i] > 0)
            {
                // The player lost the bet on this horse
                Debug.Log("Player lost the bet on horse " + (i + 1));
            }
        }
    }

    IEnumerator ShowWinAmount ()
    {
        winAudioSource.Play();
        showWinPanel.SetActive(true);
        showWinText.text = "+" + totalWinnings.ToString("F2");
        yield return new WaitForSeconds(3f);
        showWinPanel.SetActive(false);
    }
 


    private void WinHorseNumberShow(int winIndex)
    {
        if (winIndex == 0)
        {
            winhorse1.SetActive(true);
  
        }else if(winIndex == 1)
        {
            winhorse2.SetActive(true);
            
            
        }else if(winIndex == 2)
        {
            winhorse3.SetActive(true);
         
        }else if(winIndex == 3)
        {
            winhorse4.SetActive(true);

        }else if(winIndex == 4)
        {
            winhorse5.SetActive(true);
          
        }else if(winIndex == 5)
        {
            winhorse6.SetActive(true);
           
        }


    }




    public void UpdateHorseOdds(float[] hMultipliers)
    {

        for (int i = 0; i < hMultipliers.Length; i++)
        {
            horseMultipliers[i] = hMultipliers[i];
            horseMultiplierText[i].text = hMultipliers[i].ToString("F2") + "x";
            horseMultiplierTextOnGround[i].text = hMultipliers[i].ToString("F2") + "x";
            winMultiplierText[i].text = hMultipliers[i].ToString("F2") + "x";
        }
    }


    private void ResetGame()
    {        

        for (int i = 0; i < horseImages.Length; i++)
        {
            horseImages[i].transform.localPosition = originalPositions[i];
             leading_HorseLine_IMG.transform.localPosition = startExtraPosition;
            backgroundImage.transform.localPosition = bgStartPos;            
        }

        HorseIdleAnimation();
    }



    public void InitializeBots(List<BotData> data, long roundStartTime)
    {
        if (botManagerRef != null)
        {
            botManagerRef.InitializeBots(data, roundStartTime);
        }
    }


    public void EnableWaitingPanel()
    {
        waitingForNext.SetActive(true);
    }





    void BetAmountIncrease(float startTime, string[] seeds)
    {
        randomValue1 = 0;
        randomValue2 = 0;
        randomValue3 = 0;
        randomValue4 = 0;
        randomValue5 = 0;
        randomValue6 = 0;

        rvi = 0;


        long currentTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float elapsekqimeInSeconds = (currentTimestamp - startTime) / 1000f;

        // Initialize the random generators with consistent hashes
        random1 = new System.Random(GenerateConsistentHash(seeds[0]));
        random2 = new System.Random(GenerateConsistentHash(seeds[1]));
        random3 = new System.Random(GenerateConsistentHash(seeds[2]));
        random4 = new System.Random(GenerateConsistentHash(seeds[3]));
        random5 = new System.Random(GenerateConsistentHash(seeds[4]));
        random6 = new System.Random(GenerateConsistentHash(seeds[5]));

        int stepsToSkip = Mathf.FloorToInt(elapsekqimeInSeconds * 2); // 5 steps per second
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
            int increment4 = random3.Next(1, 10);
            int increment5 = random3.Next(1, 10);
            int increment6 = random3.Next(1, 10);

            if (rvi < 37)
            {
                rvi++;
                randomValue1 += increment1;
                randomValue2 += increment2;
                randomValue3 += increment3;
                randomValue4 += increment4;
                randomValue5 += increment5;
                randomValue6 += increment6;
            }


            int k = (randomValue1 * 70);
            int l = (randomValue2 * 70);
            int m = (randomValue3 * 70);
            int n = (randomValue4 * 70);
            int o = (randomValue5 * 70);
            int p = (randomValue6 * 70);

            betAmountStatus_Txt_Array[0].text = $"<color=yellow>{betsAmount_Array[0]}</color><color=#02ccfe>/{k}</color>";
            betAmountStatus_Txt_Array[1].text = $"<color=yellow>{betsAmount_Array[1]}</color><color=#02ccfe>/{l}</color>";
            betAmountStatus_Txt_Array[2].text = $"<color=yellow>{betsAmount_Array[2]}</color><color=#02ccfe>/{m}</color>";
            betAmountStatus_Txt_Array[3].text = $"<color=yellow>{betsAmount_Array[3]}</color><color=#02ccfe>/{n}</color>";
            betAmountStatus_Txt_Array[4].text = $"<color=yellow>{betsAmount_Array[4]}</color><color=#02ccfe>/{o}</color>";
            betAmountStatus_Txt_Array[5].text = $"<color=yellow>{betsAmount_Array[5]}</color><color=#02ccfe>/{p}</color>";
            
            yield return new WaitForSeconds(updateInterval);
        }
    }

    void ResetBetAmount()
    {
        betAmountStatus_Txt_Array[0].text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        betAmountStatus_Txt_Array[1].text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        betAmountStatus_Txt_Array[2].text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        betAmountStatus_Txt_Array[3].text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        betAmountStatus_Txt_Array[4].text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";
        betAmountStatus_Txt_Array[5].text = $"<color=yellow>{0}</color><color=#02ccfe>/{0}</color>";

    }


}
