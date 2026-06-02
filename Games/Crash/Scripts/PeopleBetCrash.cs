using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PeopleBetCrash : MonoBehaviour
{

    public TextMeshProUGUI jackpotText;
    public int jackpotAmount;
    public float gapBetweenCoins = 0.003f;
    public AudioSource CoinsSound;
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
    public Coroutine jackpotCoroutine;

    private void OnDisable()
    {
        CoinsSound.Stop();
    }
    public IEnumerator JackPotText()
    {
        jackpotAmount = 6595000;
        do
        {
            jackpotAmount += Random.Range(100, 1000);
            jackpotText.text = "Jackpot : " + jackpotAmount;
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        }
        while (_burstChk);
    }
    // Start is called before the first frame update
    void Start()
    {
        _burstChk = true;
        avoid = new List<int>();
        myPos = Vector3.zero;
        for (int i = 0; i < coinPrefabList.Count; i++)
        {
            InstantiatePrefabsInParent(coinPrefabList[i]);
        }
    }
    public void StopAnim()
    {
        _burstChk = false;
        if(jackpotCoroutine != null)
        StopCoroutine(jackpotCoroutine);
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
        foreach (GameObject child in coinsMoved)
        {
            Transform myTrnasform = child.GetComponent<Transform>();
            child.transform.localPosition = myPos;

        }
        
        foreach(Transform g in coinsToMoveList)
        {
            g.transform.localPosition = myPos;
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



    void InstantiatePrefabsInParent(GameObject prefabObject)
    {
        for (int i = 0; i < 100; i++)
        {
            GameObject instantiatedPrefab = Instantiate(prefabObject, parentObject.transform);
            instantiatedPrefab.GetComponent<Transform>().localPosition = Vector3.zero;
            coinsToMoveList.Add(instantiatedPrefab.GetComponent<Transform>());
        }
    }
    public IEnumerator AnimStart()
    {
        avoid.Clear();
        _burstChk = true;
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Wave());
        jackpotCoroutine = StartCoroutine(JackPotText());
    }
    int w = 0;
    public IEnumerator Wave()
    {
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
        CoinsSound.Stop();

        yield return new WaitForSeconds(Random.Range(1, 3));

        if (_burstChk && coinsToMoveList.Count != 0)
        {
            StartCoroutine(Wave());
        }
    }


    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {
        if (!_burstChk)
        {
            yield break;
        }
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

  
}
