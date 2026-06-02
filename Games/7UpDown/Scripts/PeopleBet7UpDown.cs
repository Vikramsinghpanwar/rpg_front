using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PeopleBet7UpDown : MonoBehaviour
{
    AudioSource coinSound;
    public List<int> avoid;
    public float area;
    public List<GameObject> coinPrefabList, coinsMoved;
    public List<GameObject> destinationObject;
    public List<Vector3> destinationList;
    public float speed;
    public List<Transform> coinsToMoveList;
    public GameObject parentObject;
    public bool _isAnim;
    public Vector3 myPos;
    public Animator profileAnimator;
    public Text walletAmtText;
    public int wallet;
    // Start is called before the first frame update
    void Start()
    {
        coinSound = gameObject.GetComponent<AudioSource>();
        for(int i =0; i< transform.childCount; i++)
        {
            destinationObject.Add(transform.GetChild(i).gameObject);
        }
        destinationList = new List<Vector3>();
        foreach (GameObject g in destinationObject)
        {
            destinationList.Add(g.GetComponent<Transform>().localPosition);
        }
        avoid = new List<int>();
        myPos = Vector3.zero;
        for (int i = 0; i < coinPrefabList.Count; i++)
        {
            InstantiatePrefabsInParent(coinPrefabList[i]);
        }
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
        for (int i = 0; i < 20; i++)
        {
            GameObject instantiatedPrefab = Instantiate(prefabObject, parentObject.transform);
            instantiatedPrefab.GetComponent<Transform>().localPosition = Vector3.zero;
            coinsToMoveList.Add(instantiatedPrefab.GetComponent<Transform>());

        }
    }

    public void WalletUpdate()
    {
        wallet = Random.Range(1000, 100000);
        walletAmtText.text = "₹" + wallet.ToString();
    }


    public IEnumerator AnimStart()
    {
        _isAnim = true;
        avoid.Clear();

        yield return new WaitForSeconds(1f);
        StartCoroutine(Wave());
    }

    public IEnumerator Wave()
    {
        profileAnimator.SetBool("_is", true);
        int k = 0, d = 0, c_num = 0;
        d = Random.Range(0, destinationList.Count);
        c_num = Random.Range(0, coinsToMoveList.Count);
        while (avoid.Contains(c_num))
        {
            c_num = Random.Range(2, coinsToMoveList.Count);
        }
        if (wallet > 0)
        {
            coinSound.Play();
            StartCoroutine(MoveObject(myPos, destinationList[d], coinsToMoveList[c_num]));
            switch (c_num)
            {
                
                case 2:
                    wallet -= 100;
                    break;
                case 3:
                    wallet -= 500;
                    break;
                case 4:
                    wallet -= 150;
                    break;
                case 5:
                    wallet -= 200;
                    break;
                default:
                    wallet -= 100;
                    break;
            }
            if(wallet < 0)
            {
                switch (c_num)
                {
                    case 0:
                        wallet += 10;
                        break;
                    case 1:
                        wallet += 50;
                        break;
                    case 2:
                        wallet += 100;
                        break;
                    case 3:
                        wallet += 500;
                        break;
                    case 4:
                        wallet += 1000;
                        break;
                    case 5:
                        wallet += 5000;
                        break;
                    default:
                        wallet += 10;
                        break;
                }
            }
        }
        
        walletAmtText.text = "₹" + wallet.ToString();
        avoid.Add(c_num);
        coinsMoved.Add(coinsToMoveList[c_num].gameObject);
        k++;
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        profileAnimator.SetBool("_is", false);
        yield return new WaitForSeconds(0.2f);

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
