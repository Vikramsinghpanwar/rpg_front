using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameHistoryCrash : MonoBehaviour
{
    public float pHistory1 =0;
    public float pHistory2 = 0;
    public int sameColorCount;
    Sprite pastSpriteCl;
    Transform latestCl;
    public GameObject historyPrefab;
    public int initialHistorycount;
    public Sprite blue, green, red;
    public GameObject columnPrefab;
    public GameObject historyPanelPrefab;
    public Transform historyPanel;
    public Transform historyPanelCon;

    public Color panelRed;
    public Color panelBlue;
    public Color panelGreen;
    public void HistoryUpdate(float[] record)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in historyPanelCon)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < record.Length; i++)
        {
            AddHistory(record[i]);
            //InitAddHistory(record[i]);
        }
    }
    public void AddHistory(float val)
    {
        if (sameColorCount > 3)
        {
            sameColorCount = 0;
            GameObject cl = Instantiate(columnPrefab, historyPanelCon);
            latestCl = cl.transform;
        }
        if(historyPanelCon.childCount > initialHistorycount)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
        GameObject newPrefab = Instantiate(historyPrefab, transform);
        newPrefab.transform.parent = transform;
        newPrefab.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = val.ToString("F2") + "X";
        if (val < 3)
        {
            newPrefab.GetComponent<Image>().sprite = red;
        }
        else if (val > 10)
        {
            newPrefab.GetComponent<Image>().sprite = green;
        }
        else
        {
            newPrefab.GetComponent<Image>().sprite = blue;
        }




        if (pastSpriteCl != newPrefab.GetComponent<Image>().sprite || pastSpriteCl == null)
        {
           
            sameColorCount = 0;
            GameObject cl = Instantiate(columnPrefab, historyPanelCon);
            latestCl = cl.transform;
        }
        else
        {
            if (sameColorCount > 3)
            {
                return;
            }
        }
        sameColorCount++;
        GameObject s = Instantiate(historyPanelPrefab, latestCl);
        if (val < 3)
        {
            s.transform.GetChild(0).GetComponent<Image>().color = panelRed;
        }
        else if (val > 10)
        {
            s.transform.GetChild(0).GetComponent<Image>().color = panelGreen;
        }
        else
        {
            s.transform.GetChild(0).GetComponent<Image>().color = panelBlue;

        }

        pastSpriteCl = newPrefab.GetComponent<Image>().sprite;

    }
    
    
    public void InitAddHistory(float val)
    {
        pHistory2 = pHistory1;
        pHistory1 = val;
        GameObject newPrefab = Instantiate(historyPrefab, transform);
        newPrefab.transform.parent = transform;
        newPrefab.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = val.ToString("F2") + "X";
        if (val < 3)
        {
            newPrefab.GetComponent<Image>().sprite = red;
        }
        else if (val > 10)
        {
            newPrefab.GetComponent<Image>().sprite = green;
        }
        else
        {
            newPrefab.GetComponent<Image>().sprite = blue;

        }


        if (pastSpriteCl != newPrefab.GetComponent<Image>().sprite || pastSpriteCl == null)
        {
            sameColorCount = 0;
            GameObject cl = Instantiate(columnPrefab, historyPanelCon);
            latestCl = cl.transform;
        }
        else
        {
            if (sameColorCount > 3)
            {
                return;
            }
        }
        sameColorCount++;
        GameObject s = Instantiate(historyPanelPrefab, latestCl);
        if (val < 3)
        {
            s.transform.GetChild(0).GetComponent<Image>().color = panelRed;
        }
        else if (val > 10)
        {
            s.transform.GetChild(0).GetComponent<Image>().color = panelGreen;
        }
        else
        {
            s.transform.GetChild(0).GetComponent<Image>().color = panelBlue;

        }

        pastSpriteCl = newPrefab.GetComponent<Image>().sprite;
    }

    public void ToggleHistoryPanel()
    {
        if (historyPanel.gameObject.activeInHierarchy)
        {
            historyPanel.gameObject.SetActive(false);
        }
        else
        {
            historyPanel.gameObject.SetActive(true);
        }
    }
}
