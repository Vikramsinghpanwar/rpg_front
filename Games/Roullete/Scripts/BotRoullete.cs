using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BotRoullete : MonoBehaviour
{
    Animator slideAnim;
    public AudioSource coinSound;
    public ManagerRoullete managerRef;
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
    public TextMeshProUGUI myName;
    public Image myProfile; 
    public Text walletAmtText;
    public int wallet;
    // Start is called before the first frame update
    void Start()
    {
        slideAnim = gameObject.GetComponent<Animator>();
        for(int i = 0; i< parentObject.transform.childCount; i++)
        {
            destinationList.Add(parentObject.transform.GetChild(i).transform.localPosition);
        }
        //coinSound = gameObject.GetComponent<AudioSource>();
        avoid = new List<int>();
        myPos = Vector3.zero;
        for (int i = 0; i < coinPrefabList.Count; i++)
        {
            InstantiatePrefabsInParent(coinPrefabList[i]);
        }
        WalletUpdate();
        winTextObject.SetActive(false);
        managerRef = FindObjectOfType<ManagerRoullete>();
    }
    public void Winner()
    {
        winText.text = " +" + Random.Range(1, 100).ToString() + "00";
        winTextObject.SetActive(true);
        Invoke("DisableWinTextObj", 2);
    }

    public void SetPlayerData()
    {
        if(LoadOnlinePlayers.onlinePlayerSpritesList.Count == 0)
        {
            Debug.LogError("idhar yaha kam bacha hua hai player data empty ho gai hai wapas bharna padinga");
            LoadOnlinePlayers s = FindObjectOfType<LoadOnlinePlayers>();
            s.LoadAllSpritesFromResources();
            return;
        }
        int k = Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count);
        myProfile.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
        myName.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;
        LoadOnlinePlayers.onlinePlayerSpritesList.RemoveAt(k);
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
        wallet = Random.Range(10000, 100000);
        walletAmtText.text = "₹" + wallet.ToString();
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
        foreach (GameObject child in coinsMoved)
        {
            Transform myTrnasform = child.GetComponent<Transform>();
            StartCoroutine(MoveObjectsBack(myTrnasform.localPosition, myPos, myTrnasform));
        }
    }
    void InstantiatePrefabsInParent(GameObject prefabObject)
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject instantiatedPrefab = Instantiate(prefabObject, parentObject.transform);
            instantiatedPrefab.GetComponent<Transform>().localPosition = Vector3.zero;
            coinsToMoveList.Add(instantiatedPrefab.GetComponent<Transform>());

        }
    }
    public IEnumerator AnimStart()
    {
        _isAnim = true;
        avoid.Clear();
        SetPlayerData();
        yield return new WaitForSeconds(1f);
        StartCoroutine(Wave());
    }

    public IEnumerator Wave()
    {
        int k = 0, d = 0, c_num = 0;
        d = Random.Range(0, destinationList.Count);
        c_num = Random.Range(0, coinsToMoveList.Count);
        while (avoid.Contains(c_num))
        {
            c_num = Random.Range(0, coinsToMoveList.Count);
        }
        slideAnim.SetBool("_is", true);

        StartCoroutine(MoveObject(myPos, destinationList[d], coinsToMoveList[c_num]));
        avoid.Add(c_num);
        yield return new WaitForSeconds(Random.Range(1,2));
        slideAnim.SetBool("_is", false);

        coinsToMoveList[c_num].localPosition = myPos;
        coinsMoved.Add(coinsToMoveList[c_num].gameObject);
        k++;
        yield return new WaitForSeconds(Random.Range(1, 2f));

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
        coinSound.Play();
        int k = Random.Range(100, 500);
        if (wallet > k)
        {
            wallet -= k;
        }
        else
        {
            wallet -= 10;
        }
        if(wallet < 0)
        {
            WalletUpdate();
        }
        walletAmtText.text = "₹" + wallet.ToString();
        Vector3 targetPosModified = targetPos + new Vector3(Random.Range(-areaX, areaX), Random.Range(-area, area), 0);

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
