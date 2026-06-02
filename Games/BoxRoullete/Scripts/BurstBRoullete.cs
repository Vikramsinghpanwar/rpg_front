using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstBRoullete : MonoBehaviour
{
    public AudioSource CoinsSound;
    public Animator CoverImgAnimator;
    public int burstAmt = 100;
    public float area;
    public float areaY;
    public List<GameObject> coinPrefabList, coinsMoved;
    public List<Vector3> destinationList;
    public float speed;
    public List<Transform> coinsToMoveList;
    public List<Transform> initialCoinsList;
    public GameObject parentObject;
    public GameObject pSudoParentObject;
    public Vector3 myPos;
    public bool _burstChk;
    bool _canCoinMoveBack;
    // Start is called before the first frame update
    void Start()
    {
        
        _burstChk = true;
        myPos = Vector3.zero;
        for (int i = 0; i < coinPrefabList.Count; i++)
        {
            InstantiatePrefabsInParent(coinPrefabList[i]);
        }
    }
    public void StopAnim()
    {
        _canCoinMoveBack = false;
        _burstChk = false;

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
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[0], myTrnasform));
                }
                break;
            case 1:
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[1], myTrnasform));
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



    void InstantiatePrefabsInParent(GameObject prefabObject)
    {
        for (int i = 0; i < 100; i++)
        {
            GameObject instantiatedPrefab = Instantiate(prefabObject, pSudoParentObject.transform);
            instantiatedPrefab.GetComponent<Transform>().localPosition = Vector3.zero;
            initialCoinsList.Add(instantiatedPrefab.GetComponent<Transform>());
        }
    }

    public IEnumerator CoinBack()
    {
        if (coinsMoved.Count > 0 && _canCoinMoveBack)
        {
            coinsMoved[1].transform.localPosition = myPos;
            coinsToMoveList.Add(coinsMoved[1].transform);
            coinsMoved.Remove(coinsMoved[1]);
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(CoinBack());
        }
      
    }
    public IEnumerator AnimStart()
    {
        coinsToMoveList = new List<Transform>();
     
        for (int i = 0; i< initialCoinsList.Count; i++)
        {
            coinsToMoveList.Add(null);
            coinsToMoveList[i] = initialCoinsList[i];
        }
        _canCoinMoveBack = true;
        _burstChk = true;
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Wave());
        yield return new WaitForSeconds(5f);
        StartCoroutine(CoinBack());
    }
    int w = 0;
    public IEnumerator Wave()
    {
        CoverImgAnimator.SetBool("_is", true); ;
        int k = 0, d = 0, c_num = 0;
        CoinsSound.Play();
        for(int i = 0; i< burstAmt; i++)
        {
            if(coinsToMoveList.Count < 10)
            {
                break;
            }
            d = Random.Range(0, destinationList.Count);
            c_num = Random.Range(0, coinsToMoveList.Count);
      
            StartCoroutine(MoveObject(myPos, destinationList[d], coinsToMoveList[c_num], d));
            coinsMoved.Add(coinsToMoveList[c_num].gameObject);
            k++;
            coinsToMoveList.Remove(coinsToMoveList[c_num]);
            yield return new WaitForSeconds(0.0000001f);
        
        }
        CoinsSound.Stop();

        yield return new WaitForSeconds(Random.Range(0.2f, 2f));
        CoverImgAnimator.SetBool("_is", false); ;
        yield return new WaitForSeconds(0.2f);
        if (_burstChk && coinsToMoveList.Count != 0)
        {
            StartCoroutine(Wave());
        }
    }


    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform, int d = 0)
    {

        coinTransform.SetParent(parentObject.transform);
        Vector3 ip = initialPos + new Vector3(Random.Range(0, 30), Random.Range(0, 30));
        Vector3 targetPosModified = targetPos + new Vector3(Random.Range(-area, area), Random.Range(-areaY, areaY), 0);
        if(d < 2)
        {
            targetPosModified = targetPos + new Vector3(Random.Range(-50, 50), Random.Range(-10, 10), 0);

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

        coinTransform.gameObject.GetComponent<Animator>().SetBool("_is", true);
    }

    IEnumerator MoveObjectBack(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {
        coinTransform.SetParent(pSudoParentObject.transform);

        coinTransform.gameObject.GetComponent<Animator>().SetBool("_is", false);


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
