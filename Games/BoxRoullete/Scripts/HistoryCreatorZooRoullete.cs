using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryCreatorZooRoullete : MonoBehaviour
{
    public Sprite[] elementSpriteList;
    public GameObject historyPrefab;
    public Transform contentRef;
    // Start is called before the first frame update
    public ScrollRect historyScrollRect;

   
    public void InitialHistoryCreator(int[] winArray)
    {        
        foreach(Transform child in contentRef.transform)
        {
            Destroy(child.gameObject);
        }
        for(int i = 0; i< winArray.Length; i++)
        {
            Debug.Log("his : " + winArray[i]);
            GameObject g = Instantiate(historyPrefab, contentRef);
            
            g.GetComponent<Image>().sprite = elementSpriteList[winArray[i]];
        }
    }

    public void AddHistory(int val)
    {
        historyScrollRect.verticalNormalizedPosition = 1f; // Scroll to the top
        Destroy(contentRef.GetChild(0).gameObject);
        GameObject g = Instantiate(historyPrefab, contentRef);
        g.GetComponent<Image>().sprite = elementSpriteList[val];
    }
}


