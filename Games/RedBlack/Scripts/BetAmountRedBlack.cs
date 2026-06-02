using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BetAmount : MonoBehaviour
{
    public Text amtText;
    public int amount;
    public Sprite plusActive;
    public Sprite minusActive;
    public Sprite plusInActive;
    public Sprite minusInActive;
    public Image plusImg, minusImg;
    private void Start()
    {
        minusImg.sprite = minusInActive;
        plusImg.sprite = plusActive;
        amount = 10;
        amtText.text = amount.ToString();
    }
    public void Increase()
    {
        minusImg.sprite = minusActive;
        if(amount == 10)
        {
            amount = 100;
            amtText.text = amount.ToString();
        }
        else if(amount < 2000)
        {
            amount += 100;
            amtText.text = amount.ToString();

        }
        if(amount == 2000)
        {
            plusImg.sprite = plusInActive;
        }
    }
    public void Decrease()
    {
        plusImg.sprite = plusActive;
        if(amount == 100)
        {
            amount = 10;
            amtText.text = amount.ToString();
        }
        if (amount > 100)
        {
            amount -= 100;
            amtText.text = amount.ToString();

        }
        if(amount <= 100)
        {
            minusImg.sprite = minusInActive;
        }
    }

    public void Max()
    {
        amount = 2000;
        amtText.text = amount.ToString();
        plusImg.sprite = plusInActive;

    }

}
