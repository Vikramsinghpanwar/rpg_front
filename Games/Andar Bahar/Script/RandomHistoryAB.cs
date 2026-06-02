using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomHistoryAB : MonoBehaviour
{

    public Transform inGameTrend; // For history in the game trend
    public Transform popUpTrend; // For history in the pop-up trend
    public int gridChildLimit = 10; // Fixed column limit
    public int inGameHistoryLimit = 15; // Limit for in-game trend
    public int popUpHistoryLimit = 18; // Limit for pop-up trend
    Transform latestColumn;
    public Transform gridObject;
    int previousVal;
    public GameObject columnPrefab;
    public Sprite greenSprite;
    public Sprite redSprite;
    public Sprite columnGreen;
    public Sprite columnRed;

    public List<GameObject> inGameHistoryList;
    public List<GameObject> popUpHistoryList;
    public List<GameObject> columnList;

    public GameObject historyPrefab;
    public GameObject cPrefab;
    private int redCount = 0;
    private int greenCount = 0;
    public TextMeshProUGUI baharPercentageText;
      public TextMeshProUGUI andarPercentageText;


    void Start()
    {
        inGameHistoryList = new List<GameObject>();
        popUpHistoryList = new List<GameObject>();
        columnList = new List<GameObject>();
    }

    public void GenerateHistoryData(HisObj[] historya)
    {
        ClearExistingData();
        for (int i = historya.Length - 1; i >= 0; i--)
        {
            AddHistoryData(historya[i].winner, historya[i].cardsDealt);
        }
    }

    private void ClearExistingData()
    {
        foreach (GameObject g in inGameHistoryList) Destroy(g);
        foreach (GameObject g in popUpHistoryList) Destroy(g);
        foreach (GameObject g in columnList) Destroy(g);

        inGameHistoryList.Clear();
        popUpHistoryList.Clear();
        columnList.Clear();
    }

    public void AddHistoryData(int historyArray, int cardsfFlip)
    {
        AddColumn(historyArray);
        AddHistoryToTrends(historyArray, cardsfFlip);
    }

    private void AddColumn(int val)
    {
        if (val != previousVal|| latestColumn == null || latestColumn.childCount >= 6)
        {
            if (columnList.Count >= gridChildLimit)
            {
                Destroy(columnList[0]);
                columnList.RemoveAt(0);
            }

            GameObject newColumn = Instantiate(columnPrefab, gridObject);
            columnList.Add(newColumn);
            latestColumn = newColumn.transform;

            previousVal = val;
        }
    }

    private void AddHistoryToTrends(int k, int cardsfFlip)
    {

           if (k == 0)
        greenCount++;
    else
        redCount++;
        // Remove excess entries in the inGame trend
        if (inGameHistoryList.Count >= inGameHistoryLimit)
        {
            Destroy(inGameHistoryList[0]);
            inGameHistoryList.RemoveAt(0);
        }

        // Remove excess entries in the popUp trend
        if (popUpHistoryList.Count >= popUpHistoryLimit)
        {
            Destroy(popUpHistoryList[0]);
            popUpHistoryList.RemoveAt(0);
        }

        // Create new history entries
        GameObject inGameEntry = Instantiate(historyPrefab, inGameTrend);
        GameObject popUpEntry = Instantiate(historyPrefab, popUpTrend);
        GameObject columnEntry = Instantiate(cPrefab, latestColumn);

        // Assign text and style to history entries
        string cardRange = GetCardRange(cardsfFlip);
        SetHistoryEntry(inGameEntry, cardRange, k);
        SetHistoryEntry(popUpEntry, cardRange, k);
        SetColumnEntry(columnEntry, cardRange, k ,  cardsfFlip );

        inGameHistoryList.Add(inGameEntry);
        popUpHistoryList.Add(popUpEntry);


         CalculateAndDisplayPercentages();
    }

    private void SetHistoryEntry(GameObject entry, string cardRange, int k)
    {
        TextMeshProUGUI text = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.text = cardRange;

        Image image = entry.GetComponent<Image>();
        if (image != null) image.sprite = k == 0 ? greenSprite : redSprite;
    }

    private void SetColumnEntry(GameObject column, string cardRange, int k ,int cardsfFlip )
    {
        TextMeshProUGUI[] texts = column.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 2)
        {
            texts[0].text = cardsfFlip.ToString();
            texts[1].text = cardRange; // Placeholder
        }

        Image image = column.GetComponent<Image>();
        if (image != null) image.sprite = k == 0 ? columnGreen : columnRed;
    }

    private string GetCardRange(int shuffledCards)
    {
         if (shuffledCards <= 5) return "1-5";
        if (shuffledCards <= 10) return "6-10";
        if (shuffledCards <= 15) return "11-15";
        if (shuffledCards <= 25) return "16-25";
        if (shuffledCards <= 30) return "26-30";
        if (shuffledCards <= 35) return "31-35";
        if (shuffledCards <= 40) return "35-40";
        return "41-51";
    }

    private void CalculateAndDisplayPercentages()
    {
        int totalResults = redCount + greenCount;

    // Avoid division by zero
    if (totalResults == 0)
    {
        baharPercentageText.text = "0%";
        andarPercentageText.text = "0%";
        return;
    }

    // Calculate and round percentages
    float redPercentage = Mathf.Round((float)redCount / totalResults * 100);
    float greenPercentage = Mathf.Round((float)greenCount / totalResults * 100);

    // Display percentages
    baharPercentageText.text = redPercentage.ToString("F0") + "%";
    andarPercentageText.text = greenPercentage.ToString("F0") + "%";
    
    }
}

