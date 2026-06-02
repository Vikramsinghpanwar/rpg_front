using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomHistoryKQ : MonoBehaviour
{
    public Transform inGameTrend;
    public Transform popUpTrend;
    public int gridChildLimit;
    Transform latestColumn;
    public Transform gridObject;
    char previousVal;
    public GameObject columnPrefab;
    public Sprite kingSprite;
    public Sprite queenSprite;
    public Sprite tieSprite;
    public List<GameObject> historyList;
    public List<GameObject> columnList;
    public GameObject historyPrefab;
    public int historyCount = 8;
    int dragonCount = 0;
    int tigerCount = 0;
    int previousSameCount = 0;
    public int maxColumnDataCount = 5;

    public TextMeshProUGUI dragonPercent;
    public TextMeshProUGUI tigerPercent;

    // Start is called before the first frame update
    void Start()
    {
        previousVal = 'a';
        historyList = new List<GameObject>();
    }

    public void PercentageUpdate()
    {

        if (dragonCount == 0 && tigerCount == 0)
        {
            return;
        }

        float x = (int)((dragonCount * 100) / (dragonCount + tigerCount));
        float y = (int)((tigerCount * 100) / (dragonCount + tigerCount));

        dragonPercent.text = x + "%";
        tigerPercent.text = y + "%";
    }

    public void GenerateHistoryData(char[] historyArray)
    {
        foreach (GameObject g in historyList)
        {
            Destroy(g);

        }
        historyList.Clear();

        foreach (GameObject g in columnList)
        {
            Destroy(g);

        }
        columnList.Clear();

        string s = "";
        for (int i = 0; i < historyArray.Length; i++)
        {
            s += historyArray[i] + " , ";
        }

        for (int i = historyArray.Length - 1; i >= 0; i--)
        {
            TrendGenerator(historyArray[i]);
        }
    }
    public void TrendGenerator(char val)
    {


        if (val != previousVal || previousSameCount >= maxColumnDataCount)
        {
            previousSameCount = 0;

            GameObject s = Instantiate(columnPrefab, gridObject);
            columnList.Add(s);
            s.transform.SetParent(gridObject);
            latestColumn = s.transform;
            if (gridObject.childCount > gridChildLimit)
            {
                Destroy(gridObject.GetChild(0).gameObject);
            }
        }

        previousVal = val;
        AddHistory(val);


    }

    void AddHistory(char k)
    {

        previousSameCount++;
        if (k == 'k')
        {
            dragonCount++;
        }
        else if (k == 'q')
        {
            tigerCount++;
        }
        PercentageUpdate();


        if (historyList.Count > historyCount)
        {
            Destroy(historyList[0]);
            historyList.RemoveAt(0);
        }
        if (historyPrefab == null)
        {
            //Debug.Log("are baba");
        }
        GameObject g = Instantiate(historyPrefab, inGameTrend);
        GameObject p = Instantiate(historyPrefab, popUpTrend);

        GameObject c = Instantiate(historyPrefab, latestColumn);
        c.transform.SetParent(latestColumn);

        historyList.Add(g);

        switch (k)
        {
            case 'k':
                g.GetComponent<Image>().sprite = kingSprite;
                p.GetComponent<Image>().sprite = kingSprite;
                c.GetComponent<Image>().sprite = kingSprite;
                break;
            case 'q':
                g.GetComponent<Image>().sprite = queenSprite;
                p.GetComponent<Image>().sprite = queenSprite;
                c.GetComponent<Image>().sprite = queenSprite;

                break;
            case 't':
                g.GetComponent<Image>().sprite = tieSprite;
                p.GetComponent<Image>().sprite = tieSprite;
                c.GetComponent<Image>().sprite = tieSprite;
                break;
        }


    }
}
