using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Text;

public class ManagerBRoullete : MonoBehaviour
{

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
    private bool isGenerating = false;
    public int rvi;

    public TextMeshProUGUI betOn1Text;
    public TextMeshProUGUI betOn2Text;
    public TextMeshProUGUI betOn3Text;
    public TextMeshProUGUI betOn4Text;
    public TextMeshProUGUI betOn5Text;
    public TextMeshProUGUI betOn6Text;
    public TextMeshProUGUI betOn7Text;
    public TextMeshProUGUI betOn8Text;
    public TextMeshProUGUI betOn9Text;
    public TextMeshProUGUI betOn10Text;
    public TextMeshProUGUI betOn11Text;

    SocketManagerBoxRoulette socketRef;
    public GameObject insufficientFundsPanel;
    public string[] itemNames;
    public CountDown321 countDownRef;
    public HistoryCreatorZooRoullete historyRef;
    public GameObject waitingForNxtRoundPanel;
    public float totalBidAmount;
    public float winAmnt;
    public Text winPanelAmnt;
    public AudioSource winAudio;
    public GameObject winPanel;
    public TextMeshProUGUI winnerNameTxt;
    public GameObject insufficientFundsObj;
    public TextMeshProUGUI jackpotText;
    public BetManager betManagerRef;
    public BurstBRoullete burstManager;
    public AudioSource winnerAudio;
    public List<Sprite> items;
    public GameObject winnerImgDisplayer;
    public Image winnerImgDisplayerImage;
    public Animator target1A, target2A, target3A, target4A, target5A, target6A, target7A, target8A, target9A, target10A, target11A;
    public GameObject stopBetPanel, startBetPanel, betRoda;
    [SerializeField] int cardsShuffled;
    public Sprite trp, cardPNG;
    public Image JokerImg;
    public Text walletText;
    TileGlowControllerBoxRoullete tileScript;
    float walletAmount;
    public bool _bet1, _bet2, _bet3, _bet4, _bet5, _bet6, _bet7, _bet8, _bet9, _bet10, _bet11;
    public int bet1BetVal, bet2BetVal, bet3BetVal, bet4BetVal, bet5BetVal, bet6BetVal, bet7BetVal, bet8BetVal, bet9BetVal, bet10BetVal, bet11BetVal;
    public ConnectionBRoullete conRef;
    APIs apisRef;



    #region socket function
    public void HistoryUpdate(int[] hisArray)
    {
        historyRef.InitialHistoryCreator(hisArray);
    }

    public void StartBetting(int remTime, long startTime, string[] seeds)
    {
        waitingForNxtRoundPanel.SetActive(false);
        RoundStart(remTime);
        BetAmountIncrease(startTime, seeds);

    }

    public void EnableWaitingPanel()
    {
        //StartCoroutine(WaitinPanelCountDown());
        waitingForNxtRoundPanel.SetActive(true);

    }

    public void ShowResult(int winIndex, int val)
    {
        isGenerating = false;
        StartCoroutine(ShowResultEnum(winIndex, val));
    }


    #endregion
    void Start()
    {
        apisRef = FindObjectOfType<APIs>();
        apisRef.OnWalletFetched += UpdateWallet;

        socketRef = FindObjectOfType<SocketManagerBoxRoulette>();
        countDownRef = FindObjectOfType<CountDown321>();
        historyRef = FindObjectOfType<HistoryCreatorZooRoullete>();
        insufficientFundsObj.SetActive(false);
        conRef = FindAnyObjectByType<ConnectionBRoullete>();
        winnerImgDisplayer.SetActive(false);
        betManagerRef = FindObjectOfType<BetManager>();
        tileScript = FindObjectOfType<TileGlowControllerBoxRoullete>();
        stopBetPanel.SetActive(false);
        startBetPanel.SetActive(false);
        apisRef.FetchWallet();

    }

    private void UpdateWallet(float wAmount)
    {
        walletAmount = wAmount;
        walletText.text = "₹" + walletAmount.ToString("F2");
    }


    void DeleteChildren(Transform parent)
    {
        // Loop through each child of the parent transform
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            // Destroy the child GameObject
            Destroy(parent.GetChild(i).gameObject);

        }

    }

    public IEnumerator BetAmountIncrease()
    {
        int a = 0;
        int b = 0;
        int c = 0;
        int d = 0;
        int e = 0;
        int f = 0;
        int g = 0;
        int h = 0;

        for (int i = 0; i > -1; i++)
        {
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
            a += Random.Range(100, 1000);
            b += Random.Range(100, 1000);
            c += Random.Range(100, 1000);
            d += Random.Range(100, 1000);
            e += Random.Range(100, 1000);
            f += Random.Range(100, 1000);
            g += Random.Range(100, 1000);
            h += Random.Range(100, 1000);

        }



    }
    void RoundStart(int remTime)
    {

        totalBidAmount = 0f;
        winAmnt = 0f;
        bet1BetVal = 0;
        bet2BetVal = 0;
        bet3BetVal = 0;
        bet4BetVal = 0;
        bet5BetVal = 0;
        bet6BetVal = 0;
        bet7BetVal = 0;
        bet8BetVal = 0;
        bet9BetVal = 0;
        bet10BetVal = 0;
        bet11BetVal = 0;

        _bet1 = false;
        _bet2 = false;
        _bet3 = false;
        _bet4 = false;
        _bet5 = false;
        _bet6 = false;
        _bet7 = false;
        _bet8 = false;
        _bet9 = false;
        _bet10 = false;
        _bet11 = false;



        //CardDistributor();
        StartCoroutine(GameShuffle(remTime));

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
        random6 = new System.Random(GenerateConsistentHash(seeds[5]));
        random7 = new System.Random(GenerateConsistentHash(seeds[6]));
        random8 = new System.Random(GenerateConsistentHash(seeds[7]));
        random9 = new System.Random(GenerateConsistentHash(seeds[8]));
        random10 = new System.Random(GenerateConsistentHash(seeds[9]));
        random11 = new System.Random(GenerateConsistentHash(seeds[10]));


        // Skip ahead in the random sequences based on elapsed time
        int stepsToSkip = Mathf.FloorToInt(elapsedTimeInSeconds * 2); // 5 steps per second
        for (int i = 0; i < stepsToSkip; i++)
        {
            if (rvi < 45)
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
                randomValue9 += random9.Next(1, 10);
                randomValue10 += random10.Next(1, 10);
                randomValue11 += random11.Next(1, 10);
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
            }


            int k = (randomValue1 * 10);
            int l = (randomValue2 * 10);
            int m = (randomValue3 * 10);
            int n = (randomValue4 * 10);
            int o = (randomValue5 * 10);
            int p = (randomValue6 * 10);
            int q = (randomValue7 * 10);
            int r = (randomValue8 * 10);
            int s = (int)((randomValue9 / 2) * 10);//shark
            int t = (randomValue10 * 50);
            int u = (randomValue11 * 50);

            betOn1Text.text = $"<color=yellow>{bet1BetVal}</color><color=#02ccfe>/{k}</color>";
            betOn2Text.text = $"<color=yellow>{bet2BetVal}</color><color=#02ccfe>/{l}</color>";
            betOn3Text.text = $"<color=yellow>{bet3BetVal}</color><color=#02ccfe>/{m}</color>";
            betOn4Text.text = $"<color=yellow>{bet4BetVal}</color><color=#02ccfe>/{n}</color>";
            betOn5Text.text = $"<color=yellow>{bet5BetVal}</color><color=#02ccfe>/{o}</color>";
            betOn6Text.text = $"<color=yellow>{bet6BetVal}</color><color=#02ccfe>/{p}</color>";
            betOn7Text.text = $"<color=yellow>{bet7BetVal}</color><color=#02ccfe>/{q}</color>";
            betOn8Text.text = $"<color=yellow>{bet8BetVal}</color><color=#02ccfe>/{r}</color>";
            betOn9Text.text = $"<color=yellow>{bet9BetVal}</color><color=#02ccfe>/{s}</color>";
            betOn10Text.text = $"<color=yellow>{bet10BetVal}</color><color=#02ccfe>/{t}</color>";
            betOn11Text.text = $"<color=yellow>{bet11BetVal}</color><color=#02ccfe>/{u}</color>";
            jackpotText.text = "₹ " + (k + l + m + n + o + p + q + r + s + t + u);
            yield return new WaitForSeconds(updateInterval);
        }
    }






    IEnumerator GameShuffle(int remTime)
    {

        betRoda.SetActive(false);
        Coroutine betAmountCoroutineRef = StartCoroutine(BetAmountIncrease());
        

        StartCoroutine(burstManager.AnimStart());
        startBetPanel.SetActive(true);
        StartCoroutine(tileScript.CountDown(remTime -2));

        yield return new WaitForSeconds(1f);
        startBetPanel.SetActive(false);
        yield return new WaitForSeconds(remTime - 5);
        countDownRef.Hit(3);
        yield return new WaitForSeconds(1);

        countDownRef.Hit(2);
        burstManager.StopAnim();

        yield return new WaitForSeconds(1);

        countDownRef.Hit(1);
        yield return new WaitForSeconds(1);

        stopBetPanel.SetActive(true);

        betRoda.SetActive(true);
        StopCoroutine(betAmountCoroutineRef);
        yield return new WaitForSeconds(1);
        stopBetPanel.SetActive(false);
        
    }

    IEnumerator ShowResultEnum(int val, int randVal)
    {
        tileScript.TileStart(val, randVal);
        yield return null;
    }

    public IEnumerator WinnerDeclare(int index)
    {
        //resutl show wala panel;
       
        yield return new WaitForSeconds(2);
        winnerAudio.Play();
        winnerImgDisplayer.SetActive(true);
     

        Transform childTransform = tileScript.tiles[index].transform.GetChild(0);
        string winner = "";
        if (childTransform != null)
        {
            Image image = childTransform.GetComponent<Image>();
            if (image != null)
            {
                winner = image.sprite.name;
                // Use the 'winner' variable as needed
            }
        }
        if (winner == "one")
        {
            winnerNameTxt.text = itemNames[0];
            historyRef.AddHistory(0);
            //heros
            winnerImgDisplayerImage.sprite = items[0];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);

            target1A.SetBool("isWinner", true);
            if (_bet1)
            {
                winAmnt += bet1BetVal * 5;
            }

            burstManager.Winnerr(0);
   
        }


        if (winner == "two")
        {
            winnerNameTxt.text = itemNames[1];

            historyRef.AddHistory(1);

            //Trophy
            winnerImgDisplayerImage.sprite = items[1];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);
            target2A.SetBool("isWinner", true);

            if (_bet2)
            {
                winAmnt += bet2BetVal * 5;
            }
            burstManager.Winnerr(1);

        }

        if (winner == "three")
        {
            winnerNameTxt.text = itemNames[2];

            historyRef.AddHistory(2);

            //helmet
            winnerImgDisplayerImage.sprite = items[2];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);
            target3A.SetBool("isWinner", true);
            if (_bet3)
            {
                winAmnt += bet3BetVal * 5;
            }
            burstManager.Winnerr(2);
  
        }

        if (winner == "four")
        {
            winnerNameTxt.text = itemNames[3];

            historyRef.AddHistory(3);

            //ball
            winnerImgDisplayerImage.sprite = items[3];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);
            target4A.SetBool("isWinner", true);

            if (_bet4)
            {
                winAmnt += bet4BetVal * 5;
            }

            burstManager.Winnerr(3);
      
        }



        if (winner == "five")
        {
            winnerNameTxt.text = itemNames[4];

            historyRef.AddHistory(4);

            //red ball
            winnerImgDisplayerImage.sprite = items[4];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);
            target5A.SetBool("isWinner", true);

            if (_bet5)
            {
                winAmnt += bet5BetVal * 5;
            }

            burstManager.Winnerr(4);
     
        }

        if (winner == "six")
        {
            winnerNameTxt.text = itemNames[5];

            historyRef.AddHistory(5);

            //green ball
            winnerImgDisplayerImage.sprite = items[5];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);
            target6A.SetBool("isWinner", true);

            if (_bet6)
            {
                winAmnt += bet6BetVal * 5;
            }

            burstManager.Winnerr(5);
        
        }




        if (winner == "seven")
        {
            winnerNameTxt.text = itemNames[6];

            historyRef.AddHistory(6);

            //blue ball
            winnerImgDisplayerImage.sprite = items[6];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);
            target7A.SetBool("isWinner", true);
            if (_bet7)
            {
                winAmnt += bet7BetVal * 5;
            }

            burstManager.Winnerr(6);
   
        }

        if (winner == "eight")
        {
            winnerNameTxt.text = itemNames[7];

            historyRef.AddHistory(7);

            //white ball
            winnerImgDisplayerImage.sprite = items[7];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);
            target8A.SetBool("isWinner", true);

            if (_bet8)
            {
                winAmnt += bet8BetVal * 5;
            }

            burstManager.Winnerr(7);
            burstManager.Winnerr(7);
          
        }
        if (winner == "one" || winner == "two" || winner == "three" || winner == "four")
        {
            //left side wins


            target9A.SetBool("isWinner", true);

            if (_bet10)
            {
                winAmnt += bet10BetVal * 2;
            }

            
        }

        if (winner == "five" || winner == "six" || winner == "seven" || winner == "eight")
        {
            //left side wins


            target10A.SetBool("isWinner", true);

            if (_bet11)
            {
                winAmnt += bet11BetVal * 2;
            }


        }

      


        else if (winner == "100X")
        {
            winnerImgDisplayerImage.sprite = items[8];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);
        }
        
        else if (winner == "24X")
        {
            winnerImgDisplayerImage.sprite = items[9];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);
        }

         else if (winner == "PayAll")
        {
            winnerImgDisplayerImage.sprite = items[10];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);
        }

         else if (winner == "TakeAll")
        {
            historyRef.AddHistory(8);

            winnerImgDisplayerImage.sprite = items[11];
            yield return new WaitForSeconds(2f);
            winnerImgDisplayer.SetActive(false);
        }

        yield return new WaitForSeconds(2f);
      
        if (winAmnt > 0)
        {
            winPanel.SetActive(true);
            winPanelAmnt.text = "<size=36>You Win</size>\n₹" + (winAmnt).ToString("F1");
            winAudio.Play();
            walletAmount += winAmnt;
            walletText.text = walletAmount.ToString("F2");
        }
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(2);
        winPanel.SetActive(false);

        winnerImgDisplayer.SetActive(false);
        burstManager.MoveAllcoinsBack();






        Debug.Log("setting false");
        target1A.SetBool("isWinner", false);
        target2A.SetBool("isWinner", false);
        target3A.SetBool("isWinner", false);
        target4A.SetBool("isWinner", false);
        target5A.SetBool("isWinner", false);
        target6A.SetBool("isWinner", false);
        target7A.SetBool("isWinner", false);
        target8A.SetBool("isWinner", false);
        target9A.SetBool("isWinner", false);
        target10A.SetBool("isWinner", false);
        target11A.SetBool("isWinner", false);
 
        yield return new WaitForSeconds(1);
        tileScript.tiles[index].color = new Color(1, 1, 1, 0);


       

        winnerImgDisplayerImage.sprite = null;
    }



    public void Bet(int val)
    {
        if(walletAmount < 50)
        {
            insufficientFundsPanel.SetActive(true);
            return;
        }
        int amount = betManagerRef.betVal;
        if (walletAmount < amount)
        {
            insufficientFundsObj.SetActive(true);
            return;
        }

        walletAmount -= betManagerRef.betVal;
        walletText.text = walletAmount.ToString("F2");
        totalBidAmount += betManagerRef.betVal;
        socketRef.SendBetDataToServer(val, amount);
        switch (val)
        {
            case 1:
                _bet1 = true;
                bet1BetVal += amount;
                break;

            case 2:
                _bet2 = true;
                bet2BetVal += amount;
                break;

            case 3:
                _bet3 = true;
                bet3BetVal += amount;
                break;

            case 4:
                _bet4 = true;
                bet4BetVal += amount;
                break;

            case 5:
                _bet5 = true;
                bet5BetVal += amount;
                break;

            case 6:
                _bet6 = true;
                bet6BetVal += amount;
                break;

            case 7:
                _bet7 = true;
                bet7BetVal += amount;
                break;

            case 8:
                _bet8 = true;
                bet8BetVal += amount;
                break;

            case 9:
                _bet9 = true;
                bet9BetVal += amount;
                break;

            case 10:
                _bet10 = true;
                bet10BetVal += amount;
                break;


            case 11:
                _bet11 = true;
                bet11BetVal += amount;
                break;


        }

    }





}

