using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class TrendBoard : MonoBehaviour
{
    public GameObject trendBoardParent; // The parent GameObject with a Grid Layout
    public GameObject outcomePrefab;    // Prefab for each outcome cell
    public List<TrendData> trendDataList; // List of TrendData for different outcomes
    public int maxColumns = 8;
    public int maxRows = 6;

    private List<GameObject> trendOutcomes = new List<GameObject>();

    private void Start()
    {
        InitializeTrendBoard();
    }

    private void InitializeTrendBoard()
    {
        // Clear any existing children from the trend board parent
        foreach (Transform child in trendBoardParent.transform)
        {
            Destroy(child.gameObject);
        }

        trendOutcomes.Clear();

        // Create a grid of empty cells
        for (int i = 0; i < maxRows * maxColumns; i++)
        {
            GameObject outcomeCell = Instantiate(outcomePrefab, trendBoardParent.transform);
            trendOutcomes.Add(outcomeCell);
            SetOutcomeIcon(outcomeCell, "X"); // Default to "X" for empty
        }
    }

    public void AddOutcome(string outcome)
    {
        // Shift existing outcomes to make room for the new outcome at the start
        for (int i = trendOutcomes.Count - 1; i > 0; i--)
        {
            Image prevImage = trendOutcomes[i - 1].GetComponent<Image>();
            trendOutcomes[i].GetComponent<Image>().sprite = prevImage.sprite;
        }

        SetOutcomeIcon(trendOutcomes[0], outcome); // Display the latest outcome in the first cell
    }

    private void SetOutcomeIcon(GameObject cell, string outcome)
    {
        Image cellImage = cell.GetComponent<Image>();
        TrendData data = trendDataList.Find(item => item.outcome == outcome);

        if (data != null && data.iconSprite != null)
        {
            cellImage.sprite = data.iconSprite;
        }
        else
        {
            // Set a default sprite or color if no matching outcome is found
            cellImage.color = Color.gray;
        }
    }
}
