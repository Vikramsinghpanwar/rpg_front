using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public TextMeshProUGUI d1;
    public TextMeshProUGUI d2;
    public TextMeshProUGUI d3;
    public TextMeshProUGUI d4;
    public TextMeshProUGUI d5;
    public TextMeshProUGUI pingText;
    public float lastPingTime;
    public GameObject connectingPanel;
    float baseMultiplier = 0.05f;
    float multiplierRate = 0.01f;
    long serverStartTime;
    public bool _isGameEnded;
    public float roundEndedAt;
    public GameHistoryAviator historyCreatorRef;
    public AnimationCurve animCurveExp;
    public AudioSource gameStartSound;
    public AudioSource crashSound;
    public AudioSource BGM;
    public Animator planeBoxAnimator;
    public Animator planeAnimator;
    public Text xText;
    public GameObject waitForNextRoundImg;
    public GameObject rectangleGraphImg;
    public Slider roundSlider;
    public GameObject flewAwayText;
    public DotController dControllerHorizontal;
    public DotController dControllerVertical;
    public bool _crash;
    public bool _waitingForNextRound;
    public BetSystemAviator bet1;
    public BetSystemAviator bet2;
    public PopulatePlayer populatePlayers;
    public GameObject AllBetsObject;
    public GameObject MyBetsObject;
    public string gamePhase = "";
    public string roundId = "";

    public Text walletText;
    public float walletAmount;
    public APIs apisRef;
    public float s = 1f;

    SocketManagerAviator socketManagerRef;

    private void OnEnable()
    {
        Application.targetFrameRate = 90;
        Screen.orientation = ScreenOrientation.Portrait;
    }

    #region socket Functions

    public void PingUpdate()
    {
        float currentPingTime = Time.time - lastPingTime;
        pingText.text = (currentPingTime * 1000f).ToString("F0") + "ms";


        if (currentPingTime * 1000f <= 200)
        {
            pingText.color = Color.green;

        }
        else if (currentPingTime * 1000f > 200)
        {
            pingText.color = Color.white;
        }
        else if (currentPingTime * 1000f > 500)
        {
            pingText.color = Color.yellow;
        }
        else
        {
            pingText.color = Color.red;
        }
    }
    public void StartFlyingPhase(double serverTimestamp, double rId)
    {

        connectingPanel.SetActive(false);
        serverStartTime = (long)(serverTimestamp * 1000f); // DateTimeOffset.FromUnixTimeSeconds((long)serverTimestamp).UtcDateTime;
        if (gamePhase == "Flying")
        {
            return;
        }

        roundId = rId.ToString();
        _isGameEnded = false;
        gamePhase = "Flying";
        StartCoroutine(Game());
    }

    public void StartBetting(double Id, float val = 1f)
    {
        connectingPanel.SetActive(false);
        Debug.Log("saving round Id : " + Id.ToString());
        PlayerPrefs.SetString("roundId_Aviator", Id.ToString());
        Debug.Log("saved rId : " + PlayerPrefs.GetString("roundId_Aviator"));
        roundId = Id.ToString();
        gamePhase = "Betting";
        _isGameEnded = true;
        planeBoxAnimator.SetBool("_endPlane", true);
        rectangleGraphImg.SetActive(false);
        planeAnimator.SetBool("_fly", false);
        StartCoroutine(WaitForNextRound(val));
    }


    #endregion


    public void InitializeBots(List<botDataAviator> botData, double roundStartTime, int totalBets)
    {
        populatePlayers.Populate(botData, roundStartTime);
        populatePlayers.TotalBets(totalBets);
    }
    public void CheckForPendingBets(double newId)
    {
        Debug.Log("checking for previous bets");
        if (PlayerPrefs.GetFloat("bet1InAviator") > 0)
        {
            Debug.Log("pichli bets baki hai");
            Debug.Log("old rId : " + PlayerPrefs.GetString("roundId_Aviator"));
            Debug.Log("new rId : " + newId.ToString());

            if (PlayerPrefs.GetString("roundId_Aviator") == newId.ToString())
            {
                Debug.Log("Same Round, gamePhase : " + gamePhase);

                walletAmount -= PlayerPrefs.GetFloat("bet1InAviator");
                if (gamePhase == "Flying")
                {
                    StartCoroutine(bet1.CashOutCal());
                }
                else if (gamePhase == "Betting")
                {
                    bet1.cancelBetBTN.SetActive(true);
                }
                bet1._betted = true;
                bet1.betAmount = (int)PlayerPrefs.GetFloat("bet1InAviator");

                walletText.text = "₹" + walletAmount.ToString("F2");

            }

        }

        if (PlayerPrefs.GetFloat("bet2InAviator") > 0)
        {
            Debug.Log("pichli bets2 baki hai ");

            if (PlayerPrefs.GetString("roundId_Aviator") == newId.ToString())
            {
                walletAmount -= PlayerPrefs.GetFloat("bet2InAviator");

                if (gamePhase == "Flying")
                {
                    StartCoroutine(bet2.CashOutCal());
                }
                else if (gamePhase == "Betting")
                {
                    bet2.cancelBetBTN.SetActive(true);
                }
                bet2._betted = true;
                bet2.betAmount = (int)PlayerPrefs.GetFloat("bet2InAviator");

                walletText.text = "₹" + walletAmount.ToString("F2");

            }
        }
    }


    void Start()
    {
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;
        socketManagerRef = FindObjectOfType<SocketManagerAviator>();
        connectingPanel.SetActive(true);
        historyCreatorRef = FindObjectOfType<GameHistoryAviator>();
        AllBetsObject.SetActive(true);
        MyBetsObject.SetActive(false);
        populatePlayers = FindAnyObjectByType<PopulatePlayer>();
        BGM.Play();
        apisRef.FetchWallet();
    }
    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");

    }

    public void StartWaitingForNextRound()
    {
        Debug.Log("Startgin Coroutient");

    }



    public void History(float[] record)
    {
        if (historyCreatorRef == null)
        {
            historyCreatorRef = FindObjectOfType<GameHistoryAviator>();
        }
        historyCreatorRef.HistoryUpdate(record);
    }

    public double CalculateMultiplier()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + socketManagerRef.timeDifference;
        float elapsedTime = (currentTime - serverStartTime) / 1000f;
        double multiplier = 1 + baseMultiplier * (Mathf.Exp(6 * multiplierRate * elapsedTime) * elapsedTime);
        return multiplier;
    }


    public IEnumerator TextAdder()
    {
        do
        {
            s = (float)CalculateMultiplier();
        }
        while (s < 1f);
        do
        {
            s = (float)CalculateMultiplier();
            xText.text = s.ToString("F2") + "x";

            yield return null;
        }
        while (!_isGameEnded);

        bet1.cashOutBTN.SetActive(false);
        bet2.cashOutBTN.SetActive(false);

        planeBoxAnimator.SetBool("_upDown", false);
        flewAwayText.SetActive(true);
        xText.color = Color.red;
        xText.text = (roundEndedAt).ToString("F2");
        dControllerHorizontal.DeleteAll();
        dControllerVertical.DeleteAll();
        _crash = true;
        crashSound.Play();
        yield return new WaitForSeconds(0.5f);




        if (bet1._inBet)
        {
            bet1.BetDataUpdate(s, false);
        }
        else
        {
            bet1.autoCahoutBTN_img.gameObject.GetComponent<Button>().interactable = true;
        }

        if (bet2._inBet)
        {
            bet2.BetDataUpdate(s, false);

        }
        else
        {
            bet2.autoCahoutBTN_img.gameObject.GetComponent<Button>().interactable = true;
        }
        yield return new WaitForSeconds(2);


    }

    public IEnumerator WaitForNextRound(float val)
    {
        Debug.Log("round id during waiting period  : " + roundId.ToString());
        if (bet1._betted)
        {
            PlayerPrefs.SetFloat("bet1InAviator", bet1.betAmount);
            walletAmount -= bet1.betAmount;
            walletText.text = "₹" + walletAmount.ToString("F2");
            PlayerPrefs.SetString("roundId_Aviator", roundId.ToString());
            socketManagerRef.SendBetData(bet1.betAmount, 0);

        }
        if (bet2._betted)
        {
            PlayerPrefs.SetFloat("bet2InAviator", bet2.betAmount);
            walletAmount -= bet2.betAmount;
            walletText.text = "₹" + walletAmount.ToString("F2");
            PlayerPrefs.SetString("roundId_Aviator", roundId.ToString());
            socketManagerRef.SendBetData(bet2.betAmount, 1);
        }
        //let player populate first
        yield return new WaitForSeconds(0.5f);

        if (bet1._isAutoBet)
        {
            int tmp = bet1.betAmount;
            bet1.betAmount = bet1.pastBetAmnt;
            bet1.Bet();
            bet1.betAmount = tmp;
        }

        if (bet2._isAutoBet)
        {
            int tmp = bet2.betAmount;
            bet2.betAmount = bet2.pastBetAmnt;
            bet2.Bet();
            bet2.betAmount = tmp;
        }


        _waitingForNextRound = true;
        _crash = false;
        waitForNextRoundImg.SetActive(true);
        StartCoroutine(DecreaseSlider(val));
        flewAwayText.SetActive(false);
        _waitingForNextRound = false;
    }
    void OnDestroy()
    {
        Debug.Log("Controller is being destroyed.");
    }



    public void EndGame(float val)
    {

        gamePhase = "Ending";
        roundEndedAt = val;
        if (!bet1.cancelBetBTN.activeInHierarchy)
        {
            bet1._betted = false;

        }

        if (!bet2.cancelBetBTN.activeInHierarchy)
        {
            bet2._betted = false;


        }
        bet1.betBTN.SetActive(true);
        bet2.betBTN.SetActive(true);
        bet1.Roda.SetActive(false);
        bet2.Roda.SetActive(false);


        PlayerPrefs.SetFloat("bet1InAviator", 0);
        PlayerPrefs.SetFloat("bet2InAviator", 0);
        StartCoroutine(EndRoundEnum());
        historyCreatorRef.AddHistory(val);
    }

    IEnumerator EndRoundEnum()
    {
        _isGameEnded = true;
        planeBoxAnimator.SetBool("_endPlane", true);
        planeAnimator.SetBool("_fly", true);
        yield return new WaitForSeconds(0.5f);
        rectangleGraphImg.SetActive(false);
        planeAnimator.SetBool("_fly", false);


    }
    IEnumerator DecreaseSlider(float val)
    {
        roundSlider.value = val;
        float startTime = Time.time;
        float endTime = startTime + 7f;
        float startValue = roundSlider.value;

        while (Time.time < endTime)
        {
            float normalizedTime = (Time.time - startTime) / 7f;
            roundSlider.value = Mathf.Lerp(startValue, 0f, normalizedTime);
            yield return null;
        }
        roundSlider.value = 0f;
    }

    public IEnumerator Game()
    {
        xText.color = Color.white;
        float currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + socketManagerRef.timeDifference / 1000f;

        float remainingTime = (currentTime - serverStartTime) / 1000f;

        xText.text = "";
        waitForNextRoundImg.SetActive(false);

        if (remainingTime > 0)
        {
            yield return new WaitForSecondsRealtime(remainingTime);
        }
        StartCoroutine(TextAdder());

        flewAwayText.SetActive(false);
        planeBoxAnimator.SetBool("_endPlane", false);
        rectangleGraphImg.SetActive(true);
        gameStartSound.Play();

        if (bet1._betted && bet2._betted)
        {
            float totalBet = bet1.betPlacedForAmount + bet2.betPlacedForAmount;
        }
        else if (bet1._betted)
        {

            float totalBet = bet1.betPlacedForAmount;

        }
        else if (bet2._betted)
        {
            float totalBet = bet2.betPlacedForAmount;

        }




        if (bet1._betted)
        {
            PlayerPrefs.SetFloat("bet1InAviator", bet1.betAmount);
            StartCoroutine(bet1.CashOutCal());


        }
        if (bet2._betted)
        {
            PlayerPrefs.SetFloat("bet2InAviator", bet2.betAmount);
            StartCoroutine(bet2.CashOutCal());

        }

        StartCoroutine(populatePlayers.botWin());


        dControllerHorizontal.Populate();
        dControllerVertical.Populate();


        dControllerHorizontal.MoveDots();
        dControllerVertical.MoveDots();
        if (s < 1.2f)
        {
            planeBoxAnimator.SetBool("_startPlane", true);
            yield return new WaitForSeconds(5f);
        }
        planeBoxAnimator.SetBool("_upDown", true);
    }


    public void AllBets()
    {
        MyBetsObject.SetActive(false);
    }

    public void MyBets()
    {
        MyBetsObject.SetActive(true);

    }

    public void Lobby()
    {
        SocketManagerAviator sm = FindObjectOfType<SocketManagerAviator>();
        sm.Disconnect();
        SceneManager.LoadScene(1);

    }


    public double CalculateElapsedTime(double multiplier)
    {
        const double epsilon = 1e-6; // Precision threshold
        double low = 0; // Minimum elapsed time
        double high = 10000; // Arbitrary large value as upper bound for elapsed time
        double mid = 0;

        while (high - low > epsilon)
        {
            mid = (low + high) / 2;
            double calculatedMultiplier = 1 + baseMultiplier * (Math.Exp(6 * multiplierRate * mid) * mid);

            if (calculatedMultiplier < multiplier)
            {
                low = mid; // Increase elapsed time
            }
            else
            {
                high = mid; // Decrease elapsed time
            }
        }
        Debug.Log("elapse time : " + mid);
        return mid;
    }

}
