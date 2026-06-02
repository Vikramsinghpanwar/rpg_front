using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BetManagerSlot : MonoBehaviour
{
    public float betValue;
    public TextMeshProUGUI betText;
    public List<float> betValuesList;
    int betNum;

    public bool _canPress;

    private void Start()
    {
        _canPress = true;
        InitiateList();
        betNum = 1;
        betValue = betValuesList[betNum];

        BetTextUpdate();
    }

    public void InitiateList()
    {
        betValuesList = new List<float>();
        betValuesList.Add(2);
        betValuesList.Add(5);
        betValuesList.Add(10);
        betValuesList.Add(20);
        betValuesList.Add(50);
        betValuesList.Add(100);
        betValuesList.Add(200);
        betValuesList.Add(500);
        betValuesList.Add(1000);
        betValuesList.Add(5000);
    }

    public void BetTextUpdate()
    {
        betText.text = betValue.ToString();
    }
    public void Plus()
    {
        if (!_canPress)
        {
            return;
        }
        if (betValue < 1500)
        {
            betNum += 1;
            betValue = betValuesList[betNum];

            BetTextUpdate();

        }
    }

    public void Minus()
    {
        if (!_canPress)
        {
            return;
        }
        if (betValue > 5)
        {
            betNum -= 1;
            betValue = betValuesList[betNum];

            BetTextUpdate();

        }
    }

    public void MaxBet()
    {
        if (!_canPress)
        {
            return;
        }
        betNum = betValuesList.Count;
        betValue = betValuesList[betNum-1];

        BetTextUpdate();
    }

    
}
