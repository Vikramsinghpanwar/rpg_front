using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HistoryJackpot : MonoBehaviour
{
    public GameObject resultPrefab1; 
    public GameObject resultPrefab2; 
    public GameObject resultPrefab3; 
    public GameObject resultPrefab4; 
    public GameObject resultPrefab5;
    public GameObject resultPrefab6; 
    public RectTransform resultsContainerHistory;
    public RectTransform resultsContainerLatest; 

    public TextMeshProUGUI countSetText;
    public TextMeshProUGUI countPureText;
    public TextMeshProUGUI countSeqText;
    public TextMeshProUGUI countColorText;
    public TextMeshProUGUI countPairText;
    public TextMeshProUGUI countHighText;

    public GameObject columnPrefab;
    public Transform columnsContainer;
    public GameObject ballPrefab; 
    private const int maxColumns = 27; 

    private List<GameObject> columns = new List<GameObject>();
    private List<int> previousWinnersHistory = new List<int>(); 
    private List<GameObject> resultInstancesHistory = new List<GameObject>(); 
    private List<int> previousWinnersLatest = new List<int>(); 
    private List<GameObject> resultInstancesLatest = new List<GameObject>(); 

    void Start()
    {
      //  InitializeResults(55);
    }

    public void AddServerHistoryResults(int[] serverHistory)
{
    foreach (int result in serverHistory)
    {
        AddResultToHistory(result); 
    }
}



    public void AddResultToHistory(int randomResult)
    {
       
        if (previousWinnersHistory.Count >= 55)
        {
            Destroy(resultInstancesHistory[0]);
            resultInstancesHistory.RemoveAt(0);
            previousWinnersHistory.RemoveAt(0);
        }

        previousWinnersHistory.Add(randomResult);
        DisplayResult(randomResult, resultsContainerHistory, resultInstancesHistory);

       
        if (previousWinnersLatest.Count >= 11)
        {
            Destroy(resultInstancesLatest[0]);
            resultInstancesLatest.RemoveAt(0);
            previousWinnersLatest.RemoveAt(0);
        }

        previousWinnersLatest.Add(randomResult);
        DisplayResult(randomResult, resultsContainerLatest, resultInstancesLatest);

        // Update result counts
        UpdateCounts();

        // Manage columns for red ball spawning
        if (columns.Count >= maxColumns)
        {
            Destroy(columns[0]);
            columns.RemoveAt(0);
        }

        GameObject newColumn = Instantiate(columnPrefab, columnsContainer);
        newColumn.transform.SetAsLastSibling();
        columns.Add(newColumn);

        // Spawn red ball in the correct row of the new column
        SpawnResultInColumn(newColumn, randomResult);
    }
    

    private void SpawnResultInColumn(GameObject column, int resultType)
    {
        Debug.Log("resultType: " + resultType);
        if (resultType < 0 || resultType >= column.transform.childCount)
        {
            Debug.LogError($"Invalid resultType: {resultType}");
            return;
        }

        Transform targetRow = column.transform.GetChild(resultType);
        GameObject ball = Instantiate(ballPrefab, targetRow);

        ball.transform.localPosition = Vector3.zero; // Align ball at center
        ball.transform.localScale = Vector3.one; // Reset scale
    }

    private void DisplayResult(int randomResult, RectTransform container, List<GameObject> resultInstances)
    {
        GameObject selectedPrefab = GetPrefabForResult(randomResult);
        GameObject resultInstance = Instantiate(selectedPrefab, container);

        TextMeshProUGUI pointText = resultInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (pointText != null)
        {
            switch (randomResult)
            {
                case 0: pointText.text = "SET"; break;
                case 1: pointText.text = "PURE"; break;
                case 2: pointText.text = "SEQ"; break;
                case 3: pointText.text = "COLOR"; break;
                case 4: pointText.text = "PAIR"; break;
                case 5: pointText.text = "HIGH"; break;
            }
        }

        resultInstances.Add(resultInstance);
        UpdatePositions(container, resultInstances);
    }

    private void UpdatePositions(RectTransform container, List<GameObject> resultInstances)
    {
        float gap = 45f; // Gap between results
        for (int i = 0; i < resultInstances.Count; i++)
        {
            Transform resultItem = resultInstances[i].transform;
            float xPosition = i * gap;
            resultItem.localPosition = new Vector3(xPosition, 0f, 0f);
        }
    }

    private GameObject GetPrefabForResult(int result)
    {
        switch (result)
        {
            case 0: return resultPrefab1;
            case 1: return resultPrefab2;
            case 2: return resultPrefab3;
            case 3: return resultPrefab4;
            case 4: return resultPrefab5;
            case 5: return resultPrefab6;
            default: return resultPrefab6; // Default HIGH
        }
    }

    private void UpdateCounts()
    {
        int countSet = 0, countPure = 0, countSeq = 0, countColor = 0, countPair = 0, countHigh = 0;
        int startIndex = Mathf.Max(0, previousWinnersHistory.Count - 50);

        for (int i = startIndex; i < previousWinnersHistory.Count; i++)
        {
            switch (previousWinnersHistory[i])
            {
                case 1: countSet++; break;
                case 2: countPure++; break;
                case 3: countSeq++; break;
                case 4: countColor++; break;
                case 5: countPair++; break;
                case 6: countHigh++; break;
            }
        }

        countSetText.text = countSet.ToString();
        countPureText.text = countPure.ToString();
        countSeqText.text = countSeq.ToString();
        countColorText.text = countColor.ToString();
        countPairText.text = countPair.ToString();
        countHighText.text = countHigh.ToString();
    }
}
