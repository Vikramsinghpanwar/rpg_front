using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomHistoryWL : MonoBehaviour
{
    public Transform inGameTrend;       // Parent for in-game trends
    public Transform gridObject;       // Primary grid for columns
    public Transform secondaryGrid;    // Secondary grid with alternate rules
    public int inGameTrendLimit = 8;   // Limit for in-game trend entries
    public int primaryGridChildLimit = 20; // Child limit for the primary grid
    public int secondaryGridChildLimit = 30; // Child limit for the secondary grid
    public GameObject columnPrefab;    // Prefab for columns
    public GameObject secondaryColumnPrefab;  // Prefab for columns in the secondary grid

    public Sprite greenSprite;         // Green sprite
    public Sprite redSprite;           // Red sprite
    public Sprite violetSprite;        // Violet sprite
    public Sprite secondaryRedSprite;  // Custom red sprite for secondary grid
    public Sprite secondaryGreenSprite;// Custom green sprite for secondary grid
    public GameObject historyPrefab;   // Prefab for history entries in inGameTrend
    public GameObject noTextPrefab;    // Prefab without text for secondary grid

    private List<GameObject> trendList = new List<GameObject>();           // Trend history entries
    private List<GameObject> columnList = new List<GameObject>();          // Columns for primary grid
    private List<GameObject> secondaryColumnList = new List<GameObject>(); // Columns for secondary grid
    private Transform latestColumn;         // Reference to the latest column in the primary grid
    private Transform latestSecondaryColumn; // Reference to the latest column in the secondary grid
    private string lastResultGroup = "";    // Last result group for primary grid
    private string lastSecondaryResultGroup = ""; // Last result group for secondary grid



     public TextMeshProUGUI primaryGreenPercentageText;
    public TextMeshProUGUI primaryRedPercentageText;
    public TextMeshProUGUI primaryVioletPercentageText;

    public TextMeshProUGUI secondaryRedPercentageText;
    public TextMeshProUGUI secondaryGreenPercentageText;


    private int primaryGreenCount = 0;
private int primaryRedCount = 0;
private int primaryVioletCount = 0;

private int secondaryGreenCount = 0;
private int secondaryRedCount = 0;

    void Start()
    {
        trendList = new List<GameObject>();
        columnList = new List<GameObject>();
        secondaryColumnList = new List<GameObject>();
    }

    public void GenerateHistoryData(int[] historyArray)
    {
     

        // Clear previous entries in all grids
        ClearPreviousHistory();

        foreach (int result in historyArray)
        {
            AddToInGameTrend(result);  // Add to the in-game trend
            TrendGenerator(result, true);  // Add to the primary grid
            TrendGenerator(result, false); // Add to the secondary grid
            CalculateAndDisplayPercentages();
        }
        
    }


    public void AddSingleHistoryResult(int result)
{
    // Add the latest result to history without clearing old entries.
    AddToInGameTrend(result);  // Add the result to in-game trend (one by one)
    TrendGenerator(result, true);  // Add to the primary grid (one by one)
    TrendGenerator(result, false); // Add to the secondary grid (one by one)

      CalculateAndDisplayPercentages();
}

    private void ClearPreviousHistory()
    {
        foreach (GameObject g in trendList)
        {
            Destroy(g);
        }
        trendList.Clear();

        foreach (GameObject g in columnList)
        {
            Destroy(g);
        }
        columnList.Clear();

        foreach (GameObject g in secondaryColumnList)
        {
            Destroy(g);
        }
        secondaryColumnList.Clear();
    }

    private void AddToInGameTrend(int result)
    {
         
        // Create a new entry in the inGameTrend system
        GameObject newHistory = Instantiate(historyPrefab, inGameTrend);
        TextMeshProUGUI text = newHistory.GetComponentInChildren<TextMeshProUGUI>();
        Image image = newHistory.GetComponent<Image>();

        if (text != null)
        {
            text.text = result.ToString();
        }

        // Set the sprite based on the result
        image.sprite = GetResultSprite(result, true);

        // Maintain the limit for in-game trends
        trendList.Add(newHistory);
        if (trendList.Count > inGameTrendLimit)
        {
            Destroy(trendList[0]);
            trendList.RemoveAt(0);
        }
    }

    private void TrendGenerator(int result, bool isPrimaryGrid)
    {
        Transform targetGrid = isPrimaryGrid ? gridObject : secondaryGrid;
        List<GameObject> targetColumnList = isPrimaryGrid ? columnList : secondaryColumnList;
        ref Transform latestColumnRef = ref isPrimaryGrid ? ref latestColumn : ref latestSecondaryColumn;
        ref string lastGroupRef = ref isPrimaryGrid ? ref lastResultGroup : ref lastSecondaryResultGroup;
        int gridChildLimit = isPrimaryGrid ? primaryGridChildLimit : secondaryGridChildLimit;
        GameObject columnPrefabToUse = isPrimaryGrid ? columnPrefab : secondaryColumnPrefab;
        
        Sprite resultSprite = GetResultSprite(result, isPrimaryGrid);
        string resultGroup = GetResultGroup(result, isPrimaryGrid);
    
        UpdateColorCount(resultGroup, isPrimaryGrid);
    
        if (resultGroup != lastGroupRef || latestColumnRef == null || latestColumnRef.childCount >= 5)
        {
            CreateNewColumn(resultGroup, targetGrid, targetColumnList, ref latestColumnRef, gridChildLimit, columnPrefabToUse);
        }
        AddResultToColumn(result, resultSprite, latestColumnRef, isPrimaryGrid);
        lastGroupRef = resultGroup;
    }


public void CalculateAndDisplayPercentages()
{
    // Primary Grid Percentage Calculation (green, red, violet)
    int primaryTotalCount = primaryGreenCount + primaryRedCount + primaryVioletCount;
    if (primaryTotalCount > 0)
    {
        int greenPercentagePrimary = Mathf.RoundToInt((primaryGreenCount / (float)primaryTotalCount) * 100f);
        int redPercentagePrimary = Mathf.RoundToInt((primaryRedCount / (float)primaryTotalCount) * 100f);
        int violetPercentagePrimary = Mathf.RoundToInt((primaryVioletCount / (float)primaryTotalCount) * 100f);

        primaryGreenPercentageText.text = $"{greenPercentagePrimary}%";
        primaryRedPercentageText.text = $"{redPercentagePrimary}%";
        primaryVioletPercentageText.text = $"{violetPercentagePrimary}%";
    }

    // Secondary Grid Percentage Calculation (green, red)
    int secondaryTotalCount = secondaryGreenCount + secondaryRedCount;
    if (secondaryTotalCount > 0)
    {
        int greenPercentageSecondary = Mathf.RoundToInt((secondaryGreenCount / (float)secondaryTotalCount) * 100f);
        int redPercentageSecondary = Mathf.RoundToInt((secondaryRedCount / (float)secondaryTotalCount) * 100f);

        secondaryGreenPercentageText.text = $"{greenPercentageSecondary}%";
        secondaryRedPercentageText.text = $"{redPercentageSecondary}%";
    }
}


private void UpdateColorCount(string resultGroup, bool isPrimaryGrid)
{
    if (isPrimaryGrid)
    {
        if (resultGroup == "green") primaryGreenCount++;
        else if (resultGroup == "red") primaryRedCount++;
        else if (resultGroup == "violet") primaryVioletCount++;

    }
    else
    {
        if (resultGroup == "green") secondaryGreenCount++;
        else if (resultGroup == "red") secondaryRedCount++;

    }
}



    private string GetResultGroup(int result, bool isPrimaryGrid)
    {
        if (isPrimaryGrid)
        {
            // Default rules for the primary grid
            if (result == 0 || result == 5) return "violet";
            if (result == 1 || result == 3 || result == 7 || result == 9) return "green";
            if (result == 2 || result == 4 || result == 6 || result == 8) return "red";
        }
        else
        {
            // Modified rules for the secondary grid
            if (result == 0) return "red";  // 0 is red
            if (result == 5) return "green"; // 5 is green
            if (result == 1 || result == 3 || result == 7 || result == 9) return "green";
            if (result == 2 || result == 4 || result == 6 || result == 8) return "red";
        }
        return "other";
    }

    private Sprite GetResultSprite(int result, bool isPrimaryGrid)
    {
        if (isPrimaryGrid)
        {
            // Default sprite rules
            switch (result)
            {
                case 1: case 3: case 7: case 9:
                    return greenSprite;
                case 2: case 4: case 6: case 8:
                    return redSprite;
                case 0: case 5:
                    return violetSprite;
                default:
                    return null;
            }
        }
        else
        {
            // Modified sprite rules for the secondary grid
            switch (result)
            {
                case 0:
                    return secondaryRedSprite;  // Custom red sprite for 0
                case 5:
                    return secondaryGreenSprite; // Custom green sprite for 5
                case 1: case 3: case 7: case 9:
                    return secondaryGreenSprite;
                case 2: case 4: case 6: case 8:
                    return secondaryRedSprite;
                default:
                    return null;
            }
        }
    }

    private void CreateNewColumn(string resultGroup, Transform parentGrid, List<GameObject> columnList, ref Transform latestColumn, int gridChildLimit, GameObject columnPrefab)
{
    // Remove the oldest column if the grid limit is reached
    if (columnList.Count >= gridChildLimit)
    {
        Destroy(columnList[0]);
        columnList.RemoveAt(0);
    }

    // Create a new column using the appropriate prefab
    GameObject newColumn = Instantiate(columnPrefab, parentGrid);
    newColumn.name = resultGroup;  // Name the column according to the result group
    columnList.Add(newColumn);     // Add it to the respective grid's column list
    latestColumn = newColumn.transform;  // Update the latest column reference
}


    private void AddResultToColumn(int result, Sprite resultSprite, Transform column, bool isPrimaryGrid)
{
    // Create a new entry in the column
    GameObject columnEntry = isPrimaryGrid ? Instantiate(historyPrefab, column) : Instantiate(noTextPrefab, column);
    columnEntry.GetComponent<Image>().sprite = resultSprite;

    if (isPrimaryGrid)
    {
        // Add text only for primary grid
        TextMeshProUGUI columnText = columnEntry.GetComponentInChildren<TextMeshProUGUI>();
        if (columnText != null)
        {
            columnText.text = result.ToString();
        }
    }
}
}
