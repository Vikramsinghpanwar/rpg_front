using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderManager : MonoBehaviour
{
    public List<SliderObj> objList;
    GameObject activePanel;
    Image activeBtn;
    private void Start()
    {
        for(int i = 0; i< objList.Count; i++)
        {
            objList[i].btn.color = new Color(0, 0, 0, 0);
        }
        activePanel = objList[0].panel;
        activeBtn = objList[0].btn;
        activePanel.SetActive(true);
        activeBtn.color = new Color(255, 255, 255, 1);
    }

    public void BtnClicked(int val)
    {
        activeBtn.color = new Color(0, 0, 0, 0);
        activePanel.SetActive(false);
        activePanel = objList[val].panel;
        activeBtn = objList[val].btn;
        activePanel.SetActive(true);        
        activeBtn.color = new Color(255, 255, 255, 1);
    }

}


[System.Serializable]
public class SliderObj
{
    public Image btn;
    public GameObject panel;
}
