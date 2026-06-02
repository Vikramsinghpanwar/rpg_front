using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TopDest{

  public Transform destinationObj;
  public float areaX; 
  public float areaY;

}
public class BurstAB : MonoBehaviour
{
    public AudioSource multiCoinSound;
    public List<Transform> otherPlayerPosList;
    public Transform topDistributorRef;
    public int coinsTogetherCount = 2;
    public float gapBetweenCoins = 0.003f;
    public Animator CoverImgAnimator;
    public int burstAmt = 100;

    public List<GameObject> coinPrefabList;
    public List<TopDest> destinationList;
    public float speed;
    public List<Transform> coinsToMoveListInitial;
    public List<Transform> coinsToMoveList;
    public GameObject[] parentObj;
    public GameObject psedoParentObject;
    public Vector3 myPos;
    public bool _burstChk;
    [Range(0, 100)]
    public int frequenceOf5k;
    [Range(0, 100)]
    public int frequenceOf1k;
    [Range(0, 100)]
    public int frequenceOf500;
    private int numberOfCoins = 200;
    TableBotManager botManagerRef;
    private Vector3[] worldCorners = new Vector3[4];

    private List<GameObject> coinPool;

    void Start()
    {
        coinsToMoveListInitial = new List<Transform>(numberOfCoins);
        coinsToMoveList = new List<Transform>(coinsToMoveListInitial);


        botManagerRef = FindFirstObjectByType<TableBotManager>();
        myPos = psedoParentObject.transform.position;
        _burstChk = true;
        foreach (TopDest t in destinationList)
        {
            RectTransform rectTransform = t.destinationObj.GetComponent<RectTransform>();
            rectTransform.GetWorldCorners(worldCorners);
            t.areaX = Vector3.Distance(worldCorners[0], worldCorners[3]) / 3f;
            t.areaY = Vector3.Distance(worldCorners[0], worldCorners[1]) / 3f;
        }


        for (int i = 0; i < coinPrefabList.Count; i++)
        {
            int coinCount;
            if (i == coinPrefabList.Count - 1) coinCount = frequenceOf5k;
            else if (i == coinPrefabList.Count - 2) coinCount = frequenceOf1k;
            else if (i == coinPrefabList.Count - 3) coinCount = frequenceOf500;
            else coinCount = numberOfCoins;
            InstantiateCoins(coinPrefabList[i], coinCount);
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(AnimStart());
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            StopAnim();
        }
    }
    void InstantiateCoins(GameObject coinPrefab, int count)
    {

        count = Mathf.Min(count, numberOfCoins);

        for (int i = 0; i < count; i++)
        {
            GameObject instantiatedPrefab = Instantiate(coinPrefab, psedoParentObject.transform);
            instantiatedPrefab.transform.localPosition = Vector3.zero;
            coinsToMoveListInitial.Add(instantiatedPrefab.transform);
        }
    }


    public void StopAnim()
    {
        _burstChk = false;

    }





    public IEnumerator AnimStart()
    {

        _burstChk = true;
        yield return new WaitForSeconds(0.1f);
        coinsToMoveList = new List<Transform>(coinsToMoveListInitial);
        yield return new WaitForSeconds(0.5f);
        RunCoroutine(Wave());
    }
    IEnumerator Wave()
    {
        CoverImgAnimator.SetTrigger("_is");
        multiCoinSound.Play();

        int k = 0;
        int d;
        int c_num;
        if (coinsToMoveList.Count < 20)
        {
            yield break;
        }
        for (int i = 0; i < burstAmt; i++)
        {
            if (coinsToMoveList.Count < 5)
            {
                break;
            }
            if (Random.Range(0, 2) == 0)
            {
                d = Random.Range(0, destinationList.Count - 2);
            }else
            d = Random.Range(destinationList.Count - 2, destinationList.Count);
            c_num = Random.Range(0, coinsToMoveList.Count);
            RunCoroutine(MoveObject(myPos, destinationList[d].destinationObj.position, coinsToMoveList[c_num], d));

            coinsToMoveList.RemoveAt(c_num);

            k++;
            if (i % coinsTogetherCount == 0)
            {
                yield return new WaitForSeconds(gapBetweenCoins);
            }
        }

        yield return new WaitForSeconds(Random.Range(0.5f, 0.8f));

        if (_burstChk && coinsToMoveList.Count != 0)
        {
            RunCoroutine(Wave());
        }
    }

    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform, int destination, bool exact = false, bool randomParent = true)
    {
        Vector3 targetPosModified = targetPos + (!exact ? new Vector3(Random.Range(-destinationList[destination].areaX, destinationList[destination].areaX),
                                                                     Random.Range(-destinationList[destination].areaY, destinationList[destination].areaY)) : Vector3.zero);

        if (randomParent) coinTransform.SetParent(parentObj[destination].transform);
        if (parentObj[destination].transform.childCount > 40)
        {
            Transform t = parentObj[destination].transform.GetChild(0);
            ResetCoin(t);
            coinsToMoveList.Remove(t);
        }
        float duration = 1000f / speed;
        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPosModified);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.position = Vector3.Lerp(initialPos, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.position = targetPosModified;

    }

    IEnumerator MoveCoinsToPlayers(Vector3 targetPos, Transform coinTransform)
    {
        Vector3 initialPos = coinTransform.position;

        Vector3 targetPosModified = targetPos;
        float duration = 2000f / speed;
        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPosModified);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.position = Vector3.Lerp(initialPos, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.position = targetPosModified;

    }

    IEnumerator MoveCoinsBack(Transform coinTransform)
    {
        Vector3 initialPos = coinTransform.position;

        Vector3 targetPosModified = myPos;
        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPosModified);


        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.position = Vector3.Lerp(initialPos, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.position = myPos;


        ResetCoin(coinTransform);
    }




    void ResetCoin(Transform coin)
    {
        coin.position = myPos;
        coin.SetParent(psedoParentObject.transform);
    }

    public IEnumerator WinEnum(int val, int cardWinVal, int[] otherPlayerWinList)
    {
        MoveCoinsTopDistributor(cardWinVal, val);
        yield return new WaitForSeconds(1f);
        int tmp = 0;
        if (val == 0)
        {
            tmp = 1;
        }
        CollectCoinsOnWinner(val, tmp, cardWinVal);
        yield return new WaitForSeconds(1f);

        DistributeCoins(val, cardWinVal, otherPlayerWinList);
        yield return new WaitForSeconds(4f);
    }


    public void MoveCoinsTopDistributor(int winner, int val)
    {
        multiCoinSound.Play();
        for (int i = 0; i < parentObj.Length; i++)
        {
            for (int k = 0; k < parentObj[i].transform.childCount; k++)
            {
                Transform myTrnasform = parentObj[i].transform.GetChild(k);

                if (i == winner || (val == 0 && i == 8) || (val == 1 && i == 9))
                {
                    continue;
                }

                RunCoroutine(MoveObject2TopDistributor(topDistributorRef.position, myTrnasform));
            }
        }
    }

    IEnumerator MoveObject2TopDistributor(Vector3 targetPos, Transform coinTransform)
    {
        Vector3 targetPosModified = targetPos;
        Vector3 initialPos = coinTransform.position;

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPosModified);


        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.position = Vector3.Lerp(initialPos, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.position = myPos;


        coinTransform.SetParent(topDistributorRef);
    }

    void CollectCoinsOnWinner(int val, int tmp, int cardWinVal)
    {
        multiCoinSound.Play();
        int portion = (int)(topDistributorRef.childCount / 5f);
        for (int i = 0; i < topDistributorRef.childCount - portion; i++)
        {
            Transform myTrnasform = topDistributorRef.GetChild(i);
            RunCoroutine(MoveObject(myTrnasform.position, destinationList[val == 0 ? 8 : 9].destinationObj.position, myTrnasform, 0, false, false));
        }

        for (int i = topDistributorRef.childCount - portion; i < topDistributorRef.childCount; i++)
        {
            Transform myTrnasform = topDistributorRef.GetChild(i);
            RunCoroutine(MoveObject(myTrnasform.position, destinationList[cardWinVal].destinationObj.position, myTrnasform, 0, false, false));
        }
    }

    void DistributeCoins(int val, int winNum, int[] otherPlayerWinList)
    {
        multiCoinSound.Play();

        for (int k = 0; k < topDistributorRef.childCount; k++)
        {
            Transform myTrnasform = topDistributorRef.GetChild(k);
            RunCoroutine(MoveCoinsBack(myTrnasform));
        }

        foreach (Transform t in parentObj[winNum].transform)
        {
            RunCoroutine(MoveCoinsBack(t));
        }

        if (otherPlayerWinList != null)
        {
            int winners = 0;
            foreach (int i in otherPlayerWinList)
            {
                if (i > 0) winners++;
            }

            for (int i = 0; i < 6; i++)
            {
                if (otherPlayerWinList[i] > 0)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        Transform myTrnasform = parentObj[val == 0 ? 8 : 9].transform.GetChild(0);
                        RunCoroutine(MoveCoinsToPlayers(otherPlayerPosList[i].position, myTrnasform));
                        myTrnasform.SetParent(topDistributorRef);
                    }
                }
            }

            botManagerRef.BotWin(otherPlayerWinList);

        }


        foreach (Transform t in parentObj[val == 0 ? 8 : 9].transform)
        {
            RunCoroutine(MoveCoinsBack(t));
        }

        foreach (Transform t in topDistributorRef.transform)
        {
            RunCoroutine(MoveCoinsBack(t));
        }



    }

    //////////////////////--------------------------------------
    ///Optimization

    private List<IEnumerator> activeCoroutines = new List<IEnumerator>();

    void RunCoroutine(IEnumerator coroutine)
    {
        activeCoroutines.Add(coroutine);
        StartCoroutine(RunCoroutineWrapper(coroutine));
    }

    IEnumerator RunCoroutineWrapper(IEnumerator coroutine)
    {
        yield return StartCoroutine(coroutine);
        activeCoroutines.Remove(coroutine);
    }

    // Later when you need to stop all:
    void StopAllActiveCoroutines()
    {
        foreach (var coroutine in activeCoroutines)
        {
            StopCoroutine(coroutine);
        }
        activeCoroutines.Clear();
    }

}


