using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomHIstoryRoullete : MonoBehaviour
{
    public GameObject historyPrefab;
    public GameObject parentObj;
    public int historyCount;
    public Color myred;
    public Color mygreen;
    public Color myblack;

    public List<int> redCount;
    // Start is called before the first frame update

    public void InitialHistoryCreator(string[] hisArray)
    {
        foreach(string s in hisArray)
        {
            AddHistory(int.Parse(s));
        }
    }


    // Update is called once per frame
    public void AddHistory(int val)
    {
        GameObject g = Instantiate(historyPrefab, parentObj.transform);
        if (redCount.Contains(val))
        {
            g.GetComponent<Image>().color = myred;
        }
        else if (val == 0)
        {
            g.GetComponent<Image>().color = mygreen;

        }
        else
        {
            g.GetComponent<Image>().color = myblack;

        }
        g.transform.GetChild(0).gameObject.GetComponent<Text>().text = val + "";
    }

    public  void HistoryUpdate(int val)
    {
        Destroy(parentObj.transform.GetChild(0).gameObject);
        AddHistory(val);
    }
}
