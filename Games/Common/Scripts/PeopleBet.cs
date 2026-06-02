using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PeopleBet : MonoBehaviour
{
    AudioSource coinSound;
    public Animator coverAnimator;
    public GameObject winTextObject;
    public TextMeshProUGUI winText;
    public List<int> avoid;
    public float area;
    public float areaX;
    public List<GameObject> coinPrefabList, coinsMoved;
    public List<Vector3> destinationList;
    public float speed;
    public List<Transform> coinsToMoveList;
    public GameObject parentObject;
    public bool _isAnim;
    public Vector3 myPos;

    public TextMeshProUGUI walletAmtText;
    public int wallet;
    // Start is called before the first frame update
    void Start()
    {
        coinSound = gameObject.GetComponent<AudioSource>();
        avoid = new List<int>();
        myPos = Vector3.zero;
        for (int i = 0; i < coinPrefabList.Count; i++)
        {
            InstantiatePrefabsInParent(coinPrefabList[i]);
        }
        winTextObject.SetActive(false);
    }
    public void Winner()
    {
        winText.text = " +" + Random.Range(1, 100).ToString() + "00";
        winTextObject.SetActive(true);
        Invoke("DisableWinTextObj", 4);
    }


    public void Winner(int winVal)
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
        }
    }


    public void DisableWinTextObj()
    {
        winTextObject.SetActive(false);

    }
    public void WalletUpdate()
    {
        wallet = Random.Range(1000, 100000);
        walletAmtText.text = "Rs. " + wallet.ToString();
    }


    public void MoveCoinsBackWithLove()
    {
        foreach (GameObject child in coinsMoved)
        {
            Transform myTrnasform = child.GetComponent<Transform>();
            StartCoroutine(MoveObjectsBackWithLove(myTrnasform.localPosition, myPos, myTrnasform));
        }
    }
    public void MoveAllcoinsBack()
    {
      foreach(GameObject child in coinsMoved)
        {
            Transform myTrnasform = child.GetComponent<Transform>();
            StartCoroutine(MoveObjectsBack(myTrnasform.localPosition, myPos, myTrnasform));
        }
    }
    void InstantiatePrefabsInParent(GameObject prefabObject)
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject instantiatedPrefab = Instantiate(prefabObject, parentObject.transform);
            instantiatedPrefab.GetComponent<Transform>().localPosition= Vector3.zero;
            coinsToMoveList.Add(instantiatedPrefab.GetComponent<Transform>());

        }
    }
    public IEnumerator AnimStart()
    {
        _isAnim = true;
        avoid.Clear();
        WalletUpdate();

        yield return new WaitForSeconds(1f);
        StartCoroutine(Wave());
    }

    public IEnumerator Wave()
    {
        coverAnimator.SetBool("_is", true);
        coinSound.Play();
        int k = 0, d = 0, c_num = 0 ;
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
        yield return new WaitForSeconds(Random.Range(2, 5f));
        coverAnimator.SetBool("_is", false);
        yield return new WaitForSeconds(0.1f);
        if (_isAnim && coinsToMoveList.Count != 0)
        {

            StartCoroutine(Wave());

        }

    }

    public void AnimStop()
    {
        _isAnim = false;
    }

    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {
        int k = Random.Range(100, 1000);
        if(wallet >= k)
        {
            wallet -= k;

        }
        walletAmtText.text = "Rs. " + wallet.ToString();
        Vector3 targetPosModified = targetPos + new Vector3(Random.Range(-areaX, areaX), Random.Range(-area, area), 0);

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos , targetPosModified);
        
        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.localPosition = Vector3.Lerp(initialPos, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPosModified;

        yield return new WaitForSeconds(2f);

        coinTransform.localPosition = myPos;
    }


  
    IEnumerator MoveObjectsBack(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {
       

        Vector3 targetPosModified = targetPos;
        /*
                float startTime = Time.time;
                float journeyLength = Vector3.Distance(initialPos, targetPosModified);

                while (Time.time - startTime < journeyLength / speed)
                {
                    float fraction = (Time.time - startTime) / (journeyLength / speed);
                    coinTransform.localPosition = Vector3.Lerp(initialPos, targetPosModified, fraction);
                    yield return null;
                }*/
        coinTransform.localPosition = targetPosModified;
        yield return null;

    }

    IEnumerator MoveObjectsBackWithLove(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {


        Vector3 targetPosModified = targetPos;

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPosModified);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.localPosition = Vector3.Lerp(initialPos, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPosModified;
        yield return null;

    }

}
