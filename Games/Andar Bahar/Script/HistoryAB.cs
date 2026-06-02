using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryAB : MonoBehaviour
{
    public GameObject redBox;
    public GameObject greenBox;
    public GameObject redColomn;
    public GameObject greenColomn;
    public RectTransform gameResultsContainer; 
    public RectTransform trendUpResultsContainer; 
    public RectTransform trendColomnResultsContainer; 
    private List<int> gameContainerWinners = new List<int>();    // on game
    private List<int> containerUpWinners = new List<int>(); // trend panel Up

    private List<int> containerDownWinners = new List<int>(); // trend panel down
    private List<GameObject> resultInstances = new List<GameObject>(); 
    private List<GameObject> trendUpResultInstances = new List<GameObject>(); 
    private List<GameObject> trendDownResultInstances = new List<GameObject>();

    private int currentRow = 0;   
    private int currentColumn = 0;  
    private int lastWin = -1; 

    public float gapX = 20f;  
    public float gapY = 35f; 

   
    private int redCount = 0;
    private int greenCount = 0;

      public TextMeshProUGUI baharPercentageText;
      public TextMeshProUGUI andarPercentageText;

    void Start()
    {
        // Optionally reset counters at the start
        redCount = 0;
        greenCount = 0;
    }

    private string GetCardRange(int shuffledCards)
    {
        if (shuffledCards <= 5) return "1-5";
        if (shuffledCards <= 10) return "6-10";
        if (shuffledCards <= 15) return "11-15";
        if (shuffledCards <= 20) return "16-25";
        if (shuffledCards <= 25) return "26-30";
        if (shuffledCards <= 30) return "31-35";
        if (shuffledCards <= 35) return "35-40";
        return "41-51";
    }

//   public void AddServerHistoryResults(HisObj[] historya){

//     for (int i = 0; i < historya.Length; i++)
//     {
//         Debug.Log(historya[i].winner +" & " + historya[i].cardsDealt );
//         AddHistory(historya[i].winner , historya[i].cardsDealt );
//     }

//   }
    public void AddHistory(int winNum, int shuffledCards)
    {
          // on game 
        if (gameContainerWinners.Count >= 15)
        {
            gameContainerWinners.RemoveAt(0);
            Destroy(resultInstances[0]);
            resultInstances.RemoveAt(0);
        }

 
        gameContainerWinners.Add(winNum);
        DisplayResult(winNum, shuffledCards);  


         // trend panel Up
         if (containerUpWinners.Count >= 16)
        {
            containerUpWinners.RemoveAt(0);
            Destroy(trendUpResultInstances[0]);
            trendUpResultInstances.RemoveAt(0);
        }

 
        containerUpWinners.Add(winNum);
        TrendUpResult(winNum, shuffledCards);  


        // trend panel down / colomn
        if (containerDownWinners.Count >= 40)
        {
            containerDownWinners.RemoveAt(0);
            Destroy(trendDownResultInstances[0]);
            trendDownResultInstances.RemoveAt(0);
        }

        containerDownWinners.Add(winNum);
        ColomnHistory(winNum,shuffledCards);  

        CalculateAndDisplayPercentages();
    }

    private void DisplayResult(int winNum, int shuffledCards)
    {
        GameObject resultInstance;

        if (winNum == 0) 
        {
            resultInstance = Instantiate(greenBox, gameResultsContainer);
            greenCount++;  
        }
        else 
        {
            resultInstance = Instantiate(redBox, gameResultsContainer);
            redCount++;  
        }

        TextMeshProUGUI pointText = resultInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (pointText != null)
        {
            string cardRange = GetCardRange(shuffledCards);
            pointText.text = cardRange;
        }

        resultInstances.Add(resultInstance);
    }


    private void TrendUpResult(int winNum, int shuffledCards)
    {
        GameObject resultInstance;

        if (winNum == 0) 
        {
            resultInstance = Instantiate(greenBox, trendUpResultsContainer);
            greenCount++;  
        }
        else 
        {
            resultInstance = Instantiate(redBox, trendUpResultsContainer);
            redCount++;  
        }

        TextMeshProUGUI pointText = resultInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (pointText != null)
        {
            string cardRange = GetCardRange(shuffledCards);
            pointText.text = cardRange;
        }

        trendUpResultInstances.Add(resultInstance);
    }

    public void ColomnHistory(int win ,int shuffledCards )
    {
        if (win == lastWin)
        {
            currentRow++;
        }
        else
        {
            currentRow = 0;
            currentColumn++;
        }

        if (currentColumn >= 6)
    {
        currentColumn = 0;
        currentRow++;  // Move to the next row
    }

        GameObject trendresultInstance;
        if (win == 0) 
        {
            trendresultInstance = Instantiate(greenColomn, trendColomnResultsContainer);
        }
        else 
        {
            trendresultInstance = Instantiate(redColomn, trendColomnResultsContainer);
        }

        RectTransform rt = trendresultInstance.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(
            currentColumn * (rt.rect.width + gapX),
            -currentRow * (rt.rect.height + gapY)
        );

    Transform firstChild = trendresultInstance.transform.GetChild(0);
    Transform secondChild = trendresultInstance.transform.GetChild(1);

    if (firstChild != null)
    {
        
        int cardsInFirstChild = shuffledCards; 

        TextMeshProUGUI firstChildText = firstChild.GetComponent<TextMeshProUGUI>();
        if (firstChildText != null)
        {
            
            firstChildText.text =  cardsInFirstChild.ToString();
        }
    }

    if (secondChild != null)
    {
        
        TextMeshProUGUI secondChildText = secondChild.GetComponent<TextMeshProUGUI>();
        if (secondChildText != null)
        {
            string cardRange = GetCardRange(shuffledCards);
            secondChildText.text = cardRange;
        }
    }
        lastWin = win;

        trendDownResultInstances.Add(trendresultInstance);
    }

      private void CalculateAndDisplayPercentages()
    {
        int totalResults = redCount + greenCount;
        
        if (totalResults == 0) return;  

        float redPercentage = (float)redCount / totalResults * 100;
        float greenPercentage = (float)greenCount / totalResults * 100;

         redPercentage = (int)redPercentage;
          greenPercentage = (int)greenPercentage;

    baharPercentageText.text = redPercentage.ToString() + "%";
    andarPercentageText.text = greenPercentage.ToString() + "%";
    }
}
