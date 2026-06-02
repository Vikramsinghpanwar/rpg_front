using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PawnColorSelector : MonoBehaviour
{
    public Toggle[] toggleArray;
    public int activeIndex;
    private void Start()
    {
        GameLocalData.pawnType = PawnType.yellow;
        foreach(Toggle t in toggleArray)
        {
            t.isOn = false;
        }
        activeIndex = 0;
        toggleArray[activeIndex].isOn = true;
        SelectColor((PawnType)activeIndex + 1);
    }
    public void Select(int index)
    {
        if(activeIndex == index) return;
        toggleArray[activeIndex].isOn = false;
        activeIndex = index;
        toggleArray[activeIndex].isOn = true;
        SelectColor((PawnType)activeIndex + 1); 
    }

    public void SelectColor(PawnType pawnType)
    {
        Debug.Log("Selected color: " + pawnType);
        GameLocalData.pawnType = pawnType;
        
    }
}
