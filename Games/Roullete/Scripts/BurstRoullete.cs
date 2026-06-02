using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstRoullete : MonoBehaviour
{
    public float gapBetweenCoins = 0.003f;
    public AudioSource CoinsSound;
    public Animator CoverImgAnimator;
    public List<int> avoid;
    public int burstAmt = 100;
    public float area;
    public float areaY;
    public List<GameObject> coinPrefabList, coinsMoved;
    public List<Vector3> destinationList;
    public float speed;
    public List<Transform> coinsToMoveList;
    public GameObject parentObject;
    public Vector3 myPos;
    public bool _burstChk;
    [Range(0, 100)]
    public float frequenceOf5k;
    [Range(0, 100)]
    public float frequenceOf1k;
    [Range(0, 100)]
    public float frequenceOf500;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < parentObject.transform.childCount; i++)
        {
            destinationList.Add(parentObject.transform.GetChild(i).transform.localPosition);
        }
        _burstChk = true;
        avoid = new List<int>();
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
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[1], myTrnasform));
                }
                break;
            case 1:
                //dragon wins
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[0], myTrnasform));
                }
                break;
            case 2:
                //tiger wins
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[2], myTrnasform));
                }
                break;
        }
    }


    public void MoveAllcoinsBack()
    {
        Debug.Log("Movinng all coins back");
        foreach (GameObject child in coinsMoved)
        {
            Transform myTrnasform = child.GetComponent<Transform>();
            StartCoroutine(MoveObjectBack(myTrnasform.localPosition, myPos, myTrnasform));
        }
        coinsMoved.Clear();

    }



    public void Winnerr(int winVal)
    {
        switch (winVal)
        {
            case 0:
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[1], myTrnasform));
                }
                break;
            case 1:
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[0], myTrnasform));
                }
                break;
            case 2:
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[2], myTrnasform));
                }
                break;

            case 3:
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[3], myTrnasform));
                }
                break;
            case 4:
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[4], myTrnasform));
                }
                break;
            case 5:
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[5], myTrnasform));
                }
                break;

            case 6:

                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[6], myTrnasform));
                }
                break;
            case 7:
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[7], myTrnasform));
                }
                break;

        }
    }

    void InstantiatePrefabsInParent(GameObject prefabObject, int val)
    {
        for (int i = 0; i < 100; i++)
        {
            GameObject instantiatedPrefab = Instantiate(prefabObject, parentObject.transform);
            instantiatedPrefab.GetComponent<Transform>().localPosition = Vector3.zero;
            coinsToMoveList.Add(instantiatedPrefab.GetComponent<Transform>());
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

    public void SetBack()
    {
        for(int i = 0; i< coinsMoved.Count; i++)
        {
            coinsMoved[i].transform.localPosition = myPos;
        }
        coinsMoved.Clear();
    }

    public IEnumerator AnimStart()
    {
        avoid.Clear();
        _burstChk = true;
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Wave());
    }

    public IEnumerator Wave()
    {
        CoverImgAnimator.SetTrigger("_is");
        int k = 0, d = 0, c_num = 0;
        CoinsSound.Play();
        for (int i = 0; i < burstAmt; i++)
        {

            if (coinsToMoveList.Count < (coinsMoved.Count + burstAmt))
            {
                Debug.LogWarning("Breaked");
                break;
            }
            d = Random.Range(0, destinationList.Count);
            c_num = Random.Range(0, coinsToMoveList.Count);
            while (avoid.Contains(c_num))
            {
                c_num = Random.Range(0, coinsToMoveList.Count);
            }
            StartCoroutine(MoveObject(myPos, destinationList[d], coinsToMoveList[c_num]));
            avoid.Add(c_num);
            coinsMoved.Add(coinsToMoveList[c_num].gameObject);
            k++;
            yield return new WaitForSeconds(gapBetweenCoins);
        }

        yield return new WaitForSeconds(Random.Range(1, 3));
        CoverImgAnimator.ResetTrigger("_is");

        SetBack();
        if (_burstChk && coinsToMoveList.Count != 0)
        {
            StartCoroutine(Wave());
        }
    }


    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {
        Vector3 ip = initialPos + new Vector3(Random.Range(0, 30), Random.Range(0, 30));
        Vector3 targetPosModified = targetPos + new Vector3(Random.Range(-area, area), Random.Range(-areaY, areaY), 0);

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(ip, targetPosModified);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.localPosition = Vector3.Lerp(ip, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPosModified;
    }

    IEnumerator MoveObjectBack(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {
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
