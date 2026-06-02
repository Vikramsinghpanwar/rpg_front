using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrendGeneratorBestOf5 : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject historyPrefab;
    public Sprite lose;
    public Sprite win;

    public Transform container1;
    public Transform container2;
    public Transform container3;
    public Transform container4;

    public TextMeshProUGUI percentage1;
    public TextMeshProUGUI percentage2;
    public TextMeshProUGUI percentage3;
    public TextMeshProUGUI percentage4;

    public void GenerateHistoryData(int[][] history)
    {
        for(int i = 0; i<10; i++)
        {            
            AddHistory(history[i], true);
        }
        UpdatePercentage();
    }
   

    public void AddHistory(int[] winArray, bool initial = false)
    {
        GameObject g1 = Instantiate(historyPrefab, container1);
        GameObject g2 = Instantiate(historyPrefab, container2);
        GameObject g3 = Instantiate(historyPrefab, container3);
        GameObject g4 = Instantiate(historyPrefab, container4);

        if (!initial)
        {
            Destroy(container1.GetChild(0).gameObject);
            Destroy(container2.GetChild(0).gameObject);
            Destroy(container3.GetChild(0).gameObject);
            Destroy(container4.GetChild(0).gameObject);
        }

        if (winArray[0] == 1)
        {
            g1.GetComponent<Image>().sprite = win;
        }
        else
        {
            g1.GetComponent<Image>().sprite = lose;

        }


        if (winArray[1] == 1)
        {
            g2.GetComponent<Image>().sprite = win;
        }
        else
        {
            g2.GetComponent<Image>().sprite = lose;
        }

        if (winArray[2] == 1)
        {
            g3.GetComponent<Image>().sprite = win;
        }
        else
        {
            g3.GetComponent<Image>().sprite = lose;

        }


        if (winArray[3] == 1)
        {
            g4.GetComponent<Image>().sprite = win;
        }
        else
        {
            g4.GetComponent<Image>().sprite = lose;
        }

        if (!initial)
        {
            Invoke("UpdatePercentage", 0.3f);
        }

        
    }

    void UpdatePercentage()
    {
        int p1 = 0;
        int p2 = 0;
        int p3 = 0;
        int p4 = 0;
        for(int i = 0; i< 10; i++)
        {
            if(container1.GetChild(i).gameObject.GetComponent<Image>().sprite.name == "win")
            {
                p1++;
            }

            if (container2.GetChild(i).gameObject.GetComponent<Image>().sprite.name == "win")
            {
                p2++;
            }

            if (container3.GetChild(i).gameObject.GetComponent<Image>().sprite.name == "win")
            {
                p3++;
            }

            if (container4.GetChild(i).gameObject.GetComponent<Image>().sprite.name == "win")
            {
                p4++;
            }
        }
        percentage1.text = p1 + "0%";
        percentage2.text = p2 + "0%";
        percentage3.text = p3 + "0%";
        percentage4.text = p4 + "0%";
    }
}
