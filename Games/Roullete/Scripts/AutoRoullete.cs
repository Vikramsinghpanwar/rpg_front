using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;

public class AutoRoullete : MonoBehaviour
{

    public static AutoRoullete Instance;
    private System.Random random1;

    private float updateInterval = 0.5f; // 5 times per second
    private int randomValue1 = 0;

    public bool isGenerating = false;
    public int rvi;



    public AudioSource countDownAudio;
    public AudioSource winSound;
    public Text totalBetsTxt;
    public int totalBets;
    public GameObject betStartOjb;
    public GameObject betStopOjb;
    public ManagerRoullete managerRef;
    
    public TextMeshProUGUI timerOjbTmpro;
    public TextMeshProUGUI timerTxt;
    Bets betMangerRef;

    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        timerTxt.text = "";
        timerOjbTmpro.text = "";
        betMangerRef = FindObjectOfType<Bets>();
        managerRef = FindObjectOfType<ManagerRoullete>();
        // burstRef = FindObjectOfType<BurstRoullete>();
        timerOjbTmpro.gameObject.SetActive(false);
    }
    public IEnumerator Auto(float remTime, long serverStartTime, string seed)
    {
        timerOjbTmpro.text = "";
        totalBets = 0;
        totalBetsTxt.text = "₹ " + totalBets;
        betStartOjb.SetActive(true);

        yield return new WaitForSeconds(1f);
        BetAmountIncrease(serverStartTime, seed);

        betStartOjb.SetActive(false);

        // StartCoroutine(burstRef.AnimStart());
        if(remTime > 4)
        {
            StartCoroutine(TimerEnum((int)remTime - 3));
            yield return new WaitForSeconds(remTime - 3);
        }

        // burstRef.StopAnim();
        betStopOjb.SetActive(true);
        yield return new WaitForSeconds(1f);
        betStopOjb.SetActive(false);
    }

 

    IEnumerator TimerEnum(float duration)
    {
        timerTxt.text = (duration).ToString();

        for (int i = 1; i <= duration; i++)
        {
            yield return new WaitForSeconds(1f);
            float remTime = duration - i;
            timerTxt.text = (remTime).ToString();
            if (remTime < 4 && remTime > 0)
            {
                timerOjbTmpro.gameObject.SetActive(true);
                timerOjbTmpro.text = remTime.ToString();
                countDownAudio.Play();
            }
        }
        timerOjbTmpro.gameObject.SetActive(false);

    }

    public IEnumerator Timer(int val)
    {
        for (int i = val; i >= 0; i--)
        {
            timerTxt.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
        timerTxt.text = "";


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
                randomValue1 += random1.Next(1, 10) * random1.Next(1, 10);

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
            int increment1 = random1.Next(1, 10) * random1.Next(1, 10);
   
            if (rvi < 50)
            {
                rvi++;
                randomValue1 += increment1;
     
            }


            int k = (randomValue1 * 40);
       

            totalBetsTxt.text = "₹" + k;


            yield return new WaitForSeconds(updateInterval);
        }
    }


}
