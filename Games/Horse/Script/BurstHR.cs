using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstHR : MonoBehaviour
{
    public bool _is = false;
    public AudioSource multiCoinSound;
    public List<Transform> otherPlayerPosList;
    public Transform topDistributorRef;
    public int coinsTogetherCount = 2;
    public float gapBetweenCoins = 0.003f;
    public Animator CoverImgAnimator;
    public int burstAmt = 100;
    public float area;
    public float areaY;
    public List<GameObject> coinPrefabList, coinsMoved;
    public List<Transform> destinationObjectList;
    public List<Vector3> destinationList;
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
    public float stayTime = 7f;


    // Start is called before the first frame update
    void Start()
    {
        if (_is)
        {
            destinationList = new List<Vector3>();
            foreach (Transform t in destinationObjectList)
            {
                destinationList.Add(t.localPosition);
            }
        }


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
                StartCoroutine(MoveObjectWinner(myTrnasform.localPosition, topDistributorRef.localPosition, myTrnasform, 0, true, false));
            }
        }
        yield return new WaitForSeconds(1f);

        int tmp = 0;
        if (val == 0)
        {
            tmp = 1;
        }
        multiCoinSound.Play();

        for (int i = 0; i < parentObj[tmp].transform.childCount; i++)
        {
            Transform myTrnasform = parentObj[tmp].transform.GetChild(i);
            StartCoroutine(MoveObjectWinner(myTrnasform.localPosition, destinationList[val], myTrnasform, 0, false, false));
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
        for (int s = 0; s < parentObj[val].transform.childCount; s++)
        {
            tmpCoinHolderList.Add(parentObj[val].transform.GetChild(s).transform);
        }

        for (int s = 0; s < parentObj[tmp].transform.childCount; s++)
        {
            tmpCoinHolderList.Add(parentObj[tmp].transform.GetChild(s).transform);

        }
        if (otherPlayerWinList == null)
        {
            yield break;
        }
        int tmps = tmpCoinHolderList.Count / (otherPlayerWinList.Count + 1);

        for (int i = 0; i < otherPlayerWinList.Count; i++)
        {
            for (int k = 0; k < tmps; k++)
            {
                Transform myTrnasform = tmpCoinHolderList[k];
                StartCoroutine(MoveObjectWinner(myTrnasform.localPosition, otherPlayerPosList[otherPlayerWinList[i] - 1].localPosition, myTrnasform, 0, true, false));
                tmpCoinHolderList.RemoveAt(0);
            }
        }

        for (int k = 0; k < tmpCoinHolderList.Count; k++)
        {
            Transform myTrnasform = tmpCoinHolderList[k];
            StartCoroutine(MoveObjectWinner(myTrnasform.localPosition, myPos, myTrnasform, 0, true, false));
        }


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

    public IEnumerator AnimStart(float remTime, int roundDuration)
    {
        //yield return new WaitForSeconds(0.5f);
        _burstChk = true;
        //yield return new WaitForSeconds(0.1f);
        yield return null;
        coinsToMoveList.Clear();
        for (int i = 0; i < coinsToMoveListInitial.Count; i++)
        {
            coinsToMoveList.Add(coinsToMoveListInitial[i]);
        }

        if (remTime < roundDuration - 5)
        {

            //thode coins daal do table p ek dam se
            for (int i = 0; i < burstAmt; i++)
            {
                if (coinsToMoveList.Count < 5)
                {
                    break;
                }
                int d = Random.Range(0, destinationList.Count);
                int c_num = Random.Range(0, coinsToMoveList.Count);
                Vector3 targetPosModified = destinationList[d] + new Vector3(Random.Range(-area, area), Random.Range(-areaY, areaY), 0);
                coinsToMoveList[c_num].transform.localPosition = targetPosModified;
                coinsToMoveList[c_num].transform.SetParent(parentObj[d].transform);
                coinsMoved.Add(coinsToMoveList[c_num].gameObject);
                coinsToMoveList.RemoveAt(c_num);
            }
        }

        if (remTime > 3)
        {
            StartCoroutine(Wave());
        }
    }


    public IEnumerator Wave()
    {
        CoverImgAnimator.SetTrigger("_is");
        int k = 0, d = 0, c_num = 0;

        multiCoinSound.Play();
        for (int i = 0; i < burstAmt; i++)
        {
            if (_burstChk)
            {
                if (coinsToMoveList.Count < 20)
                {
                    yield return new WaitForSeconds(2f);
                    continue;
                }
                d = Random.Range(0, destinationList.Count);


                c_num = Random.Range(0, coinsToMoveList.Count);

                StartCoroutine(MoveObject(myPos, destinationList[d], coinsToMoveList[c_num], d, false, true, c_num));
                coinsMoved.Add(coinsToMoveList[c_num].gameObject);
                coinsToMoveList.RemoveAt(c_num);

                k++;
                if (i % coinsTogetherCount == 0)
                {
                    yield return new WaitForSeconds(gapBetweenCoins);
                }
            }
        }

        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        if (_burstChk && coinsToMoveList.Count != 0)
        {
            StartCoroutine(Wave());
        }
    }


    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform, int destination, bool exact = false, bool randomParent = true, int c_num = -1)
    {
        if (!_burstChk) yield break;
        Vector3 ip = initialPos + new Vector3(Random.Range(0, 30), Random.Range(0, 30));
        Vector3 targetPosModified = targetPos;
        if (!exact)
        {
            targetPosModified = targetPos + new Vector3(Random.Range(-area, area), Random.Range(-areaY, areaY), 0);
        }


        if (randomParent)
        {
            coinTransform.SetParent(parentObj[destination].transform);
        }

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(ip, targetPosModified);

        while (Time.time - startTime < journeyLength / speed && _burstChk)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.localPosition = Vector3.Lerp(ip, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPosModified;
        if (_burstChk)
        {
            yield return new WaitForSeconds(stayTime);

        }

        if (_burstChk)
        {
            coinTransform.localPosition = initialPos;
            coinsMoved.Remove(coinTransform.gameObject);
            coinsToMoveList.Add(coinTransform);
        }
    }

    IEnumerator MoveObjectWinner(Vector3 initialPos, Vector3 targetPos, Transform coinTransform, int destination, bool exact = false, bool randomParent = true, int c_num = -1)
    {
        Vector3 ip = initialPos + new Vector3(Random.Range(0, 30), Random.Range(0, 30));
        Vector3 targetPosModified = targetPos;
        if (!exact)
        {
            targetPosModified = targetPos + new Vector3(Random.Range(-area, area), Random.Range(-areaY, areaY), 0);
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
        

        if (_burstChk)
        {
            coinTransform.localPosition = initialPos;
            coinsMoved.Remove(coinTransform.gameObject);
            coinsToMoveList.Add(coinTransform);
        }
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
