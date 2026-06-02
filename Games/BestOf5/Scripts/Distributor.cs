using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Distributor : MonoBehaviour
{
    public Transform burstParent;
    public List<Transform> playersList;
    public int playersToWinCount;
    public List<Transform> coinsToDistributeList = new List<Transform>();
    public List<GameObject> winAmntTxtList;
    ManagerBac managerRef;
    public int speed = 1000;
    // Start is called before the first frame update
    void Start()
    {
        managerRef = FindObjectOfType<ManagerBac>();
        for (int i = 0; i < 6; i++)
        {
            winAmntTxtList[i].SetActive(false);
        }
    }

 

    public List<int> GenerateRandomList()
    {
        List<int> randomArray = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            int k;
            do
            {
                k = Random.Range(0, playersList.Count);
            }
            while (randomArray.Contains(k));
            randomArray.Add(k);
        }

        return randomArray;
    }

    public void Distribute(bool _canWin)
    {
        //StartCoroutine(Distributee(_canWin));
        
    }

    IEnumerator Distributee(bool _canWin)
    {
        if (!_canWin)
        {
            yield break;
        }
        int perPlayerCoinsCount = coinsToDistributeList.Count / playersToWinCount;
        List<int> luckyPlayers = new List<int>();

        for (int i = 0; i < managerRef.botsWinArray.Length; i++)
        {
            luckyPlayers.Add(managerRef.botsWinArray[i]);
        }

        for (int i = 0; i < perPlayerCoinsCount; i++)
        {
            Transform g = coinsToDistributeList[i];
            StartCoroutine(MoveObject(g.position, playersList[luckyPlayers[0]].position, g));
            winAmntTxtList[luckyPlayers[0]].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + Random.Range(10, 100) + "00";
        }

        for (int i = perPlayerCoinsCount; i < perPlayerCoinsCount * 2; i++)
        {
            Transform g = coinsToDistributeList[i];
            StartCoroutine(MoveObject(g.position, playersList[luckyPlayers[1]].position, g));
            winAmntTxtList[luckyPlayers[1]].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + Random.Range(10, 100) + "00";

        }

        for (int i = perPlayerCoinsCount * 2; i < perPlayerCoinsCount * 3; i++)
        {
            Transform g = coinsToDistributeList[i];
            StartCoroutine(MoveObject(g.position, playersList[luckyPlayers[2]].position, g));
            winAmntTxtList[luckyPlayers[2]].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + Random.Range(10, 100) + "00";

        }

        for (int i = perPlayerCoinsCount * 3; i < coinsToDistributeList.Count; i++)
        {
            Transform g = coinsToDistributeList[i];
            StartCoroutine(MoveObject(g.position, playersList[luckyPlayers[3]].position, g));
            winAmntTxtList[luckyPlayers[3]].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + Random.Range(10, 100) + "00";

        }

        yield return new WaitForSeconds(1f);
        winAmntTxtList[luckyPlayers[0]].SetActive(true);
        winAmntTxtList[luckyPlayers[1]].SetActive(true);
        winAmntTxtList[luckyPlayers[2]].SetActive(true);
        winAmntTxtList[luckyPlayers[3]].SetActive(true);

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < 6; i++)
        {
            winAmntTxtList[i].SetActive(false);
        }

    }
    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPos);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.position = Vector3.Lerp(initialPos, targetPos, fraction);
            yield return null;
        }
        coinTransform.position = targetPos;
        coinTransform.position = burstParent.position;
    }
}
