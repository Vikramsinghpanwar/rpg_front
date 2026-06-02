using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DistributorSingleWin : MonoBehaviour
{
    public Transform dealerObj;
    public List<Vector2> winPositions;
    public Transform burstParent;
    public int playersToWinCount;
    public List<Transform> coinsToDistributeList = new List<Transform>();
    public List<GameObject> winAmntTxtList;

    public int speed = 1000;
    // Start is called before the first frame update
    void Start()
    {
        DeactivateWinAmntTxt();
    }

    public void DeactivateWinAmntTxt()
    {
        for (int i = 0; i < 6; i++)
        {
            winAmntTxtList[i].SetActive(false);
        }
    }

   


    public void Distribute(int[] winBots)
    {
        StartCoroutine(DistributeEnum(winBots));
    }
    public IEnumerator DistributeEnum(int[] winBots)
    {
     
        for(int i= 0; i<coinsToDistributeList.Count; i++)
        {
            Transform g = coinsToDistributeList[i];
            StartCoroutine(MoveObject(g.position, dealerObj.position, g));
        }
        yield return new WaitForSeconds(0.3f);
        int perPlayerCoinsCount = coinsToDistributeList.Count / playersToWinCount;
        Debug.Log("Distributing");
        List<int> luckyPlayers = new List<int>();

        foreach (int i in winBots)
        {
            luckyPlayers.Add(i);
        }

      
        Invoke("DeactivateWinAmntTxt", 1f);

        Debug.Log("coins to distribute list count : " + coinsToDistributeList.Count);

        if(luckyPlayers.Count <= 0)
        {
            yield return null;
        }
        else
        {
            for (int i = 0; i < perPlayerCoinsCount; i++)
            {
                Transform g = coinsToDistributeList[i];
                winAmntTxtList[luckyPlayers[0]].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Random.Range(10, 100) + "000";
                winAmntTxtList[luckyPlayers[0]].SetActive(true);
                coinsToDistributeList.Remove(g);
            }
        }
      


        if (luckyPlayers.Count <= 1)
        {
            yield return null;
        }
        else
        {
            for (int i = 0; i < perPlayerCoinsCount; i++)
            {
                Transform g = coinsToDistributeList[i];
                winAmntTxtList[luckyPlayers[1]].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Random.Range(10, 100) + "000";
                winAmntTxtList[luckyPlayers[1]].SetActive(true);
                coinsToDistributeList.Remove(g);

            }
        }

       



        if (luckyPlayers.Count <= 2)
        {
            yield return null;
        }
        else
        {
            for (int i = 0; i < perPlayerCoinsCount; i++)
            {
                Transform g = coinsToDistributeList[i];
                winAmntTxtList[luckyPlayers[2]].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Random.Range(10, 100) + "000";

                winAmntTxtList[luckyPlayers[2]].SetActive(true);
                coinsToDistributeList.Remove(g);

            }
        }

       

        if (luckyPlayers.Count <= 3)
        {
            yield return null;
        }
        else
        {
            int k = coinsToDistributeList.Count;
            for (int i = 0; i < k; i++)
            {
                Transform g = coinsToDistributeList[i];
                winAmntTxtList[luckyPlayers[3]].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Random.Range(10, 100) + "000";

                winAmntTxtList[luckyPlayers[3]].SetActive(true);
            }
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
        yield return new WaitForSeconds(0.5f);
        coinTransform.position = burstParent.position;
    }
}
