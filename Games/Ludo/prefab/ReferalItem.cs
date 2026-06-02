using UnityEngine;
using TMPro;

public class ReferralItem : MonoBehaviour
{
    public TMP_Text snoText;
    public TMP_Text playerIdText;
    public TMP_Text levelText;
    public TMP_Text amountText;
    public TMP_Text dateText;

    public void Setup(int index, string playerId, int level, double amount, string dateTime)
    {
        snoText.text = index.ToString();
        playerIdText.text = playerId;
        levelText.text = "Level " + level;
        amountText.text = "₹" + amount.ToString("0.##");
        dateText.text = dateTime;
    }
}
