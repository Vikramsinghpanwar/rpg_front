using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RandomRank : MonoBehaviour
{
    
    public Transform parentofTiles;
    public List<Transform> tilesObject;
    float winnerWallet;


    void Start()
    {
        winnerWallet = Random.Range(1f,3f);
        for (int i = 0; i < parentofTiles.childCount; i++)
        {
            tilesObject.Add(parentofTiles.GetChild(i));
        }
        RandomDetail();

    }
    
    public void RandomDetail()
    {
        List<int> temval;
        temval = new List<int>();
        int k;
        for(int i = tilesObject.Count -1; i>=0; i--)
        {
            do
            {
                k = Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count);
            }
            while (temval.Contains(k));
            temval.Add(k);
            float s = Random.Range(0f, 1f);
            winnerWallet += s;
            tilesObject[i].GetChild(0).transform.GetChild(0).GetComponent<Image>().sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
            tilesObject[i].GetChild(1).GetComponent<TextMeshProUGUI>().text = "Guest" + Random.Range(100000, 999999);
            tilesObject[i].GetChild(2).GetComponent<TextMeshProUGUI>().text = "Rs." + winnerWallet.ToString("F2") + "L";
            tilesObject[i].GetChild(3).GetComponent<TextMeshProUGUI>().text = i + 1 + "";
        }
    }
}
