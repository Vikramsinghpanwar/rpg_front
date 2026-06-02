using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryGeneratorCarRoullete : MonoBehaviour
{

    string[] symbolsArray = new string[] { "One",  "Two", "Three", "Four", "Five", "Six", "Seven", "Eight" };

    public CarRoulleteManager managerRef;
    public Transform trendOne;
    public Transform trendTwo;
    public Transform trendThree;
    public Transform trendFour;
    public Transform trendFive;
    public Transform trendSix;
    public Transform trendSeven;
    public Transform trendEight;

    public Transform trendGameHistory;


    List<Image> trendOneImgList;
    List<Image> trendTwoImgList;
    List<Image> trendThreeImgList;
    List<Image> trendFourImgList;
    List<Image> trendFiveImgList;
    List<Image> trendSixImgList;
    List<Image> trendSevenImgList;
    List<Image> trendEightImgList;

    List<Image> trendGameImgList;

    public List<int> winList;

    public Sprite cross;
    public Sprite red;

    
    private void Start()
    {
        managerRef = FindObjectOfType<CarRoulleteManager>();
        trendOneImgList = new List<Image>();
        trendTwoImgList = new List<Image>();
        trendThreeImgList = new List<Image>();
        trendFourImgList = new List<Image>();
        trendFiveImgList = new List<Image>();
        trendSixImgList = new List<Image>();
        trendSevenImgList = new List<Image>();
        trendEightImgList = new List<Image>();
        trendGameImgList = new List<Image>();


        for(int i = 0; i< trendOne.childCount; i++)
        {
            trendOneImgList.Add(trendOne.GetChild(i).GetComponent<Image>());
        }

        for (int i = 0; i < trendTwo.childCount; i++)
        {
            trendTwoImgList.Add(trendTwo.GetChild(i).GetComponent<Image>());
        }

        for (int i = 0; i < trendThree.childCount; i++)
        {
            trendThreeImgList.Add(trendThree.GetChild(i).GetComponent<Image>());
        }

        for (int i = 0; i < trendFour.childCount; i++)
        {
            trendFourImgList.Add(trendFour.GetChild(i).GetComponent<Image>());
        }

        for (int i = 0; i < trendFive.childCount; i++)
        {
            trendFiveImgList.Add(trendFive.GetChild(i).GetComponent<Image>());
        }

        for (int i = 0; i < trendSix.childCount; i++)
        {
            trendSixImgList.Add(trendSix.GetChild(i).GetComponent<Image>());
        }

        for (int i = 0; i < trendSeven.childCount; i++)
        {
            trendSevenImgList.Add(trendSeven.GetChild(i).GetComponent<Image>());
        }

        for (int i = 0; i < trendEight.childCount; i++)
        {
            trendEightImgList.Add(trendEight.GetChild(i).GetComponent<Image>());
        }

        for (int i = 0; i < trendEight.childCount; i++)
        {
            trendGameImgList.Add(trendGameHistory.GetChild(i).GetComponent<Image>());
        }

    }

    public void InitialHistoryGenerator(string[] hisArray)
    {
        for (int i = 0; i < 15; i++)
        {
            winList.Add(0);
        }

        for (int i= 0; i< hisArray.Length; i++)
        {
            int k = Array.IndexOf(symbolsArray, hisArray[i]);
            winList.Add(k);
            AddHistory(k);
        }
    }
    

    public void AddCrossEntry()
    {
        Sprite s;
        for (int i = 0; i< 14; i++)
        {
            s = trendOneImgList[i + 1].sprite;
            trendOneImgList[i].sprite = s;

            s = trendTwoImgList[i + 1].sprite;
            trendTwoImgList[i].sprite = s;

            s = trendThreeImgList[i + 1].sprite;
            trendThreeImgList[i].sprite = s;

            s = trendFourImgList[i + 1].sprite;
            trendFourImgList[i].sprite = s;

            s = trendFiveImgList[i + 1].sprite;
            trendFiveImgList[i].sprite = s;

            s = trendSixImgList[i + 1].sprite;
            trendSixImgList[i].sprite = s;

            s = trendSevenImgList[i + 1].sprite;
            trendSevenImgList[i].sprite = s;

            s = trendEightImgList[i + 1].sprite;
            trendEightImgList[i].sprite = s;
        }

        trendOneImgList[14].sprite = cross;
        trendTwoImgList[14].sprite = cross;
        trendThreeImgList[14].sprite = cross;
        trendFourImgList[14].sprite = cross;
        trendFiveImgList[14].sprite = cross;
        trendSixImgList[14].sprite = cross;
        trendSevenImgList[14].sprite = cross;
        trendEightImgList[14].sprite = cross;

    }
    public void AddHistory(int val)
    {

        for (int i = 0; i < 14; i++)
        {
            Sprite s = trendGameImgList[i + 1].sprite;
            trendGameImgList[i].sprite = s;
        }
        trendGameImgList[14].sprite = managerRef.items[val];
        winList.RemoveAt(0);
        winList.Add(val);
        AddCrossEntry();

        switch (val)
        {
            case 0:
                trendOneImgList[14].sprite = red;
                break;
            case 1:
                trendTwoImgList[14].sprite = red;
                break;
            case 2:
                trendThreeImgList[14].sprite = red;
                break;
            case 3:
                trendFourImgList[14].sprite = red;
                break;
            case 4:
                trendFiveImgList[14].sprite = red;
                break;
            case 5:
                trendSixImgList[14].sprite = red;
                break;
            case 6:
                trendSevenImgList[14].sprite = red;
                break;
            case 7:
                trendEightImgList[14].sprite = red;
                break;
     
        }
        
    }
}
