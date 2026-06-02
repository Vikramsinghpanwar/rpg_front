using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameHistoryAviator : MonoBehaviour
{
    public Color myGreen;
    public Color myBlue;
    public Color myPurple;
    public GameObject historyPrefab;
    public int initialHistorycount;
    public Scrollbar horizontal_Scrollbar;
    public Transform mainHistoryPanel;
    public void AddHistoryCount(float val)
    {
        AddHistory(val);
        RemoveHistory();
    }

    public void HistoryUpdate(float[] record)
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        foreach(Transform child in mainHistoryPanel)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < record.Length; i++)
        {
            AddHistory(record[i]);
        }
    }
    // Start is called before the first frame update
   
    
    public void AddHistory(float val)
    {
        RemoveHistory();
        GameObject newPrefab = Instantiate(historyPrefab, transform);
        GameObject newPrefab1 = Instantiate(historyPrefab, transform);
        newPrefab.transform.SetParent(transform);
        newPrefab1.transform.SetParent(mainHistoryPanel);
        newPrefab1.transform.SetAsFirstSibling();

        newPrefab.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = val.ToString("F2") + "X";
        newPrefab1.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = val.ToString("F2") + "X";
        if (val < 2)
        {
            newPrefab.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = myGreen;//green
            newPrefab1.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = myGreen;//green
        }
        else if (val < 10)
        {
            newPrefab.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = myBlue; //blue
            newPrefab1.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = myBlue; //blue
        }
        else
        {
            newPrefab.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = myPurple;  // purple
            newPrefab1.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = myPurple;  // purple

        }
    }

    void RemoveHistory()
    {
        Debug.Log("wanna remove : " + mainHistoryPanel.childCount);
        if(mainHistoryPanel.transform.childCount < 30)
        {
            return;
        }
        Destroy(transform.GetChild(0).gameObject);
        Destroy(mainHistoryPanel.transform.GetChild(mainHistoryPanel.transform .childCount- 1).gameObject);
    }

    public void OpenHistoryPanel()
    {
        if (mainHistoryPanel.parent.gameObject.activeInHierarchy)
        {
            mainHistoryPanel.parent.gameObject.SetActive(false);
        }
        else
        {
            mainHistoryPanel.parent.gameObject.SetActive(true);
        }
    }
}
