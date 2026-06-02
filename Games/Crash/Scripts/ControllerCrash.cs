using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControllerCrash : MonoBehaviour
{

    private System.Random random1;
    private float updateInterval = 0.5f; // 5 times per second
    private int randomValue1 = 0;
    private bool isGenerating = false;
    public int rvi;
    public bool _isGameEnded;
    public float roundEndedAt;
    float baseMultiplier = 0.05f;
    float multiplierRate = 0.01f;
    DateTime serverStartTime;
    public Image x_textColor;
    public Sprite under2;
    public Sprite under10;
    public Sprite underInfinity;
    public GameObject target;
    public TextMeshProUGUI totalBetsText;
    public RandomPlayerDrop randomPlayerDropRef;
    public PeopleBetCrash peopleBetScript;
    public GameObject betRoda;
    public AudioSource gameStartSound;
    public AudioSource crashSound;
    public AudioSource BGM;
    public Animator planeBoxAnimator;
    public Animator planeAnimator;
    public Text xText;
    public GameObject waitForNextRoundImg;
    public GameObject gameContainer;
    public GameObject rectangleGraphImg;
    public Slider roundSlider;
    public GameObject flewAwayText;
    public DotControllerCrash dControllerHorizontal;
    public DotControllerCrash dControllerVertical;
    public float s;
    public bool _crash;
    public bool _waitingForNextRound;
    public BetSystemCrash bet1;
    public Text myBetAmountTxt;
    public GameHistoryCrash historyCreatorRef;
    public string roundId;

    public Text walletText;
    public float walletAmount;
    public APIs apisRef;
    // Start is called before the first frame update
    void Start()
    {
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        peopleBetScript = FindObjectOfType<PeopleBetCrash>();
        betRoda.SetActive(false);
        gameContainer.SetActive(false);
        BGM.Play();
        historyCreatorRef = FindObjectOfType<GameHistoryCrash>();
        apisRef.FetchWallet();
    }
    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }

    #region Socket Methods

    public void HistoryUpdated()
    {
        StartCoroutine(CheckForPendingBets());
    }
    IEnumerator CheckForPendingBets()
    {
        yield return new WaitForSeconds(0.4f);
        if (PlayerPrefs.GetFloat("betInCrash") > 0)
        {
            Debug.Log("bet amount phle ka baki hai ");


            if (PlayerPrefs.GetString("roundId_Crash") == roundId.ToString())
            {
                StartCoroutine(bet1.CashOutCal());
                Debug.Log("wahi round chal rha hai");
                bet1.cashOutBTN.SetActive(true);
                bet1.betAmount = (int)PlayerPrefs.GetFloat("betInCrash");
            }
            else
            {
                Debug.Log("pPrefs : " + PlayerPrefs.GetString("roundId_Crash"));
                Debug.Log("rId : " + roundId);
            }
        }

    }
    public void StartFlyingPhase(double roundStartTime, long rounddId)
    {
        roundId = rounddId.ToString();
        Debug.Log("starting flying phase");
        _isGameEnded = false;
        StartCoroutine(Game(roundStartTime));
        SetServerStartTime(roundStartTime);
    }

    public void ShowWaitingPanel(long rounddId, float remTime, string seed)
    {
        PlayerPrefs.SetString("roundId_Crash", rounddId.ToString());
        roundId = rounddId.ToString();
        StartCoroutine(WaitForNextRound(rounddId, remTime, seed));

    }

    public void MultiplierUpdater(float val)
    {
        s = val;
    }

    public void History(float[] record)
    {
        if (historyCreatorRef == null)
        {
            historyCreatorRef = FindObjectOfType<GameHistoryCrash>();
        }
        historyCreatorRef.HistoryUpdate(record);
    }



    #endregion





    public void EndGame(float val)
    {
        if(val < 1)
        {
            val = 1f;
        }
        Debug.Log("end at : " + val);
        roundEndedAt = val;
        StartCoroutine(EndRoundEnum(val));
        historyCreatorRef.AddHistory(val);
    }

    IEnumerator EndRoundEnum(float val)
    {
        _isGameEnded = true;
        xText.text = val.ToString("F2") + "x";
        PlayerPrefs.SetFloat("betInCrash", 0f);

        planeBoxAnimator.SetBool("_endPlane", true);
        planeAnimator.SetBool("_fly", true);
        yield return new WaitForSeconds(0.5f);
        xText.text = val.ToString("F2") + "x";

        rectangleGraphImg.SetActive(false);
        planeAnimator.SetBool("_fly", false);
        peopleBetScript.gameObject.SetActive(true);
    }


    public double CalculateMultiplier()
    {
        // Get the current time and calculate the elapsed time since the server start
        DateTime currentTime = DateTime.UtcNow;
        TimeSpan elapsedTime = currentTime - serverStartTime - TimeSpan.FromSeconds(0.1f);
        if (elapsedTime < TimeSpan.Zero)
        {
            elapsedTime = TimeSpan.Zero;
        }
        double multiplier = 1 + baseMultiplier * (Mathf.Exp(6 * multiplierRate * (float)elapsedTime.TotalSeconds) * elapsedTime.TotalSeconds);
        s = (float)multiplier;
        dControllerVertical.ScalePosVUpdate(multiplier);
        dControllerHorizontal.ScalePosHUpdate(elapsedTime.TotalSeconds);

        return multiplier;
    }

    public void SetServerStartTime(double serverTimestamp)
    {
        Debug.Log("Time Reccieve from server : " + serverTimestamp);
        serverStartTime = DateTimeOffset.FromUnixTimeSeconds((long)serverTimestamp).UtcDateTime;
        Debug.Log(CalculateMultiplier());
    }


    public IEnumerator TextAdder()
    {
        x_textColor.sprite = under2;
        bool _under10 = false;
        bool _underInfinity = false;
        do
        {

            // Increment s based on the real time that has passed

            // Change color based on the value of s
            if (s >= 2 && !_under10)
            {
                x_textColor.sprite = under10;
                _under10 = true;
            }
            else if (s >= 10 && !_underInfinity)
            {
                x_textColor.sprite = underInfinity;
                _underInfinity = true;
            }

            // Update the text
            xText.text = CalculateMultiplier().ToString("F2") + "x";

            // Wait for the next frame
            yield return null;
        }
        while (!_isGameEnded);
        bet1.cashOutBTN.SetActive(false);

        randomPlayerDropRef._isdropping = false;
        planeAnimator.SetBool("_fly", true);
        planeBoxAnimator.SetBool("_upDown", false);
        yield return new WaitForSeconds(0.2f);
        rectangleGraphImg.SetActive(false);
        flewAwayText.SetActive(true);
        xText.color = Color.red;

        _crash = true;
        crashSound.Play();

    }

    public IEnumerator WaitForNextRound(long roundStartTime, float remTime, string seed)
    {
        target.SetActive(true);
        bet1.betAmount = 0;
        bet1._isRebetted = false;
        PlayerPrefs.SetString("roundId_Crash", roundId);
        StartCoroutine(peopleBetScript.AnimStart());
        rectangleGraphImg.SetActive(true);
        planeAnimator.SetBool("_fly", false);
        walletText.text = "₹" + walletAmount.ToString("F2");

        _waitingForNextRound = true;
        betRoda.SetActive(false);
        _crash = false;
        gameContainer.SetActive(false);
        waitForNextRoundImg.SetActive(true);
        StartCoroutine(DecreaseSlider(remTime));
        yield return new WaitForSeconds(0.5f);
        BetAmountIncrease(roundStartTime, seed);
        yield return new WaitForSeconds(10);


    }







    void BetAmountIncrease(long startTime, string seed)
    {
        randomValue1 = 0;


        rvi = 0;


        long currentTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float elapsedTimeInSeconds = (currentTimestamp - startTime) / 1000f;

        // Initialize the random generators with consistent hashes
        random1 = new System.Random(GenerateConsistentHash(seed));



        // Skip ahead in the random sequences based on elapsed time
        int stepsToSkip = Mathf.FloorToInt(elapsedTimeInSeconds * 2); // 5 steps per second
        for (int i = 0; i < stepsToSkip; i++)
        {
            if (rvi < 37)
            {
                rvi++;
                randomValue1 += random1.Next(1, 10);

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

            if (rvi < 37)
            {
                rvi++;
                randomValue1 += increment1;

            }


            int k = (randomValue1 * 990);

            totalBetsText.text = "<color=#ADE8FF>" + bet1.betAmount + "</color><color=yellow>/" + k + "</color>";

            yield return new WaitForSeconds(updateInterval);
        }
    }






    IEnumerator DecreaseSlider(float startVal)
    {
        roundSlider.value = startVal;
        float startTime = Time.time;
        float endTime = startTime + 10f;
        float startValue = roundSlider.value;

        while (Time.time < endTime)
        {
            float normalizedTime = (Time.time - startTime) / 10f;
            roundSlider.value = Mathf.Lerp(startValue, 0f, normalizedTime);
            yield return null;
        }

        roundSlider.value = 0f;
    }

    public IEnumerator Game(double startingPoint = 1f)
    {

        planeBoxAnimator.SetBool("_endPlane", false);
        peopleBetScript.gameObject.SetActive(false);
        xText.text = "";
        xText.color = Color.white;
        waitForNextRoundImg.SetActive(false);

        target.SetActive(false);

        isGenerating = false;
        peopleBetScript.StopAnim();
        peopleBetScript.MoveAllcoinsBack();
        flewAwayText.SetActive(false);

        gameContainer.SetActive(true);
        _waitingForNextRound = false;
        betRoda.SetActive(true);




        Debug.Log("starting at : " + startingPoint);
        for (int i = 0; i < bet1.myCoinsHolder.childCount; i++)
        {
            Destroy(bet1.myCoinsHolder.GetChild(i).gameObject);
        }
        StartCoroutine(randomPlayerDropRef.DroppingEnum());
        float amntBetted = 0;
        gameStartSound.Play();
        if (bet1._betted)
        {
            StartCoroutine(bet1.CashOutCal());
            amntBetted += bet1.betAmount;

        }

        yield return new WaitForSeconds(0.5f);
        xText.color = Color.white;
        StartCoroutine(TextAdder());
        if (s < 1.2f)
        {

            planeBoxAnimator.SetBool("_startPlane", true);
            yield return new WaitForSeconds(5f);
        }
        planeBoxAnimator.SetBool("_upDown", true);

    }
    // Update is called once per frame       
}
