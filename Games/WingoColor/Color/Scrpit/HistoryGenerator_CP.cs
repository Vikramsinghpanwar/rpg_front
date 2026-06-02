using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryGenerator_CP : MonoBehaviour
{
    public GameObject coinPrefab;
    public List<Sprite> coinImagesList;  
  
    public void Clear()
    {
        for(int i = 0; i<transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
    public void AddCoin(int coinNum, bool init = false)
    {
        GameObject g = Instantiate(coinPrefab, transform);
        g.GetComponent<Image>().sprite = coinImagesList[coinNum];
        if (!init)
        {
            Destroy(transform.GetChild(transform.childCount - 1).gameObject);
        }
    }
}
