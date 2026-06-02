using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableBotManager : MonoBehaviour
{
    public static TableBotManager Instance { get; set;}
    public class CoinData
    {
        public Transform transform;
        public Vector3 initPos;
    }
    public bots[] botsData;
    public Transform[] DestinationList;
    public float area = 100f;
    public float speed = 2000f;
    public List<GameObject> coinPrefabList;
    public GameObject starPrefab;
    int[] betAmountArray = new int[] { 10, 50, 100, 500, 1000, 5000 };
    List<BotData> botsDataServerList;
    List<CoinData> coinsOnTable;

    public Image[] luckyStarArray;

    void Awake()
    {
        if(Instance != this)
        {
            Instance = this;
        }
    }

    public void Reset()
    {
        foreach( Image i in luckyStarArray)
        {
            i.fillAmount = 0;
        }
    }
    private void OnEnable()
    {
        foreach(bots obj in botsData)
        {
            obj.coinSound = obj.coverImgAnimator.GetComponent<AudioSource>();
            obj.winText = obj.winTextObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        }
        coinsOnTable = new List<CoinData>();
    }

    public void InitializeBots(List<BotData> botsList, long roundStartTime)
    {
        botsDataServerList = new List<BotData>();
        foreach(BotData d in botsList)
        {
            botsDataServerList.Add(d);
        }
        for (int i = 0; i < botsList.Count; i++)
        {
            botsData[i].profilePic.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[botsList[i].botId];
            botsData[i].nameTxt.text = botsList[i].botName;
            //botsData[i].walletTxt.text = "Rs." + botsList[i].botWallet;
            StartCoroutine(BettingBot(botsList[i].bets, roundStartTime, i));
        }

        long currentTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float elapsedTimeInSeconds = (currentTimestamp - roundStartTime) / 1000f;
    }

    IEnumerator BettingBot(List<BotBetsData> betsList, long roundStartTime, int index)
    {
        // Sort the bets in ascending order by time
        betsList.Sort((bet1, bet2) => bet1.time.CompareTo(bet2.time));

        // Loop through the sorted bets and place them at the correct time
        for (int i = 0; i < betsList.Count; i++)
        {
            long currentTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            float elapsedTimeInSeconds = (currentTimestamp - roundStartTime) / 1000f;

            // Calculate the delay required for placing the bet
            float delay = betsList[i].time - elapsedTimeInSeconds;
        
            // If the bet's time is already passed
            if (delay <= 0)
            {
                // Adjust the wallet without waiting and place the bet immediately

                botsDataServerList[index].botWallet -= betAmountArray[betsList[i].amount];
                //botsData[index].walletTxt.text = "Rs." + botsDataServerList[index].botWallet.ToString("F2");
                if(index == 3)
                {
                    luckyStarArray[betsList[i].betOn].fillAmount += 0.3f;
                }
            }
            else
            {
                // Wait until the correct time to place the bet
                yield return new WaitForSeconds(delay);

                // After the delay, place the bet and update the wallet
                // botsDataServerList[index].botWallet -= betAmountArray[betsList[i].amount];
                //botsData[index].walletTxt.text = "Rs." + botsDataServerList[index].botWallet.ToString("F2");
                PlaceBet(index, betsList[i]);
            }

            // Ensure the coroutine continues smoothly
            yield return null;
        }
    }



    void PlaceBet(int index, BotBetsData bet)
    {
        GameObject coin = Instantiate(coinPrefabList[bet.amount], botsData[index].coinHolder.transform);
        StartCoroutine(MoveObject(
            botsData[index].coinHolder.transform.position, 
            DestinationList[bet.betOn].position, 
            coin.transform,
            speed
        ));
        botsData[index].coinSound.Play();
        botsData[index].coverImgAnimator.SetTrigger("_is");

        if(index == 3)
        {
            GameObject star = Instantiate(starPrefab, botsData[index].coinHolder.transform);
            StartCoroutine(MoveObject(
                botsData[index].coinHolder.transform.position, 
                luckyStarArray[bet.betOn].transform.position, 
                star.transform, 
                1600, 
                true
            ));
            luckyStarArray[bet.betOn].fillAmount += 0.3f;
        }
    }

    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform, float speed = 1000f, bool _instant = false)
    {
        int k = Random.Range(100, 1000);
         
        Vector3 targetPosModified = targetPos + new Vector3(Random.Range(-area, area), Random.Range(-area, area), 0);
        if(_instant) targetPosModified = targetPos;

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPosModified);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.position = Vector3.Lerp(initialPos, targetPosModified, fraction);
            yield return null;
        }
        

        coinTransform.position = targetPosModified;
        if (_instant)
        {
            coinTransform.transform.position = initialPos;
        }
        else
        {
            CoinData thisCoin = new CoinData();
            thisCoin.initPos = initialPos;
            thisCoin.transform = coinTransform;
            coinsOnTable.Add(thisCoin);
        }
    }

    public void ClearBets()
    {
        foreach(var coin in coinsOnTable)
        {
            coin.transform.position = coin.initPos; 
        }
    }

    public void BotWin(int[] winArray)
    {
        ClearBets();
        for(int i= 0; i< winArray.Length; i++)
        {
            if(winArray[i] != 0)
            {
                botsData[i].winText.text = " +" + winArray[i];
                //botsData[i].walletTxt.text = "Rs." + (botsDataServerList[i].botWallet + winArray[i]);
                botsData[i].winTextObj.SetActive(true);
            }
        }
        Invoke("OffWins", 2f);
    }

    public void OffWins()
    {
        for (int i = 0; i <botsData.Length; i++)
        {
            botsData[i].winText.text = " +";
            botsData[i].winTextObj.SetActive(false);
        }
    }

}

[System.Serializable]
public class bots
{
    public Image profilePic;
    public TextMeshProUGUI nameTxt;
    //public Text walletTxt;
    public TextMeshProUGUI winText;
    public GameObject winTextObj;
    public GameObject coinHolder;
    public Animator coverImgAnimator;
    public AudioSource coinSound;
}


[System.Serializable]
public class BotData
{
    public int botId { get; set; }
    public string botName { get; set; }
    public int botWallet { get; set; }
    public List<BotBetsData> bets { get; set; }
}

[System.Serializable]
public class BotBetsData
{
    public int amount { get; set; }
    public int time { get; set; }
    public int betOn { get; set; }
}


[System.Serializable]
public class BotsWrapper
{
    public List<BotData> botsData;
}