using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Resulthistroy : MonoBehaviour
{
    public GameObject resultPrefab1; 
    public GameObject resultPrefab2; 
    public GameObject resultPrefab3; 
    public GameObject resultPrefab4; 
    public GameObject resultPrefab5; 
    public GameObject resultPrefab6; 
    public RectTransform resultsContainer; 
    public RectTransform historyContainer50; 
    private List<int> previousWinners = new List<int>();
    private List<GameObject> resultInstances = new List<GameObject>(); // Track instantiated result prefabs


    public GameObject historyColomPrefab;




    private List<GameObject> columns = new List<GameObject>(); 

    void Start()
    {
        
      //  InitializeResults(10);

      
    }

    public void InitializeResults(int count)
    {
      previousWinners.Clear(); 
      resultInstances.ForEach(Destroy);
 

    UpdatePositions();
    }



    public void ColomHistory(float horsOdds , int winHorse){

    if (columns.Count >= 50)
    {
        Destroy(columns[0]);
        columns.RemoveAt(0);
    }

   
    GameObject newRow = Instantiate(historyColomPrefab,historyContainer50);
    newRow.transform.SetParent(historyContainer50); 


    
    TextMeshProUGUI rowText = newRow.transform.GetChild(winHorse).GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();

    
     rowText.text = horsOdds.ToString("F2"); 

  
       columns.Add(newRow);

    }

    
    



    public void AddResultToHistory(int horseID)
    {
        if (previousWinners.Count >= 10)
    {
        previousWinners.RemoveAt(0); 
        Destroy(resultInstances[0]); 
        resultInstances.RemoveAt(0); 
    }

    
    previousWinners.Add(horseID);
    DisplayResult(horseID); 


    }


    private void DisplayResult(int horseID)
    {
        Debug.Log("horse Id: " + horseID);
        GameObject selectedPrefab= null;
      
        if (horseID == 0) selectedPrefab = resultPrefab1;
        else if (horseID == 1) selectedPrefab = resultPrefab2;
        else if (horseID == 2) selectedPrefab = resultPrefab3;
        else if (horseID == 3) selectedPrefab = resultPrefab4;
        else if (horseID == 4) selectedPrefab = resultPrefab5;
        else if (horseID == 5) selectedPrefab = resultPrefab6;        

        

        
        
        GameObject resultInstance = Instantiate(selectedPrefab, resultsContainer);
        TextMeshProUGUI pointText = resultInstance.GetComponentInChildren<TextMeshProUGUI>();

        if (pointText != null)
        {
            pointText.text = "" + (horseID + 1);
        }

        resultInstances.Add(resultInstance);
        UpdatePositions();
    }

    private void UpdatePositions()
    {
        float gap = 30f;

        for (int i = 0; i < resultInstances.Count; i++)
        {
            Transform resultItem = resultInstances[i].transform; 
            float xPosition = i * gap;
            resultItem.localPosition = new Vector3(xPosition, 0f, 0f); 
        }
    }
}
