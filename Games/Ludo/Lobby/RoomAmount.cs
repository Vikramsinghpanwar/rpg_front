using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomAmount : MonoBehaviour
{
    public static RoomAmount instance;
    public Button plusButton;
    public Button minusButton;
    public TextMeshProUGUI amountText;
    public int[] roomAmounts = new int[] { 10, 20, 50, 100, 200, 500, 1000 };
    private int currentIndex = 0;
    public int amount;


    private void Awake()
    {
        instance = this;
    }
    public int GetAmount()
    { 
        if (currentIndex < 0 || currentIndex >= roomAmounts.Length)
        {
            Debug.LogError("Invalid index for room amounts.");
            return 0;
        }
        return roomAmounts[currentIndex];   }
    void Start()
    {
        plusButton.onClick.AddListener(Plus);
        minusButton.onClick.AddListener(Minus);
        UpdateAmountText();
    }

    public void UpdateAmountText()
    {
        if (currentIndex < 0 || currentIndex >= roomAmounts.Length)
        {
            amountText.text = "Invalid Amount";
            return;
        }
        amountText.text = roomAmounts[currentIndex].ToString();
    }

    void Plus()
    {
        if (currentIndex < roomAmounts.Length - 1)
        {
            currentIndex++;
            UpdateAmountText();
        }
    }
    void Minus()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateAmountText();
        }
    }

}