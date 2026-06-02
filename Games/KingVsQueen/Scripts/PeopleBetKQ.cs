using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PeopleBetKQ : MonoBehaviour
{

    public GameObject winTextObject;
    TextMeshProUGUI winText;
    public AudioSource CoinSound;
    public Animator CoverImgAnimator;
    List<int> avoid;
    public float area;
    public List<GameObject> coinPrefabList;
    List<GameObject> coinsMoved = new List<GameObject>();
    public List<Vector3> destinationList;
    public float speed;
    List<Transform> coinsToMoveList = new List<Transform>();
    public bool _isAnim;
    public Vector3 myPos;

    public TextMeshProUGUI walletAmtText;
    int wallet;
    // Start is called before the first frame update
    void Start()
    {
        WalletUpdate();
        avoid = new List<int>();
        myPos = Vector3.zero;
        for (int i = 0; i < coinPrefabList.Count; i++)
        {
            InstantiatePrefabsInParent(coinPrefabList[i]);
        }
        winText = winTextObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        winTextObject.SetActive(false);

    }

    public void MoveCoinsBackWithLove()
    {
        foreach (GameObject child in coinsMoved)
        {
            Transform myTrnasform = child.GetComponent<Transform>();
            StartCoroutine(MoveObjectsBackWithLove(myTrnasform.localPosition, myPos, myTrnasform));
        }
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

    public void Winner()
    {
        winText.text = " +" + Random.Range(1, 10).ToString() + "000";
        winTextObject.SetActive(true);
    }
    public void WalletUpdate()
    {
        wallet = Random.Range(1000, 100000);
        walletAmtText.text = "Rs. " + wallet.ToString();
    }
    public void MoveAllcoinsBack()
    {

        foreach (GameObject child in coinsMoved)
        {
            Transform myTrnasform = child.GetComponent<Transform>();
            StartCoroutine(MoveObjectBack(myTrnasform.localPosition, myPos, myTrnasform));
        }
    }
    void InstantiatePrefabsInParent(GameObject prefabObject)
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject instantiatedPrefab = Instantiate(prefabObject, gameObject.transform);
            instantiatedPrefab.GetComponent<Transform>().localPosition = Vector3.zero;
            coinsToMoveList.Add(instantiatedPrefab.GetComponent<Transform>());

        }
    }

    public void Winner(int winVal)
    {
        switch (winVal)
        {
            case 0:
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    myTrnasform.localPosition = destinationList[1];
                    //StartCoroutine(MoveObject(myTrnasform.localPosition, destinationList[1], myTrnasform));
                }
                break;
            case 1:
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    myTrnasform.localPosition = destinationList[0];
                }
                break;
            case 2:
                foreach (GameObject child in coinsMoved)
                {
                    Transform myTrnasform = child.GetComponent<Transform>();
                    myTrnasform.localPosition = destinationList[2];
                }
                break;
        }
    }

    public IEnumerator AnimStart()
    {
        _isAnim = true;
        avoid.Clear();

        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Wave());
    }

    public IEnumerator Wave()
    {
        if (_isAnim)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));
            CoverImgAnimator.SetTrigger("_is");
            int k = 0, d = 0, c_num = 0;
            d = Random.Range(0, destinationList.Count);
            c_num = Random.Range(0, coinsToMoveList.Count);
            while (avoid.Contains(c_num))
            {
                c_num = Random.Range(0, coinsToMoveList.Count);
            }
            CoinSound.Play();
            StartCoroutine(MoveObject(myPos, destinationList[d], coinsToMoveList[c_num]));
            avoid.Add(c_num);
            coinsMoved.Add(coinsToMoveList[c_num].gameObject);
            k++;
            if (_isAnim && coinsToMoveList.Count != 0)
            {
                yield return new WaitForSeconds(Random.Range(2f, 5f));
                CoverImgAnimator.ResetTrigger("_is");

                StartCoroutine(Wave());
            }
        }


    }

    public void AnimStop()
    {
        _isAnim = false;
    }

    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {
        int k = Random.Range(100, 1000);
        if (wallet >= k)
        {
            wallet -= k;
        }
        else
        {
            wallet -= 15;
        }
        walletAmtText.text = "Rs. " + wallet.ToString();
        Vector3 targetPosModified = targetPos + new Vector3(Random.Range(-area, area), Random.Range(-area, area), 0);

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPosModified);

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

    IEnumerator MoveObjectBack(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPos);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / 0.1f);
            coinTransform.localPosition = Vector3.Lerp(initialPos, targetPos, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPos;



    }

}
