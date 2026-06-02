using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TrendHistoryJM : MonoBehaviour
{
    public GameObject historyColomPrefab; // Prefab that contains 6 rows
    public RectTransform historyContainer;

    private List<GameObject> columns = new List<GameObject>(); // List to keep track of columns

  
    public void PopulateTrends(int[][] history)
    {
        for(int i = 13; i>= 0; i--)
        {
            AddHistoryColumn(history[i]);
        }
    }

    public void AddHistoryColumn(int[] result)
    {
        if (columns.Count >= 14)
        {
            Destroy(columns[0]);
            columns.RemoveAt(0);
        }

     
        GameObject newColumn = Instantiate(historyColomPrefab, historyContainer);

        
        TextMeshProUGUI[] tmp_Array = newColumn.GetComponentsInChildren<TextMeshProUGUI>();

       
      if (tmp_Array.Length == 6)
        {

            int[] res = new int[] { 0, 0, 0, 0, 0, 0 };
            for(int j = 0; j< 6; j++)
            {
                res[result[j]]++;
            }
            for(int i = 0; i< 6; i++)
            {
                tmp_Array[i].text = "";
                Image roundImg = tmp_Array[i].transform.parent.GetComponent<Image>();
                if (roundImg == null)
                {
                    
                    for (int j = 0; j < 6; j++)
                    {
                        
                    }
                    Debug.LogError("null pada hai ");
                }
                tmp_Array[i].text = res[i].ToString();

                if (res[i] == 0)
                {
                    if(roundImg != null)
                    {
                        roundImg.color = Color.gray;
                    }
                    tmp_Array[i].text = "X";
                }


            }


        }
        

        // Add this new column to the list
        columns.Add(newColumn);
    }
}
