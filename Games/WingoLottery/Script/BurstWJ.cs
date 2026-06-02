using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Dest{
    public Vector2 numDestination;
  public float areaX; 
  public float areaY;

  
}
public class BurstWJ : MonoBehaviour
{
    public int coinStayCount = 50;
    public AudioSource multiCoinSound;
    public List<Transform> otherPlayerPosList;
    public Transform topDistributorRef;
    public int coinsTogetherCount = 2;
    public float gapBetweenCoins = 0.003f;
    public Animator CoverImgAnimator;
    public int burstAmt = 100;
   
    public List<GameObject> coinPrefabList, coinsMoved;
    public List<Dest> destinationList;
    public float speed;
    public List<Transform> coinsToMoveListInitial;
    public List<Transform> coinsToMoveList;
    public GameObject[] parentObj;
    public GameObject psedoParentObject;
    public Vector3 myPos;
    public bool _burstChk;
    [Range(0, 100)]
    public float frequenceOf5k;
    [Range(0, 100)]
    public float frequenceOf1k;
    [Range(0, 100)]
    public float frequenceOf500;

    void Start()
    {
        _burstChk = true;
        myPos = Vector3.zero;
        for (int i = 0; i < coinPrefabList.Count; i++)
        {
            if (i == coinPrefabList.Count - 1)
            {
                InstantiatePrefabsInParent(coinPrefabList[i], 5000);
            }
            else if (i == coinPrefabList.Count - 2)
            {
                InstantiatePrefabsInParent(coinPrefabList[i], 1000);
            }
            else if (i == coinPrefabList.Count - 3)
            {
                InstantiatePrefabsInParent(coinPrefabList[i], 500);
            }
            else
                InstantiatePrefabsInParent(coinPrefabList[i], 0);
        }

    }


    public void StopAnim()
    {
        _burstChk = false;
    }

    public void Winner(int winVal)
    {
        switch (winVal)
        {
            case 0:
                ///tie
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[1].numDestination, myTrnasform, 0));
                }
                break;
            case 1:
                //dragon wins
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[0].numDestination, myTrnasform, 0));
                }
                break;
            case 2:
                //tiger wins
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[2].numDestination, myTrnasform, 0));
                }
                break;
        }
    }


    public void MoveAllcoinsBack()
    {
        foreach (GameObject child in coinsMoved)
        {
            Transform myTrnasform = child.GetComponent<Transform>();
            StartCoroutine(MoveObjectBack(myTrnasform.localPosition, myPos, myTrnasform));
        }
        coinsMoved.Clear();

    }

   

    public void Winnerr(int winVal, List<int> otherPlayerWinList)
    {
        StartCoroutine(WinEnum(winVal, otherPlayerWinList));
        
    }

    public IEnumerator WinEnum(int val, List<int> otherPlayerWinList)
    {
        multiCoinSound.Play();
        for (int i = 0; i < parentObj.Length; i++)
        {
            if (i == val)
            {
                continue;
            }
            for (int k = 0; k < parentObj[i].transform.childCount; k++)
            {
                Transform myTrnasform = parentObj[i].transform.GetChild(k);
                StartCoroutine(MoveObject(myTrnasform.localPosition, topDistributorRef.localPosition, myTrnasform, 0, true, false));

            }
        }
        yield return new WaitForSeconds(1f);

        int tmp = 0;
        if(val == 0)
        {
            tmp = 1;
        }
        multiCoinSound.Play();

        for (int i=0; i< parentObj[tmp].transform.childCount; i++)
        {
            Transform myTrnasform = parentObj[tmp].transform.GetChild(i);
            StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[val].numDestination, myTrnasform, 0, false, false));
        }


        for (int i = 0; i < parentObj.Length; i++)
        {
            if (i == val || i == tmp)
            {
                continue;
            }
            for (int k = 0; k < parentObj[i].transform.childCount; k++)
            {
                Transform myTrnasform = parentObj[i].transform.GetChild(k);
                myTrnasform.localPosition = myPos;
            }

        }
        yield return new WaitForSeconds(0.5f);

        multiCoinSound.Play();

        List<Transform> tmpCoinHolderList = new List<Transform>();
        for(int s = 0; s<parentObj[val].transform.childCount; s++)
        {
            tmpCoinHolderList.Add(parentObj[val].transform.GetChild(s).transform);
        }
        
        for(int s = 0; s <parentObj[tmp].transform.childCount; s++)
        {
            tmpCoinHolderList.Add(parentObj[tmp].transform.GetChild(s).transform);

        }
        if(otherPlayerWinList == null)
        {
            yield break;
        }
        int tmps = tmpCoinHolderList.Count / (otherPlayerWinList.Count + 1);

        for (int i = 0; i< otherPlayerWinList.Count; i++)
        {
            for (int k = 0; k < tmps; k++)
            {
                Transform myTrnasform = tmpCoinHolderList[k];
                StartCoroutine(MoveObject(myTrnasform.localPosition, otherPlayerPosList[otherPlayerWinList[i] -1].localPosition, myTrnasform, 0, true, false));
                tmpCoinHolderList.RemoveAt(0);
            }
        }

        for (int k = 0; k < tmpCoinHolderList.Count; k++)
        {
            Transform myTrnasform = tmpCoinHolderList[k];
            StartCoroutine(MoveObject(myTrnasform.localPosition, myPos, myTrnasform, 0, true, false));
        }

        Debug.Log("tmp list : " + tmpCoinHolderList.Count);

        for (int k = 0; k < parentObj[val].transform.childCount; k++)
        {
            Transform myTrnasform = parentObj[val].transform.GetChild(k);
            myTrnasform.localPosition = myPos;
        }

        for (int k = 0; k < parentObj[tmp].transform.childCount; k++)
        {
            Transform myTrnasform = parentObj[tmp].transform.GetChild(k);
            myTrnasform.localPosition = myPos;

        }




    }



void InstantiatePrefabsInParent(GameObject prefabObject, int val)
{
    for (int i = 0; i < 200; i++)
    {
        GameObject instantiatedPrefab = Instantiate(prefabObject, psedoParentObject.transform);
        instantiatedPrefab.GetComponent<Transform>().localPosition = Vector3.zero;
        coinsToMoveListInitial.Add(instantiatedPrefab.GetComponent<Transform>());
        if (val == 5000 && i > frequenceOf5k)
        {
            break;
        }
        if (val == 1000 && i > frequenceOf1k)
        {
            break;
        }
        if (val == 500 && i > frequenceOf500)
        {
            break;
        }
    }
}
public IEnumerator AnimStart()
    {
        _burstChk = true;
        yield return new WaitForSeconds(0.1f);
        coinsToMoveList.Clear();
        for (int i = 0; i< coinsToMoveListInitial.Count; i++)
        {
            coinsToMoveList.Add(coinsToMoveListInitial[i]);
        }
        StartCoroutine(Wave());
    }
    public IEnumerator Wave()
    {
        CoverImgAnimator.SetTrigger("_is");
        int k = 0, d = 0, c_num = 0;
        
        multiCoinSound.Play();
        for(int i = 0; i< burstAmt; i++)
        {
            if (coinsToMoveList.Count < 20)
            {
                yield return new WaitForSeconds(2f);
                continue;
            }

            d = UnityEngine.Random.Range(0, destinationList.Count);
            c_num = UnityEngine.Random.Range(0, coinsToMoveList.Count);
       
            StartCoroutine(MoveObject(myPos, destinationList[d].numDestination, coinsToMoveList[c_num], d));
            coinsMoved.Add(coinsToMoveList[c_num].gameObject);
            coinsToMoveList.RemoveAt(c_num);
            

            k++;
            if(i%coinsTogetherCount == 0)
            {
                yield return new WaitForSeconds(gapBetweenCoins);

            }
        }

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.8f));
     
        if (_burstChk && coinsToMoveList.Count != 0)
        {
            StartCoroutine(Wave());
        }
    }


    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform, int destination, bool exact = false, bool randomParent = true)
    {
        Vector3 ip = initialPos + new Vector3(UnityEngine.Random.Range(0, 30), UnityEngine.Random.Range(0, 30));
        Vector3 targetPosModified = targetPos;
           float areaX = destinationList[destination].areaX;
           float areaY = destinationList[destination].areaY;
        if (!exact)
        {
            targetPosModified = targetPos + new Vector3(UnityEngine.Random.Range(-areaX, areaX), UnityEngine.Random.Range(-areaY, areaY), 0);
        }
        if (randomParent)
        {
            coinTransform.SetParent(parentObj[destination].transform);
        }


        float startTime = Time.time;
        float journeyLength = Vector3.Distance(ip, targetPosModified);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.localPosition = Vector3.Lerp(ip, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPosModified;

        if (destination == 10 || destination == 12)
        {
            if (parentObj[destination].transform.childCount > 40)
            {
                ResetCoin(parentObj[destination].transform.GetChild(0));
            }
        }

        if (parentObj[destination].transform.childCount > coinStayCount)
        {
            ResetCoin(parentObj[destination].transform.GetChild(0));
        }
    }

    void ResetCoin(Transform coin)
    {
        coin.localPosition = myPos;
        coinsMoved.Remove(coin.gameObject);
        coinsToMoveList.Add(coin);
        coin.SetParent(psedoParentObject.transform);
    }

    IEnumerator MoveObjectBack(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {
        coinTransform.SetParent(psedoParentObject.transform);

        Vector3 ip = initialPos;// + new Vector3(Random.Range(0, 120), Random.Range(0, 120));
        Vector3 targetPosModified = targetPos;// + new Vector3(Random.Range(-area, area), Random.Range(-area, area), 0);

        /*float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPos);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / 0.1f);
            coinTransform.localPosition = Vector3.Lerp(initialPos, targetPos, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPos;*/

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPosModified);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.localPosition = Vector3.Lerp(initialPos, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPosModified;

    }
}
